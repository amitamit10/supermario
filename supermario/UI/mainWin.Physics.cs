using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace supermario
{
    partial class mainWin
    {
        // ════════════════════════════════════════════════════════════════════
        //  PHYSICS / COLLISION
        // ════════════════════════════════════════════════════════════════════
        private void CheckPlatformCollisions()
        {
            var playerRect = new Rectangle(player.Position.X, player.Position.Y,
                picboxplayer.Width, picboxplayer.Height);
            bool foundGround = false;

            foreach (var plat in platforms)
            {
                var platRect = new Rectangle(plat.Position.X, plat.Position.Y,
                    plat.PictureBox.Width, plat.PictureBox.Height);
                if (!playerRect.IntersectsWith(platRect)) continue;

                int overlapTop    = playerRect.Bottom - platRect.Top;
                int overlapBottom = platRect.Bottom - playerRect.Top;
                int overlapLeft   = playerRect.Right - platRect.Left;
                int overlapRight  = platRect.Right - playerRect.Left;
                int minOverlap    = Math.Min(Math.Min(overlapTop, overlapBottom), Math.Min(overlapLeft, overlapRight));

                if (minOverlap == overlapTop && overlapTop < 20)
                {
                    player.Position = new Point(player.Position.X, platRect.Top - picboxplayer.Height);
                    player.IsGrounded = true;
                    foundGround = true;
                    // Continue checking pipes – don't break early
                }
                else if (plat.Type == "pipe")
                {
                    // Pipes block horizontal movement when the player approaches from the side
                    bool feetBelowRim = player.Position.Y + picboxplayer.Height > platRect.Top + 10;
                    if (feetBelowRim && overlapLeft > 0 && overlapLeft <= 22 && overlapLeft < overlapTop)
                    {
                        player.Position = new Point(platRect.Left - picboxplayer.Width, player.Position.Y);
                    }
                    else if (feetBelowRim && overlapRight > 0 && overlapRight <= 22 && overlapRight < overlapTop)
                    {
                        player.Position = new Point(platRect.Right, player.Position.Y);
                    }
                }
            }
            if (!foundGround) player.IsGrounded = false;
        }

        private void CheckQuestionBlockCollisions()
        {
            var headRect = new Rectangle(player.Position.X, player.Position.Y, picboxplayer.Width, 30);
            foreach (var block in questionBlocks)
            {
                if (block.IsHit) continue;
                var blockRect = new Rectangle(block.Position.X, block.Position.Y,
                    block.Visual.Width, block.Visual.Height);
                if (!headRect.IntersectsWith(blockRect)) continue;

                block.IsHit = true;
                block.Visual.Invalidate();

                if (block.PowerUpInside == PowerUpType.Coin)
                {
                    // Instant coin reward
                    coinCount++;
                    player.Score += 50;
                }
                else
                {
                    // Spawn a moving mushroom collectible
                    SpawnMushroom(block.Position);
                }
            }
        }

        private void CheckWinCondition()
        {
            if (_levelComplete || isDying || player.Position.X < FLAGPOLE_X) return;
            _levelComplete = true;
            gameTimer.Stop();
            if (currentLevelNumber < allLevels.Length)
            {
                MessageBox.Show($"Level {currentLevelNumber} Complete! 🎉\nScore: {player.Score}  Coins: {coinCount}", "Level Complete!", MessageBoxButtons.OK);
                LoadNextLevel();
            }
            else
            {
                MessageBox.Show($"You completed ALL levels! 🏆\nFinal Score: {player.Score}  Coins: {coinCount}", "YOU WIN!", MessageBoxButtons.OK);
                RestartLevel();
            }
        }

        private void BecomeSuper()
        {
            isPlayerSuper = true;
            picboxplayer.Size = superPlayerSize;
            player.Health = Math.Min(player.Health + 1, 3);
            player.Position = new Point(player.Position.X, player.Position.Y - 16);
        }

        private void BecomeNormal()
        {
            if (!isPlayerSuper) return;
            isPlayerSuper = false;
            picboxplayer.Size = originalPlayerSize;
            player.Position = new Point(player.Position.X, player.Position.Y + 16);
        }

        // ════════════════════════════════════════════════════════════════════
        //  FALL DAMAGE
        // ════════════════════════════════════════════════════════════════════
        private void HandleFallDamage()
        {
            if (wasGroundedLastFrame && !player.IsGrounded)
            {
                maxFallStartY = player.Position.Y;
                canTakeFallDamage = true;
            }

            if (!wasGroundedLastFrame && player.IsGrounded && !isDying)
            {
                float fallDist = player.Position.Y - maxFallStartY;
                if (fallDist > FALL_DAMAGE_THRESHOLD && canTakeFallDamage && !isInvincible)
                {
                    player.TakeDamage(1);
                    canTakeFallDamage = false;
                    isInvincible = true;
                    invincibleTimer = 0f;
                    if (player.Health <= 0) { isDying = true; deathTimer = 0f; }
                }
                maxFallStartY = 0;
            }

            wasGroundedLastFrame = player.IsGrounded;
        }

        private void HandleDeathAnimation(long stepMs)
        {
            deathTimer += stepMs;
            if (deathTimer < 500)
                picboxplayer.Location = new Point(picboxplayer.Location.X,
                    (int)(player.Position.Y - (deathTimer / 500f) * 100));
            else if (deathTimer < DEATH_ANIMATION_DURATION)
                picboxplayer.Location = new Point(picboxplayer.Location.X,
                    (int)(player.Position.Y + ((deathTimer - 500) / (DEATH_ANIMATION_DURATION - 500)) * 300));
            else
            {
                isDying = false;
                isInvincible = false;
                invincibleTimer = 0f;
                player.Health = 3;
                isPlayerSuper = false;
                RestartLevel();
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  CAMERA
        // ════════════════════════════════════════════════════════════════════
        private void UpdateCamera()
        {
            int screenX = player.Position.X - cameraX;
            if (screenX > SCROLL_THRESHOLD && player.Position.X > SCROLL_THRESHOLD)
            {
                int newCam = Math.Min(player.Position.X - SCROLL_THRESHOLD, CAMERA_MAX);
                ScrollObjects(newCam - cameraX);
                cameraX = newCam;
            }
            else if (screenX < 200 && cameraX > 0)
            {
                int newCam = Math.Max(player.Position.X - 200, 0);
                ScrollObjects(newCam - cameraX);
                cameraX = newCam;
            }
            // Don't override the death-animation position that HandleDeathAnimation just set
            if (!isDying)
                picboxplayer.Location = new Point(player.Position.X - cameraX, player.Position.Y);
        }

        private void ScrollObjects(int scroll)
        {
            foreach (var p in platforms)
                p.PictureBox.Left -= scroll;
            foreach (var b in questionBlocks)
            {
                b.Visual.Left -= scroll;
                if (b.QuestionLabel != null) b.QuestionLabel.Left -= scroll;
            }
            foreach (var c in coins)
                if (!c.IsCollected) c.Visual.Left -= scroll;
            // Mushrooms, goombas, koopas, fast enemies use world-space positioning per frame
        }

        // ════════════════════════════════════════════════════════════════════
        //  LEVEL RESET / LOAD
        // ════════════════════════════════════════════════════════════════════
        private void RestartLevel()
        {
            // Death restart – wipe score and coins as a penalty (level advance keeps them)
            player.Score = 0;
            coinCount = 0;
            DoLevelSetup(currentLevelNumber);
        }

        private void LoadNextLevel() => DoLevelSetup(currentLevelNumber + 1);

        private void DoLevelSetup(int levelNum)
        {
            currentLevelNumber = levelNum;
            currentLevel = allLevels[currentLevelNumber - 1];
            gameManager.ResetGame();
            cameraX = 0; isDying = false; isInvincible = false; invincibleTimer = 0f;
            wasGroundedLastFrame = true; canTakeFallDamage = true; isPlayerSuper = false;
            _levelComplete = false;
            _lastHudHealth = -1; _lastHudLevel = -1; _lastHudSuper = false;
            _lastHudScore = -1; _lastHudCoins = -1;
            player.Respawn(new Point(100, 405));
            player.IsGrounded = true; player.Health = 3;
            player.OnDamageTaken = () => { BecomeNormal(); };
            picboxplayer.Size = originalPlayerSize;
            picboxplayer.Location = player.Position;
            ClearPlatforms(); CreateLongLevel();
            _stopwatch.Restart(); _lastTickMs = 0; _accumulatedMs = 0;
            Text = $"Super Mario – Level {currentLevelNumber}";
            UpdateHud();
            gameManager.StartGame(); gameTimer.Start();
        }

        private void ClearPlatforms()
        {
            foreach (var p in platforms) { Controls.Remove(p.PictureBox); p.PictureBox.Dispose(); }
            platforms.Clear();
            foreach (var g in goombas) { Controls.Remove(g.Visual); g.Visual.Dispose(); }
            goombas.Clear();
            foreach (var k in koopas) { Controls.Remove(k.Visual); k.Visual.Dispose(); }
            koopas.Clear();
            foreach (var fe in fastEnemies) { Controls.Remove(fe.Visual); fe.Visual.Dispose(); }
            fastEnemies.Clear();
            foreach (var je in jumpingEnemies) { Controls.Remove(je.Visual); je.Visual.Dispose(); }
            jumpingEnemies.Clear();
            foreach (var pe in patrolEnemies) { Controls.Remove(pe.Visual); pe.Visual.Dispose(); }
            patrolEnemies.Clear();
            foreach (var fl in flyingEnemies) { Controls.Remove(fl.Visual); fl.Visual.Dispose(); }
            flyingEnemies.Clear();
            ClearCoins();
            ClearPowerUps();
        }

    }
}
