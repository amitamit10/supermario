using System.Drawing;

namespace supermario
{
    // ── Goomba — הולך בסיסי חום / basic brown walker ─────────────────────
    //  נמעך במגע מלמעלה ונעלם אחרי 600ms. / squished from above, gone after 600ms.
    //  כל לוגיקת הדריכה יושבת ב-SquishableEnemy; כאן רק מהירות/ספרייטים/גדלים.
    class Goomba : SquishableEnemy
    {
        public static readonly Size NormalSize   = new Size(50, 52);
        public static readonly Size SquishedSize = new Size(60, 18);

        public Goomba(Point start)
            : base(start, NormalSize, Sprites.Goomba, 1.5f, 10, SquishedSize, 600f) { }
    }
}
