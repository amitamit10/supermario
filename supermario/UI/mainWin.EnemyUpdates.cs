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
            var playerRect = new Rectangle(player.Position.X, player.Position.Y,
                                           picboxplayer.Width, picboxplayer.Height);
            var platRects  = PlatformRects();
            var blockRects = BlockRects();

            for (int i = jumpingEnemies.Count - 1; i >= 0; i--)
            {
                var je = jumpingEnemies[i];

                if (!je.IsAlive)                     { RemoveEnemy(jumpingEnemies, i); continue; }
                if (je.Position.Y > ENEMY_DESPAWN_Y) { RemoveEnemy(jumpingEnemies, i); continue; }

                if (je.IsSquished)
                {
                    if (je.UpdateSquish(FIXED_STEP_MS)) je.Kill();
                    SyncToScreen(je);
                    continue;
                }

                je.ApplyGravity();
                if (je.Position.Y > ENEMY_FELL_Y) { RemoveEnemy(jumpingEnemies, i); continue; }

                // הקופץ יכול לחבוט בתקרה (allowCeiling). בדיקת רגליים נוספת מאשרת עמידה
                // על קצה פלטפורמה ומונעת הבהוב IsGrounded שמחצי את קצב הקפיצה.
                // The jumper can bonk ceilings (allowCeiling). An extra feet probe confirms
                // footing on a platform edge, stopping IsGrounded flicker that halves the jump rate.
                bool grounded = je.ResolvePlatformCollisions(platRects, allowCeiling: true, out _);
                if (!grounded && je.VerticalVelocity >= 0 && je.HasGroundBeneath(platRects)) grounded = true;
                je.IsGrounded = grounded;

                je.ResolveBlockCollisions(blockRects, allowCeiling: true);

                je.Update();
                SyncToScreen(je);

                if (isDying) continue;
                if (!playerRect.IntersectsWith(je.Bounds)) continue;

                if (PlayerStomps(je)) { je.Squish(); player.Bounce(); player.Score += 150; }
                else                  { HitByEnemy(); }
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
            var playerRect = new Rectangle(player.Position.X, player.Position.Y,
                                           picboxplayer.Width, picboxplayer.Height);
            var platRects  = PlatformRects();
            var blockRects = BlockRects();

            for (int i = patrolEnemies.Count - 1; i >= 0; i--)
            {
                var pe = patrolEnemies[i];

                if (!pe.IsAlive)                     { RemoveEnemy(patrolEnemies, i); continue; }
                if (pe.Position.Y > ENEMY_DESPAWN_Y) { RemoveEnemy(patrolEnemies, i); continue; }

                if (pe.IsSquished)
                {
                    if (pe.UpdateSquish(FIXED_STEP_MS)) pe.Kill();
                    SyncToScreen(pe);
                    continue;
                }

                pe.ApplyGravity();
                if (pe.Position.Y > ENEMY_FELL_Y) { RemoveEnemy(patrolEnemies, i); continue; }

                bool wallHit;
                bool grounded = pe.ResolvePlatformCollisions(platRects, allowCeiling: false, out wallHit);
                pe.IsGrounded = grounded;

                // בלוקי-שאלה: לולאה ייחודית לפטרול — מגודרת ב-wallHit ומעדכנת אותו (שלא כמו העוזר הגנרי).
                // Question blocks: patrol-specific loop — gated by wallHit and updates it (unlike the generic helper).
                if (!wallHit)
                {
                    var b = pe.Bounds;
                    foreach (var br in blockRects)
                    {
                        if (!b.IntersectsWith(br)) continue;
                        int qot = b.Bottom - br.Top, qob = br.Bottom - b.Top;
                        int qol = b.Right - br.Left, qorr = br.Right - b.Left;
                        int qmin = Math.Min(Math.Min(qot, qob), Math.Min(qol, qorr));
                        if ((qmin == qol || qmin == qorr) && qmin < qot)
                        { pe.ReverseDirection(); wallHit = true; break; }
                    }
                }

                // זיהוי קצה מדף — מדלגים אם פגיעת קיר כבר הפכה כיוון השנייה.
                // Ledge-edge turn — skipped if a wall hit already reversed direction this frame.
                if (grounded && !wallHit && pe.IsAtLedgeEdge(platRects)) pe.ReverseDirection();

                pe.Update();
                SyncToScreen(pe);

                if (isDying) continue;
                if (!playerRect.IntersectsWith(pe.Bounds)) continue;

                if (PlayerStomps(pe)) { pe.Squish(); player.Bounce(); player.Score += 175; }
                else                  { HitByEnemy(); }
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
            var playerRect = new Rectangle(player.Position.X, player.Position.Y,
                                           picboxplayer.Width, picboxplayer.Height);
            var platRects  = PlatformRects();
            var blockRects = BlockRects();

            for (int i = flyingEnemies.Count - 1; i >= 0; i--)
            {
                var fl = flyingEnemies[i];

                if (!fl.IsAlive)                     { RemoveEnemy(flyingEnemies, i); continue; }
                if (fl.Position.Y > ENEMY_DESPAWN_Y) { RemoveEnemy(flyingEnemies, i); continue; }

                if (fl.IsSquished)
                {
                    if (fl.UpdateSquish(FIXED_STEP_MS)) fl.Kill();
                    SyncToScreen(fl);
                    continue;
                }

                if (fl.HasWings)
                {
                    // תעופה בגל סינוס — Update() מנהל את שני הצירים / sine flight; Update() drives both axes
                    fl.Update();
                    SyncToScreen(fl);
                }
                else
                {
                    // כנפיים הוסרו — כבידה והתנגשויות כמו אויב רגיל / wings stripped — gravity + collisions
                    fl.ApplyGravity();
                    if (fl.Position.Y > ENEMY_FELL_Y) { RemoveEnemy(flyingEnemies, i); continue; }

                    fl.IsGrounded = fl.ResolvePlatformCollisions(platRects, allowCeiling: false, out _);
                    fl.ResolveBlockCollisions(blockRects, allowCeiling: false);

                    fl.Update();
                    SyncToScreen(fl);
                }

                if (isDying) continue;
                if (!playerRect.IntersectsWith(fl.Bounds)) continue;

                // דריכה: ניקוד תלוי אם עוד היו כנפיים — לתפוס לפני Stomp() שמסיר אותן.
                // Stomp: score depends on whether it still had wings — capture before Stomp() strips them.
                if (PlayerStomps(fl))
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
