using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Collections.Specialized;
using Camera.MAUI;
using OpenCvSharp;
using CommunityToolkit.Maui.Views;
using Rolabs.MVVM.Helpers;
using System.Threading;
using MediaManager.Forms.Xaml;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using Android.Views.Animations;
using Rolabs.MVVM.CustomViews;
using RoLabs.MVVM.Services;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Rolabs.MVVM.ViewModels
{
    public enum CameraState { Start, Stop, Pause };
    public class CameraViewModel : BaseViewModel, IDisposable
    {
        public readonly int ProcessWidth=480, ProcessHeight=640;
        private readonly int CaptureWaitTime = 33; // milliseconds

        private static CameraViewModel instance = null;
        public static CameraViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CameraViewModel();
                }
                return instance;
            }
        }

        #region properties

        private CameraInfo camera = null;
        public CameraInfo Camera
        {
            get => camera;
            set
            {
                camera = value;
                OnPropertyChanged(nameof(Camera));
                AutoStartPreview = false;
                OnPropertyChanged(nameof(AutoStartPreview));
            }
        }
        private ObservableCollection<CameraInfo> cameras = new();
        public ObservableCollection<CameraInfo> Cameras
        {
            get => cameras;
            set
            {
                cameras = value;
                OnPropertyChanged(nameof(Cameras));
            }
        }
        public int NumCameras
        {
            set
            {
                if (value > 0)
                    Camera = Cameras.First();
            }
        }

        public bool AutoStartPreview { get; set; } = false;
        public bool AutoStartRecording { get; set; } = false;

        private bool _useCamera=false;
        public bool UseCamera
        {
            get => _useCamera;
            set
            {
                SetProperty(ref _useCamera, value);
                // Trigger visibility changes or any other logic when toggling the camera use
            }
        }

        private ImageSource _imageSource;

        public ImageSource ImageSource
        {
            get => _imageSource;
            set
            {
                if (_imageSource != value)
                {
                    _imageSource = value;
                    OnPropertyChanged();
                }
            }
        }

        private IDrawable _cameraViewCanvas;
        public IDrawable CameraViewCanvas
        {
            get => _cameraViewCanvas;
            set
            {
                if (_cameraViewCanvas != value)
                {
                    _cameraViewCanvas = value;
                    OnPropertyChanged(nameof(CameraViewCanvas));
                }
            }
        }
        #endregion

        #region private variables
        VideoCapture _videoCapture;
        private Mat _frame;
        private Mat _frameTransposed;
        private Mat _frameResized;
        private string _videoPath;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _readVideoTask;
        private CameraState _cameraState = CameraState.Stop;
        #endregion

        #region private methods
        /// <summary>
        /// Asynchronously initializes the camera by copying a video file to the app data directory.
        /// </summary>
        private async void InitializeAsync()
        {
            _videoPath = await Utils.CopyFileToAppDataDirectory("test.mp4");
            OnFileCopyCompleted();
        }

        /// <summary>
        /// Handles the completion of the file copy operation.
        /// Initializes the VideoCapture object and reads the first frame.
        /// </summary>
        private void OnFileCopyCompleted()
        {
            // Handle the completion of the file copy operation
            // For example, you can start video playback or update the UI
            Console.WriteLine("File copy completed.");
            _videoCapture = new VideoCapture(_videoPath);
            _frame = new Mat();
            _frameResized = new Mat();
            _frameTransposed = new Mat();
            _videoCapture.Read(_frame);
        }

        /// <summary>
        /// Continuously captures frames from the video stream, processes them, and updates the ImageSource property.
        /// The loop runs until the provided cancellation token is canceled or a frame cannot be read.
        /// </summary>
        /// <param name="token">Cancellation token to stop the capture loop.</param>
        private async Task CaptureLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_videoCapture.IsOpened())
                {
                    // Read a frame from the video capture
                    bool ret = _videoCapture.Read(_frame);

                    Cv2.Transpose(_frame, _frameTransposed);
                    Cv2.Flip(_frameTransposed, _frameTransposed, FlipMode.Y);
                    Cv2.Resize(_frameTransposed, _frameResized, new OpenCvSharp.Size(ProcessWidth, ProcessHeight));

                    // If the frame could not be read, stop the loop and release resources
                    if (!ret)
                    {
                        _cancellationTokenSource.Cancel();
                        _cancellationTokenSource.Dispose();
                        _cancellationTokenSource = null;
                        // Reset the video capture position to the beginning
                        _videoCapture.Release();
                        _videoCapture.Open(_videoPath);
                        break;
                    }

                    ComputerVisionViewModel.Instance.GrabImageFromVideo(_frameResized);

                    //Cv2.Resize(_frameResized, _frameResized, new OpenCvSharp.Size(ProcessWidth * DeviceDisplay.Current.MainDisplayInfo.Density, ProcessHeight * DeviceDisplay.Current.MainDisplayInfo.Density));

                    await GrabImageAsync(_frameResized.ToBytes(), _frameResized.Width, _frameResized.Height);
                }

                // Wait for a short delay before capturing the next frame
                await Task.Delay(CaptureWaitTime); // Adjust the delay as needed
            }

            if (_cameraState == CameraState.Stop)
            {
                // Reset the video capture position to the beginning
                _videoCapture.Release();
                _videoCapture.Open(_videoPath);
            }
        }

        #endregion

        #region command
        public Command StartCamera { get; set; }
        public Command StopCamera { get; set; }
        public Command PauseCamera { get; set; }

        #endregion

        #region constructor
        public CameraViewModel()
        {

            StartCamera = new Command(() =>
            {
                if (_useCamera)
                {
                    AutoStartPreview = true;
                    OnPropertyChanged(nameof(AutoStartPreview));
                }
                else
                {
                    // Start capturing images
                    ReadVideo();
                }
                _cameraState = CameraState.Start;
            });

            StopCamera = new Command(() =>
            {
                AutoStartPreview = false;
                OnPropertyChanged(nameof(AutoStartPreview));

                _cameraState = CameraState.Stop;
                // Stop capturing images
                StopReadVideo();
            });

            PauseCamera = new Command(() =>
            {
                AutoStartPreview = false;
                OnPropertyChanged(nameof(AutoStartPreview));

                // Stop capturing images
                PauseReadVideo();
                _cameraState = CameraState.Pause;
            });


            OnPropertyChanged(nameof(StartCamera));
            OnPropertyChanged(nameof(StopCamera));

            InitializeAsync();
        }
        #endregion

        #region public methods

        /// <summary>
        /// Captures an image from the video stream, processes it, and updates the ImageSource property.
        /// </summary>
        public void ReadVideo()
        {
            if (_cancellationTokenSource == null && _cameraState != CameraState.Start)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _readVideoTask = Task.Run(() => CaptureLoop(_cancellationTokenSource.Token));
            }
        }

        /// <summary>
        /// Stops capturing images from the video stream.
        /// </summary>
        public void StopReadVideo()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                //_readVideoTask.Wait();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Pause capturing images from the video stream.
        /// </summary>
        public void PauseReadVideo()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _readVideoTask.Wait();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        // The method to process the image data
        public async Task GrabImageAsync(byte[] imageData, int width, int height)
        {
            byte[] clonedImageData = (byte[])imageData.Clone();
            await Task.Run(() =>
            {
                // Handle the image data (e.g., display or process it)
                System.Diagnostics.Debug.WriteLine($"[CameraViewModel] Received image data with length: {imageData.Length} {width}x{height}");
                IImage image = ImageConverter.LoadImageFromByteArray(clonedImageData);

                // Assume the target width and height are the dimensions of the GraphicsView
                float targetWidth = ProcessWidth;
                float targetHeight = ProcessHeight;

                // Calculate the aspect ratio of the original image and the target area
                float imageAspectRatio = (float)width / height;
                float targetAspectRatio = targetWidth / targetHeight;

                float drawWidth, drawHeight;
                float offsetX = 0, offsetY = 0;

                if (imageAspectRatio > targetAspectRatio)
                {
                    // Image is wider than the target area, fit to width
                    drawWidth = targetWidth;
                    drawHeight = targetWidth / imageAspectRatio;
                    offsetY = (targetHeight - drawHeight) / 2;
                }
                else
                {
                    // Image is taller than the target area, fit to height
                    drawHeight = targetHeight;
                    drawWidth = targetHeight * imageAspectRatio;
                    offsetX = (targetWidth - drawWidth) / 2;
                }

                // Create the draw rectangle
                var drawRect = new RectF(offsetX, offsetY, drawWidth, drawHeight);

                // Update the UI using Dispatcher.Dispatch
                Application.Current?.Dispatcher.Dispatch(() =>
                {
                    if(UseCamera == false)
                    {
                        CameraViewCanvas = new ImageDrawable(image, drawRect);
                    }
                    else if (Camera.Name.Contains("Front"))
                    {
                        CameraViewCanvas = new ImageDrawable(image, drawRect, -90);
                    }
                    else if (Camera.Name.Contains("Back"))
                    {
                        CameraViewCanvas = new ImageDrawable(image, drawRect, 90);
                    }
                });
            });
        }
        #endregion

        #region IDisposable implementation
        /// <summary>
        /// Releases camera resources.
        /// </summary>
        public void Dispose()
        {
            // Release camera resources
            _cancellationTokenSource?.Cancel();
            _readVideoTask?.Wait();
            _cancellationTokenSource?.Dispose();
            _videoCapture?.Release();
            _videoCapture?.Dispose();
            _frame?.Dispose();
        }
        #endregion


    }
}