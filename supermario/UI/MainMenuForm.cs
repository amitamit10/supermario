using System;
using System.Drawing;
using System.IO;
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
            Sprites.LoadAll();

            Text = "Super Mario";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(420, 630);
            BackColor = Color.FromArgb(92, 148, 252);

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

            var mario = new PictureBox
            {
                Image = Sprites.MarioIdle,
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(120, 120),
                Location = new Point((ClientSize.Width - 120) / 2, 110),
                BackColor = Color.Transparent,
            };
            Controls.Add(mario);

            int y = 250;
            AddButton("START GAME",  Color.FromArgb(48, 176, 48),   y,       (s, e) => LaunchGame());
            AddButton("HOW TO PLAY", Color.FromArgb(210, 165, 10),  y + 70,  (s, e) => ShowHowToPlay());
            AddButton("TRAIN AI",    Color.FromArgb(30, 120, 180),  y + 140, (s, e) => LaunchTrainer());
            AddButton("PLAY WITH AI",Color.FromArgb(120, 40, 180),  y + 210, (s, e) => LaunchWithAI());
            AddButton("EXIT",        Color.FromArgb(185, 38, 28),   y + 280, (s, e) => Application.Exit());
        }

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

        private void LaunchGame()
        {
            try
            {
                var game = new mainWin();
                game.FormClosed += (s, e) => { Show(); BringToFront(); };
                game.Show();
                Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start game:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LaunchWithAI()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "best_ai.json");
            if (!File.Exists(path))
            {
                MessageBox.Show(
                    "No saved AI found.\nTrain the AI first — the best result is saved automatically to best_ai.json.",
                    "No AI Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                var (net, _, _) = ML.NetworkSerializer.LoadJson(path);
                var game = new mainWin(net);
                game.FormClosed += (s, e) => { Show(); BringToFront(); };
                game.Show();
                Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load AI:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LaunchTrainer()
        {
            try
            {
                var trainer = new TrainingForm();
                trainer.FormClosed += (s, e) => { Show(); BringToFront(); };
                trainer.Show();
                Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open trainer:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
