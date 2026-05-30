using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace supermario
{
    partial class mainWin
    {
        // ════════════════════════════════════════════════════════════════════
        //  COINS
        // ════════════════════════════════════════════════════════════════════
        private void AddCoins()
        {
            var coinPositions = new List<Point>();

            // Rows of coins above each platform in the level
            foreach (var p in currentLevel)
            {
                // Skip staircase steps (very narrow blocks at specific Y values)
                if (p.Width == 40 && p.Height >= 40) continue;
                int rowY = p.Y - 50;
                int count = Math.Max(1, p.Width / 40);
                for (int j = 0; j < count; j++)
                    coinPositions.Add(new Point(p.X + 10 + j * 38, rowY));
            }

            // Per-level floating coins guiding the player through key sections
            int[] floatX, floatY;
            switch (currentLevelNumber)
            {
                case 1: floatX = LEVEL_1_FLOAT_COIN_X; floatY = LEVEL_1_FLOAT_COIN_Y; break;
                case 2: floatX = LEVEL_2_FLOAT_COIN_X; floatY = LEVEL_2_FLOAT_COIN_Y; break;
                case 3: floatX = LEVEL_3_FLOAT_COIN_X; floatY = LEVEL_3_FLOAT_COIN_Y; break;
                default:
                    floatX = new[] { 300, 500, 800, 1100, 1400, 1650, 1950, 2250, 2450, 2600 };
                    floatY = new[] { 390, 360, 370,  350,  380,  360,  370,  350,  390,  380 };
                    break;
            }
            for (int i = 0; i < floatX.Length; i++)
                coinPositions.Add(new Point(floatX[i], floatY[i]));

            foreach (var pos in coinPositions)
            {
                var pb = new PictureBox
                {
                    Size = new Size(24, 24),
                    Location = new Point(pos.X - cameraX, pos.Y),  // screen position
                    BackColor = Color.Transparent,
                };
                pb.SizeMode = PictureBoxSizeMode.StretchImage;
                pb.Image = (Sprites.Coin != null && Sprites.Coin.Length > 0) ? Sprites.Coin[0] : null;
                Controls.Add(pb);
                pb.SendToBack();
                animatedBlocks.Add(pb);
                coins.Add(new Coin(pos, pb));
            }
        }

        // המטבע מצויר כתמונה (PictureBox.Image); האנימציה מתבצעת ב-UpdateAnimatedSprites.
        // The coin is drawn as an image; its spin animation happens in UpdateAnimatedSprites.

        private void UpdateCoins()
        {
            var playerRect = new Rectangle(player.Position.X, player.Position.Y,
                picboxplayer.Width, picboxplayer.Height);

            for (int i = coins.Count - 1; i >= 0; i--)
            {
                var coin = coins[i];
                if (coin.IsCollected) continue;

                var coinRect = new Rectangle(coin.Position.X, coin.Position.Y, 24, 24);
                if (!playerRect.IntersectsWith(coinRect)) continue;

                // Collected
                coin.IsCollected = true;
                animatedBlocks.Remove(coin.Visual);
                Controls.Remove(coin.Visual);
                coin.Visual.Dispose();
                coinCount++;
                player.Score += 10;
                coins.RemoveAt(i);
            }
        }

        private void ClearCoins()
        {
            foreach (var c in coins)
            {
                if (c.Visual != null)
                {
                    animatedBlocks.Remove(c.Visual);
                    Controls.Remove(c.Visual);
                    c.Visual.Dispose();
                }
            }
            coins.Clear();
        }

        // ════════════════════════════════════════════════════════════════════
        //  MUSHROOM COLLECTIBLE
        // ════════════════════════════════════════════════════════════════════
        private void SpawnMushroom(Point blockWorldPos)
        {
            // Mushroom appears just above the question block and moves right
            var spawnPos = new Point(blockWorldPos.X + 8, blockWorldPos.Y - 36);
            var pb = new PictureBox
            {
                Size = new Size(34, 34),
                Location = new Point(spawnPos.X - cameraX, spawnPos.Y),
                BackColor = Color.Transparent,
            };
            pb.SizeMode = PictureBoxSizeMode.StretchImage;
            pb.Image = Sprites.Mushroom;
            Controls.Add(pb);
            pb.BringToFront();

            var mush = new Mushroom(spawnPos, pb);
            mush.VelocityX = 2f;
            spawnedMushrooms.Add(mush);
        }

        private void UpdateMushrooms()
        {
            var playerRect = new Rectangle(player.Position.X, player.Position.Y,
                picboxplayer.Width, picboxplayer.Height);

            for (int i = spawnedMushrooms.Count - 1; i >= 0; i--)
            {
                var m = spawnedMushrooms[i];
                if (m.IsCollected) { spawnedMushrooms.RemoveAt(i); continue; }

                if (m.Position.Y > 580)
                {
                    Controls.Remove(m.Visual);
                    m.Visual.Dispose();
                    spawnedMushrooms.RemoveAt(i);
                    continue;
                }

                // Gravity
                if (!m.IsGrounded)
                {
                    m.VerticalVelocity += 0.55f;
                    if (m.VerticalVelocity > 15f) m.VerticalVelocity = 15f;
                }
                else
                {
                    m.VerticalVelocity = 0;
                }

                // Move
                int newX = m.Position.X + (int)m.VelocityX;
                int mushroomMaxX = LEVEL_PIXEL_WIDTH - m.Visual.Width;
                if (newX < 0 || newX > mushroomMaxX) { m.VelocityX = -m.VelocityX; newX = Math.Max(0, Math.Min(mushroomMaxX, newX)); }
                int newY = m.Position.Y + (int)Math.Round(m.VerticalVelocity);
                m.Position = new Point(newX, newY);

                // Platform collisions
                bool onGround = false;
                var mRect = new Rectangle(m.Position.X, m.Position.Y, m.Visual.Width, m.Visual.Height);
                foreach (var plat in platforms)
                {
                    var pr = new Rectangle(plat.Position.X, plat.Position.Y,
                        plat.PictureBox.Width, plat.PictureBox.Height);
                    if (!mRect.IntersectsWith(pr)) continue;

                    int ot = mRect.Bottom - pr.Top;
                    int ob = pr.Bottom - mRect.Top;
                    int ol = mRect.Right - pr.Left;
                    int orr = pr.Right - mRect.Left;
                    int minO = Math.Min(Math.Min(ot, ob), Math.Min(ol, orr));

                    if (minO == ot && ot < 22)
                    {
                        m.Position = new Point(m.Position.X, pr.Top - m.Visual.Height);
                        m.VerticalVelocity = 0;
                        onGround = true;
                        break; // resolved – stop checking other platforms this frame
                    }
                    else if (minO == ol || minO == orr)
                    {
                        m.VelocityX = -m.VelocityX;
                        break; // direction reversed – stop to avoid double-flip on corners
                    }
                }
                m.IsGrounded = onGround;

                // Sync screen position
                m.Visual.Location = new Point(m.Position.X - cameraX, m.Position.Y);

                // Player collection
                var mushRect = new Rectangle(m.Position.X, m.Position.Y, m.Visual.Width, m.Visual.Height);
                if (playerRect.IntersectsWith(mushRect) && !isDying)
                {
                    m.IsCollected = true;
                    Controls.Remove(m.Visual);
                    m.Visual.Dispose();
                    spawnedMushrooms.RemoveAt(i);
                    if (!isPlayerSuper) BecomeSuper();
                    else player.Health = Math.Min(player.Health + 1, 3);
                }
            }
        }

        // הפטריה מצוירת כתמונה (PictureBox.Image) — אין כאן ציור GDI+.
        // The mushroom is drawn as an image — no GDI+ here.

    }
}
