using OpenCvSharp;

namespace RoLabs.MVVM.Services
{
    public class ImageProcessing
    {
        /// <summary>
        /// Processes the given image by converting it to grayscale and detecting ORB keypoints.
        /// </summary>
        /// <param name="img">The image to process.</param>
        static public Mat DrawFeatureExtraction(Mat img)
        {
            using var gray = new Mat();

            // Convert image to grayscale
            Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);

            if (img.Empty())
            {
                Console.WriteLine("Could not open or find the image!");
                return new Mat();
            }

            // Initialize ORB detector
            var orb = ORB.Create();

            // Detect ORB keypoints and descriptors
            KeyPoint[] keypoints;
            using var descriptors = new Mat();
            orb.DetectAndCompute(gray, null, out keypoints, descriptors);

            // Draw keypoints
            var imgWithKeypoints = img.Clone(); // Create a copy of the input image

            // Iterate through each keypoint and draw a dot
            foreach (var keypoint in keypoints)
            {
                Cv2.Circle(imgWithKeypoints, (OpenCvSharp.Point)keypoint.Pt, 6, Scalar.Green, -1); // Draw a small circle (dot) at each keypoint
            }

            return imgWithKeypoints;
        }

        /// <summary>
        /// Processes the given image by converting it to grayscale and detecting ORB keypoints.
        /// </summary>
        /// <param name="img">The image to process.</param>
        static public KeyPoint[] FeatureExtraction(Mat img)
        {
            // Detect ORB keypoints and descriptors
            KeyPoint[] keypoints;
            using var gray = new Mat();

            // Convert image to grayscale
            Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);

            if (img.Empty())
            {
                Console.WriteLine("Could not open or find the image!");
                return Array.Empty<KeyPoint>();
            }

            // Initialize ORB detector
            var orb = ORB.Create();

            using var descriptors = new Mat();
            orb.DetectAndCompute(gray, null, out keypoints, descriptors);

            //keypoints = orb.Detect(img);

            return keypoints;
        }
    }
}
