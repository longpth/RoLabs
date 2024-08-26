#pragma once

#include <opencv2/opencv.hpp>
#include <opencv2/features2d.hpp>
#include <vector>
#include "MapPoint.hpp"

class Frame {
public:
    Frame(const cv::Mat& image);

    // Getter methods
    const cv::Mat& Descriptors() const { return _descriptors; }
    const std::vector<cv::KeyPoint>& KeyPoints() const { return _keypoints; }

    void SetKeyPoints(std::vector<cv::KeyPoint> updatedKeyPoints)
    {
        _keypoints = updatedKeyPoints;
    }

    void SetKeyPoint(int indx, int x, int y)
    {
        _keypoints[indx].pt.x = x;
        _keypoints[indx].pt.y = y;
    }

    // Get the transformation matrix Tcw (camera to world)
    const cv::Mat& Tcw() const { return _Tcw; }

    // Set the transformation matrix Tcw (camera to world)
    void SetTcw(const cv::Mat& transformationMatrix) {
        transformationMatrix.copyTo(_Tcw);
    }

    void CopyFrom(const Frame& other)
    {
        _keypoints = other.KeyPoints();
        _descriptors = other.Descriptors();
        _Tcw = other.Tcw();
    }

    std::vector<MapPoint*>& GetMapPoint()
    {
        return _mapPoints;
    }

    std::vector<bool>& Outliers()
    {
        return _outliers;
    }

private:
    void detectAndCompute(const cv::Mat& image);

    std::vector<cv::KeyPoint> _keypoints;
    cv::Mat _descriptors;
    cv::Ptr<cv::ORB> _orb;

    // Camera transformation matrix (4x4) from world to camera
    cv::Mat _Tcw;

    std::vector<MapPoint*> _mapPoints;

    std::vector<bool> _outliers;
};
