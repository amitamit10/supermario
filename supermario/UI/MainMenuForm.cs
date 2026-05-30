using System;
using System.Drawing;
using System.Windows.Forms;

namespace supermario
{
    /// ════════════════════════════════════════════════════════════════════
    ///  MainMenuForm — מסך הפתיחה / the start screen
    /// --------------------------------------------------------------------
    ///  מסך בסיסי: כותרת, תמונת מריו, וארבעה כפתורים. אין אנימציה ואין ציור
    ///  GDI+ — הכול פקדים רגילים של WinForms.
    ///  A basic screen: a title, a Mario picture and four buttons. No
    ///  animation and no GDI+ drawing — just plain WinForms controls.
    /// ════════════════════════════════════════════════════════════════════
    public sealed class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            Sprites.LoadAll();                               // טעינת התמונות / load images

            Text = "Super Mario";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(420, 560);
            BackColor = Color.FromArgb(92, 148, 252);        // צבע שמיים / sky blue

            // ── כותרת / title ────────────────────────────────────────────
            var title = new Label
            {
                Text = "SUPER MARIO",
                Font = new Font("Arial", 26f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(ClientSize.Width, 70),
                Location = new Point(0, 30),
                BackColor = Color.Transparent,
            };
            Controls.Add(title);

            // ── תמונת מריו (מהסקריפט) / Mario picture (from the script) ──
            var mario = new PictureBox
            {
                Image = Sprites.MarioIdle,
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(120, 120),
                Location = new Point((ClientSize.Width - 120) / 2, 110),
                BackColor = Color.Transparent,
            };
            Controls.Add(mario);

            // ── כפתורים / buttons ────────────────────────────────────────
            int y = 250;
            AddButton("START GAME",  Color.FromArgb(48, 176, 48),  y,       (s, e) => LaunchGame());
            AddButton("HOW TO PLAY", Color.FromArgb(210, 165, 10), y + 70,  (s, e) => ShowHowToPlay());
            AddButton("TRAIN AI",    Color.FromArgb(30, 120, 180), y + 140, (s, e) => LaunchTrainer());
            AddButton("EXIT",        Color.FromArgb(185, 38, 28),  y + 210, (s, e) => Application.Exit());
        }

        // יוצר כפתור פשוט וממרכז אותו אופקית / create a simple, centred button
        private void AddButton(string text, Color back, int y, EventHandler onClick)
        {
            var b = new Button
            {
                Text = text,
                Font = new Font("Arial", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = back,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(260, 50),
                Location = new Point((ClientSize.Width - 260) / 2, y),
            };
            b.Click += onClick;
            Controls.Add(b);
        }

        // ── "איך משחקים" — תיבת הודעה פשוטה / simple how-to-play dialog ──
        private void ShowHowToPlay()
        {
            MessageBox.Show(
                "תזוזה: A/D או חיצים\n" +
                "קפיצה: W / חץ למעלה / רווח\n" +
                "השמדת אויב: קפיצה מעליו\n" +
                "מטרה: להגיע לדגל בסוף השלב\n\n" +
                "Move: A / D or Arrow keys\n" +
                "Jump: W / Up / Space\n" +
                "Defeat enemies: jump on top of them\n" +
                "Goal: reach the flag at the end",
                "How To Play", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── פתיחת המשחק / launch the game ───────────────────────────────
        private void LaunchGame()
        {
            var game = new mainWin();
            game.FormClosed += (s, e) => { Show(); BringToFront(); };
            game.Show();
            Hide();
        }

        // ── פתיחת מסך אימון ה-AI / launch the AI trainer ────────────────
        private void LaunchTrainer()
        {
            var trainer = new TrainingForm();
            trainer.FormClosed += (s, e) => { Show(); BringToFront(); };
            trainer.Show();
            Hide();
        }
    }
}
