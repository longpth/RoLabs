#pragma once


#include <opencv2/opencv.hpp>
#include <vector>
#include <algorithm>
#include "Frame.hpp"

extern void FindRtAndTriangulate(
    const cv::Mat& essential_matrix,
    const cv::Mat& cameraMatrix,
    const std::vector<cv::KeyPoint>& lastFrameKeypoints, // Keypoints from _previousFrame
    const std::vector<cv::KeyPoint>& currentFrameKeypoints, // Keypoints from _currentFrame
    const std::vector<cv::DMatch>& good_matches,
    std::vector<MapPoint*>& map_points,
    cv::Mat& rotation,
    cv::Mat& translation,
    std::vector<cv::KeyPoint>& inlierLastFrameKeypoints, // Output inliers for lastFrame
    std::vector<cv::KeyPoint>& inlierCurrentFrameKeypoints // Output inliers for currentFrame
    );


extern cv::Mat createTransformationMatrix(const cv::Mat& R, const cv::Mat& t);

extern std::vector<cv::DMatch> matchKeyPoints(const Frame& frame1, const Frame& frame2);

extern cv::Mat estimateEssentialMatrix(const Frame& frame1, const Frame& frame2, const std::vector<cv::DMatch>& good_matches, const cv::Mat& cameraMatrix);