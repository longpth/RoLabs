
#include "helpers.hpp"
#include "MapPoint.hpp"

// Function to decompose essential matrix and triangulate points
void FindRtAndTriangulate(
    const cv::Mat& essential_matrix,
    const cv::Mat& cameraMatrix,
    const std::vector<cv::KeyPoint>& lastFrameKeypoints, // Keypoints from _previousFrame
    const std::vector<cv::KeyPoint>& currentFrameKeypoints, // Keypoints from _currentFrame
    const std::vector<cv::DMatch>& good_matches,
    std::vector<MapPoint*>& map_points,
    cv::Mat& rotation,
    cv::Mat& translation,
    std::vector<cv::KeyPoint>& inlierLastFrameKeypoints, // Output inliers for lastFrame
    std::vector<cv::KeyPoint>& inlierCurrentFrameKeypoints, // Output inliers for currentFrame
    std::vector<cv::DMatch>& inlierMatches) // Output inlier matches
{
    // Decompose the essential matrix into R and t
    cv::Mat R, t;
    cv::Mat mask; // mask to distinguish between inliers and outliers
    std::vector<cv::Point2f> points1, points2;

    for (const auto& match : good_matches)
    {
        // Associate keypoints from the matches
        points1.push_back(currentFrameKeypoints[match.queryIdx].pt);
        points2.push_back(lastFrameKeypoints[match.trainIdx].pt);
    }

    // Recover pose
    int inliersCount = cv::recoverPose(essential_matrix, points1, points2, cameraMatrix, R, t, mask); //five-point.cpp opencv

    // Projection matrices for the two views
    cv::Mat P0 = cameraMatrix * cv::Mat::eye(3, 4, CV_64F); // [I | 0]
    cv::Mat P1(3, 4, CV_64F);
    R.copyTo(P1(cv::Rect(0, 0, 3, 3))); // Copy R into the first 3x3 part of P1
    t.copyTo(P1(cv::Rect(3, 0, 1, 3))); // Copy t into the last column of P1
    P1 = cameraMatrix * P1;

    rotation = R.clone();
    translation = t.clone();

    // Filter inliers and prepare for triangulation
    inlierLastFrameKeypoints.clear();
    inlierCurrentFrameKeypoints.clear();
    inlierMatches.clear();

    std::vector<cv::Point2f> inlierPoints1, inlierPoints2;
    for (int i = 0; i < mask.rows; i++)
    {
        if (mask.at<uchar>(i))
        {
            // Keep the inliers
            inlierCurrentFrameKeypoints.push_back(currentFrameKeypoints[good_matches[i].queryIdx]);
            inlierLastFrameKeypoints.push_back(lastFrameKeypoints[good_matches[i].trainIdx]);
            inlierPoints1.push_back(points1[i]);
            inlierPoints2.push_back(points2[i]);

            // Update the matches to only include inliers
            cv::DMatch inlierMatch;
            inlierMatch.queryIdx = good_matches[i].queryIdx; // New index
            inlierMatch.trainIdx = good_matches[i].trainIdx; // New index
            inlierMatches.push_back(inlierMatch);
        }
    }

    // Triangulate points
    cv::Mat points4D;
    cv::triangulatePoints(P0, P1, inlierPoints1, inlierPoints2, points4D);

    // Convert homogeneous coordinates to 3D points
    map_points.clear();
    for (int i = 0; i < points4D.cols; i++)
    {
        cv::Mat x = points4D.col(i);
        x /= x.at<float>(3); // Normalize to get (X, Y, Z, 1)
        cv::Point3d point3D = cv::Point3d(
            x.at<float>(0),
            x.at<float>(1),
            x.at<float>(2)
        );

        MapPoint* mapPoint = new MapPoint();

        mapPoint->setPosition(point3D);

        map_points.push_back(mapPoint);
    }
}

cv::Mat createTransformationMatrix(const cv::Mat& R, const cv::Mat& t)
{
    // Ensure R is 3x3 and t is 3x1
    CV_Assert(R.rows == 3 && R.cols == 3);
    CV_Assert(t.rows == 3 && t.cols == 1);

    // Create a 4x4 identity matrix
    cv::Mat T = cv::Mat::eye(4, 4, R.type());

    // Copy the rotation matrix into the top-left 3x3 part of T
    R.copyTo(T(cv::Rect(0, 0, 3, 3)));

    // Copy the translation std::vector into the top-right 3x1 part of T
    t.copyTo(T(cv::Rect(3, 0, 1, 3)));

    return T;
}

std::vector<cv::DMatch> matchKeyPoints(const Frame& frame1, const Frame& frame2) {
    // Use BFMatcher to match descriptors
    cv::BFMatcher matcher(cv::NORM_HAMMING);
    std::vector<std::vector<cv::DMatch>> knnMatches;
    matcher.knnMatch(frame1.Descriptors(), frame2.Descriptors(), knnMatches, 2);

    std::vector<cv::DMatch> goodMatches;
    std::vector<int> idx1, idx2;
    std::set<int> idx1s, idx2s;

    for (const auto& matchPair : knnMatches) {
        if (matchPair[0].distance < 0.75f * matchPair[1].distance) {
            const cv::KeyPoint& p1 = frame1.KeyPoints()[matchPair[0].queryIdx];
            const cv::KeyPoint& p2 = frame2.KeyPoints()[matchPair[0].trainIdx];

            // Be within ORB distance 32
            if (matchPair[0].distance < 32) {
                // Check for unique matches
                if (idx1s.find(matchPair[0].queryIdx) == idx1s.end() &&
                    idx2s.find(matchPair[0].trainIdx) == idx2s.end()) {

                    idx1.push_back(matchPair[0].queryIdx);
                    idx2.push_back(matchPair[0].trainIdx);
                    idx1s.insert(matchPair[0].queryIdx);
                    idx2s.insert(matchPair[0].trainIdx);
                    goodMatches.push_back(matchPair[0]);
                }
            }
        }
    }

    // Ensure no duplicates (assert statements)
    assert(std::set<int>(idx1.begin(), idx1.end()).size() == idx1.size());
    assert(std::set<int>(idx2.begin(), idx2.end()).size() == idx2.size());

    return goodMatches;
}

cv::Mat estimateEssentialMatrix(const Frame& frame1, const Frame& frame2, const std::vector<cv::DMatch>& good_matches, const cv::Mat& cameraMatrix) {
    // Extract the matched keypoints
    std::vector<cv::Point2f> points1, points2;
    for (const auto& match : good_matches) {
        points1.push_back(frame1.KeyPoints()[match.queryIdx].pt);
        points2.push_back(frame2.KeyPoints()[match.trainIdx].pt);
    }

    // Compute the Essential Matrix using RANSAC to handle outliers
    cv::Mat essential_matrix = cv::findEssentialMat(points1, points2, cameraMatrix, cv::RANSAC, 0.999, 1.0);
    return essential_matrix;
}
