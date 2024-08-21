#include "types_c.h"
#include "my_functions.h"
#include <string>

typedef std::string StrExceptionStatus;

extern int RoLabsFeatureExtraction(cv::Mat* image, std::vector<cv::KeyPoint>* keypoints);

CVAPI(int) RoLabsFeatureExtraction_export(
    cv::Mat* image, std::vector<cv::KeyPoint>* keypoints)
{
    try {
        return RoLabsFeatureExtraction(image, keypoints);
    }
    catch (const std::exception& e) {
        return -1;
    }
}
