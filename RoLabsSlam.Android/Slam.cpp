#include "Slam.hpp"

Slam::Slam() : _running(false) {}

Slam::~Slam() {
    stop();  // Ensure threads are stopped when the object is destroyed
}

void Slam::start() {
    if (!_running) {
        _running = true;
        _frameCount = 0;
        _tracking_thread = std::thread(&Slam::trackingThread, this);
        _mapping_thread = std::thread(&Slam::mappingThread, this);
    }
}

void Slam::stop() {
    _running = false;
    if (_tracking_thread.joinable()) {
        _tracking_thread.join();
    }
    if (_mapping_thread.joinable()) {
        _mapping_thread.join();
    }
}

void Slam::trackingThread() {
    while (_running) {
        cv::Mat image;
        {
            std::lock_guard<std::mutex> lock(_image_mutex);
            if (_current_image.empty()) continue;
            image = _current_image.clone();
        }

        // Tracking code here (feature extraction, pose estimation, etc.)
        auto new_frame = std::make_shared<Frame>(image);
        {
            std::lock_guard<std::mutex> lock(_frame_mutex);
            _currentFrame = new_frame;
        }

        //if (_frameCount >= 2 && _initializationDone == false)
        //{
        //    initialization();
        //}

    }
}

void Slam::mappingThread() {
    while (_running) {
        // Mapping code here (map updates, loop closure, etc.)
        usleep(1000);  // Sleep to reduce CPU usage
    }
}

void Slam::grabImage(const cv::Mat& image) {
    std::lock_guard<std::mutex> lock(_image_mutex);
    _current_image = image.clone();
}

void Slam::getDebugKeyPoints(std::vector<cv::KeyPoint>* keypoints) const {
    if (!keypoints) return;
    std::lock_guard<std::mutex> lock(_frame_mutex);
    if (_currentFrame) {
        *keypoints = _currentFrame->KeyPoints();
    }
}
