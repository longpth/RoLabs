#pragma once

#include <thread>
#include <mutex>
#include <opencv2/opencv.hpp>
#include "MyTypes.hpp"
#include "Frame.hpp"

class Slam {
public:
    Slam();
    ~Slam();

    void Start();
    void Stop();
    void GrabImage(const cv::Mat& image);
    void GetDebugKeyPoints(std::vector<cv::KeyPoint>* keypointsCurrent, std::vector<cv::KeyPoint>* keypointsPrevious) const;
    void SetCameraInfo(float cx, float cy, float fx, float fy);
    void GetCurrentFramePose(cv::Mat* pose);
    void Track();

private:
    void initialization();
    void mappingThread();

    std::thread _mapping_thread;
    std::mutex _image_mutex;
    mutable std::mutex _frame_mutex;
    cv::Mat _currentImage;
    bool _running;

    int _frameCount;

    bool _initializationDone;

    std::shared_ptr<Frame> _currentFrame;
    std::shared_ptr<Frame> _previousFrame;
    std::shared_ptr<Frame> _initialFrame;

    CameraInfo _cameraInfo;
};
