#pragma once

#include <thread>
#include <mutex>
#include <opencv2/opencv.hpp>
#include "Frame.hpp"

struct CameraInfo
{
    float cx;
    float cy;
    float fx;
    float fy;
};

class Slam {
public:
    Slam();
    ~Slam();

    void start();
    void stop();
    void grabImage(const cv::Mat& image);
    void getDebugKeyPoints(std::vector<cv::KeyPoint>* keypoints) const;
    void initialization();
    void setCameraInfo(float cx, float cy, float fx, float fy);

private:
    void trackingThread();
    void mappingThread();
    std::vector<cv::DMatch> matchKeyPoints(const Frame& frame1, const Frame& frame2);
    std::vector<cv::DMatch> filterGoodMatches(const std::vector<cv::DMatch>& matches);
    cv::Mat estimateEssentialMatrix(const Frame& frame1, const Frame& frame2, const std::vector<cv::DMatch>& good_matches, const cv::Mat& cameraMatrix);

    std::thread _tracking_thread;
    std::thread _mapping_thread;
    std::mutex _image_mutex;
    mutable std::mutex _frame_mutex;
    cv::Mat _currentImage;
    bool _running;

    int _frameCount;

    bool _initializationDone;

    std::shared_ptr<Frame> _currentFrame;
    std::shared_ptr<Frame> _lastFrame;

    CameraInfo _cameraInfo;
};
