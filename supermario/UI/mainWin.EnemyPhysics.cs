using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace supermario
{
    /// ════════════════════════════════════════════════════════════════════
    ///  mainWin — עוזרי עדכון אויבים / enemy-update helpers
    /// --------------------------------------------------------------------
    ///  עוזרים קצרים המשותפים לכל ששת לולאות ה-UpdateXxx (ב-mainWin.EnemyUpdates.cs).
    ///  בניגוד לעוזרי הפיזיקה ב-Enemy עצמו, אלה תלויים בטופס (Controls), במצלמה
    ///  (cameraX) ובשחקן — ולכן הם יושבים כאן, ב-mainWin, ולא במחלקת האויב.
    ///
    ///  Short helpers shared by all six UpdateXxx loops. Unlike the pure-physics
    ///  helpers on Enemy, these need the form (Controls), the camera (cameraX)
    ///  and the player — so they live here on mainWin, not on the enemy.
    /// ════════════════════════════════════════════════════════════════════
    partial class mainWin
    {
        // מסיר אויב מת/שנפל: מוריד את התמונה מהטופס, משחרר אותה, ומוציא מהרשימה.
        // Remove a dead/fallen enemy: drop its image from the form, dispose it, remove from the list.
        private void RemoveEnemy<T>(List<T> list, int i) where T : Enemy
        {
            Controls.Remove(list[i].Visual);
            list[i].Visual.Dispose();
            list.RemoveAt(i);
        }

        // ממקם את תמונת האויב על המסך לפי המצלמה (מיקום-עולם פחות הזזת המצלמה).
        // Place the enemy's image on screen from its world position minus the camera scroll.
        private void SyncToScreen(Enemy e)
            => e.Visual.Location = new Point(e.Position.X - cameraX, e.Position.Y);

        // האם השחקן דורך על האויב מלמעלה (נופל, מעל מרכז האויב, ומהירות אנכית כלפי מטה)?
        // Is the player stomping the enemy from above (falling, above its mid-line, moving down)?
        private bool PlayerStomps(Enemy e)
        {
            int playerBottom = player.Position.Y + picboxplayer.Height;
            bool falling = playerBottom - e.Position.Y < 24;
            bool above   = player.Position.Y < e.Position.Y + e.Visual.Height / 2;
            return falling && above && player.VerticalVelocity >= 0;
        }

        // מלבני העולם של הפלטפורמות ובלוקי-השאלה (נבנים פעם אחת לפריים, משותפים לכל האויבים).
        // World rectangles for platforms / question-blocks (built once per frame, shared by all enemies).
        private List<Rectangle> PlatformRects()
        {
            var rects = new List<Rectangle>(platforms.Count);
            foreach (var p in platforms)
                rects.Add(new Rectangle(p.Position.X, p.Position.Y, p.PictureBox.Width, p.PictureBox.Height));
            return rects;
        }

        private List<Rectangle> BlockRects()
        {
            var rects = new List<Rectangle>(questionBlocks.Count);
            foreach (var qb in questionBlocks)
                rects.Add(new Rectangle(qb.Position.X, qb.Position.Y, qb.Visual.Width, qb.Visual.Height));
            return rects;
        }
    }
}
