cmake_minimum_required(VERSION 3.0)

project(OpenCvSharpExtern)

# Set C++ standard
set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Set the Android NDK toolchain
if(ANDROID)
    set(CMAKE_TOOLCHAIN_FILE "${ANDROID_NDK}/build/cmake/android.toolchain.cmake")
    set(ANDROID_NDK "${ANDROID_NDK}")
    set(ANDROID_NATIVE_API_LEVEL 21) # Specify your minimum API level
    set(ANDROID_ABI "arm64-v8a") # Specify your target ABI
endif()

# Remove MinSizeRel & RelWithDebInfo
SET(CMAKE_CONFIGURATION_TYPES "Debug;Release" CACHE STRING "limited configs" FORCE)

# Include directories
include_directories(${CMAKE_SOURCE_DIR})

# Set the path to the OpenCV build directory
set(OpenCV_DIR "/home/longpth/workspace/work/125.MyRobotics/opencv_files/build_android")

# Find OpenCV
find_package(OpenCV REQUIRED)

# Define the library
add_library(OpenCvSharpExtern SHARED
    aruco.cpp
    bgsegm.cpp
    calib3d.cpp
    core.cpp
    cuda.cpp
    dnn.cpp
    dnn_superres.cpp
    face.cpp
    features2d.cpp
    flann.cpp
    highgui.cpp
    img_hash.cpp
    imgcodecs.cpp
    imgproc.cpp
    line_descriptor.cpp
    ml.cpp
    objdetect.cpp
    optflow.cpp
    photo.cpp
    quality.cpp
    shape.cpp
    std_string.cpp
    std_vector.cpp
    stitching.cpp
    superres.cpp
    text.cpp
    tracking.cpp
    video.cpp
    videoio.cpp
    # wechat_qrcode.cpp
    xfeatures2d.cpp
    ximgproc.cpp
    xphoto.cpp
)

# Link OpenCV libraries
target_link_libraries(OpenCvSharpExtern ${OpenCV_LIBS})

# Define the output directory for the library
set_target_properties(OpenCvSharpExtern PROPERTIES
    ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib
    LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib
    RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin
)

# If you need additional configurations or dependencies, add them here