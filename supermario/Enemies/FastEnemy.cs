using System.Drawing;

namespace supermario
{
    // ── FastEnemy — גומבה אדומה ומהירה / fast red walker ─────────────────
    //  כמו Goomba אך כפול מהירות. / like Goomba but twice as fast.
    class FastEnemy : Enemy
    {
        public bool IsSquished { get; private set; }

        private float squishTimer;
        private const float SQUISH_DURATION = 500f;

        public static readonly Size NormalSize   = new Size(46, 48);
        public static readonly Size SquishedSize = new Size(56, 16);

        public FastEnemy(Point start) : base(start, NormalSize, Sprites.Fast, 3.2f, 5) { }

        public void Squish()
        {
            if (!IsAlive || IsSquished) return;
            IsSquished = true;
            ShowFlattened(Sprites.Squished, SquishedSize, NormalSize);
        }

        public bool UpdateSquish(long stepMs) { squishTimer += stepMs; return squishTimer >= SQUISH_DURATION; }
    }
}
