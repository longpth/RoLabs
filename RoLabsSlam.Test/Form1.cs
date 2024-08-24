using OpenCvSharp;
using RoLabsSlamSharp;

namespace RoLabsSlam.Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeVideoCapture();
        }

        private VideoCapture _videoCapture;
        private Mat _frame;
        private System.Windows.Forms.Timer _timer;
        private RolabsSlamSharpWrapper _rolabsSlamWrapper;
        private bool _isStart = false;

        private void InitializeVideoCapture()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectDir = Directory.GetParent(baseDir).Parent.Parent.FullName;
            string videoPath = projectDir + @"\..\..\RoLabs\Resources\Raw\slam\video\euroc_V2_01_easy.mp4";
            _videoCapture = new VideoCapture(videoPath);
            _frame = new Mat();
            _timer = new System.Windows.Forms.Timer
            {
                Interval = 33 // Approx. 30 FPS
            };
            _timer.Tick += Timer_Tick;

            _rolabsSlamWrapper = new RolabsSlamSharpWrapper();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_videoCapture.Read(_frame))
            {
                // Convert the Mat to Bitmap on the background thread
                Bitmap newBitmap = BitmapConverter.ToBitmap(_frame);

                // Use Invoke to update the UI on the main thread
                pictureBoxRaw.Invoke(new Action(() =>
                {
                    pictureBoxRaw.Image?.Dispose();
                    pictureBoxRaw.Image = newBitmap;
                }));

                _rolabsSlamWrapper.GrabImage(_frame);
                KeyPoint[] keyPoints = _rolabsSlamWrapper.GetDebugKeyPoints();

                Mat debugImg = _frame.Clone();

                // Draw circles at each keypoint
                foreach (var keypoint in keyPoints)
                {
                    // Draw a circle at each keypoint position
                    Cv2.Circle(debugImg, (OpenCvSharp.Point)keypoint.Pt, 3, Scalar.Green, 2);
                }

                // Convert the Mat to Bitmap on the background thread
                Bitmap processBitmap = BitmapConverter.ToBitmap(debugImg);

                // Use Invoke to update the UI on the main thread
                pictureBoxProcess.Invoke(new Action(() =>
                {
                    pictureBoxProcess.Image?.Dispose();
                    pictureBoxProcess.Image = processBitmap;
                }));

            }
            else
            {
                _timer.Stop();
                _videoCapture.Release();
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (!_isStart)
            {
                _timer.Start();
                _rolabsSlamWrapper.Start();
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            _isStart = false;
            _timer.Stop();
            _rolabsSlamWrapper.Stop();
        }
    }
}
