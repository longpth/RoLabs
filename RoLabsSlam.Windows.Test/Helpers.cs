using System;
using OpenTK.Mathematics;
using OpenCvSharp;

namespace RoLabsSlam.Windows.Test
{
    public static class MatExtensions
    {
        public static Matrix4 ToMatrix4(this Mat mat)
        {
            //if (mat.Rows != 3 || mat.Cols != 4)
            //{
            //    throw new ArgumentException("The input Mat must be a 3x4 matrix.");
            //}

            // Convert the 3x4 Mat to a 4x4 Matrix4
            var OpenGLMatrix = new Matrix4(
                mat.At<float>(0, 0), mat.At<float>(0, 1), mat.At<float>(0, 2), mat.At<float>(0, 3),
                mat.At<float>(1, 0), mat.At<float>(1, 1), mat.At<float>(1, 2), mat.At<float>(1, 3),
                mat.At<float>(2, 0), mat.At<float>(2, 1), mat.At<float>(2, 2), mat.At<float>(2, 3),
                0f, 0f, 0f, 1f // The last row [0, 0, 0, 1]
            );

            return OpenGLMatrix;
        }
    }
}
