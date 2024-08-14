using Microsoft.ML.Data;

namespace RoLabs.MVVM.ML.DataStructures
{
    public class ImageNetPrediction
    {
        [ColumnName("grid")]
        public float[] PredictedLabels;
    }
}
