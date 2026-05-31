using System.Drawing;

namespace supermario
{
    // ── FastEnemy — חיפושית אדומה ומהירה / fast red beetle ───────────────
    //  כמו Goomba אך מהירה בהרבה. / like Goomba but much faster.
    class FastEnemy : SquishableEnemy
    {
        public static readonly Size NormalSize   = new Size(46, 48);
        public static readonly Size SquishedSize = new Size(56, 16);

        public FastEnemy(Point start)
            : base(start, NormalSize, Sprites.Fast, 3.2f, 5, SquishedSize, 500f) { }
    }
}
