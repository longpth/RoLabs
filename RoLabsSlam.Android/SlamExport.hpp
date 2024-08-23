#pragma once

#include <opencv2/opencv.hpp>
#include <vector>
#include <memory>
#include "types_c.h"
#include "my_functions.h"

// Forward declare Slam class to avoid including the full definition here
class Slam;

CVAPI(Slam*) Slam_create();
CVAPI(void) Slam_destroy(Slam* slam);

// Wrap Slam methods
CVAPI(void) Slam_grabImage(Slam* slam, cv::Mat* image);
CVAPI(void) Slam_getDebugKeyPoints(Slam* slam, std::vector<cv::KeyPoint>* keypoints);
CVAPI(void) Slam_stop(Slam* slam);
CVAPI(void) Slam_start(Slam* slam);
CVAPI(void) Slam_setIntrinsicsMatrix(Slam* slam, float fx, float fy, float cx, float cy);
