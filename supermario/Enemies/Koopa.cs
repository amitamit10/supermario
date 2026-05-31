using System.Drawing;

namespace supermario
{
    // ── Koopa — צב ירוק / green turtle ───────────────────────────────────
    //  קפיצה ראשונה -> קליפה (נעלמת אחרי זמן); קפיצה שנייה על הקליפה -> מוות.
    //  First stomp -> shell (despawns after a while); second stomp -> dead.
    class Koopa : Enemy
    {
        public bool IsShell { get; private set; }

        private float shellTimer;
        private const float SHELL_DURATION = 1200f;

        public static readonly Size NormalSize = new Size(50, 56);
        public static readonly Size ShellSize  = new Size(50, 34);

        public Koopa(Point start) : base(start, NormalSize, Sprites.Koopa, 1.2f, 10) { }

        // נדרך עליו: הופך לקליפה, ובפעם השנייה מת / first becomes a shell, then dies
        public void Stomp()
        {
            if (!IsAlive) return;
            if (!IsShell)
            {
                IsShell = true;
                ShowFlattened(Sprites.KoopaShell, ShellSize, NormalSize);
            }
            else
            {
                Kill();
            }
        }

        public bool UpdateShell(long stepMs) { shellTimer += stepMs; return shellTimer >= SHELL_DURATION; }
    }
}
