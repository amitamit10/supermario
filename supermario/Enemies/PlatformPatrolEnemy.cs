using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace supermario
{

    // ────────────────────────────────────────────────────────────────────────
    //  PLATFORM PATROL ENEMY  –  orange patroller that turns at ledge edges
    // ────────────────────────────────────────────────────────────────────────
    class PlatformPatrolEnemy
    {
        public Point Position { get; set; }
        public PictureBox Visual { get; }
        public bool IsAlive { get; private set; }
        public bool IsSquished { get; private set; }
        public bool IsGrounded { get; set; }
        public float VerticalVelocity { get; set; }
        public int Direction { get; set; }

        private float squishTimer = 0f;
        private int walkFrame = 0;
        private int walkTick = 0;
        private const float SQUISH_DURATION = 500f;
        private const float WALK_SPEED = 1.5f;

        public static readonly Size NormalSize   = new Size(48, 50);
        public static readonly Size SquishedSize = new Size(58, 16);

        public PlatformPatrolEnemy(Point startPosition)
        {
            Position = startPosition;
            Direction = -1;
            IsAlive = true;
            IsGrounded = false;
            VerticalVelocity = 0f;
            Visual = new PictureBox { Size = NormalSize, Location = startPosition, BackColor = Color.Transparent };
            Visual.Paint += DrawSprite;
        }

        public void Update()
        {
            if (!IsAlive || IsSquished) return;
            walkTick++;
            if (walkTick >= 10) { walkTick = 0; walkFrame = (walkFrame + 1) % 2; }
            int newX = Position.X + (int)(Direction * WALK_SPEED);
            if (newX < 0 || newX > 2960) { Direction = -Direction; newX = Position.X + (int)(Direction * WALK_SPEED); }
            Position = new Point(newX, Position.Y);
            Visual.Invalidate();
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

        private void DrawSprite(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = Visual.Width, h = Visual.Height;

            if (IsSquished) { DrawSquished(g, w, h); return; }

            int lOff = walkFrame == 0 ? 3 : -3;
            // Wide flat feet
            using (var b = new SolidBrush(Color.FromArgb(180, 90, 10)))
            {
                DrawRoundRect(g, b, 2, h - 16 + lOff, 20, 14, 4);
                DrawRoundRect(g, b, w - 22, h - 16 - lOff, 20, 14, 4);
            }

            int bTop = 3, bH = h - 19;
            using (var lg = new LinearGradientBrush(new Point(0, bTop), new Point(w, bTop + bH),
                Color.FromArgb(235, 145, 40), Color.FromArgb(185, 95, 15)))
                g.FillEllipse(lg, 2, bTop, w - 4, bH);

            using (var sh = new SolidBrush(Color.FromArgb(60, 255, 220, 160)))
                g.FillEllipse(sh, w / 4, bTop + 3, w / 3, bH / 3);

            // Antenna
            using (var ant = new Pen(Color.FromArgb(160, 75, 5), 2f) { EndCap = LineCap.Round })
                g.DrawLine(ant, w / 2, bTop, w / 2, bTop - 8);
            using (var antTip = new SolidBrush(Color.FromArgb(220, 120, 20)))
                g.FillEllipse(antTip, w / 2 - 4, bTop - 12, 8, 8);

            // Determined narrow eyes
            int eY = bTop + bH / 2 - 2;
            g.FillEllipse(Brushes.White, 6, eY, 12, 10);
            g.FillEllipse(Brushes.White, w - 18, eY, 12, 10);
            using (var pu = new SolidBrush(Color.FromArgb(40, 20, 0)))
            {
                g.FillEllipse(pu, 9, eY + 2, 5, 5);
                g.FillEllipse(pu, w - 15, eY + 2, 5, 5);
            }
            // Flat brow line
            using (var brow = new Pen(Color.FromArgb(140, 65, 5), 2.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(brow, 5, eY - 1, 18, eY - 1);
                g.DrawLine(brow, w - 18, eY - 1, w - 5, eY - 1);
            }
        }

        private void DrawSquished(Graphics g, int w, int h)
        {
            using (var lg = new LinearGradientBrush(new Point(0, 0), new Point(0, h),
                Color.FromArgb(225, 135, 35), Color.FromArgb(170, 85, 10)))
                g.FillEllipse(lg, 2, 0, w - 4, h);
            g.FillEllipse(Brushes.White, 5, 1, 6, 6);
            g.FillEllipse(Brushes.White, w - 11, 1, 6, 6);
            using (var hi = new SolidBrush(Color.FromArgb(50, 255, 210, 130)))
                g.FillEllipse(hi, w / 4, 1, w / 3, h / 2);
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
