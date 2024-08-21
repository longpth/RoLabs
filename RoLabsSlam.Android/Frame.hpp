#pragma once

#include <opencv2/opencv.hpp>
#include <opencv2/features2d.hpp>
#include <vector>

#pragma once

#include <opencv2/opencv.hpp>
#include <opencv2/features2d.hpp>
#include <vector>

class Frame {
public:
    Frame(const cv::Mat& image);

    // Getter methods
    const cv::Mat& Descriptors() const { return _descriptors; }
    const std::vector<cv::KeyPoint>& KeyPoints() const { return _keypoints; }

    // If you intended to pass keypoints to an external array or vector
    void KeyPoints(std::vector<cv::KeyPoint>& kps) const { kps = _keypoints; }

private:
    void detectAndCompute(const cv::Mat& image);

    std::vector<cv::KeyPoint> _keypoints;
    cv::Mat _descriptors;
    cv::Ptr<cv::ORB> _orb;
};