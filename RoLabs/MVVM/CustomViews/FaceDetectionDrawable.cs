public class FaceDetectionDrawable : IDrawable
{
    private readonly Rect[] _faceRectangles;
    private readonly Color _color;
    private readonly int _originalImageWidth;
    private readonly int _originalImageHeight;

    public FaceDetectionDrawable(Rect[] faceRectangles, Color color, int originalImageWidth, int originalImageHeight)
    {
        _faceRectangles = faceRectangles;
        _color = color;
        _originalImageWidth = originalImageWidth;
        _originalImageHeight = originalImageHeight;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // Get the canvas width and height from dirtyRect
        float canvasWidth = dirtyRect.Width;
        float canvasHeight = dirtyRect.Height;

        // Set the color for the rectangles
        canvas.StrokeColor = _color;
        canvas.StrokeSize = 3;

        // Calculate scale factors to transform original coordinates to canvas coordinates
        float scaleX = canvasWidth / _originalImageWidth;
        float scaleY = canvasHeight / _originalImageHeight;

        // Draw each rectangle around the detected face, scaling it to fit the display size
        foreach (var rect in _faceRectangles)
        {
            // Scale the rectangle from image coordinates to canvas coordinates
            var scaledRect = new RectF(
                (float)rect.X * scaleX,
                (float)rect.Y * scaleY,
                (float)rect.Width * scaleX,
                (float)rect.Height * scaleY
            );

            canvas.DrawRectangle(scaledRect);
        }
    }
}
