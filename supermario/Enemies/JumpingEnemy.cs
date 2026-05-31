using System.Drawing;

namespace supermario
{
    // ── JumpingEnemy — קופץ כחול / blue bouncer ──────────────────────────
    //  הולך כרגיל, אך כשהוא על הקרקע הוא קופץ כל ~90 פריימים.
    //  Walks normally, but leaps every ~90 frames while grounded.
    //  לוגיקת הדריכה יורשת מ-SquishableEnemy; כאן רק הקפיצה התקופתית.
    class JumpingEnemy : SquishableEnemy
    {
        private int jumpTick;
        private const float JUMP_VELOCITY = -9f;
        private const int   JUMP_INTERVAL = 90;
        private const float WALK_SPEED    = 1.8f;

        public static readonly Size NormalSize   = new Size(48, 50);
        public static readonly Size SquishedSize = new Size(58, 16);

        public JumpingEnemy(Point start)
            : base(start, NormalSize, Sprites.Jumper, WALK_SPEED, 8, SquishedSize, 500f) { }

        // דורס את Update כדי להוסיף קפיצה תקופתית / overrides Update to add the periodic jump
        public override void Update()
        {
            if (!IsAlive || IsSquished) return;
            AnimateWalk();

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
            else
            {
                jumpTick = 0;   // איפוס קצב הקפיצה באוויר / reset cadence while airborne
            }

            WalkHorizontally(WALK_SPEED);
        }
    }
}
