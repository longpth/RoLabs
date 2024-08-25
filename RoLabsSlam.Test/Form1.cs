using OpenCvSharp;
using RoLabsSlamSharp;
using System;
using System.Windows.Forms;
//using SharpDX;
//using SharpDX.Direct3D11;
//using SharpDX.DXGI;
//using SharpDX.Direct3D;
//using Device = SharpDX.Direct3D11.Device;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.WinForms;
using RoLabsSlam.Windows.Test;

namespace RoLabsSlam.Test
{
    public partial class Form1 : Form
    {
        private VideoCapture _videoCapture;
        private Mat _frame;
        private System.Windows.Forms.Timer _timer;
        private RolabsSlamSharpWrapper _rolabsSlamWrapper;
        private bool _isStart = false;

        //3D rendering camera pose
        private Render3D _render3D;

        public Form1()
        {
            InitializeComponent();
            InitializeVideoCapture();
        }

        private void glControl_Load(object? sender, EventArgs e)
        {
            Render3D _render3D = new Render3D(glControl);

            _render3D.glControl_Load(sender, e);
        }

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
                _rolabsSlamWrapper.SetCameraIntrinsics(458.654f, 457.296f, 367.215f, 248.375f);
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
