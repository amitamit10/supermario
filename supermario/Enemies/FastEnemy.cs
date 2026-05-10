using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace supermario
{

    // ────────────────────────────────────────────────────────────────────────
    //  FAST ENEMY  –  red fast-patrol goomba variant
    // ────────────────────────────────────────────────────────────────────────
    class FastEnemy
    {
        public Point Position { get; set; }
        public PictureBox Visual { get; }
        public bool IsAlive { get; private set; }
        public bool IsSquished { get; private set; }
        public bool IsGrounded { get; set; }
        public float VerticalVelocity { get; set; }
        public int Direction { get; set; }

        private float squishTimer = 0f;
        private const float SQUISH_DURATION = 500f;
        private const float WALK_SPEED = 3.2f;
        private int walkFrame = 0;
        private int walkTick = 0;

        public static readonly Size NormalSize   = new Size(46, 48);
        public static readonly Size SquishedSize = new Size(56, 16);

        public FastEnemy(Point startPosition)
        {
            Position = startPosition;
            Direction = -1;
            IsAlive = true;
            IsGrounded = false;
            VerticalVelocity = 0f;

            Visual = new PictureBox
            {
                Size = NormalSize,
                Location = startPosition,
                BackColor = Color.Transparent,
            };
            Visual.Paint += DrawSprite;
        }

        public void Update()
        {
            if (!IsAlive || IsSquished) return;
            walkTick++;
            if (walkTick >= 5) { walkTick = 0; walkFrame = (walkFrame + 1) % 2; }
            int newX = Position.X + (int)Math.Round(Direction * WALK_SPEED);
            if (newX < 0 || newX > 2960) { Direction = -Direction; newX = Position.X + (int)Math.Round(Direction * WALK_SPEED); }
            Position = new Point(newX, Position.Y);
        }

        public void ReverseDirection() => Direction = -Direction;

        public void Squish()
        {
            if (!IsAlive || IsSquished) return;
            IsSquished = true;
            int dy = NormalSize.Height - SquishedSize.Height;
            Position = new Point(Position.X, Position.Y + dy);
            Visual.Size = SquishedSize;
            Visual.Invalidate();
        }

        public bool UpdateSquish(long stepMs) { squishTimer += stepMs; return squishTimer >= SQUISH_DURATION; }
        public void Kill() => IsAlive = false;
        public Rectangle Bounds => new Rectangle(Position.X, Position.Y, Visual.Width, Visual.Height);

        // ── GDI+ sprite ───────────────────────────────────────────────────
        private void DrawSprite(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = Visual.Width, h = Visual.Height;

            if (IsSquished) { DrawSquished(g, w, h); return; }

            int lOff = walkFrame == 0 ? 3 : -3;

            // Feet
            using (var b = new SolidBrush(Color.FromArgb(160, 40, 10)))
            {
                DrawRoundRect(g, b, 3,     h - 13 + lOff,  16, 12, 4);
                DrawRoundRect(g, b, w - 19, h - 13 - lOff, 16, 12, 4);
            }

            // Body (red gradient)
            int bTop = 5, bH = h - 16;
            using (var lg = new LinearGradientBrush(
                new Point(0, bTop), new Point(w, bTop + bH),
                Color.FromArgb(225, 65, 30), Color.FromArgb(160, 30, 10)))
                g.FillEllipse(lg, 2, bTop, w - 4, bH);

            using (var sh = new SolidBrush(Color.FromArgb(50, 255, 185, 155)))
                g.FillEllipse(sh, w / 4, bTop + 3, w / 3, bH / 3);

            // Dark cap
            using (var cp = new GraphicsPath())
            using (var cb = new SolidBrush(Color.FromArgb(125, 25, 5)))
            {
                cp.AddArc(2, bTop, w - 4, bH, 180, 180);
                cp.CloseFigure();
                g.FillPath(cb, cp);
            }

            // Angry brows
            using (var brow = new Pen(Color.FromArgb(60, 12, 0), 3f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                int midY = bTop + bH / 2 - 5;
                g.DrawLine(brow, 5,     midY - 2, 17,    midY + 3);
                g.DrawLine(brow, w - 17, midY + 3, w - 5, midY - 2);
            }

            // Eyes
            int eY = bTop + bH / 2 - 2;
            g.FillEllipse(Brushes.White, 5,     eY, 12, 12);
            g.FillEllipse(Brushes.White, w - 17, eY, 12, 12);
            using (var pu = new SolidBrush(Color.FromArgb(20, 8, 0)))
            {
                g.FillEllipse(pu, 9,     eY + 3, 5, 5);
                g.FillEllipse(pu, w - 14, eY + 3, 5, 5);
            }
            using (var es = new SolidBrush(Color.FromArgb(180, 255, 255, 255)))
            {
                g.FillEllipse(es, 10,    eY + 4, 3, 3);
                g.FillEllipse(es, w - 13, eY + 4, 3, 3);
            }
        }

        private void DrawSquished(Graphics g, int w, int h)
        {
            using (var lg = new LinearGradientBrush(
                new Point(0, 0), new Point(0, h),
                Color.FromArgb(210, 55, 25), Color.FromArgb(140, 30, 10)))
                g.FillEllipse(lg, 2, 0, w - 4, h);

            g.FillEllipse(Brushes.White, 5, 1, 6, 6);
            g.FillEllipse(Brushes.White, w - 11, 1, 6, 6);

            using (var pen = new Pen(Color.FromArgb(220, 220, 235), 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(pen, 7,     2, 13,    h - 2);
                g.DrawLine(pen, 13,    2, 7,     h - 2);
                g.DrawLine(pen, w - 13, 2, w - 7, h - 2);
                g.DrawLine(pen, w - 7,  2, w - 13, h - 2);
            }
            using (var hi = new SolidBrush(Color.FromArgb(40, 255, 190, 160)))
                g.FillEllipse(hi, w / 4, 1, w / 3, h / 3);
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
