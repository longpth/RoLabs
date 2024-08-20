using Microsoft.Maui.Graphics;
using System.Collections.Generic;

namespace Rolabs.MVVM.CustomViews
{
    public class PointsDrawable : IDrawable
    {
        private readonly List<PointF> _points;

        public PointsDrawable(List<PointF> points)
        {
            _points = points ?? new List<PointF>();
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (_points == null)
                return;

            // Set the paint properties
            canvas.FillColor = Colors.Green;

            // Draw each point
            foreach (var point in _points)
            {
                canvas.FillCircle(point.X, point.Y, 5); // Draw a circle at each point
            }
        }
    }
}
