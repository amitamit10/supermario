using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace supermario
{
    public partial class mainWin : Form
    {
        private struct PlatformData
        {
            public int X, Y, Width, Height;
            public PlatformData(int x, int y, int w, int h) { X = x; Y = y; Width = w; Height = h; }
        }

        private Player player;
        private GameManager gameManager;
        private Timer gameTimer;
        private List<GameObjectS> platforms = new List<GameObjectS>();
        private List<QuestionBlock> questionBlocks = new List<QuestionBlock>();
        private PlatformData[] currentLevel;
        private int currentLevelNumber = 1;
        private PlatformData[][] allLevels;
        private Random levelRandom = new Random();

        // ── Goombas ───────────────────────────────────────────────────────────
        private List<Goomba> goombas = new List<Goomba>();

        // Goomba spawn X positions per level (Y = ground top 513 − goomba height 48 = 465)
        private static readonly int[] GOOMBA_X_LEVEL_1 = { 420, 750, 1100, 1450, 1820, 2180, 2450 };
        private static readonly int[] GOOMBA_X_LEVEL_2 = { 350, 650, 950, 1320, 1650, 1950, 2300 };
        private const int GOOMBA_GROUND_Y = 465;

        // ── Fixed-timestep accumulator ────────────────────────────────────────
        private Stopwatch _stopwatch = new Stopwatch();
        private long _lastTickMs = 0;
        private long _accumulatedMs = 0;
        private const long FIXED_STEP_MS = 16;
        // ─────────────────────────────────────────────────────────────────────

        private bool moveRight = false, moveLeft = false, jump = false;
        private int cameraX = 0;
        private const int SCROLL_THRESHOLD = 400;

        // ── Mario sizes ───────────────────────────────────────────────────────
        private Size originalPlayerSize = new Size(75, 60);
        private Size superPlayerSize = new Size(90, 72);

        // ── Death animation ───────────────────────────────────────────────────
        private bool isDying = false;
        private float deathTimer = 0f;
        private const float DEATH_ANIMATION_DURATION = 2000f;

        // ── Pit detection – Mario dies if he falls below this Y ───────────────
        private const int PIT_DEATH_Y = 580;

        // ── Level data ────────────────────────────────────────────────────────
        private static readonly PlatformData[] SECTION_STAIRS = new PlatformData[] { new PlatformData(0, 30, 120, 20), new PlatformData(150, -20, 120, 20), new PlatformData(300, -70, 120, 20) };
        private static readonly PlatformData[] SECTION_GAP_JUMPS = new PlatformData[] { new PlatformData(0, -20, 100, 20), new PlatformData(170, -20, 100, 20), new PlatformData(340, -20, 100, 20) };
        private static readonly PlatformData[] SECTION_WAVE = new PlatformData[] { new PlatformData(0, 0, 100, 20), new PlatformData(150, -40, 100, 20), new PlatformData(300, 0, 100, 20), new PlatformData(450, -40, 100, 20) };
        private static readonly PlatformData[] SECTION_HIGH_PLATFORMS = new PlatformData[] { new PlatformData(0, -80, 100, 20), new PlatformData(150, -130, 100, 20), new PlatformData(300, -80, 100, 20) };
        private static readonly PlatformData[] SECTION_CHALLENGE = new PlatformData[] { new PlatformData(0, -30, 80, 20), new PlatformData(130, -70, 80, 20), new PlatformData(260, -30, 80, 20), new PlatformData(390, -70, 80, 20) };
        private static readonly PlatformData[][] ALL_SECTIONS = new PlatformData[][] { SECTION_STAIRS, SECTION_GAP_JUMPS, SECTION_WAVE, SECTION_HIGH_PLATFORMS, SECTION_CHALLENGE };

        private static readonly PlatformData[] LEVEL_1 = new PlatformData[] { new PlatformData(200, 483, 120, 20), new PlatformData(350, 433, 120, 20), new PlatformData(500, 383, 120, 20), new PlatformData(700, 433, 100, 20), new PlatformData(870, 433, 100, 20), new PlatformData(1040, 433, 100, 20), new PlatformData(1200, 413, 100, 20), new PlatformData(1350, 353, 100, 20), new PlatformData(1500, 413, 100, 20), new PlatformData(1650, 333, 100, 20), new PlatformData(1800, 283, 100, 20), new PlatformData(1950, 333, 100, 20), new PlatformData(2100, 433, 120, 20), new PlatformData(2270, 433, 120, 20), new PlatformData(2440, 433, 120, 20), new PlatformData(2650, 383, 200, 20) };
        private static readonly PlatformData[] LEVEL_2 = new PlatformData[] { new PlatformData(150, 473, 80, 20), new PlatformData(300, 433, 80, 20), new PlatformData(450, 393, 80, 20), new PlatformData(600, 353, 80, 20), new PlatformData(750, 403, 70, 20), new PlatformData(880, 353, 70, 20), new PlatformData(1010, 403, 70, 20), new PlatformData(1140, 353, 70, 20), new PlatformData(1270, 303, 100, 20), new PlatformData(1420, 253, 100, 20), new PlatformData(1570, 303, 100, 20), new PlatformData(1720, 253, 100, 20), new PlatformData(1870, 353, 100, 20), new PlatformData(2020, 403, 100, 20), new PlatformData(2170, 433, 100, 20), new PlatformData(2300, 373, 80, 20), new PlatformData(2430, 333, 80, 20), new PlatformData(2560, 373, 80, 20), new PlatformData(2700, 383, 200, 20) };

        // ═════════════════════════════════════════════════════════════════════
        public mainWin()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            KeyPreview = true;
            DoubleBuffered = true;
            Focus();

            gameManager = new GameManager();
            allLevels = new PlatformData[][] { LEVEL_1, LEVEL_2, GenerateRandomLevel(6), GenerateRandomLevel(7), GenerateRandomLevel(8) };
            currentLevelNumber = 1;
            currentLevel = allLevels[0];

            if (picboxplayer != null)
            {
                picboxplayer.BringToFront();
                try { picboxplayer.Image = Properties.Resources.dcaeqy1_614416a8_3ae1_4448_94b4_e3ecefa3e53a; picboxplayer.BackColor = Color.Transparent; }
                catch { picboxplayer.BackColor = Color.Red; }
                picboxplayer.SizeMode = PictureBoxSizeMode.StretchImage;
                picboxplayer.Size = originalPlayerSize;
            }

            player = new Player(new Point(100, 405), null);
            player.IsGrounded = true;

            // ── Wire up the new SMB-style events ──────────────────────────────
            player.OnBecameSmall += HandleBecameSmall;   // Super → Small (no life lost)
            player.OnDied += HandlePlayerDied;    // Small hit or pit → life lost

            picboxplayer.Location = player.Position;
            picboxplayer.BringToFront();

            CreateLongLevel();

            gameTimer = new Timer { Interval = 8 };
            gameTimer.Tick += GameLoop;
            KeyDown += MainWin_KeyDown;
            KeyUp += MainWin_KeyUp;

            Text = $"Super Mario - Level {currentLevelNumber}";
        }

        // ── Main game loop (fixed timestep) ───────────────────────────────────
        private void GameLoop(object sender, EventArgs e)
        {
            if (!gameManager.IsGameRunning) return;

            long now = _stopwatch.ElapsedMilliseconds;
            long elapsed = now - _lastTickMs;
            _lastTickMs = now;

            if (elapsed > 100) elapsed = 100;
            _accumulatedMs += elapsed;

            bool didStep = false;
            while (_accumulatedMs >= FIXED_STEP_MS)
            {
                PhysicsStep();
                _accumulatedMs -= FIXED_STEP_MS;
                didStep = true;
            }

            if (didStep)
            {
                UpdateCamera();
                DrawHUD();
                CheckWinCondition();
                Text = $"Super Mario - Level {currentLevelNumber} | Lives: {player.Lives} | {(player.State == MarioState.Super ? "SUPER MARIO" : "Small Mario")} | Pos:({player.Position.X},{player.Position.Y})";
            }
        }

        // ── One fixed 16 ms physics tick ──────────────────────────────────────
        private void PhysicsStep()
        {
            if (isDying)
            {
                HandleDeathAnimation(FIXED_STEP_MS);
                return;
            }

            int dir = (moveRight ? 1 : 0) + (moveLeft ? -1 : 0);
            player.Move(dir, jump);

            player.UpdateInvincibility(FIXED_STEP_MS);

            CheckPlatformCollisions();
            CheckQuestionBlockCollisions();
            CheckPitFall();
            UpdateGoombas();
        }

        // ── Pit detection: replaces fall-damage system ─────────────────────────
        /// <summary>
        /// If Mario falls below the screen he always loses a life,
        /// regardless of whether he is Small or Super.
        /// </summary>
        private void CheckPitFall()
        {
            if (isDying) return;
            if (player.Position.Y > PIT_DEATH_Y)
                player.FallIntoPit();
        }

        // ── Goomba system ─────────────────────────────────────────────────────

        private void UpdateGoombas()
        {
            for (int i = goombas.Count - 1; i >= 0; i--)
            {
                Goomba g = goombas[i];

                if (!g.IsAlive)
                {
                    Controls.Remove(g.Visual);
                    g.Visual.Dispose();
                    goombas.RemoveAt(i);
                    continue;
                }

                if (g.IsSquished)
                {
                    if (g.UpdateSquish(FIXED_STEP_MS)) g.Kill();
                    g.Visual.Location = new Point(g.Position.X - cameraX, g.Position.Y);
                    continue;
                }

                CheckGoombaEdge(g);
                g.Update();
                ApplyGoombaPhysics(g);
                CheckGoombaPlayerCollision(g);

                g.Visual.Location = new Point(g.Position.X - cameraX, g.Position.Y);
            }
        }

        private void ApplyGoombaPhysics(Goomba g)
        {
            if (!g.IsGrounded)
            {
                g.VerticalVelocity += 0.6f;
                if (g.VerticalVelocity > 15f) g.VerticalVelocity = 15f;
                g.Position = new Point(g.Position.X, g.Position.Y + (int)g.VerticalVelocity);
            }
            else
            {
                g.VerticalVelocity = 0f;
            }

            Rectangle gRect = g.Bounds;
            bool landed = false;

            foreach (var plat in platforms)
            {
                Rectangle pRect = new Rectangle(plat.Position.X, plat.Position.Y,
                                                plat.PictureBox.Width, plat.PictureBox.Height);
                if (!gRect.IntersectsWith(pRect)) continue;

                int overlapTop = gRect.Bottom - pRect.Top;
                if (overlapTop > 0 && overlapTop < 20)
                {
                    g.Position = new Point(g.Position.X, pRect.Top - g.Visual.Height);
                    g.IsGrounded = true;
                    landed = true;
                    break;
                }
                // Wall bounce
                int overlapLeft = gRect.Right - pRect.Left;
                int overlapRight = pRect.Right - gRect.Left;
                if (Math.Min(overlapLeft, overlapRight) < 15) g.ReverseDirection();
            }

            if (!landed) g.IsGrounded = false;
            if (g.Position.Y > PIT_DEATH_Y) g.Kill();
        }

        /// <summary>Turn around when the ground ahead disappears (classic SMB edge AI).</summary>
        private void CheckGoombaEdge(Goomba g)
        {
            if (!g.IsGrounded) return;
            int leadX = g.Direction == 1 ? g.Position.X + g.Visual.Width + 2 : g.Position.X - 2;
            int checkY = g.Position.Y + g.Visual.Height + 4;
            if (!HasGroundAt(leadX, checkY)) g.ReverseDirection();
        }

        private bool HasGroundAt(int worldX, int worldY)
        {
            foreach (var plat in platforms)
            {
                int l = plat.Position.X, r = l + plat.PictureBox.Width;
                int t = plat.Position.Y, b = t + plat.PictureBox.Height;
                if (worldX >= l && worldX <= r && worldY >= t && worldY <= b) return true;
            }
            return false;
        }

        /// <summary>
        /// Stomp  = Mario's feet enter Goomba from the top → squish + bounce.
        /// Side hit = Mario takes damage (with invincibility window).
        /// </summary>
        private void CheckGoombaPlayerCollision(Goomba g)
        {
            if (player.IsInvincible) return;

            Rectangle playerRect = new Rectangle(player.Position.X, player.Position.Y,
                                                 picboxplayer.Width, picboxplayer.Height);
            Rectangle gRect = g.Bounds;
            if (!playerRect.IntersectsWith(gRect)) return;

            int overlapTop = playerRect.Bottom - gRect.Top;

            bool isStomp = overlapTop > 0
                        && overlapTop < 26
                        && playerRect.Bottom > gRect.Top
                        && player.Position.Y < g.Position.Y;   // Mario centre above Goomba centre

            if (isStomp)
            {
                g.Squish();
                player.StompBounce();
            }
            else
            {
                player.TakeDamage();
            }
        }

        private void SpawnGoombas()
        {
            int[] spawnXArr;

            if (currentLevelNumber == 1) spawnXArr = GOOMBA_X_LEVEL_1;
            else if (currentLevelNumber == 2) spawnXArr = GOOMBA_X_LEVEL_2;
            else
            {
                var xList = new List<int>();
                for (int x = 400; x < 2700; x += 380 + levelRandom.Next(-60, 60))
                    xList.Add(x);
                spawnXArr = xList.ToArray();
            }

            foreach (int spawnX in spawnXArr)
            {
                var g = new Goomba(new Point(spawnX, GOOMBA_GROUND_Y));
                Controls.Add(g.Visual);
                g.Visual.BringToFront();
                goombas.Add(g);
            }

            picboxplayer.BringToFront();  // always keep Mario on top
        }

        private void ClearGoombas()
        {
            foreach (var g in goombas) { Controls.Remove(g.Visual); g.Visual.Dispose(); }
            goombas.Clear();
        }

        // ── Player event handlers ─────────────────────────────────────────────

        /// <summary>Super Mario was hit → shrink to Small, no life lost.</summary>
        private void HandleBecameSmall()
        {
            // player.State is already MarioState.Small when this fires
            picboxplayer.Size = originalPlayerSize;
            player.Position = new Point(player.Position.X, player.Position.Y + (superPlayerSize.Height - originalPlayerSize.Height));
        }

        /// <summary>Mario lost a life (hit as Small, or fell into a pit).</summary>
        private void HandlePlayerDied()
        {
            if (isDying) return; // already in death animation

            if (player.Lives <= 0)
            {
                HandleGameOver();
            }
            else
            {
                // Start the classic "bounce up, fall" death animation
                isDying = true;
                deathTimer = 0f;
            }
        }

        /// <summary>Game Over: show screen, reset everything including lives.</summary>
        private void HandleGameOver()
        {
            gameTimer.Stop();
            _stopwatch.Stop();
            gameManager.EndGame();

            DialogResult result = MessageBox.Show(
                "GAME OVER!\n\nYou ran out of lives.\n\nClick OK to play again from Level 1.",
                "Game Over",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            if (result == DialogResult.OK)
            {
                currentLevelNumber = 1;
                currentLevel = allLevels[0];
                FullReset();
            }
        }

        // ── Win condition ──────────────────────────────────────────────────────
        private void CheckWinCondition()
        {
            if (player.Position.X >= 2750)
            {
                gameTimer.Stop();
                if (currentLevelNumber < allLevels.Length)
                {
                    MessageBox.Show($"Level {currentLevelNumber} Complete!", "Level Complete!", MessageBoxButtons.OK);
                    LoadNextLevel();
                }
                else
                {
                    MessageBox.Show("Congratulations! You've completed all levels!", "Game Complete!", MessageBoxButtons.OK);
                    currentLevelNumber = 1;
                    currentLevel = allLevels[0];
                    FullReset();
                }
            }
        }

        // ── Level creation ────────────────────────────────────────────────────
        private void CreateLongLevel()
        {
            CreateBrickGround();
            ClearPowerUps();
            foreach (var p in currentLevel)
                AddPlatform(p.X, p.Y, p.Width, p.Height);
            AddQuestionBlocks();

            PictureBox finishLine = new PictureBox { Size = new Size(50, 150), Location = new Point(2750, 363), BackColor = Color.Gold };
            Controls.Add(finishLine);
            finishLine.SendToBack();
            platforms.Add(new GameObjectS(finishLine, finishLine.Location, "finish"));

            picboxplayer.BringToFront();
            SpawnGoombas();
        }

        private void CreateBrickGround()
        {
            for (int x = 0; x < 3000; x += 40)
            {
                PictureBox brick = new PictureBox { Size = new Size(40, 40), Location = new Point(x, 513), SizeMode = PictureBoxSizeMode.StretchImage, BorderStyle = BorderStyle.FixedSingle };
                try { brick.Image = Properties.Resources.oie_cnoULepStRjX; }
                catch { brick.BackColor = Color.Peru; }
                Controls.Add(brick);
                brick.SendToBack();
                platforms.Add(new GameObjectS(brick, brick.Location, "ground"));
            }
        }

        private void AddQuestionBlocks()
        {
            int[] xPos = { 400, 650, 950, 1300, 1700, 2150, 2500 };
            int[] yPos = { 350, 380, 320, 360, 280, 350, 380 };

            for (int i = 0; i < xPos.Length && i < yPos.Length; i++)
            {
                PictureBox box = new PictureBox { Size = new Size(50, 50), Location = new Point(xPos[i], yPos[i]), BackColor = Color.Gold, BorderStyle = BorderStyle.FixedSingle };
                Controls.Add(box);
                box.SendToBack();

                Label lbl = new Label { Text = "?", Font = new Font("Arial", 32, FontStyle.Bold), ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter, Size = new Size(50, 50), Location = new Point(xPos[i], yPos[i]), BackColor = Color.Transparent };
                Controls.Add(lbl);
                lbl.BringToFront();

                questionBlocks.Add(new QuestionBlock(box.Location, box, lbl, PowerUpType.Mushroom));
            }
        }

        private void AddPlatform(int x, int y, int w, int h)
        {
            PictureBox p = new PictureBox { Size = new Size(w, h), Location = new Point(x, y), SizeMode = PictureBoxSizeMode.StretchImage };
            try { p.Image = Properties.Resources.oie_cnoULepStRjX; }
            catch { p.BackColor = Color.Green; }
            Controls.Add(p);
            p.SendToBack();
            platforms.Add(new GameObjectS(p, p.Location, "platform"));
        }

        private void ClearPowerUps()
        {
            foreach (var b in questionBlocks)
            {
                if (b.Visual != null) { Controls.Remove(b.Visual); b.Visual.Dispose(); }
                if (b.QuestionLabel != null) { Controls.Remove(b.QuestionLabel); b.QuestionLabel.Dispose(); }
            }
            questionBlocks.Clear();
        }

        // ── Input ─────────────────────────────────────────────────────────────
        private void MainWin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D) moveRight = true;
            if (e.KeyCode == Keys.A) moveLeft = true;
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Space) jump = true;

            if (e.KeyCode == Keys.Enter && !gameManager.IsGameRunning)
            {
                _stopwatch.Restart();
                _lastTickMs = 0;
                _accumulatedMs = 0;

                gameManager.StartGame();
                gameTimer.Start();
                Text = $"Super Mario - Level {currentLevelNumber} - RUNNING";
            }

            if (e.KeyCode == Keys.Escape)
            {
                gameTimer.Stop();
                _stopwatch.Stop();
                gameManager.EndGame();
                Text = $"Super Mario - Level {currentLevelNumber} - PAUSED";
            }
        }

        private void MainWin_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D) moveRight = false;
            if (e.KeyCode == Keys.A) moveLeft = false;
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Space) jump = false;
        }

        // ── Collision ─────────────────────────────────────────────────────────
        private void CheckPlatformCollisions()
        {
            Rectangle playerRect = new Rectangle(player.Position.X, player.Position.Y, picboxplayer.Width, picboxplayer.Height);
            bool foundGround = false;

            foreach (var plat in platforms)
            {
                Rectangle platRect = new Rectangle(plat.Position.X, plat.Position.Y, plat.PictureBox.Width, plat.PictureBox.Height);

                if (playerRect.IntersectsWith(platRect))
                {
                    int overlapTop = playerRect.Bottom - platRect.Top;
                    int overlapBottom = platRect.Bottom - playerRect.Top;
                    int overlapLeft = playerRect.Right - platRect.Left;
                    int overlapRight = platRect.Right - playerRect.Left;

                    int minOverlap = Math.Min(Math.Min(overlapTop, overlapBottom), Math.Min(overlapLeft, overlapRight));

                    if (minOverlap == overlapTop && overlapTop < 20)
                    {
                        player.Position = new Point(player.Position.X, platRect.Top - picboxplayer.Height);
                        player.IsGrounded = true;
                        foundGround = true;
                        break;
                    }
                }
            }

            if (!foundGround) player.IsGrounded = false;
        }

        /// <summary>
        /// Hitting a ? block from below:
        ///   - Small Mario  → grows into Super Mario (collect mushroom)
        ///   - Super Mario  → block deactivates (no extra power-up, like in original SMB
        ///                    where the block just turns grey)
        /// </summary>
        private void CheckQuestionBlockCollisions()
        {
            // Check the top edge of Mario's sprite (his head)
            Rectangle headRect = new Rectangle(player.Position.X + 10, player.Position.Y, picboxplayer.Width - 20, 20);

            foreach (var block in questionBlocks)
            {
                if (block.IsHit) continue;

                Rectangle blockRect = new Rectangle(block.Position.X, block.Position.Y, block.Visual.Width, block.Visual.Height);

                if (headRect.IntersectsWith(blockRect))
                {
                    // Deactivate block visually
                    block.IsHit = true;
                    block.Visual.BackColor = Color.LightGray;
                    block.QuestionLabel.Text = "";

                    if (player.State == MarioState.Small)
                    {
                        // Small → Super: let Player update its state, then resize sprite
                        player.CollectMushroom();
                        GrowToSuper();
                    }
                    // If already Super: block just turns grey (no power-up in this build)
                }
            }
        }

        // ── Size changes ──────────────────────────────────────────────────────

        /// <summary>Grow Mario's sprite when he becomes Super.</summary>
        private void GrowToSuper()
        {
            int heightDiff = superPlayerSize.Height - originalPlayerSize.Height;
            picboxplayer.Size = superPlayerSize;
            // Shift up so feet stay on the ground
            player.Position = new Point(player.Position.X, player.Position.Y - heightDiff);
        }

        // ── Camera / rendering ────────────────────────────────────────────────
        private void UpdateCamera()
        {
            int screenX = player.Position.X - cameraX;

            if (screenX > SCROLL_THRESHOLD && player.Position.X > SCROLL_THRESHOLD)
            {
                int newCam = player.Position.X - SCROLL_THRESHOLD;
                if (newCam > 2000) newCam = 2000;
                int scroll = newCam - cameraX;
                cameraX = newCam;
                ScrollObjects(scroll);
            }
            else if (screenX < 200 && cameraX > 0)
            {
                int newCam = player.Position.X - 200;
                if (newCam < 0) newCam = 0;
                int scroll = newCam - cameraX;
                cameraX = newCam;
                ScrollObjects(scroll);
            }

            picboxplayer.Location = new Point(player.Position.X - cameraX, player.Position.Y);
            picboxplayer.BringToFront();

            // Invincibility flicker: toggle visibility each 100 ms
            picboxplayer.Visible = player.IsVisible();
        }

        private void ScrollObjects(int scroll)
        {
            foreach (var p in platforms) p.PictureBox.Left -= scroll;
            foreach (var b in questionBlocks) { b.Visual.Left -= scroll; b.QuestionLabel.Left -= scroll; }
            // Goombas are positioned every tick from their world Position, so no PictureBox shift needed here
            // (UpdateGoombas sets Visual.Location = world.Position - cameraX each frame)
        }

        // ── HUD ───────────────────────────────────────────────────────────────
        /// <summary>
        /// Shows remaining lives as ❤ symbols (one per life) and a SUPER badge
        /// when Mario has the mushroom power-up — exactly like the original SMB HUD.
        /// </summary>
        private void DrawHUD()
        {
            // Remove old HUD labels
            var oldLabels = Controls.OfType<Label>().Where(l => l.Name == "hudLabel").ToList();
            foreach (var l in oldLabels) { Controls.Remove(l); l.Dispose(); }

            // Draw one heart per remaining life
            for (int i = 0; i < player.Lives; i++)
            {
                Label heart = new Label
                {
                    Name = "hudLabel",
                    Text = "❤",
                    Font = new Font("Arial", 20, FontStyle.Bold),
                    ForeColor = Color.Red,
                    AutoSize = true,
                    Location = new Point(10 + (i * 32), 10),
                    BackColor = Color.Transparent
                };
                Controls.Add(heart);
                heart.BringToFront();
            }

            // "SUPER" badge when Mario has the mushroom
            if (player.State == MarioState.Super)
            {
                Label superLabel = new Label
                {
                    Name = "hudLabel",
                    Text = "★ SUPER",
                    Font = new Font("Arial", 14, FontStyle.Bold),
                    ForeColor = Color.Yellow,
                    AutoSize = true,
                    Location = new Point(10 + (player.Lives * 32) + 10, 12),
                    BackColor = Color.Transparent
                };
                Controls.Add(superLabel);
                superLabel.BringToFront();
            }
        }

        // ── Death animation ───────────────────────────────────────────────────
        private void HandleDeathAnimation(long stepMs)
        {
            deathTimer += stepMs;

            if (deathTimer < 500)
            {
                // Bounce up
                picboxplayer.Location = new Point(
                    picboxplayer.Location.X,
                    (int)(player.Position.Y - (deathTimer / 500f) * 100));
            }
            else if (deathTimer < DEATH_ANIMATION_DURATION)
            {
                // Fall down
                picboxplayer.Location = new Point(
                    picboxplayer.Location.X,
                    (int)(player.Position.Y + ((deathTimer - 500) / (DEATH_ANIMATION_DURATION - 500)) * 300));
            }
            else
            {
                // Animation finished → respawn at current level (lives already decremented)
                isDying = false;
                RespawnAfterDeath();
            }
        }

        // ── Respawn / restart / load ──────────────────────────────────────────

        /// <summary>Respawn after death animation ends. Lives are NOT reset here.</summary>
        private void RespawnAfterDeath()
        {
            isDying = false;
            cameraX = 0;

            // Reset player position and state (Small), keep current lives count
            player.Respawn(new Point(100, 405));
            player.IsGrounded = true;

            picboxplayer.Size = originalPlayerSize;
            picboxplayer.Visible = true;
            picboxplayer.Location = player.Position;

            ClearPlatforms();
            CreateLongLevel();

            _stopwatch.Restart();
            _lastTickMs = 0;
            _accumulatedMs = 0;

            gameManager.StartGame();
            gameTimer.Start();
        }

        /// <summary>Full reset: lives go back to 3, level back to 1 (Game Over / level clear).</summary>
        private void FullReset()
        {
            isDying = false;
            cameraX = 0;

            player.Respawn(new Point(100, 405));
            player.IsGrounded = true;
            player.Lives = 3;              // only place lives are restored

            picboxplayer.Size = originalPlayerSize;
            picboxplayer.Visible = true;
            picboxplayer.Location = player.Position;

            ClearPlatforms();
            CreateLongLevel();

            _stopwatch.Restart();
            _lastTickMs = 0;
            _accumulatedMs = 0;

            Text = $"Super Mario - Level {currentLevelNumber}";
            gameManager.StartGame();
            gameTimer.Start();
        }

        private void LoadNextLevel()
        {
            currentLevelNumber++;
            currentLevel = allLevels[currentLevelNumber - 1];

            isDying = false;
            cameraX = 0;

            // Keep lives as they are — just reset position and state
            player.Respawn(new Point(100, 405));
            player.IsGrounded = true;

            picboxplayer.Size = originalPlayerSize;
            picboxplayer.Visible = true;
            picboxplayer.Location = player.Position;

            ClearPlatforms();
            CreateLongLevel();

            _stopwatch.Restart();
            _lastTickMs = 0;
            _accumulatedMs = 0;

            Text = $"Super Mario - Level {currentLevelNumber}";
            gameManager.StartGame();
            gameTimer.Start();
        }

        private void ClearPlatforms()
        {
            foreach (var p in platforms) { Controls.Remove(p.PictureBox); p.PictureBox.Dispose(); }
            platforms.Clear();
            ClearPowerUps();
            ClearGoombas();
        }

        // ── Procedural level generation ───────────────────────────────────────
        private PlatformData[] GenerateRandomLevel(int numSections)
        {
            List<PlatformData> result = new List<PlatformData>();
            int xOff = 200, yBase = 483;

            var sections = new List<PlatformData[]>();
            for (int i = 0; i < numSections; i++)
                sections.Add(ALL_SECTIONS[levelRandom.Next(ALL_SECTIONS.Length)]);

            foreach (var sec in sections)
            {
                foreach (var p in sec)
                {
                    int nx = xOff + p.X, ny = yBase + p.Y;
                    if (ny >= 250 && ny <= 483) result.Add(new PlatformData(nx, ny, p.Width, p.Height));
                }

                if (sec.Length > 0)
                {
                    int maxX = 0;
                    foreach (var p in sec) if (p.X + p.Width > maxX) maxX = p.X + p.Width;
                    xOff += maxX + 100;
                }
            }

            if (result.Count < 5) return GenerateRandomLevel(numSections + 1);
            return result.ToArray();
        }

        // ── Form load ─────────────────────────────────────────────────────────
        private void mainWin_Load(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Controls:\nW / Space  – Jump\nA  – Move Left\nD  – Move Right\n\n" +
                "❤ You start with 3 LIVES.\n" +
                "🍄 Hit ? blocks to grab a Mushroom → become Super Mario!\n\n" +
                "👾 GOOMBAS:\n" +
                "  Jump ON TOP of a Goomba to squish it!\n" +
                "  Touch one from the side and you take damage.\n" +
                "  As Small Mario: one hit = lose a life.\n" +
                "  As Super Mario: one hit = shrink (no life lost).\n" +
                "⚠  Falling into a pit always costs a life!\n\n" +
                "Press ENTER to START!",
                "Super Mario",
                MessageBoxButtons.OK);

            Text = $"Super Mario - Level {currentLevelNumber}";
            _stopwatch.Start();
        }
    }

    // ── Supporting types (unchanged) ─────────────────────────────────────────
    public enum PowerUpType { Mushroom }

    public class Mushroom
    {
        public Point Position { get; set; }
        public PictureBox Visual { get; set; }
        public bool IsCollected { get; set; }
        public Mushroom(Point pos, PictureBox visual) { Position = pos; Visual = visual; IsCollected = false; }
    }

    public class QuestionBlock
    {
        public Point Position { get; set; }
        public PictureBox Visual { get; set; }
        public Label QuestionLabel { get; set; }
        public bool IsHit { get; set; }
        public PowerUpType PowerUpInside { get; set; }

        public QuestionBlock(Point pos, PictureBox visual, Label label, PowerUpType powerUp)
        {
            Position = pos;
            Visual = visual;
            QuestionLabel = label;
            IsHit = false;
            PowerUpInside = powerUp;
        }
    }
}