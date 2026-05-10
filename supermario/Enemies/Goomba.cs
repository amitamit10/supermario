using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace supermario
{
    class Goomba
    {
        // ── State ──────────────────────────────────────────────────────────────────────
        public Point Position { get; set; }
        public PictureBox Visual { get; }
        public bool IsAlive { get; private set; }
        public bool IsSquished { get; private set; }
        public bool IsGrounded { get; set; }
        public float VerticalVelocity { get; set; }
        public int Direction { get; set; }   // -1 = left, 1 = right

        private float squishTimer = 0f;
        private const float SQUISH_DURATION = 600f;
        private const float WALK_SPEED = 1.5f;

        // Walk animation
        private int walkFrame = 0;
        private int walkTick = 0;

        public static readonly Size NormalSize = new Size(50, 52);
        public static readonly Size SquishedSize = new Size(60, 18);

        // ─────────────────────────────────────────────────────────────────────────
        public Goomba(Point startPosition)
        {
            Position = startPosition;
            Direction = -1;
            IsAlive = true;
            IsSquished = false;
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

            // Walk animation
            walkTick++;
            if (walkTick >= 10)
            {
                walkTick = 0;
                walkFrame = (walkFrame + 1) % 2;
                Visual.Invalidate();
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
            Visual.Size = SquishedSize;
            int dy = NormalSize.Height - SquishedSize.Height;
            Position = new Point(Position.X, Position.Y + dy);
            Visual.Invalidate();
        }

        public bool UpdateSquish(long stepMs) { squishTimer += stepMs; return squishTimer >= SQUISH_DURATION; }
        public void Kill() => IsAlive = false;
        public Rectangle Bounds => new Rectangle(Position.X, Position.Y, Visual.Width, Visual.Height);

        // ── GDI+ Sprite ───────────────────────────────────────────────────────────────────
        private void DrawSprite(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;

            int w = Visual.Width;
            int h = Visual.Height;

            if (!IsSquished && TextureLoader.TryGetSheet("enemies", out var enemiesSheet))
            {
                g.DrawFrame(enemiesSheet, walkFrame % 2, 64, 64, new Rectangle(0, 0, w, h));
                return;
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (IsSquished)
            {
                DrawSquished(g, w, h);
                return;
            }

            // ── Foot/leg wobble ───────────────────────────────────────────────────
            int leftLegOff = walkFrame == 0 ? 3 : -3;
            int rightLegOff = -leftLegOff;

            // ── Feet ──────────────────────────────────────────────────────────────
            Color footDark = Color.FromArgb(90, 40, 5);
            Color footMid = Color.FromArgb(130, 65, 15);

            using (var footBrush = new SolidBrush(footDark))
            {
                DrawRoundedRect(g, footBrush, 4, h - 14 + leftLegOff, 18, 14, 4);
                DrawRoundedRect(g, footBrush, w - 22, h - 14 + rightLegOff, 18, 14, 4);
            }
            using (var shoeBrush = new SolidBrush(Color.FromArgb(60, footMid)))
            {
                g.FillEllipse(shoeBrush, 6, h - 12 + leftLegOff, 10, 5);
                g.FillEllipse(shoeBrush, w - 20, h - 12 + rightLegOff, 10, 5);
            }

            // ── Body ──────────────────────────────────────────────────────────────
            int bodyTop = 6;
            int bodyH = h - 18;
            using (var bodyBrush = new LinearGradientBrush(
                new Point(0, bodyTop), new Point(w, bodyTop + bodyH),
                Color.FromArgb(175, 95, 35),
                Color.FromArgb(120, 55, 10)))
                g.FillEllipse(bodyBrush, 2, bodyTop, w - 4, bodyH);

            using (var bodySheen = new SolidBrush(Color.FromArgb(50, 255, 200, 140)))
                g.FillEllipse(bodySheen, w / 4, bodyTop + 4, w / 3, bodyH / 3);

            // ── Dark mushroom-cap top ─────────────────────────────────────────────────
            using (var capPath = new GraphicsPath())
            using (var capBrush = new SolidBrush(Color.FromArgb(85, 35, 5)))
            {
                capPath.AddArc(2, bodyTop, w - 4, bodyH, 180, 180);
                capPath.CloseFigure();
                g.FillPath(capBrush, capPath);
            }
            using (var capSheen = new SolidBrush(Color.FromArgb(35, 255, 160, 100)))
                g.FillEllipse(capSheen, w / 3, bodyTop + 4, w / 5, bodyH / 5);

            // ── Angry eyebrows ────────────────────────────────────────────────────
            using (var brow = new Pen(Color.FromArgb(50, 15, 0), 3.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                int midY = bodyTop + bodyH / 2 - 5;
                g.DrawLine(brow, 5, midY - 3, 19, midY + 3);
                g.DrawLine(brow, w - 19, midY + 3, w - 5, midY - 3);
            }

            // ── Eyes ──────────────────────────────────────────────────────────────
            int eyeY = bodyTop + bodyH / 2 - 2;
            g.FillEllipse(Brushes.White, 5, eyeY, 14, 14);
            g.FillEllipse(Brushes.White, w - 19, eyeY, 14, 14);
            using (var pupil = new SolidBrush(Color.FromArgb(20, 10, 0)))
            {
                g.FillEllipse(pupil, 10, eyeY + 3, 7, 7);
                g.FillEllipse(pupil, w - 17, eyeY + 3, 7, 7);
            }
            using (var eyeSheen = new SolidBrush(Color.FromArgb(180, 255, 255, 255)))
            {
                g.FillEllipse(eyeSheen, 11, eyeY + 4, 3, 3);
                g.FillEllipse(eyeSheen, w - 16, eyeY + 4, 3, 3);
            }

            // ── Fangs ──────────────────────────────────────────────────────────────
            int fangY = eyeY + 14;
            g.FillRectangle(Brushes.White, 11, fangY, 7, 5);
            g.FillRectangle(Brushes.White, w - 18, fangY, 7, 5);
            using (var fangShadow = new SolidBrush(Color.FromArgb(80, 200, 160, 140)))
            {
                g.FillRectangle(fangShadow, 11, fangY + 3, 7, 2);
                g.FillRectangle(fangShadow, w - 18, fangY + 3, 7, 2);
            }
        }

        private void DrawSquished(Graphics g, int w, int h)
        {
            using (var b = new LinearGradientBrush(new Point(0, 0), new Point(0, h),
                Color.FromArgb(160, 80, 20), Color.FromArgb(100, 45, 5)))
                g.FillEllipse(b, 2, 0, w - 4, h);

            g.FillEllipse(Brushes.White, 6, 1, 6, 6);
            g.FillEllipse(Brushes.White, w - 12, 1, 6, 6);

            using (var pen = new Pen(Color.FromArgb(230, 230, 240), 2.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(pen, 8, 2, 15, h - 2);
                g.DrawLine(pen, 15, 2, 8, h - 2);
                g.DrawLine(pen, w - 15, 2, w - 8, h - 2);
                g.DrawLine(pen, w - 8, 2, w - 15, h - 2);
            }

            using (var hi = new SolidBrush(Color.FromArgb(40, 255, 200, 150)))
                g.FillEllipse(hi, w / 4, 1, w / 3, h / 3);
        }

        private static void DrawRoundedRect(Graphics g, Brush b, int x, int y, int w, int h, int r)
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
