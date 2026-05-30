using System;
using System.Drawing;
using System.Windows.Forms;

namespace supermario
{
    /// ════════════════════════════════════════════════════════════════════
    ///  Enemy — מחלקת בסיס לכל האויבים / base class for every enemy
    /// --------------------------------------------------------------------
    ///  כל ששת סוגי האויבים יורשים מכאן. הבסיס מרכז את כל מה שמשותף:
    ///  מיקום, תיבת התמונה (PictureBox), אנימציית הליכה, תנועה אופקית,
    ///  והפיכה ל"מעוך". כל תת-מחלקה מגדירה רק מהירות, תמונות והתנהגות
    ///  מיוחדת (קפיצה / תעופה / קליפה).
    ///
    ///  All six enemy types inherit from this. The base holds everything they
    ///  share: position, the PictureBox, walk animation, horizontal movement
    ///  and the "squish". Each subclass only sets its speed, sprites and any
    ///  special behaviour (jump / fly / shell).
    /// ════════════════════════════════════════════════════════════════════
    abstract class Enemy
    {
        // ── מצב / State ──────────────────────────────────────────────────
        public Point Position { get; set; }
        public PictureBox Visual { get; }
        public bool IsAlive { get; private set; } = true;
        public bool IsGrounded { get; set; }
        public float VerticalVelocity { get; set; }
        public int Direction { get; set; } = -1;          // -1 = שמאל / left, +1 = ימין / right

        // ── אנימציה ותנועה / Animation & movement ───────────────────────
        private readonly Animator _walk;
        protected readonly float Speed;
        private const int WORLD_WIDTH = 3000;             // רוחב הרמה בפיקסלים / level width

        protected Enemy(Point start, Size size, Image[] walkFrames, float speed, int ticksPerFrame)
        {
            Position = start;
            Speed = speed;
            _walk = new Animator(walkFrames, ticksPerFrame);
            Visual = new PictureBox
            {
                Size = size,
                Location = start,
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = (walkFrames != null && walkFrames.Length > 0) ? walkFrames[0] : null,
            };
        }

        // ── פעולות משותפות / Shared actions ─────────────────────────────
        public void ReverseDirection() => Direction = -Direction;
        public void Kill() => IsAlive = false;
        public Rectangle Bounds => new Rectangle(Position.X, Position.Y, Visual.Width, Visual.Height);

        // ברירת המחדל: אנימציה + צעד אופקי. אויב מיוחד (קופץ/מעופף) דורס.
        // Default behaviour: animate + walk. Special enemies override this.
        public virtual void Update()
        {
            if (!IsAlive) return;
            AnimateWalk();
            WalkHorizontally(Speed);
        }

        // ── עזרים לשימוש תת-המחלקות / Helpers for subclasses ─────────────

        // מחליף את תמונת ההליכה כשהגיע הזמן / swap the walk frame when due
        protected void AnimateWalk()
        {
            if (_walk.Tick()) Visual.Image = _walk.Current;
        }

        // צעד אופקי + היפוך כיוון בקצה העולם / horizontal step, flip at world edge
        protected void WalkHorizontally(float speed)
        {
            int newX = Position.X + (int)Math.Round(Direction * speed);
            if (newX < 0 || newX > WORLD_WIDTH - Visual.Width)
            {
                Direction = -Direction;
                newX = Position.X + (int)Math.Round(Direction * speed);
            }
            Position = new Point(newX, Position.Y);
        }

        // מציג תמונת "מעוך"/קליפה ומכווץ את התיבה כלפי מטה
        // Show the squished/shell image and shrink the box downward.
        protected void ShowFlattened(Image flatImage, Size flatSize, Size normalSize)
        {
            Visual.Image = flatImage;
            Visual.Size = flatSize;
            Position = new Point(Position.X, Position.Y + (normalSize.Height - flatSize.Height));
        }
    }
}
