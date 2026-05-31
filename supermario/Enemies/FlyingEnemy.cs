using System;
using System.Drawing;

namespace supermario
{
    // ── FlyingEnemy — קואפה מעופפת / winged Parakoopa ────────────────────
    //  עף בתנועת גל סינוס. קפיצה ראשונה מורידה כנפיים (אז הוא נופל והולך),
    //  קפיצה שנייה מועכת אותו.
    //  Flies in a sine wave. First stomp removes the wings (then it falls and
    //  walks), the second stomp squishes it.
    class FlyingEnemy : Enemy
    {
        public bool HasWings { get; private set; } = true;
        public bool IsSquished { get; private set; }

        private readonly int baseY;
        private float flyTimer;
        private float squishTimer;
        private const float SQUISH_DURATION = 600f;
        private const float FLY_SPEED = 1.8f;
        private const float FLY_AMPLITUDE = 22f;
        private const float FLY_FREQUENCY = 0.055f;
        private const float WALK_SPEED = 1.2f;

        public static readonly Size NormalSize   = new Size(52, 48);
        public static readonly Size SquishedSize = new Size(62, 16);

        public FlyingEnemy(Point start) : base(start, NormalSize, Sprites.Flyer, FLY_SPEED, 8)
        {
            baseY = start.Y;
        }

        // עם כנפיים: גל סינוס. בלי כנפיים: הליכה רגילה (הכבידה בידי mainWin).
        // With wings: sine-wave flight. Without: plain walking (gravity handled by mainWin).
        public override void Update()
        {
            if (!IsAlive || IsSquished) return;
            AnimateWalk();

            if (HasWings)
            {
                flyTimer += FLY_FREQUENCY;
                int maxX = WORLD_WIDTH - Visual.Width;
                int newX = Position.X + (int)Math.Round(Direction * FLY_SPEED);
                if (newX < 0 || newX > maxX)
                {
                    Direction = -Direction;
                    newX = Position.X + (int)Math.Round(Direction * FLY_SPEED);
                }
                if (newX < 0) newX = 0; else if (newX > maxX) newX = maxX;   // הצמדה לגבול / clamp
                int newY = baseY + (int)(Math.Sin(flyTimer) * FLY_AMPLITUDE);
                Position = new Point(newX, newY);
            }
            else
            {
                WalkHorizontally(WALK_SPEED);
            }
        }

        // קפיצה ראשונה: כנפיים יורדות. שנייה: נמעך. שלישית: מת.
        public void Stomp()
        {
            if (!IsAlive) return;
            if (HasWings)
            {
                HasWings = false;
                VerticalVelocity = 0f;
                IsGrounded = false;
            }
            else if (!IsSquished)
            {
                IsSquished = true;
                ShowFlattened(Sprites.Squished, SquishedSize, NormalSize);
            }
            else
            {
                Kill();
            }
        }

        public bool UpdateSquish(long stepMs) { squishTimer += stepMs; return squishTimer >= SQUISH_DURATION; }
    }
}
