using System.Drawing;
using System.Windows.Forms;

namespace supermario
{
    // ════════════════════════════════════════════════════════════════════
    //  מבני נתונים של אובייקטים נאספים / Data for collectible objects
    //  (הציור עצמו נעשה ע"י PictureBox.Image — אין כאן GDI+.)
    // ════════════════════════════════════════════════════════════════════

    // סוג הבונוס שיוצא מבלוק שאלה / power-up type inside a question block
    public enum PowerUpType { Mushroom, Coin }

    // פטריה (הופכת את השחקן ל"סופר") / mushroom power-up
    public class Mushroom
    {
        public Point Position { get; set; }
        public PictureBox Visual { get; set; }
        public bool IsCollected { get; set; }
        public float VelocityX { get; set; } = 1.8f;
        public float VerticalVelocity { get; set; }
        public bool IsGrounded { get; set; }

        public Mushroom(Point pos, PictureBox visual) { Position = pos; Visual = visual; }
    }

    // מטבע / coin
    public class Coin
    {
        public Point Position { get; set; }
        public PictureBox Visual { get; set; }
        public bool IsCollected { get; set; }
        public Coin(Point pos, PictureBox visual) { Position = pos; Visual = visual; }
    }

    // בלוק שאלה (מכיל מטבע או פטריה) / question block (holds a coin or a mushroom)
    public class QuestionBlock
    {
        public Point Position { get; set; }
        public PictureBox Visual { get; set; }
        public Label QuestionLabel { get; set; }
        public bool IsHit { get; set; }
        public PowerUpType PowerUpInside { get; set; }
        public QuestionBlock(Point pos, PictureBox visual, Label label, PowerUpType powerUp)
        { Position = pos; Visual = visual; QuestionLabel = label; PowerUpInside = powerUp; }
    }
}
