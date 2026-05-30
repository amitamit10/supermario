using System.Drawing;

namespace supermario
{
    // ── Goomba — הולך בסיסי חום / basic brown walker ─────────────────────
    //  נמעך במגע מלמעלה ונעלם אחרי 600ms. / squished from above, gone after 600ms.
    class Goomba : Enemy
    {
        public bool IsSquished { get; private set; }

        private float squishTimer;
        private const float SQUISH_DURATION = 600f;

        public static readonly Size NormalSize   = new Size(50, 52);
        public static readonly Size SquishedSize = new Size(60, 18);

        public Goomba(Point start) : base(start, NormalSize, Sprites.Goomba, 1.5f, 10) { }

        // קפיצה עליו -> נמעך / stomped -> squish
        public void Squish()
        {
            if (!IsAlive || IsSquished) return;
            IsSquished = true;
            ShowFlattened(Sprites.Squished, SquishedSize, NormalSize);
        }

        // סופר את זמן ההמתנה לפני היעלמות / counts down before despawning
        public bool UpdateSquish(long stepMs) { squishTimer += stepMs; return squishTimer >= SQUISH_DURATION; }
    }
}
