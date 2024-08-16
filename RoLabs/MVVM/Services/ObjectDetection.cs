using Android.Graphics;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;
using Rolabs.MVVM.Helpers;
using Rolabs.MVVM.ML.YoloParser;
using RoLabs.MVVM.ML.YoloParser;
using System.Collections.Generic;
using System.IO;
using static Android.Provider.MediaStore;

namespace RoLabs.MVVM.Services
{
    public class ObjectDetection
    {
        private static ObjectDetection _instance;
        private static readonly object _lock = new object();
        private static string _modelPath;

        private readonly MLContext mlContext;
        private readonly ITransformer _transformer;
        private IList<YoloBoundingBox> _boundingBoxes = new List<YoloBoundingBox>();

        // Private constructor to prevent direct instantiation
        private ObjectDetection()
        {
            this.mlContext = new MLContext();
            _transformer = LoadModel(_modelPath);
        }

        // Public static method to get the instance of the class
        public static ObjectDetection Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _modelPath = Utils.CopyFileToAppDataDirectory("models//tinyyolov2-8.onnx").GetAwaiter().GetResult();
                        _instance = new ObjectDetection();
                    }
                }
                return _instance;
            }
        }

        public struct ImageNetSettings
        {
            public const int imageHeight = 416;
            public const int imageWidth = 416;
        }

        public struct TinyYoloModelSettings
        {
            public const string ModelInput = "image";
            public const string ModelInputShape = "image_shape";
            public const string BoxesOutput = "grid";
            public const string ScoresOutput = "yolonms_layer_1:1";
            public const string IndicesOutput = "yolonms_layer_1:2";
        }

        public class ImageNetData
        {
            [ImageType(416, 416)]
            public MLImage Image { get; set; }
        }

        private ITransformer LoadModel(string modelLocation)
        {
            Console.WriteLine("Read model");
            Console.WriteLine($"Model location: {modelLocation}");
            Console.WriteLine($"Default parameters: image size=({ImageNetSettings.imageWidth},{ImageNetSettings.imageHeight})");

            // Dummy data to fit the pipeline
            var data = mlContext.Data.LoadFromEnumerable(new List<ImageNetData>());

            var pipeline = mlContext.Transforms.ResizeImages(
                                outputColumnName: "imageResized",
                                imageWidth: ImageNetSettings.imageWidth,
                                imageHeight: ImageNetSettings.imageHeight,
                                inputColumnName: nameof(ImageNetData.Image))
                            .Append(mlContext.Transforms.ExtractPixels(
                                outputColumnName: TinyYoloModelSettings.ModelInput,
                                inputColumnName: "imageResized"))
                            .Append(mlContext.Transforms.ApplyOnnxModel(
                                modelFile: modelLocation,
                                outputColumnNames: new[]
                                {
                                TinyYoloModelSettings.BoxesOutput,
                                    //TinyYoloModelSettings.ScoresOutput,
                                    //TinyYoloModelSettings.IndicesOutput
                                },
                                inputColumnNames: new[] { TinyYoloModelSettings.ModelInput }));

            var model = pipeline.Fit(data);

            return model;
        }

        private IList<YoloBoundingBox> PredictDataUsingModel(IDataView testData, ITransformer model)
        {
            IDataView scoredData = model.Transform(testData);

            IEnumerable<float[]> probabilities = scoredData.GetColumn<float[]>(TinyYoloModelSettings.BoxesOutput);
            //var scores = scoredData.GetColumn<float[]>(TinyYoloModelSettings.ScoresOutput);
            //var indices = scoredData.GetColumn<int[]>(TinyYoloModelSettings.IndicesOutput);

            // Post-process model output
            YoloOutputParser parser = new YoloOutputParser();

            var probabilityMap = probabilities.First();

            var boundingBoxes = parser.ParseOutputs(probabilityMap);

            IList<YoloBoundingBox> detectedObjects = parser.FilterBoundingBoxes(boundingBoxes, 5, 0.5F);

            var filteredBoundingBoxes = parser.FilterBoundingBoxes(detectedObjects, 5, 0.5F);

            return filteredBoundingBoxes;
        }

        public IList<YoloBoundingBox> Score(byte[] imageData)
        {
            // Convert byte[] to MLImage
            using var memoryStream = new MemoryStream(imageData);
            MLImage mlImage = MLImage.CreateFromStream(memoryStream);

            // Create an ImageNetData object with the MLImage
            var imageRecord = new ImageNetData
            {
                Image = mlImage,
            };

            var imageList = new List<ImageNetData> { imageRecord };

            // Load the data into an IDataView
            var data = mlContext.Data.LoadFromEnumerable(imageList);

            return PredictDataUsingModel(data, _transformer);
        }
    }
}
