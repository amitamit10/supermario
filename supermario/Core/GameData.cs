using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace supermario
{
    public enum PowerUpType { Mushroom, Coin }

    public class Mushroom
    {
        public Point Position { get; set; }
        public PictureBox Visual { get; set; }
        public bool IsCollected { get; set; }
        public float VelocityX { get; set; }
        public float VerticalVelocity { get; set; }
        public bool IsGrounded { get; set; }

        public Mushroom(Point pos, PictureBox visual)
        {
            Position = pos;
            Visual = visual;
            IsCollected = false;
            VelocityX = 1.8f;
            VerticalVelocity = 0f;
            IsGrounded = false;
        }
    }

    public class Coin
    {
        public Point Position { get; set; }
        public PictureBox Visual { get; set; }
        public bool IsCollected { get; set; }
        public Coin(Point pos, PictureBox visual) { Position = pos; Visual = visual; IsCollected = false; }
    }

    public class QuestionBlock
    {
        public Point Position { get; set; }
        public PictureBox Visual { get; set; }
        public Label QuestionLabel { get; set; }
        public bool IsHit { get; set; }
        public PowerUpType PowerUpInside { get; set; }
        public QuestionBlock(Point pos, PictureBox visual, Label label, PowerUpType powerUp)
        { Position = pos; Visual = visual; QuestionLabel = label; IsHit = false; PowerUpInside = powerUp; }
    }

    internal static class GraphicsExtensions
    {
        public static void DrawFrame(this Graphics g, Image sheet, int frameIndex, int frameWidth, int frameHeight, Rectangle destRect)
        {
            if (sheet == null || frameWidth <= 0 || frameHeight <= 0 || destRect.Width <= 0 || destRect.Height <= 0)
                return;

            var state = g.Save();
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            var sourceRect = new Rectangle(frameIndex * frameWidth, 0, frameWidth, frameHeight);
            g.DrawImage(sheet, destRect, sourceRect, GraphicsUnit.Pixel);
            g.Restore(state);
        }

        public static void DrawTiledFrame(this Graphics g, Image sheet, int frameIndex, int frameWidth, int frameHeight,
            Rectangle destRect, int tileWidth, int tileHeight)
        {
            if (sheet == null || tileWidth <= 0 || tileHeight <= 0 || destRect.Width <= 0 || destRect.Height <= 0)
                return;

            var state = g.Save();
            g.SetClip(destRect);
            for (int y = destRect.Top; y < destRect.Bottom; y += tileHeight)
            {
                for (int x = destRect.Left; x < destRect.Right; x += tileWidth)
                {
                    DrawFrame(g, sheet, frameIndex, frameWidth, frameHeight, new Rectangle(x, y, tileWidth, tileHeight));
                }
            }
            g.Restore(state);
        }

        public static void FillRoundedRect(this System.Drawing.Graphics g, System.Drawing.Brush b, int x, int y, int w, int h, int r)
        {
            if (w <= 0 || h <= 0) return;
            r = System.Math.Min(r, System.Math.Min(w / 2, h / 2));
            using (var path = new GraphicsPath())
            {
                path.AddArc(x, y, r * 2, r * 2, 180, 90);
                path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
                path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
                path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
                path.CloseFigure();
                g.FillPath(b, path);
            }
        }
    }
}
