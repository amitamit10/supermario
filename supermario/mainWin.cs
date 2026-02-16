using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

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

        private bool moveRight = false, moveLeft = false, jump = false;
        private int cameraX = 0;
        private const int SCROLL_THRESHOLD = 400;
        private bool isPlayerSuper = false;
        private Size originalPlayerSize = new Size(135, 108);
        private Size superPlayerSize = new Size(155, 124);
        private bool isDying = false;
        private float deathTimer = 0f;
        private const float DEATH_ANIMATION_DURATION = 2000f;
        private float maxFallStartY = 0;
        private bool wasGroundedLastFrame = false;
        private const float FALL_DAMAGE_THRESHOLD = 60f;
        private bool canTakeFallDamage = true;

        private static readonly PlatformData[] SECTION_STAIRS = new PlatformData[] { new PlatformData(0, 30, 120, 20), new PlatformData(150, -20, 120, 20), new PlatformData(300, -70, 120, 20) };
        private static readonly PlatformData[] SECTION_GAP_JUMPS = new PlatformData[] { new PlatformData(0, -20, 100, 20), new PlatformData(170, -20, 100, 20), new PlatformData(340, -20, 100, 20) };
        private static readonly PlatformData[] SECTION_WAVE = new PlatformData[] { new PlatformData(0, 0, 100, 20), new PlatformData(150, -40, 100, 20), new PlatformData(300, 0, 100, 20), new PlatformData(450, -40, 100, 20) };
        private static readonly PlatformData[] SECTION_HIGH_PLATFORMS = new PlatformData[] { new PlatformData(0, -80, 100, 20), new PlatformData(150, -130, 100, 20), new PlatformData(300, -80, 100, 20) };
        private static readonly PlatformData[] SECTION_CHALLENGE = new PlatformData[] { new PlatformData(0, -30, 80, 20), new PlatformData(130, -70, 80, 20), new PlatformData(260, -30, 80, 20), new PlatformData(390, -70, 80, 20) };
        private static readonly PlatformData[][] ALL_SECTIONS = new PlatformData[][] { SECTION_STAIRS, SECTION_GAP_JUMPS, SECTION_WAVE, SECTION_HIGH_PLATFORMS, SECTION_CHALLENGE };

        private static readonly PlatformData[] LEVEL_1 = new PlatformData[] { new PlatformData(200, 483, 120, 20), new PlatformData(350, 433, 120, 20), new PlatformData(500, 383, 120, 20), new PlatformData(700, 433, 100, 20), new PlatformData(870, 433, 100, 20), new PlatformData(1040, 433, 100, 20), new PlatformData(1200, 413, 100, 20), new PlatformData(1350, 353, 100, 20), new PlatformData(1500, 413, 100, 20), new PlatformData(1650, 333, 100, 20), new PlatformData(1800, 283, 100, 20), new PlatformData(1950, 333, 100, 20), new PlatformData(2100, 433, 120, 20), new PlatformData(2270, 433, 120, 20), new PlatformData(2440, 433, 120, 20), new PlatformData(2650, 383, 200, 20) };
        private static readonly PlatformData[] LEVEL_2 = new PlatformData[] { new PlatformData(150, 473, 80, 20), new PlatformData(300, 433, 80, 20), new PlatformData(450, 393, 80, 20), new PlatformData(600, 353, 80, 20), new PlatformData(750, 403, 70, 20), new PlatformData(880, 353, 70, 20), new PlatformData(1010, 403, 70, 20), new PlatformData(1140, 353, 70, 20), new PlatformData(1270, 303, 100, 20), new PlatformData(1420, 253, 100, 20), new PlatformData(1570, 303, 100, 20), new PlatformData(1720, 253, 100, 20), new PlatformData(1870, 353, 100, 20), new PlatformData(2020, 403, 100, 20), new PlatformData(2170, 433, 100, 20), new PlatformData(2300, 373, 80, 20), new PlatformData(2430, 333, 80, 20), new PlatformData(2560, 373, 80, 20), new PlatformData(2700, 383, 200, 20) };

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
                try
                {
                    picboxplayer.Image = Properties.Resources.dcaeqy1_614416a8_3ae1_4448_94b4_e3ecefa3e53a;
                    picboxplayer.BackColor = Color.Transparent;
                }
                catch
                {
                    picboxplayer.BackColor = Color.Red;
                }
                picboxplayer.SizeMode = PictureBoxSizeMode.StretchImage;
                picboxplayer.Size = originalPlayerSize;
            }

            // Initialize player at starting position
            player = new Player(new Point(100, 405), null);
            player.IsGrounded = true;
            player.Health = 3;

            // Subscribe to damage event to handle shrinking
            player.OnDamageTaken += () => { BecomeNormal(); };

            // Immediately sync visual position
            picboxplayer.Location = player.Position;
            picboxplayer.BringToFront();

            CreateLongLevel();

            gameTimer = new Timer { Interval = 16 }; // ~60 FPS (was 10ms which is 100 FPS)
            gameTimer.Tick += GameLoop;
            KeyDown += MainWin_KeyDown;
            KeyUp += MainWin_KeyUp;

            Text = $"Super Mario - Level {currentLevelNumber}";
        }

        private void CreateLongLevel()
        {
            CreateBrickGround();
            ClearPowerUps();
            foreach (var p in currentLevel)
            {
                AddPlatform(p.X, p.Y, p.Width, p.Height);
            }
            AddQuestionBlocks();

            PictureBox finishLine = new PictureBox
            {
                Size = new Size(50, 150),
                Location = new Point(2750, 363),
                BackColor = Color.Gold
            };
            Controls.Add(finishLine);
            finishLine.SendToBack();
            platforms.Add(new GameObjectS(finishLine, finishLine.Location, "finish"));

            // Ensure player is on top
            picboxplayer.BringToFront();
        }

        private void CreateBrickGround()
        {
            for (int x = 0; x < 3000; x += 40)
            {
                PictureBox brick = new PictureBox
                {
                    Size = new Size(40, 40),
                    Location = new Point(x, 513),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BorderStyle = BorderStyle.FixedSingle
                };

                try
                {
                    brick.Image = Properties.Resources.oie_cnoULepStRjX;
                }
                catch
                {
                    brick.BackColor = Color.Peru;
                }

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
                // Create the yellow question block
                PictureBox box = new PictureBox
                {
                    Size = new Size(50, 50),
                    Location = new Point(xPos[i], yPos[i]),
                    BackColor = Color.Gold,
                    BorderStyle = BorderStyle.FixedSingle
                    // NO SizeMode - we want solid color background!
                };

                Controls.Add(box);
                box.SendToBack();

                // Create question mark label ON TOP of the box
                Label lbl = new Label
                {
                    Text = "?",
                    Font = new Font("Arial", 32, FontStyle.Bold),
                    ForeColor = Color.Black,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(50, 50),
                    Location = new Point(xPos[i], yPos[i]),
                    BackColor = Color.Transparent
                };
                Controls.Add(lbl);
                lbl.BringToFront();

                questionBlocks.Add(new QuestionBlock(box.Location, box, lbl, PowerUpType.Mushroom));
            }
        }

        private void AddPlatform(int x, int y, int w, int h)
        {
            PictureBox p = new PictureBox
            {
                Size = new Size(w, h),
                Location = new Point(x, y),
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            try
            {
                p.Image = Properties.Resources.oie_cnoULepStRjX;
            }
            catch
            {
                p.BackColor = Color.Green;
            }

            Controls.Add(p);
            p.SendToBack();
            platforms.Add(new GameObjectS(p, p.Location, "platform"));
        }

        private void ClearPowerUps()
        {
            foreach (var b in questionBlocks)
            {
                if (b.Visual != null)
                {
                    Controls.Remove(b.Visual);
                    b.Visual.Dispose();
                }
                if (b.QuestionLabel != null)
                {
                    Controls.Remove(b.QuestionLabel);
                    b.QuestionLabel.Dispose();
                }
            }
            questionBlocks.Clear();
        }

        private void MainWin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D) moveRight = true;
            if (e.KeyCode == Keys.A) moveLeft = true;
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Space) jump = true;

            if (e.KeyCode == Keys.Enter && !gameManager.IsGameRunning)
            {
                gameManager.StartGame();
                gameTimer.Start();
                Text = $"Super Mario - Level {currentLevelNumber} - RUNNING";
            }

            if (e.KeyCode == Keys.Escape)
            {
                gameTimer.Stop();
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

        private void GameLoop(object sender, EventArgs e)
        {
            if (!gameManager.IsGameRunning) return;

            if (isDying)
            {
                HandleDeathAnimation();
                return;
            }

            // Update player
            int dir = (moveRight ? 1 : 0) + (moveLeft ? -1 : 0);
            player.Move(dir, jump);

            // Check collisions
            CheckPlatformCollisions();
            CheckQuestionBlockCollisions();
            HandleFallDamage();

            // Update camera and rendering
            UpdateCamera();
            DrawHearts();

            // Check win condition
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
                    RestartLevel();
                }
                return;
            }

            // Update title
            Text = $"Super Mario - Level {currentLevelNumber} | Health: {player.Health} | {(isPlayerSuper ? "SUPER! 💪" : "Normal")} | Pos:({player.Position.X},{player.Position.Y})";
        }

        private void CheckPlatformCollisions()
        {
            Rectangle playerRect = new Rectangle(player.Position.X, player.Position.Y, picboxplayer.Width, picboxplayer.Height);
            bool foundGround = false;

            foreach (var plat in platforms)
            {
                Rectangle platRect = new Rectangle(plat.Position.X, plat.Position.Y, plat.PictureBox.Width, plat.PictureBox.Height);

                if (playerRect.IntersectsWith(platRect))
                {
                    // Calculate overlap from all sides
                    int overlapTop = playerRect.Bottom - platRect.Top;
                    int overlapBottom = platRect.Bottom - playerRect.Top;
                    int overlapLeft = playerRect.Right - platRect.Left;
                    int overlapRight = platRect.Right - playerRect.Left;

                    // Find minimum overlap
                    int minOverlap = Math.Min(Math.Min(overlapTop, overlapBottom), Math.Min(overlapLeft, overlapRight));

                    // Landing on top of platform
                    if (minOverlap == overlapTop && overlapTop < 20)
                    {
                        player.Position = new Point(player.Position.X, platRect.Top - picboxplayer.Height);
                        player.IsGrounded = true;
                        foundGround = true;
                        break;
                    }
                }
            }

            if (!foundGround)
            {
                player.IsGrounded = false;
            }
        }

        private void CheckQuestionBlockCollisions()
        {
            // Check head collision with question blocks
            Rectangle headRect = new Rectangle(player.Position.X, player.Position.Y, picboxplayer.Width, 30);

            foreach (var block in questionBlocks)
            {
                if (block.IsHit) continue;

                Rectangle blockRect = new Rectangle(block.Position.X, block.Position.Y, block.Visual.Width, block.Visual.Height);

                if (headRect.IntersectsWith(blockRect))
                {
                    block.IsHit = true;
                    block.Visual.BackColor = Color.LightGray;
                    block.QuestionLabel.Text = "";

                    if (!isPlayerSuper)
                    {
                        BecomeSuper();
                    }
                    else
                    {
                        player.Health = Math.Min(player.Health + 1, 3);
                    }
                }
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
            if (isPlayerSuper)
            {
                isPlayerSuper = false;
                picboxplayer.Size = originalPlayerSize;
                player.Position = new Point(player.Position.X, player.Position.Y + 16);
            }
        }

        private void UpdateCamera()
        {
            int screenX = player.Position.X - cameraX;

            // Scroll right when player moves past threshold
            if (screenX > SCROLL_THRESHOLD && player.Position.X > SCROLL_THRESHOLD)
            {
                int newCam = player.Position.X - SCROLL_THRESHOLD;
                if (newCam > 2000) newCam = 2000;
                int scroll = newCam - cameraX;
                cameraX = newCam;

                foreach (var p in platforms)
                {
                    p.PictureBox.Left -= scroll;
                }
                foreach (var b in questionBlocks)
                {
                    b.Visual.Left -= scroll;
                    b.QuestionLabel.Left -= scroll;
                }
            }
            // Scroll left when player moves back
            else if (screenX < 200 && cameraX > 0)
            {
                int newCam = player.Position.X - 200;
                if (newCam < 0) newCam = 0;
                int scroll = newCam - cameraX;
                cameraX = newCam;

                foreach (var p in platforms)
                {
                    p.PictureBox.Left -= scroll;
                }
                foreach (var b in questionBlocks)
                {
                    b.Visual.Left -= scroll;
                    b.QuestionLabel.Left -= scroll;
                }
            }

            // CRITICAL: Always update player visual position based on world position and camera
            picboxplayer.Location = new Point(player.Position.X - cameraX, player.Position.Y);
            picboxplayer.BringToFront();
        }

        private void HandleFallDamage()
        {
            // Track when player starts falling
            if (wasGroundedLastFrame && !player.IsGrounded)
            {
                maxFallStartY = player.Position.Y;
                canTakeFallDamage = true;
            }

            // Check fall damage when landing
            if (!wasGroundedLastFrame && player.IsGrounded && !isDying)
            {
                float fallDist = maxFallStartY - player.Position.Y;

                if (fallDist > FALL_DAMAGE_THRESHOLD && canTakeFallDamage)
                {
                    player.TakeDamage(1); // This will automatically trigger BecomeNormal via event
                    canTakeFallDamage = false;

                    if (player.Health <= 0)
                    {
                        isDying = true;
                        deathTimer = 0f;
                    }
                }
                maxFallStartY = 0;
            }

            wasGroundedLastFrame = player.IsGrounded;
        }

        private void HandleDeathAnimation()
        {
            deathTimer += 10;

            if (deathTimer < 500)
            {
                // Rise up
                picboxplayer.Location = new Point(picboxplayer.Location.X, (int)(player.Position.Y - (deathTimer / 500f) * 100));
            }
            else if (deathTimer < DEATH_ANIMATION_DURATION)
            {
                // Fall down
                picboxplayer.Location = new Point(picboxplayer.Location.X, (int)(player.Position.Y + ((deathTimer - 500) / (DEATH_ANIMATION_DURATION - 500)) * 300));
            }
            else
            {
                // Reset
                isDying = false;
                player.Health = 3;
                isPlayerSuper = false;
                RestartLevel();
            }
        }

        private void DrawHearts()
        {
            // Remove old hearts
            var hearts = Controls.OfType<Label>().Where(l => l.Name == "heartLabel").ToList();
            foreach (var h in hearts)
            {
                Controls.Remove(h);
                h.Dispose();
            }

            // Draw new hearts
            for (int i = 0; i < player.Health; i++)
            {
                Label heart = new Label
                {
                    Name = "heartLabel",
                    Text = "❤",
                    Font = new Font("Arial", 20, FontStyle.Bold),
                    ForeColor = Color.Red,
                    AutoSize = true,
                    Location = new Point(10 + (i * 30), 10),
                    BackColor = Color.Transparent
                };
                Controls.Add(heart);
                heart.BringToFront();
            }
        }

        private void RestartLevel()
        {
            gameManager.ResetGame();
            cameraX = 0;
            isDying = false;
            wasGroundedLastFrame = false;
            canTakeFallDamage = true;
            isPlayerSuper = false;

            player.Respawn(new Point(100, 405));
            player.IsGrounded = true;
            player.Health = 3;

            // Re-subscribe to damage event
            player.OnDamageTaken += () => { BecomeNormal(); };

            picboxplayer.Size = originalPlayerSize;
            picboxplayer.Location = player.Position;

            ClearPlatforms();
            CreateLongLevel();

            gameManager.StartGame();
            gameTimer.Start();
        }

        private void LoadNextLevel()
        {
            currentLevelNumber++;
            currentLevel = allLevels[currentLevelNumber - 1];

            gameManager.ResetGame();
            cameraX = 0;
            isDying = false;
            wasGroundedLastFrame = false;
            canTakeFallDamage = true;
            isPlayerSuper = false;

            player.Respawn(new Point(100, 405));
            player.IsGrounded = true;
            player.Health = 3;

            // Re-subscribe to damage event
            player.OnDamageTaken += () => { BecomeNormal(); };

            picboxplayer.Size = originalPlayerSize;
            picboxplayer.Location = player.Position;

            ClearPlatforms();
            CreateLongLevel();

            Text = $"Super Mario - Level {currentLevelNumber}";
            gameManager.StartGame();
            gameTimer.Start();
        }

        private void ClearPlatforms()
        {
            foreach (var p in platforms)
            {
                Controls.Remove(p.PictureBox);
                p.PictureBox.Dispose();
            }
            platforms.Clear();
            ClearPowerUps();
        }

        private PlatformData[] GenerateRandomLevel(int numSections)
        {
            List<PlatformData> result = new List<PlatformData>();
            int xOff = 200;
            int yBase = 483;

            var sections = new List<PlatformData[]>();
            for (int i = 0; i < numSections; i++)
            {
                sections.Add(ALL_SECTIONS[levelRandom.Next(ALL_SECTIONS.Length)]);
            }

            foreach (var sec in sections)
            {
                foreach (var p in sec)
                {
                    int nx = xOff + p.X;
                    int ny = yBase + p.Y;

                    if (ny >= 250 && ny <= 483)
                    {
                        result.Add(new PlatformData(nx, ny, p.Width, p.Height));
                    }
                }

                if (sec.Length > 0)
                {
                    int maxX = 0;
                    foreach (var p in sec)
                    {
                        if (p.X + p.Width > maxX)
                            maxX = p.X + p.Width;
                    }
                    xOff += maxX + 100;
                }
            }

            if (result.Count < 5)
                return GenerateRandomLevel(numSections + 1);

            return result.ToArray();
        }

        private void mainWin_Load(object sender, EventArgs e)
        {
            MessageBox.Show("Controls:\nW/Space - Jump\nA - Move Left\nD - Move Right\n\n⚠️ Press ENTER to START!\n\nLevel " + currentLevelNumber + ": Reach the GOLD block!\n\n💡 Hit yellow ? blocks with your head!\n🍄 1st mushroom: Grow bigger + gain ❤\n🍄 More mushrooms: Gain extra ❤", "Super Mario", MessageBoxButtons.OK);
            Text = $"Super Mario - Level {currentLevelNumber}";
        }
    }

    public enum PowerUpType { Mushroom }

    public class Mushroom
    {
        public Point Position { get; set; }
        public PictureBox Visual { get; set; }
        public bool IsCollected { get; set; }

        public Mushroom(Point pos, PictureBox visual)
        {
            Position = pos;
            Visual = visual;
            IsCollected = false;
        }
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