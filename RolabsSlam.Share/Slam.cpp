#include "Slam.hpp"
#include "helpers.hpp"
#include "Optimizer.hpp"

#if defined(_WIN64) || defined(_WIN32)

#else
#include "pch.h"
#endif

Slam::Slam() : _running(false) {
    _cameraInfo.cx = 700;
    _cameraInfo.cy = 700;
    _cameraInfo.fx = 240;
    _cameraInfo.fy = 320;
}

Slam::~Slam() {
    Stop();  // Ensure threads are stopped when the object is destroyed
}

void Slam::Start() {
    if (!_running) {
        _running = true;
        _frameCount = 0;
        _initializationDone = false;
        _mapping_thread = std::thread(&Slam::mappingThread, this);
    }
}

void Slam::Stop() {
    _running = false;
    if (_mapping_thread.joinable()) {
        _mapping_thread.join();
    }
}

void Slam::Track() {

    cv::Mat image;
    {
        std::lock_guard<std::mutex> lock(_image_mutex);
        image = _currentImage.clone();
    }

    // Tracking code here (feature extraction, pose estimation, etc.)
    auto new_frame = std::make_shared<Frame>(image);
    {
        //std::lock_guard<std::mutex> lock(_frame_mutex);
        _currentFrame = new_frame;
    }

    if (_frameCount >= 2)
    {
        if (!_initializationDone)
        {
            initialization();
        }
        else
        {
            // TODO: for testing the pose recover from camera essential matrix
            initialization();
        }
    }
    _previousFrame = _currentFrame;
}

void Slam::mappingThread() {
    while (_running) {
        // Mapping code here (map updates, loop closure, etc.)
#if defined(_WIN64) || defined(_WIN32)
#else
        usleep(1000);  // Sleep to reduce CPU usage
#endif
    }
}

void Slam::GrabImage(const cv::Mat& image) {
    //std::lock_guard<std::mutex> lock(_image_mutex);
    _currentImage = image.clone();
    _frameCount++;
}

void Slam::GetDebugKeyPoints(std::vector<cv::KeyPoint>* keypointsCurrent, std::vector<cv::KeyPoint>* keypointsPrevious) const {
    //std::lock_guard<std::mutex> lock(_frame_mutex);
    if (_currentFrame && keypointsCurrent) {
        *keypointsCurrent = _currentFrame->KeyPoints();
    }
    if (_previousFrame && keypointsPrevious)
    {
        *keypointsPrevious = _previousFrame->KeyPoints();
    }
}

void Slam::initialization()
{
    // Camera intrinsic parameters (assuming fx, fy, cx, cy are known and properly initialized)
    cv::Mat cameraMatrix = (cv::Mat_<double>(3, 3) << _cameraInfo.fx, 0, _cameraInfo.cx, 0, _cameraInfo.fy, _cameraInfo.cy, 0, 0, 1);

    // Ensure that both frames are valid
    if (!_currentFrame || !_previousFrame || _currentFrame->KeyPoints().empty() || _previousFrame->KeyPoints().empty()) {
        std::cerr << "Error: Invalid frames or no keypoints detected." << std::endl;
        _initializationDone = false;
        return;
    }

    // Match keypoints between frames
    std::vector<cv::DMatch> good_matches = matchKeyPoints(*_currentFrame, *_previousFrame);
    // Ensure there are at least 8 matches
    if (good_matches.size() < 8) {
        std::cerr << "Error: No matches found between frames." << std::endl;
        _initializationDone = false;
        return;
    }

    // Estimate the Essential Matrix, previous to current frame
    cv::Mat essential_matrix = estimateEssentialMatrix(*_currentFrame, *_previousFrame, good_matches, cameraMatrix);
    if (essential_matrix.empty()) {
        std::cerr << "Error: Could not estimate the Essential Matrix." << std::endl;
        _initializationDone = false;
        return;
    }
    std::cout << "[Cpp] Essential Matrix = " << essential_matrix << std::endl;

    cv::Mat R, t;
    std::vector<cv::KeyPoint> inlierPreviousFrameKeypoints, inlierCurrentFrameKeypoints;

    // Decompose essential matrix, triangulate points, and filter outliers
    FindRtAndTriangulate(
        essential_matrix,
        cameraMatrix,
        _previousFrame->KeyPoints(),
        _currentFrame->KeyPoints(),
        good_matches,
        _currentFrame->GetMapPoint(),
        R,
        t,
        inlierPreviousFrameKeypoints,
        inlierCurrentFrameKeypoints);

    // Update the keypoints and matches in the current and last frame with only inliers
    _previousFrame->SetKeyPoints(inlierPreviousFrameKeypoints);
    _currentFrame->SetKeyPoints(inlierCurrentFrameKeypoints);

    // Update the outliers in the current frame
    _currentFrame->Outliers().clear(); // Clear previous outliers
    for (size_t i = 0; i < inlierCurrentFrameKeypoints.size(); ++i)
    {
        // Mark inliers as non-outliers (-1 or some similar value)
        _currentFrame->Outliers().push_back(false);
    }

    cv::Mat transformation = createTransformationMatrix(R, t);

    std::cout << "[Cpp] transformation = " << transformation << std::endl;

    // Update the current frame's transformation matrix (camera to world)
    _currentFrame->SetTcw(transformation);

    //Optimizer::PoseOptimization(_currentFrame, _cameraInfo);

    _initialFrame = std::make_shared<Frame>(*_currentFrame);

    // Mark initialization as done
    _initializationDone = true;
}

void Slam::SetCameraInfo(float cx, float cy, float fx, float fy)
{
    _cameraInfo.cx = cx;
    _cameraInfo.cy = cy;
    _cameraInfo.fx = fx;
    _cameraInfo.fy = fx;
}

void Slam::GetCurrentFramePose(cv::Mat *pose)
{
    if (_currentFrame) {
        //std::cout << "[Cpp] get current transformation = " << _currentFrame->Tcw() << std::endl;
        _currentFrame->Tcw().copyTo(*pose);
    }
}
