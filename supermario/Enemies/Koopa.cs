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
            int newX = Position.X + (int)Math.Round(Direction * WALK_SPEED);
            if (newX < 0 || newX > 2960) { Direction = -Direction; newX = Position.X + (int)Math.Round(Direction * WALK_SPEED); }
            Position = new Point(newX, Position.Y);
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

        // ── GDI+ sprite ───────────────────────────────────────────────────
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
}
