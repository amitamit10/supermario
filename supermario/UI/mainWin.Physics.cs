using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace supermario
{
    partial class mainWin
    {
        // ══════════════════════════════════════════════════════════════════
        //  PHYSICS / COLLISION
        // ══════════════════════════════════════════════════════════════════
        private void CheckPlatformCollisions()
        {
            Rectangle previousRect = new Rectangle(player.PreviousPosition.X, player.PreviousPosition.Y,
                picboxplayer.Width, picboxplayer.Height);
            Rectangle playerRect = new Rectangle(player.Position.X, player.Position.Y,
                picboxplayer.Width, picboxplayer.Height);
            bool foundGround = false;

            foreach (var plat in platforms)
            {
                if (plat.Type == "finish") continue;

                Rectangle platRect = new Rectangle(plat.Position.X, plat.Position.Y,
                    plat.PictureBox.Width, plat.PictureBox.Height);

                bool horizontallyOverlaps = playerRect.Right > platRect.Left && playerRect.Left < platRect.Right;
                // Tolerate ±1 px rounding drift from LandOn so the grounded flag isn't
                // dropped for a frame between physics step and collision pass.
                if (Math.Abs(playerRect.Bottom - platRect.Top) <= 1 && horizontallyOverlaps)
                {
                    foundGround = true;
                    continue;
                }

                if (!playerRect.IntersectsWith(platRect)) continue;

                if (plat.Type == "ground")
                {
                    player.LandOn(platRect.Top, picboxplayer.Height);
                    foundGround = true;
                    playerRect = new Rectangle(player.Position.X, player.Position.Y,
                        picboxplayer.Width, picboxplayer.Height);
                    continue;
                }

                bool crossedTop = previousRect.Bottom <= platRect.Top && player.VerticalVelocity >= 0;
                bool crossedBottom = previousRect.Top >= platRect.Bottom && player.VerticalVelocity < 0;
                bool crossedLeft = previousRect.Right <= platRect.Left && player.HorizontalVelocity > 0;
                bool crossedRight = previousRect.Left >= platRect.Right && player.HorizontalVelocity < 0;

                if (crossedTop)
                {
                    player.LandOn(platRect.Top, picboxplayer.Height);
                    foundGround = true;
                }
                else if (crossedBottom)
                {
                    player.HitCeiling(platRect.Bottom);
                }
                else if (crossedLeft)
                {
                    player.BlockHorizontal(platRect.Left - picboxplayer.Width);
                }
                else if (crossedRight)
                {
                    player.BlockHorizontal(platRect.Right);
                }
                else
                {
                    ResolveSmallestOverlap(playerRect, platRect, ref foundGround);
                }

                playerRect = new Rectangle(player.Position.X, player.Position.Y,
                    picboxplayer.Width, picboxplayer.Height);
            }

            // ── Question blocks: solid physical blocks + bottom-only activation ─────
            foreach (var block in questionBlocks)
            {
                Rectangle blockRect = new Rectangle(block.Position.X, block.Position.Y,
                    block.Visual.Width, block.Visual.Height);

                bool hOverlap = playerRect.Right > blockRect.Left && playerRect.Left < blockRect.Right;
                if (Math.Abs(playerRect.Bottom - blockRect.Top) <= 1 && hOverlap) { foundGround = true; continue; }

                if (!playerRect.IntersectsWith(blockRect)) continue;

                bool qCrossedTop    = previousRect.Bottom <= blockRect.Top    && player.VerticalVelocity >= 0;
                bool qCrossedBottom = previousRect.Top    >= blockRect.Bottom  && player.VerticalVelocity < 0;
                bool qCrossedLeft   = previousRect.Right  <= blockRect.Left    && player.HorizontalVelocity > 0;
                bool qCrossedRight  = previousRect.Left   >= blockRect.Right   && player.HorizontalVelocity < 0;

                if      (qCrossedTop)    { player.LandOn(blockRect.Top, picboxplayer.Height); foundGround = true; }
                else if (qCrossedBottom) { player.HitCeiling(blockRect.Bottom); if (!block.IsHit) ActivateQuestionBlock(block); }
                else if (qCrossedLeft)   { player.BlockHorizontal(blockRect.Left - picboxplayer.Width); }
                else if (qCrossedRight)  { player.BlockHorizontal(blockRect.Right); }
                else                     { ResolveQBlockOverlap(playerRect, blockRect, block, ref foundGround); }

                playerRect = new Rectangle(player.Position.X, player.Position.Y,
                    picboxplayer.Width, picboxplayer.Height);
            }

            if (!foundGround) player.LeaveGround();
        }

        private void ResolveQBlockOverlap(Rectangle playerRect, Rectangle blockRect, QuestionBlock block, ref bool foundGround)
        {
            int overlapTop    = playerRect.Bottom - blockRect.Top;
            int overlapBottom = blockRect.Bottom  - playerRect.Top;
            int overlapLeft   = playerRect.Right  - blockRect.Left;
            int overlapRight  = blockRect.Right   - playerRect.Left;
            int minO = Math.Min(Math.Min(overlapTop, overlapBottom), Math.Min(overlapLeft, overlapRight));

            if      (minO == overlapTop && player.VerticalVelocity >= 0) { player.LandOn(blockRect.Top, picboxplayer.Height); foundGround = true; }
            else if (minO == overlapBottom) { bool wasMovingUp = player.VerticalVelocity < 0; player.HitCeiling(blockRect.Bottom); if (!block.IsHit && wasMovingUp) ActivateQuestionBlock(block); }
            else if (minO == overlapTop)
            {
                // Smallest overlap is from above (player's bottom poking into block top)
                // even though VV < 0 — a grazing top-corner contact, not a from-below
                // hit. Push the player up onto the block instead of teleporting them
                // through to the underside.
                player.LandOn(blockRect.Top, picboxplayer.Height);
                foundGround = true;
            }
            else if (minO == overlapLeft)   { player.BlockHorizontal(blockRect.Left - picboxplayer.Width); }
            else                            { player.BlockHorizontal(blockRect.Right); }
        }

        private void ActivateQuestionBlock(QuestionBlock block)
        {
            block.IsHit = true;
            block.Visual.Invalidate();
            if (block.PowerUpInside == PowerUpType.Coin) { coinCount++; player.Score += 50; }
            else SpawnMushroom(block.Position);
        }

        private void ResolveSmallestOverlap(Rectangle playerRect, Rectangle platRect, ref bool foundGround)
        {
            int overlapTop    = playerRect.Bottom - platRect.Top;
            int overlapBottom = platRect.Bottom   - playerRect.Top;
            int overlapLeft   = playerRect.Right  - platRect.Left;
            int overlapRight  = platRect.Right    - playerRect.Left;
            int minOverlap    = Math.Min(Math.Min(overlapTop, overlapBottom), Math.Min(overlapLeft, overlapRight));

            if (minOverlap == overlapTop && player.VerticalVelocity >= 0)
            {
                // Player's bottom grazed the platform surface – land on top.
                player.LandOn(platRect.Top, picboxplayer.Height);
                foundGround = true;
            }
            else if (minOverlap == overlapBottom)
            {
                // Player's head is inside the platform underside – ceiling bounce
                // regardless of vertical velocity (covers the moving-upward edge case
                // where crossedBottom was not caught by the directional checks).
                player.HitCeiling(platRect.Bottom);
            }
            else if (minOverlap == overlapTop)
            {
                // Top is smallest overlap but player is moving upward (rare corner);
                // treat as a ceiling to prevent the player clipping through sideways.
                player.HitCeiling(platRect.Bottom);
            }
            else if (minOverlap == overlapLeft)
            {
                player.BlockHorizontal(platRect.Left - picboxplayer.Width);
            }
            else
            {
                player.BlockHorizontal(platRect.Right);
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
                player.Score = 0;
                coinCount = 0;
                DoLevelSetup(1);
            }
        }

        private void BecomeSuper()
        {
            if (isPlayerSuper) return;
            isPlayerSuper = true;
            picboxplayer.Size = superPlayerSize;
            player.MaxX = LEVEL_PIXEL_WIDTH - picboxplayer.Width;
            int heightDelta = superPlayerSize.Height - originalPlayerSize.Height;
            player.Position = new Point(player.Position.X, player.Position.Y - heightDelta);
        }

        // Enemy contact damage – super state absorbs one hit instead of stacking
        // (health drop + shrink) penalties from a single touch.
        private void HitByEnemy()
        {
            if (isInvincible) return;
            isInvincible = true;
            invincibleTimer = 0f;
            if (isPlayerSuper)
            {
                BecomeNormal();
                return;
            }
            player.TakeDamage(1);
            if (player.Health <= 0) { isDying = true; deathTimer = 0f; }
        }

        private void BecomeNormal()
        {
            if (!isPlayerSuper) return;
            isPlayerSuper = false;
            picboxplayer.Size = originalPlayerSize;
            player.MaxX = LEVEL_PIXEL_WIDTH - picboxplayer.Width;
            int heightDelta = superPlayerSize.Height - originalPlayerSize.Height;
            player.Position = new Point(player.Position.X, player.Position.Y + heightDelta);
        }

        // ══════════════════════════════════════════════════════════════════
        //  FALL DAMAGE
        // ══════════════════════════════════════════════════════════════════
        private void HandleFallDamage()
        {
            if (wasGroundedLastFrame && !player.IsGrounded)
            {
                maxFallStartY = player.Position.Y;
                canTakeFallDamage = true;
            }
            else if (!player.IsGrounded)
            {
                // Track the apex of the jump/fall – fall damage should be measured
                // from the highest point reached, not from the take-off Y.
                if (player.Position.Y < maxFallStartY)
                    maxFallStartY = player.Position.Y;
            }

            if (!wasGroundedLastFrame && player.IsGrounded && !isDying)
            {
                float fallDist = player.Position.Y - maxFallStartY;
                if (fallDist > FALL_DAMAGE_THRESHOLD && canTakeFallDamage && !isInvincible)
                {
                    canTakeFallDamage = false;
                    HitByEnemy();
                }
                maxFallStartY = 0;
            }

            wasGroundedLastFrame = player.IsGrounded;
        }

        private void HandleDeathAnimation(long stepMs)
        {
            deathTimer += stepMs;
            // phase 1: jump up 100 px from death position
            // phase 2: fall from the top of the jump (deathY - 100) downward 400 px
            // Both phases share the same reference so there is no snap at the transition.
            int deathTopY = (int)(player.Position.Y - 100);
            if (deathTimer < 500)
                picboxplayer.Location = new Point(picboxplayer.Location.X,
                    (int)(player.Position.Y - (deathTimer / 500f) * 100));
            else if (deathTimer < DEATH_ANIMATION_DURATION)
                picboxplayer.Location = new Point(picboxplayer.Location.X,
                    (int)(deathTopY + ((deathTimer - 500) / (DEATH_ANIMATION_DURATION - 500)) * 400));
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

        // ══════════════════════════════════════════════════════════════════
        //  CAMERA
        // ══════════════════════════════════════════════════════════════════
        private bool UpdateCamera()
        {
            int newCam = cameraX;
            int screenX = player.Position.X - cameraX;

            if (screenX > SCROLL_THRESHOLD && player.Position.X > SCROLL_THRESHOLD)
                newCam = Math.Min(player.Position.X - SCROLL_THRESHOLD, CameraMax);
            else if (screenX < 200 && cameraX > 0)
                newCam = Math.Max(player.Position.X - 200, 0);

            bool cameraMoved = newCam != cameraX;
            if (cameraMoved)
            {
                ScrollObjects(newCam - cameraX);
                cameraX = newCam;
            }

            UpdatePlayerScreenLocation();
            return cameraMoved;
        }

        private void UpdatePlayerScreenLocation()
        {
            // Don't override the death-animation position that HandleDeathAnimation just set.
            if (isDying) return;

            Point screenLocation = new Point(player.Position.X - cameraX, player.Position.Y);
            if (picboxplayer.Location != screenLocation)
                picboxplayer.Location = screenLocation;
        }

        private void ScrollObjects(int scroll)
        {
            if (scroll == 0) return;

            SuspendLayout();
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
            ResumeLayout(false);
        }

        // ══════════════════════════════════════════════════════════════════
        //  LEVEL RESET / LOAD
        // ══════════════════════════════════════════════════════════════════
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
            wasGroundedLastFrame = true; canTakeFallDamage = true; isPlayerSuper = false; facingRight = true;
            _levelComplete = false;
            _lastHudHealth = -1; _lastHudLevel = -1; _lastHudSuper = false;
            _lastHudScore = -1; _lastHudCoins = -1;
            player.Respawn(GetPlayerStartPosition());
            player.IsGrounded = true; player.Health = 3;
            player.OnDamageTaken = () => { BecomeNormal(); };
            picboxplayer.Size = originalPlayerSize;
            player.MaxX = LEVEL_PIXEL_WIDTH - picboxplayer.Width;
            picboxplayer.Location = player.Position;
            ClearPlatforms(); CreateLongLevel();
            _stopwatch.Restart(); _lastTickMs = 0; _accumulatedMs = 0;
            Text = $"Super Mario – Level {currentLevelNumber}";
            UpdateHud();
            gameManager.StartGame(); gameTimer.Stop(); gameTimer.Start();
        }

        private Point GetPlayerStartPosition()
        {
            return new Point(PLAYER_START_X, GROUND_TOP_Y - originalPlayerSize.Height);
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
            animatedBlocks.Clear();
        }

    }
}
