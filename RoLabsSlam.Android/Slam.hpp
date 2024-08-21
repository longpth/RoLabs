#pragma once

#include <thread>
#include <mutex>
#include <opencv2/opencv.hpp>
#include "Frame.hpp"

class Slam {
public:
    Slam();
    ~Slam();

    void start();
    void stop();
    void grabImage(const cv::Mat& image);
    void getDebugKeyPoints(std::vector<cv::KeyPoint>* keypoints) const;

private:
    void trackingThread();
    void mappingThread();

    std::thread _tracking_thread;
    std::thread _mapping_thread;
    std::mutex _image_mutex;
    mutable std::mutex _frame_mutex;
    cv::Mat _current_image;
    bool _running;

    std::shared_ptr<Frame> _currentFrame;
    std::shared_ptr<Frame> _lastFrame;
};
