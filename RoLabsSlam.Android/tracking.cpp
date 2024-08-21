#include <android/log.h>
#include <opencv2/opencv.hpp>
#include <sstream>
#include <vector>
#include "my_functions.h"

int RoLabsFeatureExtraction(cv::Mat* image, std::vector<cv::KeyPoint>* keypoints)
{
    // Initialize the ORB detector
    cv::Ptr<cv::FeatureDetector> orb = cv::ORB::create();

    cv::Mat mask;

    // Detect keypoints
    orb->detect(*image, *keypoints);

    return (*keypoints).size();
}

void TestCopyImage(cv::Mat* image, cv::Mat* outImage)
{
    *outImage = image->clone();
}

int TestAddFunc(int a, int b)
{
    return a + b;
}