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
        //  LEVEL BUILDING
        // ════════════════════════════════════════════════════════════════════
        private void CreateLongLevel()
        {
            CreateBrickGround();
            ClearPowerUps();
            foreach (var p in currentLevel)
                AddPlatform(p.X, p.Y, p.Width, p.Height);
            AddPipes();
            AddQuestionBlocks();
            AddCoins();
            AddFinishFlagpole();
            SpawnGoombas();
            SpawnKoopas();
            SpawnFastEnemies();
            SpawnJumpingEnemies();
            SpawnPatrolEnemies();
            SpawnFlyingEnemies();
            picboxplayer.BringToFront();
            _hudLabel?.BringToFront();
            _scoreLabel?.BringToFront();
            foreach (var lbl in _heartLabels) lbl?.BringToFront();
        }

        private void CreateBrickGround()
        {
            for (int x = 0; x < 3000; x += 40)
            {
                var brick = new PictureBox
                {
                    Size = new Size(40, 40),
                    Location = new Point(x, 513),
                    BackColor = Color.Transparent,
                };
                brick.Paint += DrawGroundBrick;
                Controls.Add(brick);
                brick.SendToBack();
                platforms.Add(new GameObjectS(brick, brick.Location, "ground"));
            }
        }

        private static void DrawGroundBrick(object sender, PaintEventArgs e)
        {
            var pb = (PictureBox)sender;
            var g = e.Graphics;
            int w = pb.Width, h = pb.Height;
            using (var fill = new SolidBrush(Color.FromArgb(185, 100, 40)))
                g.FillRectangle(fill, 0, 0, w, h);
            using (var mortar = new Pen(Color.FromArgb(120, 60, 15), 2f))
            {
                g.DrawLine(mortar, 0, h / 2, w, h / 2);
                g.DrawLine(mortar, w / 2, 0, w / 2, h / 2);
                g.DrawLine(mortar, w / 4, h / 2, w / 4, h);
                g.DrawLine(mortar, 3 * w / 4, h / 2, 3 * w / 4, h);
            }
            using (var hi1 = new SolidBrush(Color.FromArgb(70, 255, 210, 150)))
                g.FillRectangle(hi1, 0, 0, w, 3);
            using (var hi2 = new SolidBrush(Color.FromArgb(40, 255, 210, 150)))
                g.FillRectangle(hi2, 0, 0, 2, h);
            using (var border = new Pen(Color.FromArgb(90, 40, 5), 1f))
                g.DrawRectangle(border, 0, 0, w - 1, h - 1);
        }

        private void AddPlatform(int x, int y, int w, int h)
        {
            var p = new PictureBox
            {
                Size = new Size(w, h),
                Location = new Point(x, y),
                BackColor = Color.Transparent,
            };
            p.Paint += DrawPlatformTile;
            Controls.Add(p);
            p.SendToBack();
            platforms.Add(new GameObjectS(p, p.Location, "platform"));
        }

        private static void DrawPlatformTile(object sender, PaintEventArgs e)
        {
            var pb = (PictureBox)sender;
            var g = e.Graphics;
            int w = pb.Width, h = pb.Height;
            using (var fill = new SolidBrush(Color.FromArgb(210, 140, 65)))
                g.FillRectangle(fill, 0, 0, w, h);
            int bw = 40, bh = 20;
            using (var mortar = new Pen(Color.FromArgb(150, 85, 25), 1.5f))
            {
                for (int by = 0; by < h; by += bh)
                {
                    g.DrawLine(mortar, 0, by, w, by);
                    int offset = ((by / bh) % 2 == 0) ? 0 : bw / 2;
                    for (int bx = offset; bx < w + bw; bx += bw)
                        g.DrawLine(mortar, bx, by, bx, Math.Min(by + bh, h));
                }
            }
            using (var hi = new SolidBrush(Color.FromArgb(80, 255, 220, 160)))
                g.FillRectangle(hi, 0, 0, w, 4);
            using (var border = new Pen(Color.FromArgb(110, 60, 10), 2f))
                g.DrawRectangle(border, 1, 1, w - 2, h - 2);
        }

        // ════════════════════════════════════════════════════════════════════
        //  PIPES
        // ════════════════════════════════════════════════════════════════════
        private PlatformData[] GetCurrentLevelPipes()
        {
            switch (currentLevelNumber)
            {
                case 1: return LEVEL_1_PIPES;
                case 2: return LEVEL_2_PIPES;
                case 3: return LEVEL_3_PIPES;
                default: return new PlatformData[0];
            }
        }

        private void AddPipes()
        {
            foreach (var p in GetCurrentLevelPipes())
                AddPipe(p.X, p.Y, p.Width, p.Height);
        }

        private void AddPipe(int x, int y, int w, int h)
        {
            var p = new PictureBox
            {
                Size = new Size(w, h),
                Location = new Point(x, y),
                BackColor = Color.Transparent,
            };
            p.Paint += DrawPipeTile;
            Controls.Add(p);
            p.SendToBack();
            platforms.Add(new GameObjectS(p, p.Location, "pipe"));
        }

        private static void DrawPipeTile(object sender, PaintEventArgs e)
        {
            var pb = (PictureBox)sender;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = pb.Width, h = pb.Height;

            // ── Pipe body (slightly inset from rim) ──────────────────────────
            int bx = 8, bw2 = w - 16;
            using (var body = new LinearGradientBrush(
                new Point(bx, 0), new Point(bx + bw2, 0),
                Color.FromArgb(40, 160, 40), Color.FromArgb(15, 85, 15)))
                g.FillRectangle(body, bx, 22, bw2, h - 22);
            // Body highlight stripe
            using (var hi = new SolidBrush(Color.FromArgb(70, 140, 220, 140)))
                g.FillRectangle(hi, bx + 2, 24, 10, h - 26);
            // Body border
            using (var bd = new Pen(Color.FromArgb(10, 60, 10), 2f))
                g.DrawRectangle(bd, bx, 22, bw2 - 1, h - 23);

            // ── Pipe rim / head (full width, first 22 px) ────────────────────
            using (var rim = new LinearGradientBrush(
                new Point(0, 0), new Point(w, 0),
                Color.FromArgb(70, 185, 55), Color.FromArgb(20, 100, 15)))
                g.FillRectangle(rim, 0, 0, w, 22);
            // Rim top highlight
            using (var rh = new SolidBrush(Color.FromArgb(90, 160, 230, 160)))
                g.FillRectangle(rh, 3, 3, 18, 8);
            // Rim border
            using (var rb = new Pen(Color.FromArgb(10, 60, 10), 2f))
                g.DrawRectangle(rb, 0, 0, w - 1, 21);
        }

        private void AddFinishFlagpole()
        {
            var flag = new PictureBox
            {
                Size = new Size(80, 200),
                Location = new Point(2750, 313),
                BackColor = Color.Transparent,
            };
            flag.Paint += DrawFlagpole;
            Controls.Add(flag);
            flag.SendToBack();
            platforms.Add(new GameObjectS(flag, flag.Location, "finish"));
        }

        private void DrawFlagpole(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int h = 200;
            using (var lg = new LinearGradientBrush(new Point(30, 0), new Point(46, 0),
                Color.FromArgb(200, 200, 210), Color.FromArgb(90, 90, 110)))
                g.FillRectangle(lg, 34, 0, 12, h);
            using (var poleSheen = new SolidBrush(Color.FromArgb(80, 255, 255, 255)))
                g.FillRectangle(poleSheen, 36, 0, 3, h);
            using (var ball = new SolidBrush(Color.FromArgb(255, 210, 30)))
                g.FillEllipse(ball, 26, 0, 28, 28);
            using (var ballSheen = new SolidBrush(Color.FromArgb(180, 255, 255, 150)))
                g.FillEllipse(ballSheen, 30, 3, 10, 10);
            var flagPts = new PointF[] { new PointF(46, 18), new PointF(46, 70), new PointF(78, 44) };
            using (var flagGreen = new SolidBrush(Color.FromArgb(60, 180, 60)))
                g.FillPolygon(flagGreen, flagPts);
            using (var flagSheen = new SolidBrush(Color.FromArgb(80, 255, 80, 80)))
                g.FillPolygon(flagSheen, new PointF[] { new PointF(48, 20), new PointF(58, 30), new PointF(50, 38) });
            using (var base1 = new SolidBrush(Color.FromArgb(80, 80, 90)))
                g.FillRectangle(base1, 14, h - 30, 50, 30);
            using (var base2 = new SolidBrush(Color.FromArgb(110, 110, 125)))
                g.FillRectangle(base2, 14, h - 30, 50, 8);
            using (var borderPen = new Pen(Color.FromArgb(50, 50, 60), 2f))
                g.DrawRectangle(borderPen, 14, h - 30, 50, 30);
            using (var f = new Font("Courier New", 7f, FontStyle.Bold))
            using (var b = new SolidBrush(Color.White))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString("GOAL", f, b, new RectangleF(14, h - 22, 50, 14), sf);
        }

        // ════════════════════════════════════════════════════════════════════
        //  QUESTION BLOCKS
        // ════════════════════════════════════════════════════════════════════
        private QBlockDef[] GetCurrentLevelQBlocks()
        {
            switch (currentLevelNumber)
            {
                case 1: return LEVEL_1_QBLOCKS;
                case 2: return LEVEL_2_QBLOCKS;
                case 3: return LEVEL_3_QBLOCKS;
                default:
                    return new QBlockDef[] {
                        new QBlockDef(400, 350, PowerUpType.Mushroom), new QBlockDef(700, 380, PowerUpType.Coin),
                        new QBlockDef(950, 320, PowerUpType.Mushroom), new QBlockDef(1250, 360, PowerUpType.Coin),
                        new QBlockDef(1600, 290, PowerUpType.Mushroom), new QBlockDef(1900, 340, PowerUpType.Coin),
                        new QBlockDef(2200, 370, PowerUpType.Mushroom), new QBlockDef(2500, 310, PowerUpType.Coin),
                    };
            }
        }

        private void AddQuestionBlocks()
        {
            foreach (var def in GetCurrentLevelQBlocks())
            {
                var box = new PictureBox
                {
                    Size = new Size(50, 50),
                    Location = new Point(def.X, def.Y),
                    BackColor = Color.Transparent,
                };

                var block = new QuestionBlock(box.Location, box, null, def.Type);

                box.Paint += (s, pe) =>
                {
                    var g = pe.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    int bw = box.Width, bh = box.Height;

                    if (block.IsHit)
                    {
                        using (var fill = new SolidBrush(Color.FromArgb(181, 116, 56)))
                            g.FillRectangle(fill, 0, 0, bw, bh);
                        using (var mortar = new Pen(Color.FromArgb(100, 60, 20), 2f))
                        {
                            g.DrawLine(mortar, 0, bh / 2, bw, bh / 2);
                            g.DrawLine(mortar, bw / 4, 0, bw / 4, bh / 2);
                            g.DrawLine(mortar, 3 * bw / 4, bh / 2, 3 * bw / 4, bh);
                        }
                        using (var border = new Pen(Color.FromArgb(80, 40, 10), 3f))
                            g.DrawRectangle(border, 1, 1, bw - 3, bh - 3);
                    }
                    else
                    {
                        Color[] palette = {
                            Color.FromArgb(255, 210,   0), Color.FromArgb(255, 230,  60),
                            Color.FromArgb(255, 255, 100), Color.FromArgb(255, 230,  60),
                            Color.FromArgb(255, 210,   0), Color.FromArgb(220, 160,   0),
                        };
                        using (var fill = new SolidBrush(palette[questionAnimFrame % 6]))
                            g.FillRectangle(fill, 0, 0, bw, bh);
                        using (var shine = new SolidBrush(Color.FromArgb(100, 255, 255, 200)))
                            g.FillRectangle(shine, 3, 3, bw - 6, 6);
                        using (var border = new Pen(Color.FromArgb(140, 80, 0), 3f))
                            g.DrawRectangle(border, 1, 1, bw - 3, bh - 3);
                        int qOff = (questionAnimFrame % 2 == 0) ? 0 : -2;
                        string sym = block.PowerUpInside == PowerUpType.Coin ? "C" : "?";
                        using (var qFont = new Font("Arial", 22, FontStyle.Bold))
                        using (var qBrush = new SolidBrush(Color.FromArgb(100, 50, 0)))
                        using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                            g.DrawString(sym, qFont, qBrush, new RectangleF(0, qOff, bw, bh), sf);
                    }
                };

                Controls.Add(box);
                box.SendToBack();
                animatedBlocks.Add(box);
                questionBlocks.Add(block);
            }
        }

        private void ClearPowerUps()
        {
            foreach (var b in questionBlocks)
            {
                if (b.Visual != null) { animatedBlocks.Remove(b.Visual); Controls.Remove(b.Visual); b.Visual.Dispose(); }
                if (b.QuestionLabel != null) { Controls.Remove(b.QuestionLabel); b.QuestionLabel.Dispose(); }
            }
            questionBlocks.Clear();

            foreach (var m in spawnedMushrooms)
            {
                if (m.Visual != null) { Controls.Remove(m.Visual); m.Visual.Dispose(); }
            }
            spawnedMushrooms.Clear();
        }

        // ════════════════════════════════════════════════════════════════════
        //  RANDOM LEVEL GENERATOR
        // ════════════════════════════════════════════════════════════════════
        private PlatformData[] GenerateRandomLevel(int numSections)
        {
            // Opening sections feel grounded; mid sections add challenge; closing ends near flag
            var openingPool = new[] { SECTION_LONG_RUN, SECTION_BRIDGE, SECTION_GENTLE_HOP, SECTION_STAIRS };
            var midPool = new[] {
                SECTION_GAP_JUMPS, SECTION_WAVE, SECTION_ZIGZAG, SECTION_STAIR_UP,
                SECTION_WIDE_GAPS, SECTION_VALLEY, SECTION_PYRAMID, SECTION_DOUBLE_GAP,
                SECTION_CHALLENGE, SECTION_MULTI_LEVEL, SECTION_CASTLE, SECTION_TRIPLE_JUMP,
                SECTION_BATTLEMENTS, SECTION_BIG_GAP, SECTION_DESCENT_STAIRS,
            };
            var hardPool = new[] {
                SECTION_HIGH, SECTION_ARCH, SECTION_SUSPENDED, SECTION_LEDGE_HOP,
                SECTION_DESCEND, SECTION_CLOUD_WALK,
            };

            List<PlatformData> result;
            int extra = 0;
            do
            {
                result = new List<PlatformData>();
                int xOff = 200, yBase = 483;
                int total = numSections + extra;
                int prevSectionIdx = -1;

                for (int i = 0; i < total; i++)
                {
                    PlatformData[][] pool;
                    if (i == 0)                              pool = new[] { openingPool[levelRandom.Next(openingPool.Length)] };
                    else if (i < total * 2 / 3)             pool = new[] { midPool[levelRandom.Next(midPool.Length)] };
                    else                                     pool = new[] { hardPool[levelRandom.Next(hardPool.Length)] };

                    // Avoid repeating same section type two consecutive times
                    int tries = 0;
                    var sec = pool[0];
                    while (tries < 3 && Array.IndexOf(ALL_SECTIONS, sec) == prevSectionIdx)
                    {
                        pool = new[] { midPool[levelRandom.Next(midPool.Length)] };
                        sec = pool[0];
                        tries++;
                    }
                    prevSectionIdx = Array.IndexOf(ALL_SECTIONS, sec);

                    foreach (var p in sec)
                    {
                        int ny = yBase + p.Y;
                        if (ny >= 250 && ny <= 483)
                            result.Add(new PlatformData(xOff + p.X, ny, p.Width, p.Height));
                    }
                    if (sec.Length > 0)
                        xOff += sec.Max(p => p.X + p.Width) + 130;
                }

                extra++;
            } while (result.Count < 8 && extra < 20);

            return result.ToArray();
        }

    }
}
