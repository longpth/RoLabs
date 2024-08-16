using IImage = Microsoft.Maui.Graphics.IImage;

namespace Rolabs.MVVM.CustomViews
{
    public class ImageDrawable : IDrawable
    {
        private readonly IImage _image;
        private readonly RectF _drawRect;
        private readonly float _rotationAngle;

        public ImageDrawable(IImage image, RectF drawRect, float rotationAngle = 0)
        {
            _image = image;
            _drawRect = drawRect;
            _rotationAngle = rotationAngle;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (_image != null)
            {
                // Save the current state of the canvas
                canvas.SaveState();

                // Move the canvas origin to the center of the drawRect
                canvas.Translate(_drawRect.Center.X, _drawRect.Center.Y);

                // Rotate the canvas
                canvas.Rotate(_rotationAngle);

                // Draw the image, offset by half its size to center it on the new origin
                canvas.DrawImage(_image, -_drawRect.Width / 2, -_drawRect.Height / 2, _drawRect.Width, _drawRect.Height);

                // Restore the canvas state to avoid affecting other drawing operations
                canvas.RestoreState();
            }
        }
    }

}
