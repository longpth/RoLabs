using System.Diagnostics;
using System.Runtime.InteropServices;
using AndroidX.Interpolator.View.Animation;
using Kotlin;
using OpenCvSharp;
using OpenCvSharp.Internal;
using OpenCvSharp.Internal.Vectors;


namespace RoLabsSlamSharp
{
    public class RolabsSlamSharpWrapper : IDisposable
    {
        private const string DllExtern = "libRoLabsSlam_Android.so";

        // P/Invoke for the native function
        [DllImport(DllExtern, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int RoLabsFeatureExtraction_export(
            IntPtr image, IntPtr keypoints);

        // P/Invoke for the native function
        [DllImport(DllExtern, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern ExceptionStatus TestCopyImage_export(
            IntPtr image, IntPtr imageOut);

        // P/Invoke for the native function
        [DllImport(DllExtern, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int TestAddFunc_export(int a, int b);

        public KeyPoint[] RoLabsFeatureExtraction(Mat image)
        {
            using var keypointsVec = new VectorOfKeyPoint();

            int keypointsCnts;

            //int a = 5, b = 6, c = 0;

            //c = TestAddFunc_export(a, b);

            keypointsCnts = RoLabsFeatureExtraction_export(image.CvPtr, keypointsVec.CvPtr);

            GC.KeepAlive(image);
            KeyPoint[] keypoints = keypointsVec.ToArray();
            return keypoints;
        }

        public void TestCopyImage(Mat image, Mat outImage)
        {
            NativeMethods.HandleException(
                                            TestCopyImage_export(image.CvPtr, outImage.CvPtr));
            GC.KeepAlive(image);
        }

        ~RolabsSlamSharpWrapper()
        {
            Dispose();
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources if needed
            GC.SuppressFinalize(this);
        }
    }
}
