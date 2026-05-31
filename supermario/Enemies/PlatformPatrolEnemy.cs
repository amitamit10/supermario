using System.Drawing;

namespace supermario
{
    // ── PlatformPatrolEnemy — סייר כתום שמסתובב בקצה פלטפורמה ─────────────
    //  orange patroller; turns around at ledge edges (the edge check lives in
    //  mainWin.EnemyUpdates). מהירות וצורה כמו Goomba.
    class PlatformPatrolEnemy : SquishableEnemy
    {
        public static readonly Size NormalSize   = new Size(48, 50);
        public static readonly Size SquishedSize = new Size(58, 16);

        public PlatformPatrolEnemy(Point start)
            : base(start, NormalSize, Sprites.Patrol, 1.5f, 10, SquishedSize, 500f) { }
    }
}
