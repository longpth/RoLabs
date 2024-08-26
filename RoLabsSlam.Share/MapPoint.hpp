#pragma once

#include <opencv2/core.hpp>
#include <opencv2/features2d.hpp>
#include <vector>

class MapPoint
{
public:
    // Constructors
    MapPoint();
    MapPoint(const cv::Point3d& position);

    // Getters
    cv::Point3d getPosition() const;
    const std::vector<cv::KeyPoint>& getKeyPoints() const;
    cv::Mat getDescriptor() const;
    int getVisibleCount() const;
    int getFoundCount() const;

    // Setters
    void setPosition(const cv::Point3d& position);
    void addKeyPoint(const cv::KeyPoint& keypoint);
    void setDescriptor(const cv::Mat& descriptor);
    void incrementVisibleCount();
    void incrementFoundCount();

    // Utility: Add an observation
    void addObservation(const cv::KeyPoint& keypoint, const cv::Mat& descriptor);

private:
    cv::Point3d _position;                // 3D position of the map point
    std::vector<cv::KeyPoint> _keyPoints; // Keypoints associated with this map point
    cv::Mat _descriptor;                  // Descriptor associated with the map point
    int _visibleCount;                    // Number of times the point was observed in different frames
    int _foundCount;                      // Number of times the point was successfully matched/found
};

