using System;
using System.Drawing;
using System.Windows.Forms;

namespace supermario
{
    partial class mainWin
    {
        // ════════════════════════════════════════════════════════════════════
        //  ציור השחקן / Player sprite
        // ════════════════════════════════════════════════════════════════════
        // מציב את תמונת מריו המתאימה למצב (עמידה / הליכה / קפיצה) ולכיוון הפנייה.
        // הציור עצמו נעשה אוטומטית ע"י ה-PictureBox — אין כאן GDI+.
        // Picks Mario's image for the current state (idle / walk / jump) and facing
        // direction. The PictureBox draws it automatically — no GDI+ here.
        private void UpdatePlayerSprite()
        {
            // הבהוב בזמן חוסן זמני / blink while invincible
            if (isInvincible && ((int)(invincibleTimer / 100f) % 2 == 0))
            {
                picboxplayer.Visible = false;
                return;
            }
            picboxplayer.Visible = true;

            Image img;
            if (!player.IsGrounded || player.VerticalVelocity != 0)
                img = facingRight ? Sprites.MarioJump : Sprites.MarioJumpLeft;          // באוויר / airborne
            else if (isWalking)
            {
                int f = (globalTick / 6) % 2;                                            // שני פריימי הליכה
                img = facingRight ? Sprites.MarioWalk?[f] : Sprites.MarioWalkLeft?[f];
            }
            else
                img = facingRight ? Sprites.MarioIdle : Sprites.MarioIdleLeft;           // עמידה / idle

            if (img != null && picboxplayer.Image != img) picboxplayer.Image = img;
        }

        // ════════════════════════════════════════════════════════════════════
        //  HUD — נוצר פעם אחת, הטקסט מתעדכן כל פריים
        //  HUD — controls created once, text updated each tick
        // ════════════════════════════════════════════════════════════════════
        private void InitHud()
        {
            _hudLabel   = CreateHudLabel(y: 8,  height: 38, fore: Color.White);
            _scoreLabel = CreateHudLabel(y: 48, height: 30, fore: Color.FromArgb(255, 230, 80));

            // לבבות — PictureBox עם תמונת לב (מלא/ריק). אין ציור GDI+ / hearts as images
            for (int i = 0; i < 3; i++)
            {
                _hearts[i] = new PictureBox
                {
                    Name = "heart",
                    Size = new Size(30, 28),
                    Location = new Point(180 + i * 34, 8),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BackColor = Color.Transparent,
                };
                Controls.Add(_hearts[i]);
                _hearts[i].BringToFront();
            }

            UpdateHud();
        }

        // יוצר תווית HUD אחידה (רוחב/רקע/גופן/יישור זהים), מוסיף לטופס ומקדים לחזית.
        // Creates a uniformly-styled HUD label, adds it to the form and brings it forward.
        private Label CreateHudLabel(int y, int height, Color fore)
        {
            var lbl = new Label
            {
                AutoSize  = false,
                Size      = new Size(320, height),
                Location  = new Point(8, y),
                BackColor = Color.FromArgb(160, 20, 20, 40),
                ForeColor = fore,
                Font      = new Font("Courier New", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
            };
            Controls.Add(lbl);
            lbl.BringToFront();
            return lbl;
        }

        private void UpdateHud()
        {
            if (_hudLabel == null || _scoreLabel == null) return;

            if (currentLevelNumber != _lastHudLevel || isPlayerSuper != _lastHudSuper)
            {
                _lastHudLevel = currentLevelNumber;
                _lastHudSuper = isPlayerSuper;
                _hudLabel.Text = $"  LVL {currentLevelNumber}     {(isPlayerSuper ? "★ SUPER" : "")}";
                Text = $"Super Mario – Level {currentLevelNumber}{(isPlayerSuper ? "  ★ SUPER" : "")}";
            }

            if (player.Score != _lastHudScore || coinCount != _lastHudCoins)
            {
                _lastHudScore = player.Score;
                _lastHudCoins = coinCount;
                _scoreLabel.Text = $"  SCORE {player.Score:D6}   COINS {coinCount:D3}";
            }

            if (player.Health != _lastHudHealth)
            {
                _lastHudHealth = player.Health;
                for (int i = 0; i < 3; i++)
                {
                    if (_hearts[i] == null) continue;
                    bool filled = i < player.Health;
                    _hearts[i].Image = filled ? Sprites.HeartFull : Sprites.HeartEmpty;
                }
            }
        }
    }
}
