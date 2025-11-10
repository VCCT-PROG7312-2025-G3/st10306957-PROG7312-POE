using System.Drawing;
using System.Drawing.Drawing2D;

namespace PROG7312_POE.Extensions
{
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush,
            int x, int y, int width, int height, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(x, y, radius, radius, 180, 90);
            path.AddArc(x + width - radius, y, radius, radius, 270, 90);
            path.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90);
            path.AddArc(x, y + height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            g.FillPath(brush, path);
        }
    }
}