using System.Drawing;

namespace supermario
{
    /// ════════════════════════════════════════════════════════════════════
    ///  SquishableEnemy — בסיס לאויבים הנמעכים / base for stomp-squishable enemies
    /// --------------------------------------------------------------------
    ///  מרכז את לוגיקת הדריכה שהייתה משוכפלת ב-Goomba/FastEnemy/PlatformPatrolEnemy/
    ///  JumpingEnemy: סימון "מעוך", החלפת התמונה לשטוחה, וספירת הזמן עד היעלמות.
    ///  תת-המחלקה מגדירה רק מהירות/ספרייטים/גדלים (ו-JumpingEnemy מוסיף קפיצה).
    ///
    ///  Centralizes the squish logic that used to be copy-pasted into Goomba,
    ///  FastEnemy, PlatformPatrolEnemy and JumpingEnemy: the "squished" flag, the
    ///  flatten-image swap, and the countdown before despawning. A subclass only
    ///  sets its speed/sprites/sizes (JumpingEnemy also adds its periodic jump).
    /// ════════════════════════════════════════════════════════════════════
    abstract class SquishableEnemy : Enemy
    {
        public bool IsSquished { get; private set; }

        private float squishTimer;
        private readonly float squishDuration;
        private readonly Size  normalSize;
        private readonly Size  squishedSize;

        protected SquishableEnemy(Point start, Size normalSize, Image[] walkFrames,
                                  float speed, int ticksPerFrame,
                                  Size squishedSize, float squishDuration)
            : base(start, normalSize, walkFrames, speed, ticksPerFrame)
        {
            this.normalSize     = normalSize;
            this.squishedSize   = squishedSize;
            this.squishDuration = squishDuration;
        }

        // קפיצה עליו -> נמעך (תמונה שטוחה) / stomped -> squish (flattened image)
        public void Squish()
        {
            if (!IsAlive || IsSquished) return;
            IsSquished = true;
            ShowFlattened(Sprites.Squished, squishedSize, normalSize);
        }

        // סופר את זמן ההמתנה לפני היעלמות / counts down before despawning
        public bool UpdateSquish(long stepMs) { squishTimer += stepMs; return squishTimer >= squishDuration; }
    }
}
