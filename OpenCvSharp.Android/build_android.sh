#!/bin/bash

# Paths to Android NDK, OpenCV, and the project directory
ANDROID_NDK_HOME=/home/longpth/workspace/work/001.AndroidStudio/sdk/ndk/25.1.8937393
OPENCV_DIR=/home/longpth/workspace/work/125.MyRobotics/opencv_files/build_android
PROJECT_DIR=/home/longpth/workspace/work/125.MyRobotics/opencvsharp/src/OpenCvSharpExtern

# Create a build directory and navigate to it
mkdir -p $PROJECT_DIR/build
cd $PROJECT_DIR/build

ANDROID_API_LEVEL=21

# Set the target Android API level and ABI (e.g., arm64-v8a, armeabi-v7a, x86, x86_64)
# ANDROID_ABI=arm64-v8a
ANDROID_ABI=x86_64

# Run CMake to configure the project
cmake -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK_HOME/build/cmake/android.toolchain.cmake \
      -DANDROID_NDK=$ANDROID_NDK_HOME \
      -DANDROID_NATIVE_API_LEVEL=$ANDROID_API_LEVEL \
      -DANDROID_ABI=$ANDROID_ABI \
      -DOpenCV_DIR=$OPENCV_DIR \
      -DCMAKE_BUILD_TYPE=Release \
      $PROJECT_DIR
