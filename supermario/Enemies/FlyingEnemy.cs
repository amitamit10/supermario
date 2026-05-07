using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace supermario
{

    // ────────────────────────────────────────────────────────────────────────
    //  FLYING ENEMY  –  winged Parakoopa; first stomp removes wings
    // ────────────────────────────────────────────────────────────────────────
    class FlyingEnemy
    {
        public Point Position { get; set; }
        public PictureBox Visual { get; }
        public bool IsAlive { get; private set; }
        public bool HasWings { get; private set; }
        public bool IsSquished { get; private set; }
        public bool IsGrounded { get; set; }
        public float VerticalVelocity { get; set; }
        public int Direction { get; set; }

        private readonly int baseY;
        private float flyTimer = 0f;
        private float squishTimer = 0f;
        private int walkFrame = 0;
        private int walkTick = 0;
        private const float SQUISH_DURATION = 600f;
        private const float FLY_SPEED = 1.8f;
        private const float FLY_AMPLITUDE = 22f;
        private const float FLY_FREQUENCY = 0.055f;

        public static readonly Size NormalSize   = new Size(52, 48);
        public static readonly Size SquishedSize = new Size(62, 16);

        public FlyingEnemy(Point startPosition)
        {
            Position = startPosition;
            baseY = startPosition.Y;
            Direction = -1;
            IsAlive = true;
            HasWings = true;
            IsGrounded = false;
            VerticalVelocity = 0f;
            Visual = new PictureBox { Size = NormalSize, Location = startPosition, BackColor = Color.Transparent };
            Visual.Paint += DrawSprite;
        }

        public void Update()
        {
            if (!IsAlive || IsSquished) return;

            walkTick++;
            if (walkTick >= 8) { walkTick = 0; walkFrame = (walkFrame + 1) % 2; }

            if (HasWings)
            {
                flyTimer += FLY_FREQUENCY;
                int newX = Position.X + (int)(Direction * FLY_SPEED);
                if (newX < 0 || newX > 2960) { Direction = -Direction; newX = Position.X + (int)(Direction * FLY_SPEED); }
                int newY = baseY + (int)(Math.Sin(flyTimer) * FLY_AMPLITUDE);
                Position = new Point(newX, newY);
            }
            else
            {
                int newX = Position.X + (int)(Direction * 1.2f);
                if (newX < 0 || newX > 2960) { Direction = -Direction; newX = Position.X + (int)(Direction * 1.2f); }
                Position = new Point(newX, Position.Y);
            }
            Visual.Invalidate();
        }

        public void ReverseDirection() => Direction = -Direction;

        public void Stomp()
        {
            if (!IsAlive) return;
            if (HasWings)
            {
                HasWings = false;
                VerticalVelocity = 0f;
                IsGrounded = false;
                Visual.Invalidate();
            }
            else if (!IsSquished)
            {
                IsSquished = true;
                int dy = NormalSize.Height - SquishedSize.Height;
                Position = new Point(Position.X, Position.Y + dy);
                Visual.Size = SquishedSize;
                Visual.Invalidate();
            }
            else { Kill(); }
        }

        public bool UpdateSquish(long stepMs) { squishTimer += stepMs; return squishTimer >= SQUISH_DURATION; }
        public void Kill() => IsAlive = false;
        public Rectangle Bounds => new Rectangle(Position.X, Position.Y, Visual.Width, Visual.Height);

        private void DrawSprite(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = Visual.Width, h = Visual.Height;

            if (IsSquished) { DrawSquished(g, w, h); return; }

            if (HasWings)
            {
                // Wing flap offset
                int wOff = walkFrame == 0 ? -4 : 4;
                // Left wing
                using (var wg = new SolidBrush(Color.FromArgb(220, 240, 255, 240)))
                    g.FillEllipse(wg, -10, h / 3 + wOff, 22, 16);
                using (var wb = new Pen(Color.FromArgb(60, 160, 60), 1f))
                    g.DrawEllipse(wb, -10, h / 3 + wOff, 22, 16);
                // Right wing
                using (var wg = new SolidBrush(Color.FromArgb(220, 240, 255, 240)))
                    g.FillEllipse(wg, w - 12, h / 3 + wOff, 22, 16);
                using (var wb = new Pen(Color.FromArgb(60, 160, 60), 1f))
                    g.DrawEllipse(wb, w - 12, h / 3 + wOff, 22, 16);
            }

            // Legs
            int legOff = walkFrame == 0 ? 2 : -2;
            using (var b = new SolidBrush(Color.FromArgb(70, 130, 40)))
            {
                DrawRoundRect(g, b, 4, h - 14 + legOff, 14, 12, 3);
                DrawRoundRect(g, b, w - 18, h - 14 - legOff, 14, 12, 3);
            }

            // Shell body
            int sTop = 2, sH = h - 16;
            using (var lg2 = new LinearGradientBrush(new Point(0, sTop), new Point(w, sTop + sH),
                Color.FromArgb(70, 185, 55), Color.FromArgb(30, 110, 25)))
                g.FillEllipse(lg2, 2, sTop, w - 4, sH);

            using (var sh = new SolidBrush(Color.FromArgb(70, 255, 255, 180)))
                g.FillEllipse(sh, w / 4, sTop + 2, w / 3, sH / 4);

            using (var pen = new Pen(Color.FromArgb(50, 0, 90, 0), 1.5f))
            {
                g.DrawEllipse(pen, 7, sTop + 4, w - 14, sH - 8);
                g.DrawLine(pen, w / 2, sTop + 2, w / 2, sTop + sH - 2);
            }

            // Face
            int fX = w / 4, fY = sTop + sH / 2, fW = w / 2, fH = 16;
            using (var face = new SolidBrush(Color.FromArgb(240, 215, 165)))
                g.FillEllipse(face, fX, fY, fW, fH);

            g.FillEllipse(Brushes.White, fX + 2, fY + 1, 7, 7);
            g.FillEllipse(Brushes.White, fX + fW - 9, fY + 1, 7, 7);
            using (var pu = new SolidBrush(Color.FromArgb(25, 15, 0)))
            {
                g.FillEllipse(pu, fX + 3, fY + 2, 4, 4);
                g.FillEllipse(pu, fX + fW - 8, fY + 2, 4, 4);
            }
        }

        private void DrawSquished(Graphics g, int w, int h)
        {
            using (var lg2 = new LinearGradientBrush(new Point(0, 0), new Point(0, h),
                Color.FromArgb(65, 185, 55), Color.FromArgb(25, 100, 20)))
                g.FillEllipse(lg2, 2, 0, w - 4, h);
            using (var sh = new SolidBrush(Color.FromArgb(65, 255, 255, 190)))
                g.FillEllipse(sh, w / 3, 2, w / 3, h / 2);
        }

        private static void DrawRoundRect(Graphics g, Brush b, int x, int y, int w, int h, int r)
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
