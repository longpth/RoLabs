#include "Slam.hpp"

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
    stop();  // Ensure threads are stopped when the object is destroyed
}

void Slam::start() {
    if (!_running) {
        _running = true;
        _frameCount = 0;
        _initializationDone = 0;
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
            if (_currentImage.empty()) continue;
            image = _currentImage.clone();
        }

        // Tracking code here (feature extraction, pose estimation, etc.)
        auto new_frame = std::make_shared<Frame>(image);
        {
            std::lock_guard<std::mutex> lock(_frame_mutex);
            _currentFrame = new_frame;
        }

        if (_frameCount >= 2)
        {
            if (_initializationDone)
            {
                initialization();
            }
            else
            {
                // TODO
            }
        }

        _lastFrame = _currentFrame;

    }
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

void Slam::grabImage(const cv::Mat& image) {
    std::lock_guard<std::mutex> lock(_image_mutex);
    _currentImage = image.clone();
}

void Slam::getDebugKeyPoints(std::vector<cv::KeyPoint>* keypoints) const {
    if (!keypoints) return;
    std::lock_guard<std::mutex> lock(_frame_mutex);
    if (_currentFrame) {
        *keypoints = _currentFrame->KeyPoints();
    }
}

void Slam::initialization()
{
    // Camera intrinsic parameters (assuming fx, fy, cx, cy are known and properly initialized)
    cv::Mat cameraMatrix = (cv::Mat_<double>(3, 3) << _cameraInfo.fx, 0, _cameraInfo.cx, 0, _cameraInfo.fy, _cameraInfo.cy, 0, 0, 1);

    // Ensure that both frames are valid
    if (!_currentFrame || !_lastFrame || _currentFrame->KeyPoints().empty() || _lastFrame->KeyPoints().empty()) {
        std::cerr << "Error: Invalid frames or no keypoints detected." << std::endl;
        return;
    }

    // Match keypoints between frames
    std::vector<cv::DMatch> matches = matchKeyPoints(*_lastFrame , *_currentFrame);
    if (matches.empty()) {
        std::cerr << "Error: No matches found between frames." << std::endl;
        return;
    }

    // Filter good matches
    std::vector<cv::DMatch> good_matches = filterGoodMatches(matches);
    if (good_matches.empty()) {
        std::cerr << "Error: No good matches found between frames." << std::endl;
        return;
    }

    // Estimate the Essential Matrix
    cv::Mat essential_matrix = estimateEssentialMatrix(*_lastFrame, *_currentFrame, good_matches, cameraMatrix);
    if (essential_matrix.empty()) {
        std::cerr << "Error: Could not estimate the Essential Matrix." << std::endl;
        return;
    }
}

std::vector<cv::DMatch> Slam::matchKeyPoints(const Frame& frame1, const Frame& frame2) {
    // Use BFMatcher to match descriptors
    cv::BFMatcher matcher(cv::NORM_HAMMING, true); // NORM_HAMMING for ORB, true for cross-check
    std::vector<cv::DMatch> matches;
    matcher.match(frame1.Descriptors(), frame2.Descriptors(), matches);

    // Optionally, you can sort matches based on their distances
    std::sort(matches.begin(), matches.end(), [](const cv::DMatch& a, const cv::DMatch& b) {
        return a.distance < b.distance;
        });

    return matches;
}

std::vector<cv::DMatch> Slam::filterGoodMatches(const std::vector<cv::DMatch>& matches) {
    std::vector<cv::DMatch> good_matches;
    float min_dist = matches.front().distance;
    for (const auto& match : matches) {
        if (match.distance <= std::max(2 * min_dist, 30.0f)) { // Consider adapting the threshold
            good_matches.push_back(match);
        }
    }
    return good_matches;
}

cv::Mat Slam::estimateEssentialMatrix(const Frame& frame1, const Frame& frame2, const std::vector<cv::DMatch>& good_matches, const cv::Mat& cameraMatrix) {
    // Extract the matched keypoints
    std::vector<cv::Point2f> points1, points2;
    for (const auto& match : good_matches) {
        points1.push_back(frame1.KeyPoints()[match.queryIdx].pt);
        points2.push_back(frame2.KeyPoints()[match.trainIdx].pt);
    }

    // Compute the Essential Matrix using RANSAC to handle outliers
    cv::Mat essential_matrix = cv::findEssentialMat(points1, points2, cameraMatrix, cv::RANSAC, 0.999, 1.0);
    return essential_matrix;
}

void Slam::setCameraInfo(float cx, float cy, float fx, float fy)
{
    _cameraInfo.cx = cx;
    _cameraInfo.cy = cy;
    _cameraInfo.fx = fx;
    _cameraInfo.fy = fx;
}
