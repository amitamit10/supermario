using System;
using System.Collections.Generic;
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
        protected const int WORLD_WIDTH = 3000;           // רוחב הרמה בפיקסלים / level width

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
            int maxX = WORLD_WIDTH - Visual.Width;
            int newX = Position.X + (int)Math.Round(Direction * speed);
            if (newX < 0 || newX > maxX)
            {
                Direction = -Direction;
                newX = Position.X + (int)Math.Round(Direction * speed);
            }
            // הצמדה לגבולות כדי שלא יחרוג מהעולם / clamp inside the world bounds
            if (newX < 0) newX = 0; else if (newX > maxX) newX = maxX;
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

        // ════════════════════════════════════════════════════════════════
        //  פיזיקה משותפת / Shared physics
        // ----------------------------------------------------------------
        //  עוזרים שמשמשים את לולאות העדכון של כל סוגי האויבים ב-mainWin.
        //  הם מקבלים מלבני-עולם (Rectangle) כך שהבסיס לא צריך להכיר את
        //  טיפוסי המשחק (GameObjectS/QuestionBlock). זה מסיר את הכפילות
        //  הענקית שהייתה בין שש מתודות ה-UpdateXxx.
        //
        //  Helpers used by every enemy's update loop in mainWin. They take
        //  world rectangles so the base needn't know the game types — this
        //  removes the huge duplication that used to live across the six
        //  UpdateXxx methods.
        // ════════════════════════════════════════════════════════════════
        protected const float GRAVITY        = 0.6f;   // כבידה לפריים / gravity per frame
        protected const float MAX_FALL_SPEED = 15f;    // מהירות נפילה מרבית / terminal fall speed

        // כבידה: צבירת מהירות אנכית (עד תקרה) ואינטגרציה למיקום. על הקרקע — לא עושה כלום.
        // Gravity: accumulate capped vertical velocity and integrate. No-op when grounded.
        public void ApplyGravity()
        {
            if (IsGrounded) return;
            VerticalVelocity += GRAVITY;
            if (VerticalVelocity > MAX_FALL_SPEED) VerticalVelocity = MAX_FALL_SPEED;
            Position = new Point(Position.X, Position.Y + (int)Math.Round(VerticalVelocity));
        }

        // חפיפה בין שני מלבנים: עומק החדירה בכל צד + הצד הרדוד ביותר (min).
        // Overlap of two rectangles: penetration depth on each side + the shallowest (min).
        protected static (int top, int bottom, int left, int right, int min) Overlap(Rectangle a, Rectangle b)
        {
            int top    = a.Bottom - b.Top;
            int bottom = b.Bottom - a.Top;
            int left   = a.Right  - b.Left;
            int right  = b.Right  - a.Left;
            return (top, bottom, left, right, Math.Min(Math.Min(top, bottom), Math.Min(left, right)));
        }

        // התנגשות עם פלטפורמות: נחיתה על הקצה העליון, תקרה (allowCeiling — ל-Jumper בלבד),
        // והיפוך כיוון בפגיעת קיר. מחזיר האם נחת; wallHit מדווח על פגיעת צד.
        // Platform collision: land on top, optional ceiling (Jumper only), reverse on wall.
        // Returns whether it landed; wallHit reports a side hit.
        public bool ResolvePlatformCollisions(IEnumerable<Rectangle> platformRects, bool allowCeiling, out bool wallHit)
        {
            bool grounded = false;
            wallHit = false;
            var rect = Bounds;
            foreach (var pr in platformRects)
            {
                if (!rect.IntersectsWith(pr)) continue;
                var o = Overlap(rect, pr);

                if (o.min == o.top && o.top < 30)
                { Position = new Point(Position.X, pr.Top - Visual.Height); VerticalVelocity = 0; grounded = true; break; }

                if (allowCeiling && o.min == o.bottom && o.bottom < 20 && VerticalVelocity < 0)
                { Position = new Point(Position.X, pr.Bottom); VerticalVelocity = 0; break; }

                // ירד מעבר לקצה העליון בפריים אחד — מצמידים מעלה במקום ליפול דרכו.
                // Descended past the top in one frame — snap up instead of falling through.
                if (o.min == o.bottom && VerticalVelocity >= 0)
                { Position = new Point(Position.X, pr.Top - Visual.Height); VerticalVelocity = 0; grounded = true; break; }

                if (o.min == o.left || o.min == o.right)
                {
                    // דוחפים החוצה מהקיר והופכים כיוון (רק בפעם הראשונה כדי לא לרצד).
                    // Push out of the wall and reverse (only once, to avoid jitter).
                    if (!wallHit)
                    {
                        Position = (o.min == o.left)
                            ? new Point(pr.Left - Visual.Width, Position.Y)
                            : new Point(pr.Right, Position.Y);
                        ReverseDirection();
                        wallHit = true;
                        rect = Bounds;
                    }
                }
            }
            return grounded;
        }

        // התנגשות עם בלוקי-שאלה (כקירות): היפוך כיוון בפגיעת צד; תקרה אופציונלית (Jumper).
        // Question-block (wall) collision: reverse on a side hit; optional ceiling (Jumper).
        public void ResolveBlockCollisions(IEnumerable<Rectangle> blockRects, bool allowCeiling)
        {
            var b = Bounds;
            foreach (var br in blockRects)
            {
                if (!b.IntersectsWith(br)) continue;
                var o = Overlap(b, br);
                if (allowCeiling && o.min == o.bottom && o.bottom < 20 && VerticalVelocity < 0)
                { Position = new Point(Position.X, br.Bottom); VerticalVelocity = 0; break; }
                if ((o.min == o.left || o.min == o.right) && o.min < o.top)
                { ReverseDirection(); break; }
            }
        }

        // בדיקת קרקע מתחת לרגליים (פס 2px) — מאשרת עמידה כשהאויב יושב בדיוק על קצה
        // פלטפורמה (ש-IntersectsWith לא סופר כחפיפה). משמש את JumpingEnemy.
        // 2px probe just below the feet — confirms footing when the enemy rests exactly
        // on a platform top (which IntersectsWith treats as non-overlapping). Used by JumpingEnemy.
        public bool HasGroundBeneath(IEnumerable<Rectangle> platformRects)
        {
            var feet = new Rectangle(Position.X, Position.Y + Visual.Height, Visual.Width, 2);
            foreach (var pr in platformRects)
                if (feet.IntersectsWith(pr)) return true;
            return false;
        }

        // בודק אם אין קרקע ממש לפני הרגליים (לכיוון התנועה) — לפטרול שמסתובב בקצה מדף.
        // Checks for missing ground just ahead of the feet (in the move direction) —
        // used by the patrol enemy to turn around at a platform edge.
        public bool IsAtLedgeEdge(IEnumerable<Rectangle> platformRects)
        {
            const int probeW = 10;
            int probeX = Direction > 0 ? Position.X + Visual.Width : Position.X - probeW;
            int probeY = Position.Y + Visual.Height + 4;
            var probe  = new Rectangle(probeX, probeY, probeW, 6);
            foreach (var pr in platformRects)
                if (probe.IntersectsWith(pr)) return false;   // יש קרקע לפנים / ground ahead
            return true;                                       // קצה מדף / at an edge
        }
    }
}
