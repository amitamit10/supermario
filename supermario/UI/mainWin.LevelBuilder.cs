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
            foreach (var heart in _hearts) heart?.BringToFront();
        }

        private void CreateBrickGround()
        {
            var ground = new PictureBox
            {
                Size = new Size(LEVEL_PIXEL_WIDTH, 40),
                Location = new Point(0, GROUND_TOP_Y),
                BackColor = Color.FromArgb(185, 100, 40),
            };
            ground.BackgroundImage = Sprites.Brick;            // לבנים פרוסות / tiled bricks
            ground.BackgroundImageLayout = ImageLayout.Tile;
            Controls.Add(ground);
            ground.SendToBack();
            platforms.Add(new GameObjectS(ground, ground.Location, "ground"));
        }

        // הקרקע מצוירת ע"י BackgroundImage פרוס (Sprites.Brick) — אין ציור GDI+.
        // The ground is drawn by a tiled BackgroundImage — no GDI+ drawing.

        private void AddPlatform(int x, int y, int w, int h)
        {
            var p = new PictureBox
            {
                Size = new Size(w, h),
                Location = new Point(x, y),
                BackColor = Color.FromArgb(210, 140, 65),
            };
            p.BackgroundImage = Sprites.Brick;                 // לבנים פרוסות / tiled bricks
            p.BackgroundImageLayout = ImageLayout.Tile;
            Controls.Add(p);
            p.SendToBack();
            platforms.Add(new GameObjectS(p, p.Location, "platform"));
        }

        // פלטפורמות מצוירות ע"י BackgroundImage פרוס (Sprites.Brick) — אין ציור GDI+.
        // Platforms are drawn by a tiled BackgroundImage — no GDI+ drawing.

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
            p.SizeMode = PictureBoxSizeMode.StretchImage;
            p.Image = Sprites.Pipe;
            Controls.Add(p);
            p.SendToBack();
            platforms.Add(new GameObjectS(p, p.Location, "pipe"));
        }

        // הצינור מצויר כתמונה (Sprites.Pipe) הנמתחת לגודל התיבה — אין ציור GDI+.
        // The pipe is drawn as a stretched image — no GDI+ drawing.

        private void AddFinishFlagpole()
        {
            var flag = new PictureBox
            {
                Size = new Size(80, 200),
                Location = new Point(2750, 313),
                BackColor = Color.Transparent,
            };
            flag.SizeMode = PictureBoxSizeMode.StretchImage;
            flag.Image = Sprites.Flag;
            Controls.Add(flag);
            flag.SendToBack();
            platforms.Add(new GameObjectS(flag, flag.Location, "finish"));
        }

        // עמוד הדגל מצויר כתמונה (Sprites.Flag) הנמתחת לגודל התיבה — אין ציור GDI+.
        // The goal flagpole is drawn as a stretched image — no GDI+ drawing.

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

                // בלוק השאלה מצויר כתמונה; ההבהוב והמעבר ל"ריק" ב-UpdateAnimatedSprites.
                // The question block is drawn as an image; its blink / "hit" swap happens in UpdateAnimatedSprites.
                box.SizeMode = PictureBoxSizeMode.StretchImage;
                box.Image = (Sprites.Question != null && Sprites.Question.Length > 0) ? Sprites.Question[0] : null;

                Controls.Add(box);
                box.SendToBack();
                questionBlocks.Add(block);
            }
        }

        private void ClearPowerUps()
        {
            foreach (var b in questionBlocks)
            {
                if (b.Visual != null) { Controls.Remove(b.Visual); b.Visual.Dispose(); }
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
                    PlatformData[][] pickFrom;
                    if (i == 0)                              pickFrom = openingPool;
                    else if (i < total * 2 / 3)              pickFrom = midPool;
                    else                                     pickFrom = hardPool;

                    var sec = pickFrom[levelRandom.Next(pickFrom.Length)];

                    // Avoid repeating the same section type two consecutive times.
                    // Retry with a generous cap so we don't infinite-loop if a pool is
                    // ever edited to contain duplicate references (all pools today have
                    // distinct entries, but defense-in-depth).
                    if (pickFrom.Length > 1)
                    {
                        for (int t = 0; t < 16 && Array.IndexOf(ALL_SECTIONS, sec) == prevSectionIdx; t++)
                            sec = pickFrom[levelRandom.Next(pickFrom.Length)];
                    }
                    prevSectionIdx = Array.IndexOf(ALL_SECTIONS, sec);

                    int kept = 0;
                    foreach (var p in sec)
                    {
                        int ny = yBase + p.Y;
                        if (ny >= 250 && ny <= 483)
                        {
                            result.Add(new PlatformData(xOff + p.X, ny, p.Width, p.Height));
                            kept++;
                        }
                    }
                    // Always advance xOff so the next section doesn't pile on top of
                    // this one. If no platform survived the y-window, advance by a
                    // sensible default gap rather than leaving xOff in place.
                    if (sec.Length > 0 && kept > 0)
                        xOff += sec.Max(p => p.X + p.Width) + 130;
                    else
                        xOff += 200;
                }

                extra++;
            } while (result.Count < 8 && extra < 20);

            return result.ToArray();
        }

    }
}
