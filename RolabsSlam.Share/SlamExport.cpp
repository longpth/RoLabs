#include "SlamExport.hpp"
#include "Slam.hpp"

CVAPI(Slam*) Slam_create() {
    return new Slam();
}

CVAPI(void) Slam_destroy(Slam* slam) {
    delete slam;
}

CVAPI(void) Slam_grabImage(Slam* slam, cv::Mat* image) {
    if (slam && image) {
        slam->GrabImage(*image);
    }
}

CVAPI(void) Slam_getDebugKeyPoints(Slam* slam, std::vector<cv::KeyPoint>* keypoints) {
    if (slam && keypoints) {
        slam->GetDebugKeyPoints(keypoints);
    }
}

CVAPI(void) Slam_stop(Slam* slam) {
    if (slam) {
        slam->Stop();
    }
}

CVAPI(void) Slam_start(Slam * slam) {
    if (slam) {
        slam->Start();
    }
}

CVAPI(void) Slam_setIntrinsicsMatrix(Slam* slam, float fx, float fy, float cx, float cy) {
    if (slam) {
        slam->SetCameraInfo(fx, fy, cx, cy);
    }
}