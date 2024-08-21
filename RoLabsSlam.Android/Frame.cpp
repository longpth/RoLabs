#include "Frame.hpp"

Frame::Frame(const cv::Mat& image) : _orb(cv::ORB::create()) {
    detectAndCompute(image);
}

void Frame::detectAndCompute(const cv::Mat& image) {
    _orb->detectAndCompute(image, cv::noArray(), _keypoints, _descriptors);
}
