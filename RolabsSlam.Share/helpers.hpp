#pragma once

#include <opencv2/opencv.hpp>
#include <vector>

// Function to decompose essential matrix and triangulate points
void FindRtAndTriangulate(
    const cv::Mat& essential_matrix,
    const cv::Mat& cameraMatrix,
    const std::vector<cv::KeyPoint>& lastFrameKeypoints, // Keypoints from _lastFrame
    const std::vector<cv::KeyPoint>& currentFrameKeypoints, // Keypoints from _currentFrame
    const std::vector<cv::DMatch>& good_matches,
    std::vector<cv::Point3d>& map_points,
    cv::Mat& rotation,
    cv::Mat& translation)
{
    // Decompose the essential matrix into R and t
    cv::Mat R, t;
    cv::Mat mask; // mask to distinguish between inliers and outliers
    std::vector<cv::Point2f> points1, points2;

    for (const auto& match : good_matches)
    {
        // Associate keypoints from the matches
        points1.push_back(lastFrameKeypoints[match.queryIdx].pt);
        points2.push_back(currentFrameKeypoints[match.trainIdx].pt);
    }

    // Recover pose
    cv::recoverPose(essential_matrix, points1, points2, cameraMatrix, R, t, mask);

    // Projection matrices for the two views
    cv::Mat P0 = cameraMatrix * cv::Mat::eye(3, 4, CV_64F); // [I | 0]
    cv::Mat P1(3, 4, CV_64F);
    R.copyTo(P1(cv::Rect(0, 0, 3, 3))); // Copy R into the first 3x3 part of P1
    t.copyTo(P1(cv::Rect(3, 0, 1, 3))); // Copy t into the last column of P1
    P1 = cameraMatrix * P1;

    rotation = R.clone();
    translation = t.clone();

    // Triangulate points
    cv::Mat points4D;
    cv::triangulatePoints(P0, P1, points1, points2, points4D);

    // Convert homogeneous coordinates to 3D points
    map_points.clear();
    for (int i = 0; i < points4D.cols; i++)
    {
        cv::Mat x = points4D.col(i);
        x /= x.at<float>(3); // Normalize to get (X, Y, Z, 1)
        cv::Point3d point3D(
            x.at<float>(0),
            x.at<float>(1),
            x.at<float>(2)
        );
        map_points.push_back(point3D);
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

    // Copy the translation vector into the top-right 3x1 part of T
    t.copyTo(T(cv::Rect(3, 0, 1, 3)));

    return T;
}
