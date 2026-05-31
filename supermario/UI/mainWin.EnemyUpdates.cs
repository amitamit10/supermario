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
            var playerRect = new Rectangle(player.Position.X, player.Position.Y,
                                           picboxplayer.Width, picboxplayer.Height);
            var platRects  = PlatformRects();
            var blockRects = BlockRects();

            for (int i = goombas.Count - 1; i >= 0; i--)
            {
                var g = goombas[i];

                if (!g.IsAlive)                     { RemoveEnemy(goombas, i); continue; }
                if (g.Position.Y > ENEMY_DESPAWN_Y) { RemoveEnemy(goombas, i); continue; }

                // אויב מעוך: רק סופר זמן עד היעלמות / squished: just tick the timer
                if (g.IsSquished)
                {
                    if (g.UpdateSquish(FIXED_STEP_MS)) g.Kill();
                    SyncToScreen(g);
                    continue;
                }

                g.ApplyGravity();
                if (g.Position.Y > ENEMY_FELL_Y) { RemoveEnemy(goombas, i); continue; }

                g.IsGrounded = g.ResolvePlatformCollisions(platRects, allowCeiling: false, out _);
                g.ResolveBlockCollisions(blockRects, allowCeiling: false);

                g.Update();
                SyncToScreen(g);

                // התנגשות עם השחקן: דריכה מלמעלה מועכת; אחרת השחקן נפגע.
                // Player collision: a stomp from above squishes; otherwise the player is hurt.
                if (isDying) continue;
                if (!playerRect.IntersectsWith(g.Bounds)) continue;

                if (PlayerStomps(g)) { g.Squish(); player.Bounce(); player.Score += 100; }
                else                 { HitByEnemy(); }
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
            var playerRect = new Rectangle(player.Position.X, player.Position.Y,
                                           picboxplayer.Width, picboxplayer.Height);
            var platRects  = PlatformRects();
            var blockRects = BlockRects();

            for (int i = koopas.Count - 1; i >= 0; i--)
            {
                var k = koopas[i];

                if (!k.IsAlive)                     { RemoveEnemy(koopas, i); continue; }
                if (k.Position.Y > ENEMY_DESPAWN_Y) { RemoveEnemy(koopas, i); continue; }

                // מצב קליפה: ללא פיזיקה — רק טיימר; הליכה לתוך הקליפה בועטת בה לנקודות.
                // טיפול בהתנגשות השחקן כאן (לא בענף ה"מת") כי הקליפה היא מצב חי.
                // Shell state: no physics, just the timer; walking into it kicks it for points.
                // Player collision handled here (not the dead branch) since the shell is alive.
                if (k.IsShell)
                {
                    if (k.UpdateShell(FIXED_STEP_MS)) k.Kill();
                    SyncToScreen(k);
                    if (!isDying && playerRect.IntersectsWith(k.Bounds))
                    {
                        k.Stomp();              // דריכה שנייה על הקליפה הורגת / second stomp kills
                        player.Score += 50;
                    }
                    continue;
                }

                k.ApplyGravity();
                if (k.Position.Y > ENEMY_FELL_Y) { RemoveEnemy(koopas, i); continue; }

                k.IsGrounded = k.ResolvePlatformCollisions(platRects, allowCeiling: false, out _);
                k.ResolveBlockCollisions(blockRects, allowCeiling: false);

                k.Update();
                SyncToScreen(k);

                if (isDying) continue;
                if (!playerRect.IntersectsWith(k.Bounds)) continue;

                if (PlayerStomps(k)) { k.Stomp(); player.Bounce(); player.Score += 150; }
                else                 { HitByEnemy(); }
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
            var playerRect = new Rectangle(player.Position.X, player.Position.Y,
                                           picboxplayer.Width, picboxplayer.Height);
            var platRects  = PlatformRects();
            var blockRects = BlockRects();

            for (int i = fastEnemies.Count - 1; i >= 0; i--)
            {
                var fe = fastEnemies[i];

                if (!fe.IsAlive)                     { RemoveEnemy(fastEnemies, i); continue; }
                if (fe.Position.Y > ENEMY_DESPAWN_Y) { RemoveEnemy(fastEnemies, i); continue; }

                if (fe.IsSquished)
                {
                    if (fe.UpdateSquish(FIXED_STEP_MS)) fe.Kill();
                    SyncToScreen(fe);
                    continue;
                }

                fe.ApplyGravity();
                if (fe.Position.Y > ENEMY_FELL_Y) { RemoveEnemy(fastEnemies, i); continue; }

                fe.IsGrounded = fe.ResolvePlatformCollisions(platRects, allowCeiling: false, out _);
                fe.ResolveBlockCollisions(blockRects, allowCeiling: false);

                fe.Update();
                SyncToScreen(fe);

                if (isDying) continue;
                if (!playerRect.IntersectsWith(fe.Bounds)) continue;

                if (PlayerStomps(fe)) { fe.Squish(); player.Bounce(); player.Score += 200; }
                else                  { HitByEnemy(); }
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

                if (je.Position.Y > 600)
                {
                    Controls.Remove(je.Visual);
                    je.Visual.Dispose();
                    jumpingEnemies.RemoveAt(i);
                    continue;
                }

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
                    else if (min == ob && je.VerticalVelocity >= 0)
                    {
                        // Fell past the platform in one frame – snap up.
                        je.Position = new Point(je.Position.X, pr.Top - je.Visual.Height);
                        je.VerticalVelocity = 0;
                        jeGrounded = true;
                        break;
                    }
                    else if (min == ol || min == orr)
                    {
                        if (!jeWallHit)
                        {
                            if (min == ol)
                                je.Position = new Point(pr.Left - je.Visual.Width, je.Position.Y);
                            else
                                je.Position = new Point(pr.Right, je.Position.Y);
                            je.ReverseDirection();
                            jeWallHit = true;
                            jeRect = je.Bounds;
                        }
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

                if (pe.Position.Y > 600)
                {
                    Controls.Remove(pe.Visual);
                    pe.Visual.Dispose();
                    patrolEnemies.RemoveAt(i);
                    continue;
                }

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
                    else if (min == ob && pe.VerticalVelocity >= 0)
                    {
                        pe.Position = new Point(pe.Position.X, pr.Top - pe.Visual.Height);
                        pe.VerticalVelocity = 0;
                        peGrounded = true;
                        break;
                    }
                    else if (min == ol || min == orr)
                    {
                        if (!peWallHit)
                        {
                            if (min == ol)
                                pe.Position = new Point(pr.Left - pe.Visual.Width, pe.Position.Y);
                            else
                                pe.Position = new Point(pr.Right, pe.Position.Y);
                            pe.ReverseDirection();
                            peWallHit = true;
                            peRect = pe.Bounds;
                        }
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

                    if (fl.Position.Y > 600)
                    {
                        Controls.Remove(fl.Visual);
                        fl.Visual.Dispose();
                        flyingEnemies.RemoveAt(i);
                        continue;
                    }

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
                        else if (min == ob && fl.VerticalVelocity >= 0)
                        {
                            fl.Position = new Point(fl.Position.X, pr.Top - fl.Visual.Height);
                            fl.VerticalVelocity = 0;
                            flGrounded = true;
                            break;
                        }
                        else if (min == ol || min == orr)
                        {
                            if (!flWallHit)
                            {
                                if (min == ol)
                                    fl.Position = new Point(pr.Left - fl.Visual.Width, fl.Position.Y);
                                else
                                    fl.Position = new Point(pr.Right, fl.Position.Y);
                                fl.ReverseDirection();
                                flWallHit = true;
                                flRect = fl.Bounds;
                            }
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
