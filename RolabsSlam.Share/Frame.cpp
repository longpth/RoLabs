#include "Frame.hpp"

Frame::Frame(const cv::Mat& image) : _orb(cv::ORB::create()) {
    detectAndCompute(image);
    // Define the Twc matrix at the origin (identity matrix)
    _Tcw = cv::Mat::eye(4, 4, CV_64F); // 4x4 identity matrix
}

void Frame::detectAndCompute(const cv::Mat& image) {
    _orb->detectAndCompute(image, cv::noArray(), _keypoints, _descriptors);
}
