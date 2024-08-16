using Microsoft.Maui.Graphics;

namespace Rolabs.MVVM.CustomViews
{
    public class FaceDetectionDrawable : IDrawable
    {
        private readonly Rect[] _faceRectangles;
        private readonly Color _color;

        public FaceDetectionDrawable(Rect[] faceRectangles, Color color)
        {
            _faceRectangles = faceRectangles;
            _color = color;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // Set the color for the rectangles
            canvas.StrokeColor = _color;
            canvas.StrokeSize = 3;

            // Draw each rectangle around the detected face
            foreach (var rect in _faceRectangles)
            {
                canvas.DrawRectangle(rect);
            }
        }
    }
}
