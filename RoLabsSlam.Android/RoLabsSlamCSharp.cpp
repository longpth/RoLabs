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

extern void TestCopyImage(cv::Mat* image, cv::Mat* outImage);

CVAPI(ExceptionStatus) TestCopyImage_export(
    cv::Mat* image, cv::Mat* outImage)
{
    BEGIN_WRAP
        TestCopyImage(image, outImage);
    END_WRAP
}

extern int TestAddFunc(int, int);

CVAPI(int) TestAddFunc_export(
    int a, int b)
{
    return TestAddFunc(a, b);
}
