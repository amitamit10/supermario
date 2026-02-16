using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace supermario
{
    class Goomba
    {
        // ── State ──────────────────────────────────────────────────────────────
        public Point Position { get; set; }
        public PictureBox Visual { get; }
        public bool IsAlive { get; private set; }
        public bool IsSquished { get; private set; }
        public bool IsGrounded { get; set; }
        public float VerticalVelocity { get; set; }
        public int Direction { get; set; }   // -1 = left, 1 = right

        private float squishTimer = 0f;
        private const float SQUISH_DURATION = 500f;   // ms to show flat goomba before removing
        private const float WALK_SPEED = 1.5f;

        public static readonly Size NormalSize = new Size(48, 48);
        public static readonly Size SquishedSize = new Size(58, 18);

        // ─────────────────────────────────────────────────────────────────────
        public Goomba(Point startPosition)
        {
            Position = startPosition;
            Direction = -1;   // walk left, classic SMB behaviour
            IsAlive = true;
            IsSquished = false;
            IsGrounded = false;
            VerticalVelocity = 0f;

            Visual = new PictureBox
            {
                Size = NormalSize,
                Location = startPosition,
                BackColor = Color.Transparent
            };
            Visual.Paint += DrawSprite;
        }

        // ── Per-tick horizontal movement ──────────────────────────────────────
        public void Update()
        {
            if (!IsAlive || IsSquished) return;

            int newX = Position.X + (int)(Direction * WALK_SPEED);

            // World boundary bounce
            if (newX < 0 || newX > 2960)
            {
                Direction = -Direction;
                newX = Position.X + (int)(Direction * WALK_SPEED);
            }

            Position = new Point(newX, Position.Y);
        }

        public void ReverseDirection() => Direction = -Direction;

        // ── Stomp ─────────────────────────────────────────────────────────────
        /// <summary>Called by mainWin when Mario lands on top. Returns true immediately.</summary>
        public void Squish()
        {
            if (!IsAlive || IsSquished) return;
            IsSquished = true;
            Visual.Size = SquishedSize;
            // shift down so feet stay on the ground line
            int dy = NormalSize.Height - SquishedSize.Height;
            Position = new Point(Position.X, Position.Y + dy);
            Visual.Invalidate();
        }

        /// <summary>Advance squish timer. Returns true when the goomba should be removed.</summary>
        public bool UpdateSquish(long stepMs)
        {
            squishTimer += stepMs;
            return squishTimer >= SQUISH_DURATION;
        }

        public void Kill() => IsAlive = false;

        // ── Collision rectangle ───────────────────────────────────────────────
        public Rectangle Bounds => new Rectangle(Position.X, Position.Y, Visual.Width, Visual.Height);

        // ── GDI+ sprite ───────────────────────────────────────────────────────
        private void DrawSprite(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = Visual.Width;
            int h = Visual.Height;

            if (IsSquished)
            {
                // Flat body
                g.FillEllipse(new SolidBrush(Color.SaddleBrown), 0, 0, w, h);
                // X eyes – defeated
                using (var pen = new Pen(Color.White, 2f))
                {
                    g.DrawLine(pen, 8, 2, 16, h - 2);
                    g.DrawLine(pen, 16, 2, 8, h - 2);
                    g.DrawLine(pen, w - 16, 2, w - 8, h - 2);
                    g.DrawLine(pen, w - 8, 2, w - 16, h - 2);
                }
            }
            else
            {
                // ── Feet ──
                g.FillRectangle(new SolidBrush(Color.FromArgb(120, 60, 0)), 4, h - 13, 15, 13);
                g.FillRectangle(new SolidBrush(Color.FromArgb(120, 60, 0)), w - 19, h - 13, 15, 13);

                // ── Body / head ──
                g.FillEllipse(new SolidBrush(Color.SaddleBrown), 2, 4, w - 4, h - 10);

                // darker cap / top
                using (var path = new GraphicsPath())
                {
                    path.AddArc(2, 4, w - 4, (h - 10) / 2, 180, 180);
                    path.CloseFigure();
                    g.FillPath(new SolidBrush(Color.FromArgb(100, 45, 0)), path);
                }

                // ── Angry eyebrows ──
                using (var pen = new Pen(Color.FromArgb(60, 20, 0), 3f))
                {
                    g.DrawLine(pen, 6, 20, 19, 24);   // left brow, tilts down toward centre
                    g.DrawLine(pen, w - 19, 24, w - 6, 20); // right brow
                }

                // ── White eye backing ──
                g.FillEllipse(Brushes.White, 6, 22, 13, 13);
                g.FillEllipse(Brushes.White, w - 19, 22, 13, 13);

                // ── Black pupils (shifted toward centre for angry look) ──
                g.FillEllipse(Brushes.Black, 10, 26, 6, 6);
                g.FillEllipse(Brushes.Black, w - 16, 26, 6, 6);

                // ── Bottom teeth ──
                g.FillRectangle(Brushes.White, 14, h - 18, 6, 5);
                g.FillRectangle(Brushes.White, w - 20, h - 18, 6, 5);
            }
        }
    }
}