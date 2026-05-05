using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace supermario
{
    // ────────────────────────────────────────────────────────────────────────
    //  KOOPA  –  green turtle enemy; first stomp → shell, second → removed
    // ────────────────────────────────────────────────────────────────────────
    class Koopa
    {
        public Point Position { get; set; }
        public PictureBox Visual { get; }
        public bool IsAlive { get; private set; }
        public bool IsShell { get; private set; }
        public bool IsGrounded { get; set; }
        public float VerticalVelocity { get; set; }
        public int Direction { get; set; }

        private float shellTimer = 0f;
        private const float SHELL_DURATION = 1200f;
        private const float WALK_SPEED = 1.2f;
        private int walkFrame = 0;
        private int walkTick = 0;

        public static readonly Size NormalSize = new Size(50, 56);
        public static readonly Size ShellSize  = new Size(50, 34);

        public Koopa(Point startPosition)
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
            if (!IsAlive || IsShell) return;
            walkTick++;
            if (walkTick >= 10) { walkTick = 0; walkFrame = (walkFrame + 1) % 2; }
            int newX = Position.X + (int)(Direction * WALK_SPEED);
            if (newX < 0 || newX > 2960) { Direction = -Direction; newX = Position.X + (int)(Direction * WALK_SPEED); }
            Position = new Point(newX, Position.Y);
            Visual.Invalidate();
        }

        public void ReverseDirection() => Direction = -Direction;

        public void Stomp()
        {
            if (!IsAlive) return;
            if (!IsShell)
            {
                IsShell = true;
                int dy = NormalSize.Height - ShellSize.Height;
                Position = new Point(Position.X, Position.Y + dy);
                Visual.Size = ShellSize;
                Visual.Invalidate();
            }
            else
            {
                Kill();
            }
        }

        public bool UpdateShell(long stepMs) { shellTimer += stepMs; return shellTimer >= SHELL_DURATION; }
        public void Kill() => IsAlive = false;
        public Rectangle Bounds => new Rectangle(Position.X, Position.Y, Visual.Width, Visual.Height);

        // ── GDI+ sprite ───────────────────────────────────────────────────────
        private void DrawSprite(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = Visual.Width, h = Visual.Height;

            if (IsShell) { DrawShell(g, w, h); return; }

            // Legs
            int lOff = walkFrame == 0 ? 2 : -2;
            using (var b = new SolidBrush(Color.FromArgb(70, 130, 40)))
            {
                DrawRoundRect(g, b, 3,     h - 15 + lOff,  16, 13, 4);
                DrawRoundRect(g, b, w - 19, h - 15 + (-lOff), 16, 13, 4);
            }

            // Shell body
            int sTop = 3, sH = h - 18;
            using (var lg = new LinearGradientBrush(
                new Point(0, sTop), new Point(w, sTop + sH),
                Color.FromArgb(70, 185, 55), Color.FromArgb(30, 110, 25)))
                g.FillEllipse(lg, 2, sTop, w - 4, sH);

            // Shell sheen
            using (var sh = new SolidBrush(Color.FromArgb(70, 255, 255, 180)))
                g.FillEllipse(sh, w / 4, sTop + 3, w / 3, sH / 4);

            // Shell hex lines
            using (var pen = new Pen(Color.FromArgb(50, 0, 90, 0), 1.5f))
            {
                g.DrawEllipse(pen, 8, sTop + 5, w - 16, sH - 10);
                g.DrawLine(pen, w / 2, sTop + 3, w / 2, sTop + sH - 3);
            }

            // Face
            int fX = w / 4, fY = sTop + sH / 2, fW = w / 2, fH = 18;
            using (var face = new SolidBrush(Color.FromArgb(240, 215, 165)))
                g.FillEllipse(face, fX, fY, fW, fH);

            // Eyes
            g.FillEllipse(Brushes.White, fX + 2,          fY + 1, 8, 8);
            g.FillEllipse(Brushes.White, fX + fW - 10,    fY + 1, 8, 8);
            using (var p2 = new SolidBrush(Color.FromArgb(25, 15, 0)))
            {
                g.FillEllipse(p2, fX + 4,       fY + 3, 4, 4);
                g.FillEllipse(p2, fX + fW - 8,  fY + 3, 4, 4);
            }
            using (var sh2 = new SolidBrush(Color.FromArgb(180, 255, 255, 255)))
            {
                g.FillEllipse(sh2, fX + 5,      fY + 3, 2, 2);
                g.FillEllipse(sh2, fX + fW - 7, fY + 3, 2, 2);
            }
        }

        private void DrawShell(Graphics g, int w, int h)
        {
            using (var lg = new LinearGradientBrush(
                new Point(0, 0), new Point(0, h),
                Color.FromArgb(65, 185, 55), Color.FromArgb(25, 100, 20)))
                g.FillEllipse(lg, 2, 0, w - 4, h);

            using (var sh = new SolidBrush(Color.FromArgb(65, 255, 255, 190)))
                g.FillEllipse(sh, w / 3, 2, w / 3, h / 3);

            using (var pen = new Pen(Color.FromArgb(50, 0, 90, 0), 1.5f))
            {
                g.DrawLine(pen, w / 2, 2, w / 2, h - 2);
                g.DrawLine(pen, 4, h / 2, w - 4, h / 2);
            }
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

        // ── GDI+ sprite ───────────────────────────────────────────────────────
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
            g.FillEllipse(new SolidBrush(Color.FromArgb(220, 120, 20)), w / 2 - 4, bTop - 12, 8, 8);

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
        private const float FLY_AMPLITUDE = 28f;
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
