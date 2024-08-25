#include "MapPoint.hpp"

// Constructors
MapPoint::MapPoint() : _visibleCount(0), _foundCount(0) {}

MapPoint::MapPoint(const cv::Point3d& position)
    : _position(position), _visibleCount(0), _foundCount(0) {}

// Getters
cv::Point3d MapPoint::getPosition() const
{
    return _position;
}

const std::vector<cv::KeyPoint>& MapPoint::getKeyPoints() const
{
    return _keyPoints;
}

cv::Mat MapPoint::getDescriptor() const
{
    return _descriptor;
}

int MapPoint::getVisibleCount() const
{
    return _visibleCount;
}

int MapPoint::getFoundCount() const
{
    return _foundCount;
}

// Setters
void MapPoint::setPosition(const cv::Point3d& position)
{
    _position = position;
}

void MapPoint::addKeyPoint(const cv::KeyPoint& keypoint)
{
    _keyPoints.push_back(keypoint);
}

void MapPoint::setDescriptor(const cv::Mat& descriptor)
{
    _descriptor = descriptor.clone();
}

void MapPoint::incrementVisibleCount()
{
    ++_visibleCount;
}

void MapPoint::incrementFoundCount()
{
    ++_foundCount;
}

// Utility: Add an observation
void MapPoint::addObservation(const cv::KeyPoint& keypoint, const cv::Mat& descriptor)
{
    addKeyPoint(keypoint);
    setDescriptor(descriptor);
    incrementVisibleCount();
}
