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

    // Get the transformation matrix Twc (world to camera)
    const cv::Mat& Twc() const { return _Twc; }

    // Set the transformation matrix Twc (world to camera)
    void SetTwc(const cv::Mat& transformationMatrix) {
        transformationMatrix.copyTo(_Twc);
    }

private:
    void detectAndCompute(const cv::Mat& image);

    std::vector<cv::KeyPoint> _keypoints;
    cv::Mat _descriptors;
    cv::Ptr<cv::ORB> _orb;

    // Camera transformation matrix (4x4) from world to camera
    cv::Mat _Twc;
};
