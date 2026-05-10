using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace supermario
{

    // ────────────────────────────────────────────────────────────────────────
    //  JUMPING ENEMY  –  blue bouncer that periodically leaps
    // ────────────────────────────────────────────────────────────────────────
    class JumpingEnemy
    {
        public Point Position { get; set; }
        public PictureBox Visual { get; }
        public bool IsAlive { get; private set; }
        public bool IsSquished { get; private set; }
        public bool IsGrounded { get; set; }
        public float VerticalVelocity { get; set; }
        public int Direction { get; set; }

        private float squishTimer = 0f;
        private int jumpTick = 0;
        private int walkFrame = 0;
        private int walkTick = 0;
        private const float SQUISH_DURATION = 500f;
        private const float WALK_SPEED = 1.8f;
        private const float JUMP_VELOCITY = -9f;
        private const int JUMP_INTERVAL = 90;

        public static readonly Size NormalSize   = new Size(48, 50);
        public static readonly Size SquishedSize = new Size(58, 16);

        public JumpingEnemy(Point startPosition)
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
            if (walkTick >= 8) { walkTick = 0; walkFrame = (walkFrame + 1) % 2; }

            if (IsGrounded)
            {
                jumpTick++;
                if (jumpTick >= JUMP_INTERVAL)
                {
                    jumpTick = 0;
                    VerticalVelocity = JUMP_VELOCITY;
                    IsGrounded = false;
                }
            }

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

        private void DrawSprite(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = Visual.Width, h = Visual.Height;

            if (IsSquished) { DrawSquished(g, w, h); return; }

            int lOff = walkFrame == 0 ? 3 : -3;
            // Spring coil feet
            using (var fc = new Pen(Color.FromArgb(60, 80, 180), 2.5f))
            {
                for (int s = 0; s < 3; s++)
                {
                    int sy = h - 14 + s * 4 + (walkFrame == 0 ? lOff : -lOff);
                    g.DrawLine(fc, 5, sy, 14, sy + 2);
                    g.DrawLine(fc, 14, sy + 2, 5, sy + 4);
                }
                for (int s = 0; s < 3; s++)
                {
                    int sy = h - 14 + s * 4 + (walkFrame == 0 ? -lOff : lOff);
                    g.DrawLine(fc, w - 14, sy, w - 5, sy + 2);
                    g.DrawLine(fc, w - 5, sy + 2, w - 14, sy + 4);
                }
            }

            int bTop = 4, bH = h - 18;
            using (var lg = new LinearGradientBrush(new Point(0, bTop), new Point(w, bTop + bH),
                Color.FromArgb(90, 100, 220), Color.FromArgb(50, 60, 160)))
                g.FillEllipse(lg, 2, bTop, w - 4, bH);

            using (var sh = new SolidBrush(Color.FromArgb(60, 180, 200, 255)))
                g.FillEllipse(sh, w / 4, bTop + 3, w / 3, bH / 3);

            // Forehead bump (indicates jumper)
            using (var bump = new SolidBrush(Color.FromArgb(80, 110, 200)))
                g.FillEllipse(bump, w / 2 - 8, bTop - 4, 16, 12);

            // Wide surprised eyes
            int eY = bTop + bH / 2 - 4;
            g.FillEllipse(Brushes.White, 4, eY, 15, 15);
            g.FillEllipse(Brushes.White, w - 19, eY, 15, 15);
            using (var pu = new SolidBrush(Color.FromArgb(15, 10, 80)))
            {
                g.FillEllipse(pu, 7, eY + 3, 7, 7);
                g.FillEllipse(pu, w - 16, eY + 3, 7, 7);
            }
            using (var es = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
            {
                g.FillEllipse(es, 8, eY + 4, 3, 3);
                g.FillEllipse(es, w - 15, eY + 4, 3, 3);
            }
        }

        private void DrawSquished(Graphics g, int w, int h)
        {
            using (var lg = new LinearGradientBrush(new Point(0, 0), new Point(0, h),
                Color.FromArgb(80, 90, 200), Color.FromArgb(50, 55, 140)))
                g.FillEllipse(lg, 2, 0, w - 4, h);
            g.FillEllipse(Brushes.White, 5, 1, 6, 6);
            g.FillEllipse(Brushes.White, w - 11, 1, 6, 6);
            using (var hi = new SolidBrush(Color.FromArgb(50, 200, 220, 255)))
                g.FillEllipse(hi, w / 4, 1, w / 3, h / 2);
        }
    }
}
