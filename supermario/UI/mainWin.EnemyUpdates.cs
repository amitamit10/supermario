using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace supermario
{
    partial class mainWin
    {
        // ════════════════════════════════════════════════════════════════════
        //  GOOMBA SPAWNING & UPDATE
        // ════════════════════════════════════════════════════════════════════
        private EnemyDef[] GetCurrentLevelGoombas()
        {
            switch (currentLevelNumber)
            {
                case 1: return LEVEL_1_GOOMBAS;
                case 2: return LEVEL_2_GOOMBAS;
                case 3: return LEVEL_3_GOOMBAS;
                default:
                    return new EnemyDef[] {
                        new EnemyDef(600, 461), new EnemyDef(950, 461),
                        new EnemyDef(1250, 461), new EnemyDef(1550, 461),
                        new EnemyDef(1850, 461), new EnemyDef(2100, 461),
                    };
            }
        }

        private void SpawnGoombas()
        {
            foreach (var def in GetCurrentLevelGoombas())
            {
                var goomba = new Goomba(new Point(def.X, def.Y));
                Controls.Add(goomba.Visual);
                goombas.Add(goomba);
            }
        }

        private void UpdateGoombas()
        {
            var playerRect = new Rectangle(
                player.Position.X, player.Position.Y,
                picboxplayer.Width, picboxplayer.Height);

            for (int i = goombas.Count - 1; i >= 0; i--)
            {
                var goomba = goombas[i];

                if (!goomba.IsAlive)
                {
                    Controls.Remove(goomba.Visual);
                    goomba.Visual.Dispose();
                    goombas.RemoveAt(i);
                    continue;
                }

                if (goomba.Position.Y > 620)
                {
                    Controls.Remove(goomba.Visual);
                    goomba.Visual.Dispose();
                    goombas.RemoveAt(i);
                    continue;
                }

                // Squished enemies don't need gravity or platform checks; handle and skip early
                if (goomba.IsSquished)
                {
                    if (goomba.UpdateSquish(FIXED_STEP_MS)) goomba.Kill();
                    goomba.Visual.Location = new Point(goomba.Position.X - cameraX, goomba.Position.Y);
                    continue;
                }

                if (!goomba.IsGrounded)
                {
                    goomba.VerticalVelocity += 0.6f;
                    if (goomba.VerticalVelocity > 15f) goomba.VerticalVelocity = 15f;
                    goomba.Position = new Point(goomba.Position.X, goomba.Position.Y + (int)Math.Round(goomba.VerticalVelocity));
                }

                if (goomba.Position.Y > 600) { goomba.Kill(); continue; }

                bool gGrounded = false;
                bool gWallHit = false;
                var gRect = goomba.Bounds;
                foreach (var plat in platforms)
                {
                    var pr = new Rectangle(
                        plat.Position.X,
                        plat.Position.Y,
                        plat.PictureBox.Width,
                        plat.PictureBox.Height);

                    if (!gRect.IntersectsWith(pr)) continue;

                    int overlapTop    = gRect.Bottom - pr.Top;
                    int overlapBottom = pr.Bottom - gRect.Top;
                    int overlapLeft   = gRect.Right - pr.Left;
                    int overlapRight  = pr.Right - gRect.Left;
                    int minOverlap    = Math.Min(Math.Min(overlapTop, overlapBottom), Math.Min(overlapLeft, overlapRight));

                    if (minOverlap == overlapTop && overlapTop < 30)
                    {
                        goomba.Position = new Point(goomba.Position.X, pr.Top - goomba.Visual.Height);
                        goomba.VerticalVelocity = 0;
                        gGrounded = true;
                        break;
                    }
                    else if (minOverlap == overlapLeft || minOverlap == overlapRight)
                    {
                        // Wall hit; reverse but keep scanning so a floor platform later
                        // in the list can still set gGrounded.
                        if (!gWallHit) { goomba.ReverseDirection(); gWallHit = true; }
                        continue;
                    }
                }
                goomba.IsGrounded = gGrounded;

                // Question block wall collision
                var gBounds2 = goomba.Bounds;
                foreach (var qb in questionBlocks)
                {
                    var br = new Rectangle(qb.Position.X, qb.Position.Y, qb.Visual.Width, qb.Visual.Height);
                    if (!gBounds2.IntersectsWith(br)) continue;
                    int qot = gBounds2.Bottom - br.Top;
                    int qob = br.Bottom - gBounds2.Top;
                    int qol = gBounds2.Right - br.Left;
                    int qorr = br.Right - gBounds2.Left;
                    int qmin = Math.Min(Math.Min(qot, qob), Math.Min(qol, qorr));
                    if ((qmin == qol || qmin == qorr) && qmin < qot)
                    { goomba.ReverseDirection(); break; }
                }

                goomba.Update();
                goomba.Visual.Location = new Point(goomba.Position.X - cameraX, goomba.Position.Y);

                if (isDying) continue;
                var gWorldRect = new Rectangle(goomba.Position.X, goomba.Position.Y, goomba.Visual.Width, goomba.Visual.Height);
                if (!playerRect.IntersectsWith(gWorldRect)) continue;

                int playerBottom = player.Position.Y + picboxplayer.Height;
                int goombaTop    = goomba.Position.Y;
                bool fallingDown = playerBottom - goombaTop < 24;
                bool playerAbove = player.Position.Y < goomba.Position.Y + goomba.Visual.Height / 2;

                if (fallingDown && playerAbove && player.VerticalVelocity >= 0)
                {
                    goomba.Squish();
                    player.Bounce();
                    player.Score += 100;
                }
                else
                {
                    HitByEnemy();
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  KOOPA SPAWNING & UPDATE
        // ════════════════════════════════════════════════════════════════════
        private EnemyDef[] GetCurrentLevelKoopas()
        {
            switch (currentLevelNumber)
            {
                case 1: return LEVEL_1_KOOPAS;
                case 2: return LEVEL_2_KOOPAS;
                case 3: return LEVEL_3_KOOPAS;
                default:
                    return new EnemyDef[] {
                        new EnemyDef(750, 457), new EnemyDef(1350, 457),
                        new EnemyDef(1700, 457), new EnemyDef(2300, 457),
                    };
            }
        }

        private void SpawnKoopas()
        {
            foreach (var def in GetCurrentLevelKoopas())
            {
                var k = new Koopa(new Point(def.X, def.Y));
                Controls.Add(k.Visual);
                koopas.Add(k);
            }
        }

        private void UpdateKoopas()
        {
            var playerRect = new Rectangle(
                player.Position.X, player.Position.Y,
                picboxplayer.Width, picboxplayer.Height);

            for (int i = koopas.Count - 1; i >= 0; i--)
            {
                var k = koopas[i];

                if (!k.IsAlive)
                {
                    Controls.Remove(k.Visual);
                    k.Visual.Dispose();
                    koopas.RemoveAt(i);
                    continue;
                }

                if (k.Position.Y > 620)
                {
                    Controls.Remove(k.Visual);
                    k.Visual.Dispose();
                    koopas.RemoveAt(i);
                    continue;
                }

                // Shell state: no gravity needed, just tick timer and skip physics.
                // Walking-into-shell kicks it for points; bypass the rest of the physics
                // path so player-collision is handled here, not by the dead branch below.
                if (k.IsShell)
                {
                    if (k.UpdateShell(FIXED_STEP_MS)) k.Kill();
                    k.Visual.Location = new Point(k.Position.X - cameraX, k.Position.Y);

                    if (!isDying)
                    {
                        var shellWorld = new Rectangle(k.Position.X, k.Position.Y, k.Visual.Width, k.Visual.Height);
                        if (playerRect.IntersectsWith(shellWorld))
                        {
                            k.Stomp(); // second Stomp on shell calls Kill()
                            player.Score += 50;
                        }
                    }
                    continue;
                }

                // Gravity
                if (!k.IsGrounded)
                {
                    k.VerticalVelocity += 0.6f;
                    if (k.VerticalVelocity > 15f) k.VerticalVelocity = 15f;
                    k.Position = new Point(k.Position.X, k.Position.Y + (int)Math.Round(k.VerticalVelocity));
                }

                if (k.Position.Y > 600) { k.Kill(); continue; }

                // Platform collision
                bool kGrounded = false;
                bool kWallHit = false;
                var kRect = k.Bounds;
                foreach (var plat in platforms)
                {
                    var pr = new Rectangle(
                        plat.Position.X, plat.Position.Y,
                        plat.PictureBox.Width, plat.PictureBox.Height);
                    if (!kRect.IntersectsWith(pr)) continue;

                    int ot = kRect.Bottom - pr.Top, ob = pr.Bottom - kRect.Top;
                    int ol = kRect.Right - pr.Left, orr = pr.Right - kRect.Left;
                    int min = Math.Min(Math.Min(ot, ob), Math.Min(ol, orr));

                    if (min == ot && ot < 30)
                    {
                        k.Position = new Point(k.Position.X, pr.Top - k.Visual.Height);
                        k.VerticalVelocity = 0;
                        kGrounded = true;
                        break;
                    }
                    else if (min == ol || min == orr)
                    {
                        if (!kWallHit) { k.ReverseDirection(); kWallHit = true; }
                        continue;
                    }
                }
                k.IsGrounded = kGrounded;

                var kBounds2 = k.Bounds;
                foreach (var qb in questionBlocks)
                {
                    var br = new Rectangle(qb.Position.X, qb.Position.Y, qb.Visual.Width, qb.Visual.Height);
                    if (!kBounds2.IntersectsWith(br)) continue;
                    int qot = kBounds2.Bottom - br.Top;
                    int qob = br.Bottom - kBounds2.Top;
                    int qol = kBounds2.Right - br.Left;
                    int qorr = br.Right - kBounds2.Left;
                    int qmin = Math.Min(Math.Min(qot, qob), Math.Min(qol, qorr));
                    if ((qmin == qol || qmin == qorr) && qmin < qot)
                    { k.ReverseDirection(); break; }
                }

                k.Update();
                k.Visual.Location = new Point(k.Position.X - cameraX, k.Position.Y);

                if (isDying) continue;
                var kWorld = new Rectangle(k.Position.X, k.Position.Y, k.Visual.Width, k.Visual.Height);
                if (!playerRect.IntersectsWith(kWorld)) continue;

                int pBottom = player.Position.Y + picboxplayer.Height;
                bool falling = pBottom - k.Position.Y < 24;
                bool above   = player.Position.Y < k.Position.Y + k.Visual.Height / 2;

                if (falling && above && player.VerticalVelocity >= 0)
                {
                    k.Stomp();
                    player.Bounce();
                    player.Score += 150;
                }
                else
                {
                    HitByEnemy();
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  FAST ENEMY SPAWNING & UPDATE
        // ════════════════════════════════════════════════════════════════════
        private EnemyDef[] GetCurrentLevelFastEnemies()
        {
            switch (currentLevelNumber)
            {
                case 1: return LEVEL_1_FAST_ENEMIES;
                case 2: return LEVEL_2_FAST_ENEMIES;
                case 3: return LEVEL_3_FAST_ENEMIES;
                default:
                    return new EnemyDef[] {
                        new EnemyDef(1100, 465), new EnemyDef(1800, 465), new EnemyDef(2400, 465),
                    };
            }
        }

        private void SpawnFastEnemies()
        {
            foreach (var def in GetCurrentLevelFastEnemies())
            {
                var fe = new FastEnemy(new Point(def.X, def.Y));
                Controls.Add(fe.Visual);
                fastEnemies.Add(fe);
            }
        }

        private void UpdateFastEnemies()
        {
            var playerRect = new Rectangle(
                player.Position.X, player.Position.Y,
                picboxplayer.Width, picboxplayer.Height);

            for (int i = fastEnemies.Count - 1; i >= 0; i--)
            {
                var fe = fastEnemies[i];

                if (!fe.IsAlive)
                {
                    Controls.Remove(fe.Visual);
                    fe.Visual.Dispose();
                    fastEnemies.RemoveAt(i);
                    continue;
                }

                if (fe.Position.Y > 620)
                {
                    Controls.Remove(fe.Visual);
                    fe.Visual.Dispose();
                    fastEnemies.RemoveAt(i);
                    continue;
                }

                if (fe.IsSquished)
                {
                    if (fe.UpdateSquish(FIXED_STEP_MS)) fe.Kill();
                    fe.Visual.Location = new Point(fe.Position.X - cameraX, fe.Position.Y);
                    continue;
                }

                // Gravity
                if (!fe.IsGrounded)
                {
                    fe.VerticalVelocity += 0.6f;
                    if (fe.VerticalVelocity > 15f) fe.VerticalVelocity = 15f;
                    fe.Position = new Point(fe.Position.X, fe.Position.Y + (int)Math.Round(fe.VerticalVelocity));
                }

                if (fe.Position.Y > 600) { fe.Kill(); continue; }

                // Platform collision
                bool feGrounded = false;
                bool feWallHit = false;
                var feRect = fe.Bounds;
                foreach (var plat in platforms)
                {
                    var pr = new Rectangle(
                        plat.Position.X, plat.Position.Y,
                        plat.PictureBox.Width, plat.PictureBox.Height);
                    if (!feRect.IntersectsWith(pr)) continue;

                    int ot = feRect.Bottom - pr.Top, ob = pr.Bottom - feRect.Top;
                    int ol = feRect.Right - pr.Left, orr = pr.Right - feRect.Left;
                    int min = Math.Min(Math.Min(ot, ob), Math.Min(ol, orr));

                    if (min == ot && ot < 30)
                    {
                        fe.Position = new Point(fe.Position.X, pr.Top - fe.Visual.Height);
                        fe.VerticalVelocity = 0;
                        feGrounded = true;
                        break;
                    }
                    else if (min == ol || min == orr)
                    {
                        if (!feWallHit) { fe.ReverseDirection(); feWallHit = true; }
                        continue;
                    }
                }
                fe.IsGrounded = feGrounded;

                var feBounds2 = fe.Bounds;
                foreach (var qb in questionBlocks)
                {
                    var br = new Rectangle(qb.Position.X, qb.Position.Y, qb.Visual.Width, qb.Visual.Height);
                    if (!feBounds2.IntersectsWith(br)) continue;
                    int qot = feBounds2.Bottom - br.Top;
                    int qob = br.Bottom - feBounds2.Top;
                    int qol = feBounds2.Right - br.Left;
                    int qorr = br.Right - feBounds2.Left;
                    int qmin = Math.Min(Math.Min(qot, qob), Math.Min(qol, qorr));
                    if ((qmin == qol || qmin == qorr) && qmin < qot)
                    { fe.ReverseDirection(); break; }
                }

                fe.Update();
                fe.Visual.Location = new Point(fe.Position.X - cameraX, fe.Position.Y);

                if (isDying) continue;
                var feWorld = new Rectangle(fe.Position.X, fe.Position.Y, fe.Visual.Width, fe.Visual.Height);
                if (!playerRect.IntersectsWith(feWorld)) continue;

                int pBot = player.Position.Y + picboxplayer.Height;
                bool fall = pBot - fe.Position.Y < 24;
                bool abv  = player.Position.Y < fe.Position.Y + fe.Visual.Height / 2;

                if (fall && abv && player.VerticalVelocity >= 0)
                {
                    fe.Squish();
                    player.Bounce();
                    player.Score += 200;
                }
                else
                {
                    HitByEnemy();
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  JUMPING ENEMY SPAWNING & UPDATE
        // ════════════════════════════════════════════════════════════════════
        private EnemyDef[] GetCurrentLevelJumping()
        {
            switch (currentLevelNumber)
            {
                case 1: return LEVEL_1_JUMPING;
                case 2: return LEVEL_2_JUMPING;
                case 3: return LEVEL_3_JUMPING;
                default:
                    return new EnemyDef[] {
                        new EnemyDef(700, 463), new EnemyDef(1300, 463), new EnemyDef(2000, 463),
                    };
            }
        }

        private void SpawnJumpingEnemies()
        {
            foreach (var def in GetCurrentLevelJumping())
            {
                var je = new JumpingEnemy(new Point(def.X, def.Y));
                Controls.Add(je.Visual);
                jumpingEnemies.Add(je);
            }
        }

        private void UpdateJumpingEnemies()
        {
            var playerRect = new Rectangle(
                player.Position.X, player.Position.Y,
                picboxplayer.Width, picboxplayer.Height);

            for (int i = jumpingEnemies.Count - 1; i >= 0; i--)
            {
                var je = jumpingEnemies[i];

                if (!je.IsAlive)
                {
                    Controls.Remove(je.Visual);
                    je.Visual.Dispose();
                    jumpingEnemies.RemoveAt(i);
                    continue;
                }

                if (je.Position.Y > 620)
                {
                    Controls.Remove(je.Visual);
                    je.Visual.Dispose();
                    jumpingEnemies.RemoveAt(i);
                    continue;
                }

                if (je.IsSquished)
                {
                    if (je.UpdateSquish(FIXED_STEP_MS)) je.Kill();
                    je.Visual.Location = new Point(je.Position.X - cameraX, je.Position.Y);
                    continue;
                }

                // Gravity
                if (!je.IsGrounded)
                {
                    je.VerticalVelocity += 0.6f;
                    if (je.VerticalVelocity > 15f) je.VerticalVelocity = 15f;
                    je.Position = new Point(je.Position.X, je.Position.Y + (int)Math.Round(je.VerticalVelocity));
                }

                if (je.Position.Y > 600) { je.Kill(); continue; }

                bool jeGrounded = false;
                bool jeWallHit = false;
                var jeRect = je.Bounds;
                foreach (var plat in platforms)
                {
                    var pr = new Rectangle(
                        plat.Position.X, plat.Position.Y,
                        plat.PictureBox.Width, plat.PictureBox.Height);
                    if (!jeRect.IntersectsWith(pr)) continue;

                    int ot = jeRect.Bottom - pr.Top, ob = pr.Bottom - jeRect.Top;
                    int ol = jeRect.Right - pr.Left, orr = pr.Right - jeRect.Left;
                    int min = Math.Min(Math.Min(ot, ob), Math.Min(ol, orr));

                    if (min == ot && ot < 30)
                    {
                        je.Position = new Point(je.Position.X, pr.Top - je.Visual.Height);
                        je.VerticalVelocity = 0;
                        jeGrounded = true;
                        break;
                    }
                    else if (min == ob && ob < 20 && je.VerticalVelocity < 0)
                    {
                        // Ceiling hit while jumping upward
                        je.Position = new Point(je.Position.X, pr.Bottom);
                        je.VerticalVelocity = 0;
                        break;
                    }
                    else if (min == ol || min == orr)
                    {
                        if (!jeWallHit) { je.ReverseDirection(); jeWallHit = true; }
                        continue;
                    }
                }
                je.IsGrounded = jeGrounded;

                // An enemy resting exactly on a platform top has bottom == platTop,
                // which IntersectsWith treats as non-overlapping, so the loop above
                // would flip IsGrounded off every other frame. That halves the jump
                // timer cadence (it only ticks while grounded). Re-confirm footing
                // with a 2px probe just below the feet when not rising.
                if (!jeGrounded && je.VerticalVelocity >= 0)
                {
                    var feet = new Rectangle(je.Position.X, je.Position.Y + je.Visual.Height, je.Visual.Width, 2);
                    foreach (var plat in platforms)
                    {
                        var pr = new Rectangle(plat.Position.X, plat.Position.Y,
                            plat.PictureBox.Width, plat.PictureBox.Height);
                        if (feet.IntersectsWith(pr)) { je.IsGrounded = true; break; }
                    }
                }

                var jeBounds2 = je.Bounds;
                foreach (var qb in questionBlocks)
                {
                    var br = new Rectangle(qb.Position.X, qb.Position.Y, qb.Visual.Width, qb.Visual.Height);
                    if (!jeBounds2.IntersectsWith(br)) continue;
                    int qot = jeBounds2.Bottom - br.Top;
                    int qob = br.Bottom - jeBounds2.Top;
                    int qol = jeBounds2.Right - br.Left;
                    int qorr = br.Right - jeBounds2.Left;
                    int qmin = Math.Min(Math.Min(qot, qob), Math.Min(qol, qorr));
                    if (qmin == qob && qob < 20 && je.VerticalVelocity < 0)
                    { je.Position = new Point(je.Position.X, br.Bottom); je.VerticalVelocity = 0; break; }
                    if ((qmin == qol || qmin == qorr) && qmin < qot)
                    { je.ReverseDirection(); break; }
                }

                je.Update();
                je.Visual.Location = new Point(je.Position.X - cameraX, je.Position.Y);

                if (isDying) continue;
                var jeWorld = new Rectangle(je.Position.X, je.Position.Y, je.Visual.Width, je.Visual.Height);
                if (!playerRect.IntersectsWith(jeWorld)) continue;

                int pBottom = player.Position.Y + picboxplayer.Height;
                bool falling = pBottom - je.Position.Y < 24;
                bool above   = player.Position.Y < je.Position.Y + je.Visual.Height / 2;

                if (falling && above && player.VerticalVelocity >= 0)
                {
                    je.Squish();
                    player.Bounce();
                    player.Score += 150;
                }
                else
                {
                    HitByEnemy();
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  PLATFORM PATROL ENEMY SPAWNING & UPDATE
        // ════════════════════════════════════════════════════════════════════
        private EnemyDef[] GetCurrentLevelPatrol()
        {
            switch (currentLevelNumber)
            {
                case 1: return LEVEL_1_PATROL;
                case 2: return LEVEL_2_PATROL;
                case 3: return LEVEL_3_PATROL;
                default:
                    return new EnemyDef[] {
                        new EnemyDef(900, 463), new EnemyDef(1600, 463),
                    };
            }
        }

        private void SpawnPatrolEnemies()
        {
            foreach (var def in GetCurrentLevelPatrol())
            {
                var pe = new PlatformPatrolEnemy(new Point(def.X, def.Y));
                Controls.Add(pe.Visual);
                patrolEnemies.Add(pe);
            }
        }

        private void UpdatePatrolEnemies()
        {
            var playerRect = new Rectangle(
                player.Position.X, player.Position.Y,
                picboxplayer.Width, picboxplayer.Height);

            for (int i = patrolEnemies.Count - 1; i >= 0; i--)
            {
                var pe = patrolEnemies[i];

                if (!pe.IsAlive)
                {
                    Controls.Remove(pe.Visual);
                    pe.Visual.Dispose();
                    patrolEnemies.RemoveAt(i);
                    continue;
                }

                if (pe.Position.Y > 620)
                {
                    Controls.Remove(pe.Visual);
                    pe.Visual.Dispose();
                    patrolEnemies.RemoveAt(i);
                    continue;
                }

                if (pe.IsSquished)
                {
                    if (pe.UpdateSquish(FIXED_STEP_MS)) pe.Kill();
                    pe.Visual.Location = new Point(pe.Position.X - cameraX, pe.Position.Y);
                    continue;
                }

                // Gravity
                if (!pe.IsGrounded)
                {
                    pe.VerticalVelocity += 0.6f;
                    if (pe.VerticalVelocity > 15f) pe.VerticalVelocity = 15f;
                    pe.Position = new Point(pe.Position.X, pe.Position.Y + (int)Math.Round(pe.VerticalVelocity));
                }

                if (pe.Position.Y > 600) { pe.Kill(); continue; }

                bool peGrounded = false;
                bool peWallHit = false;
                var peRect = pe.Bounds;
                foreach (var plat in platforms)
                {
                    var pr = new Rectangle(
                        plat.Position.X, plat.Position.Y,
                        plat.PictureBox.Width, plat.PictureBox.Height);
                    if (!peRect.IntersectsWith(pr)) continue;

                    int ot = peRect.Bottom - pr.Top, ob = pr.Bottom - peRect.Top;
                    int ol = peRect.Right - pr.Left, orr = pr.Right - peRect.Left;
                    int min = Math.Min(Math.Min(ot, ob), Math.Min(ol, orr));

                    if (min == ot && ot < 30)
                    {
                        pe.Position = new Point(pe.Position.X, pr.Top - pe.Visual.Height);
                        pe.VerticalVelocity = 0;
                        peGrounded = true;
                        break;
                    }
                    else if (min == ol || min == orr)
                    {
                        if (!peWallHit) { pe.ReverseDirection(); peWallHit = true; }
                        continue;
                    }
                }
                pe.IsGrounded = peGrounded;

                // Question block wall collision for patrol enemies
                if (!peWallHit)
                {
                    var peBounds2 = pe.Bounds;
                    foreach (var qb in questionBlocks)
                    {
                        var br = new Rectangle(qb.Position.X, qb.Position.Y, qb.Visual.Width, qb.Visual.Height);
                        if (!peBounds2.IntersectsWith(br)) continue;
                        int qot = peBounds2.Bottom - br.Top;
                        int qob = br.Bottom - peBounds2.Top;
                        int qol = peBounds2.Right - br.Left;
                        int qorr = br.Right - peBounds2.Left;
                        int qmin = Math.Min(Math.Min(qot, qob), Math.Min(qol, qorr));
                        if ((qmin == qol || qmin == qorr) && qmin < qot)
                        { pe.ReverseDirection(); peWallHit = true; break; }
                    }
                }

                // Edge detection – skip if wall collision already reversed direction this frame
                if (peGrounded && !peWallHit)
                {
                    const int probeW = 10;
                    int probeX = pe.Direction > 0
                        ? pe.Position.X + pe.Visual.Width
                        : pe.Position.X - probeW;
                    int probeY  = pe.Position.Y + pe.Visual.Height + 4;
                    var probe   = new Rectangle(probeX, probeY, probeW, 6);
                    bool groundAhead = false;
                    foreach (var plat in platforms)
                    {
                        var pr = new Rectangle(
                            plat.Position.X, plat.Position.Y,
                            plat.PictureBox.Width, plat.PictureBox.Height);
                        if (probe.IntersectsWith(pr)) { groundAhead = true; break; }
                    }
                    if (!groundAhead) pe.ReverseDirection();
                }

                pe.Update();
                pe.Visual.Location = new Point(pe.Position.X - cameraX, pe.Position.Y);

                if (isDying) continue;
                var peWorld = new Rectangle(pe.Position.X, pe.Position.Y, pe.Visual.Width, pe.Visual.Height);
                if (!playerRect.IntersectsWith(peWorld)) continue;

                int pBot2 = player.Position.Y + picboxplayer.Height;
                bool fall2 = pBot2 - pe.Position.Y < 24;
                bool abv2  = player.Position.Y < pe.Position.Y + pe.Visual.Height / 2;

                if (fall2 && abv2 && player.VerticalVelocity >= 0)
                {
                    pe.Squish();
                    player.Bounce();
                    player.Score += 175;
                }
                else
                {
                    HitByEnemy();
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  FLYING ENEMY SPAWNING & UPDATE
        // ════════════════════════════════════════════════════════════════════
        private EnemyDef[] GetCurrentLevelFlying()
        {
            switch (currentLevelNumber)
            {
                case 1: return LEVEL_1_FLYING;
                case 2: return LEVEL_2_FLYING;
                case 3: return LEVEL_3_FLYING;
                default:
                    return new EnemyDef[] {
                        new EnemyDef(1000, 400), new EnemyDef(1800, 395), new EnemyDef(2500, 390),
                    };
            }
        }

        private void SpawnFlyingEnemies()
        {
            foreach (var def in GetCurrentLevelFlying())
            {
                var fl = new FlyingEnemy(new Point(def.X, def.Y));
                Controls.Add(fl.Visual);
                flyingEnemies.Add(fl);
            }
        }

        private void UpdateFlyingEnemies()
        {
            var playerRect = new Rectangle(
                player.Position.X, player.Position.Y,
                picboxplayer.Width, picboxplayer.Height);

            for (int i = flyingEnemies.Count - 1; i >= 0; i--)
            {
                var fl = flyingEnemies[i];

                if (!fl.IsAlive)
                {
                    Controls.Remove(fl.Visual);
                    fl.Visual.Dispose();
                    flyingEnemies.RemoveAt(i);
                    continue;
                }

                if (fl.Position.Y > 620)
                {
                    Controls.Remove(fl.Visual);
                    fl.Visual.Dispose();
                    flyingEnemies.RemoveAt(i);
                    continue;
                }

                // Squished enemies need no physics; tick timer and skip
                if (fl.IsSquished)
                {
                    if (fl.UpdateSquish(FIXED_STEP_MS)) fl.Kill();
                    fl.Visual.Location = new Point(fl.Position.X - cameraX, fl.Position.Y);
                    continue;
                }

                if (fl.HasWings)
                {
                    // Sine-wave flight – Update() manages both axes
                    fl.Update();
                    fl.Visual.Location = new Point(fl.Position.X - cameraX, fl.Position.Y);
                }
                else
                {
                    // Wings stripped – apply gravity and platform collision
                    if (!fl.IsGrounded)
                    {
                        fl.VerticalVelocity += 0.6f;
                        if (fl.VerticalVelocity > 15f) fl.VerticalVelocity = 15f;
                        fl.Position = new Point(fl.Position.X, fl.Position.Y + (int)Math.Round(fl.VerticalVelocity));
                    }

                    if (fl.Position.Y > 600) { fl.Kill(); continue; }

                    bool flGrounded = false;
                    bool flWallHit = false;
                    var flRect = fl.Bounds;
                    foreach (var plat in platforms)
                    {
                        var pr = new Rectangle(
                            plat.Position.X, plat.Position.Y,
                            plat.PictureBox.Width, plat.PictureBox.Height);
                        if (!flRect.IntersectsWith(pr)) continue;

                        int ot = flRect.Bottom - pr.Top, ob = pr.Bottom - flRect.Top;
                        int ol = flRect.Right - pr.Left, orr = pr.Right - flRect.Left;
                        int min = Math.Min(Math.Min(ot, ob), Math.Min(ol, orr));

                        if (min == ot && ot < 30)
                        {
                            fl.Position = new Point(fl.Position.X, pr.Top - fl.Visual.Height);
                            fl.VerticalVelocity = 0;
                            flGrounded = true;
                            break;
                        }
                        else if (min == ol || min == orr)
                        {
                            if (!flWallHit) { fl.ReverseDirection(); flWallHit = true; }
                            continue;
                        }
                    }
                    fl.IsGrounded = flGrounded;

                    // Question block wall collision (no-wings mode)
                    var flBounds2 = fl.Bounds;
                    foreach (var qb in questionBlocks)
                    {
                        var br = new Rectangle(qb.Position.X, qb.Position.Y, qb.Visual.Width, qb.Visual.Height);
                        if (!flBounds2.IntersectsWith(br)) continue;
                        int qot = flBounds2.Bottom - br.Top;
                        int qob = br.Bottom - flBounds2.Top;
                        int qol = flBounds2.Right - br.Left;
                        int qorr = br.Right - flBounds2.Left;
                        int qmin = Math.Min(Math.Min(qot, qob), Math.Min(qol, qorr));
                        if ((qmin == qol || qmin == qorr) && qmin < qot)
                        { fl.ReverseDirection(); break; }
                    }

                    fl.Update();
                    fl.Visual.Location = new Point(fl.Position.X - cameraX, fl.Position.Y);
                }

                if (isDying) continue;
                var flWorld = new Rectangle(fl.Position.X, fl.Position.Y, fl.Visual.Width, fl.Visual.Height);
                if (!playerRect.IntersectsWith(flWorld)) continue;

                int pBot3 = player.Position.Y + picboxplayer.Height;
                bool fall3 = pBot3 - fl.Position.Y < 24;
                bool abv3  = player.Position.Y < fl.Position.Y + fl.Visual.Height / 2;

                if (fall3 && abv3 && player.VerticalVelocity >= 0)
                {
                    bool hadWings = fl.HasWings;
                    fl.Stomp();
                    player.Bounce();
                    player.Score += hadWings ? 200 : 300;
                }
                else
                {
                    HitByEnemy();
                }
            }
        }
    }
}
