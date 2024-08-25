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

    void Start();
    void Stop();
    void GrabImage(const cv::Mat& image);
    void GetDebugKeyPoints(std::vector<cv::KeyPoint>* keypoints) const;
    void SetCameraInfo(float cx, float cy, float fx, float fy);

private:
    void initialization();
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
    std::shared_ptr<Frame> _initialFrame;

    CameraInfo _cameraInfo;
};
