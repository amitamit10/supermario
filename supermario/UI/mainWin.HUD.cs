using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace supermario
{
    partial class mainWin
    {
        // ════════════════════════════════════════════════════════════════════
        //  PLAYER SPRITE
        // ════════════════════════════════════════════════════════════════════
        private void DrawPlayerSprite(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;

            int w = picboxplayer.Width;
            int h = picboxplayer.Height;

            if (isInvincible && ((int)(invincibleTimer / 100f) % 2 == 0))
                return;

            var img = Properties.Resources.dcaeqy1_614416a8_3ae1_4448_94b4_e3ecefa3e53a;

            if (!facingRight)
            {
                g.TranslateTransform(w, 0);
                g.ScaleTransform(-1, 1);
            }

            g.DrawImage(img, 0, 0, w, h);
        }


        // ════════════════════════════════════════════════════════════════════
        //  HUD  –  controls created once, text updated each tick
        // ════════════════════════════════════════════════════════════════════
        private void InitHud()
        {
            _hudLabel = new Label
            {
                Name = "hudLabel",
                AutoSize = false,
                Size = new Size(320, 38),
                Location = new Point(8, 8),
                BackColor = Color.FromArgb(160, 20, 20, 40),
                ForeColor = Color.White,
                Font = new Font("Courier New", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
            };
            Controls.Add(_hudLabel);
            _hudLabel.BringToFront();

            _scoreLabel = new Label
            {
                Name = "scoreLabel",
                AutoSize = false,
                Size = new Size(320, 30),
                Location = new Point(8, 48),
                BackColor = Color.FromArgb(160, 20, 20, 40),
                ForeColor = Color.FromArgb(255, 230, 80),
                Font = new Font("Courier New", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
            };
            Controls.Add(_scoreLabel);
            _scoreLabel.BringToFront();

            for (int i = 0; i < 3; i++)
            {
                _heartLabels[i] = new Label
                {
                    Name = "heartLabel",
                    Font = new Font("Arial", 20, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(180 + i * 36, 6),
                    BackColor = Color.Transparent,
                };
                Controls.Add(_heartLabels[i]);
                _heartLabels[i].BringToFront();
            }

            UpdateHud();
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
                    if (_heartLabels[i] == null) continue;
                    bool filled = i < player.Health;
                    _heartLabels[i].Text = filled ? "❤" : "♡";
                    _heartLabels[i].ForeColor = filled
                        ? Color.FromArgb(255, 60, 80)
                        : Color.FromArgb(140, 100, 110);
                }
            }
        }

    }
}
