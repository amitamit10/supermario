using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private List<Goomba> goombas = new List<Goomba>();
        private List<Koopa> koopas = new List<Koopa>();
        private List<FastEnemy> fastEnemies = new List<FastEnemy>();
        private List<Mushroom> spawnedMushrooms = new List<Mushroom>();
        private List<Coin> coins = new List<Coin>();
        private int coinCount = 0;

        private PlatformData[] currentLevel;
        private int currentLevelNumber = 1;
        private PlatformData[][] allLevels;
        private Random levelRandom = new Random();

        private Stopwatch _stopwatch = new Stopwatch();
        private long _lastTickMs = 0;
        private long _accumulatedMs = 0;
        private const long FIXED_STEP_MS = 16;

        // ── Question-block animation ─────────────────────────────────────────
        private Timer questionAnimTimer;
        private int questionAnimFrame = 0;
        private List<PictureBox> animatedBlocks = new List<PictureBox>();

        // ── Coin spin animation ──────────────────────────────────────────────
        private int coinAnimFrame = 0;

        // ── Player direction / animation ─────────────────────────────────────
        private bool facingRight = true;
        private int walkFrame = 0;
        private int walkFrameTimer = 0;
        private bool isWalking = false;

        // ── Background cloud / scenery ───────────────────────────────────────
        private static readonly (int wx, int y, int w)[] CLOUDS = {
            (200, 55, 130), (550, 35, 90), (900, 65, 160), (1200, 45, 110),
            (1500, 70, 140), (1800, 40, 100), (2100, 60, 130), (2400, 50, 90),
            (2700, 75, 120), (3000, 45, 110), (3300, 65, 150), (3600, 55, 95),
        };
        private static readonly (int wx, int y, int r)[] HILLS = {
            (300,  520, 180), (700,  530, 140), (1100, 515, 220),
            (1500, 525, 160), (1900, 510, 240), (2300, 520, 170),
            (2700, 515, 200), (3100, 530, 150), (3500, 520, 190),
        };

        private bool moveRight = false, moveLeft = false, jump = false;
        private int cameraX = 0;
        private const int SCROLL_THRESHOLD = 400;
        private const int LEVEL_PIXEL_WIDTH = 3000;
        private const int FLAGPOLE_X = 2750;
        private const int CAMERA_MAX = LEVEL_PIXEL_WIDTH - 982;
        private bool isPlayerSuper = false;
        private Size originalPlayerSize = new Size(68, 68);
        private Size superPlayerSize = new Size(82, 82);
        private bool isDying = false;
        private float deathTimer = 0f;
        private const float DEATH_ANIMATION_DURATION = 2000f;
        private float maxFallStartY = 0;
        private bool wasGroundedLastFrame = true;
        private const float FALL_DAMAGE_THRESHOLD = 60f;
        private bool canTakeFallDamage = true;
        private bool _levelComplete = false;

        // ── Invincibility frames ─────────────────────────────────────────────
        private bool isInvincible = false;
        private float invincibleTimer = 0f;
        private const float INVINCIBLE_DURATION = 1500f;

        // ── HUD ──────────────────────────────────────────────────────────────
        private Label _hudLabel;
        private Label _scoreLabel;
        private readonly Label[] _heartLabels = new Label[3];
        private int _lastHudHealth = -1;
        private bool _lastHudSuper = false;
        private int _lastHudLevel = -1;
        private int _lastHudScore = -1;
        private int _lastHudCoins = -1;

        // ════════════════════════════════════════════════════════════════════
        //  LEVEL SECTION TEMPLATES
        // ════════════════════════════════════════════════════════════════════
        private static readonly PlatformData[] SECTION_STAIRS = {
            new PlatformData(0, -20, 120, 20), new PlatformData(150, -70, 120, 20), new PlatformData(300, -120, 120, 20)
        };
        private static readonly PlatformData[] SECTION_GAP_JUMPS = {
            new PlatformData(0, -50, 100, 20), new PlatformData(170, -50, 100, 20), new PlatformData(340, -50, 100, 20)
        };
        private static readonly PlatformData[] SECTION_WAVE = {
            new PlatformData(0, -30, 100, 20), new PlatformData(150, -70, 100, 20),
            new PlatformData(300, -30, 100, 20), new PlatformData(450, -70, 100, 20)
        };
        private static readonly PlatformData[] SECTION_HIGH = {
            new PlatformData(0, -100, 100, 20), new PlatformData(150, -150, 100, 20), new PlatformData(300, -100, 100, 20)
        };
        private static readonly PlatformData[] SECTION_CHALLENGE = {
            new PlatformData(0, -50, 80, 20), new PlatformData(120, -90, 80, 20),
            new PlatformData(240, -50, 80, 20), new PlatformData(360, -90, 80, 20)
        };
        private static readonly PlatformData[] SECTION_DESCEND = {
            new PlatformData(0, -120, 100, 20), new PlatformData(150, -80, 100, 20),
            new PlatformData(300, -50, 100, 20), new PlatformData(450, -25, 100, 20)
        };
        private static readonly PlatformData[] SECTION_BRIDGE = {
            new PlatformData(80, -60, 280, 20)
        };
        private static readonly PlatformData[] SECTION_ZIGZAG = {
            new PlatformData(0, -50, 70, 20), new PlatformData(100, -90, 70, 20),
            new PlatformData(200, -50, 70, 20), new PlatformData(300, -90, 70, 20),
            new PlatformData(400, -50, 70, 20)
        };
        private static readonly PlatformData[] SECTION_ARCH = {
            new PlatformData(0, -40, 100, 20), new PlatformData(150, -90, 100, 20),
            new PlatformData(300, -140, 90, 20), new PlatformData(450, -90, 100, 20),
            new PlatformData(600, -40, 100, 20)
        };
        private static readonly PlatformData[] SECTION_WIDE_GAPS = {
            new PlatformData(0, -50, 130, 20), new PlatformData(240, -40, 130, 20), new PlatformData(480, -50, 130, 20)
        };
        private static readonly PlatformData[] SECTION_LEDGE_HOP = {
            new PlatformData(0, -40, 65, 20), new PlatformData(95, -70, 65, 20),
            new PlatformData(190, -40, 65, 20), new PlatformData(285, -70, 65, 20),
            new PlatformData(380, -40, 65, 20)
        };
        private static readonly PlatformData[] SECTION_SUSPENDED = {
            new PlatformData(0, -130, 100, 20), new PlatformData(160, -170, 90, 20), new PlatformData(310, -130, 100, 20)
        };
        private static readonly PlatformData[] SECTION_VALLEY = {
            new PlatformData(0, -100, 100, 20), new PlatformData(150, -40, 100, 20), new PlatformData(300, -100, 100, 20)
        };
        private static readonly PlatformData[] SECTION_MULTI_LEVEL = {
            new PlatformData(0, -30, 100, 20), new PlatformData(150, -80, 90, 20),
            new PlatformData(290, -130, 80, 20), new PlatformData(420, -80, 90, 20),
            new PlatformData(560, -30, 100, 20)
        };
        private static readonly PlatformData[] SECTION_CASTLE = {
            new PlatformData(0, -60, 90, 20), new PlatformData(110, -100, 40, 20),
            new PlatformData(170, -60, 90, 20), new PlatformData(280, -100, 40, 20),
            new PlatformData(340, -60, 90, 20)
        };
        // ── New sections ─────────────────────────────────────────────────────
        private static readonly PlatformData[] SECTION_TRIPLE_JUMP = {
            new PlatformData(0, -40, 75, 20), new PlatformData(115, -80, 75, 20),
            new PlatformData(230, -120, 75, 20), new PlatformData(345, -80, 75, 20)
        };
        private static readonly PlatformData[] SECTION_PYRAMID = {
            new PlatformData(0, -30, 100, 20), new PlatformData(130, -65, 90, 20),
            new PlatformData(250, -100, 80, 20), new PlatformData(360, -65, 90, 20),
            new PlatformData(480, -30, 100, 20)
        };
        private static readonly PlatformData[] SECTION_LONG_RUN = {
            new PlatformData(0, -55, 340, 20)
        };
        private static readonly PlatformData[] SECTION_STAGGER_NARROW = {
            new PlatformData(0, -60, 58, 20), new PlatformData(80, -100, 58, 20),
            new PlatformData(160, -60, 58, 20), new PlatformData(240, -100, 58, 20),
            new PlatformData(320, -60, 58, 20), new PlatformData(400, -100, 58, 20)
        };
        private static readonly PlatformData[] SECTION_SKYSCRAPER = {
            new PlatformData(0, -160, 90, 20), new PlatformData(150, -200, 70, 20),
            new PlatformData(280, -160, 90, 20)
        };
        private static readonly PlatformData[] SECTION_BOUNCY = {
            new PlatformData(0, -50, 80, 20), new PlatformData(130, -30, 70, 20),
            new PlatformData(250, -80, 80, 20), new PlatformData(380, -40, 80, 20),
            new PlatformData(510, -70, 80, 20)
        };

        private static readonly PlatformData[][] ALL_SECTIONS = {
            SECTION_STAIRS, SECTION_GAP_JUMPS, SECTION_WAVE, SECTION_HIGH, SECTION_CHALLENGE,
            SECTION_DESCEND, SECTION_BRIDGE, SECTION_ZIGZAG, SECTION_ARCH, SECTION_WIDE_GAPS,
            SECTION_LEDGE_HOP, SECTION_SUSPENDED, SECTION_VALLEY, SECTION_MULTI_LEVEL, SECTION_CASTLE,
            SECTION_TRIPLE_JUMP, SECTION_PYRAMID, SECTION_LONG_RUN, SECTION_STAGGER_NARROW,
            SECTION_SKYSCRAPER, SECTION_BOUNCY
        };

        // ════════════════════════════════════════════════════════════════════
        //  HAND-CRAFTED LEVELS
        // ════════════════════════════════════════════════════════════════════
        private static readonly PlatformData[] LEVEL_1 = {
            // Tutorial opening – wide, forgiving platforms close to ground
            new PlatformData(200, 470, 180, 20), new PlatformData(430, 450, 160, 20),
            new PlatformData(640, 430, 130, 20), new PlatformData(830, 450, 100, 20),
            // Classic ascending staircase
            new PlatformData(990, 453, 80, 20), new PlatformData(1090, 423, 80, 20),
            new PlatformData(1190, 393, 80, 20), new PlatformData(1290, 363, 80, 20),
            // Gap jumps at equal height – rhythm building
            new PlatformData(1440, 403, 110, 20), new PlatformData(1600, 403, 110, 20), new PlatformData(1760, 403, 90, 20),
            // Rising challenge
            new PlatformData(1910, 373, 100, 20), new PlatformData(2060, 333, 100, 20),
            // Gradual step-down descent
            new PlatformData(2210, 363, 100, 20), new PlatformData(2360, 393, 110, 20),
            // Long safe bridge breather
            new PlatformData(2530, 423, 220, 20),
            // Final wave approach to goal
            new PlatformData(2640, 403, 100, 20), new PlatformData(2720, 413, 140, 20)
        };

        private static readonly PlatformData[] LEVEL_2 = {
            // Ascending stair intro
            new PlatformData(150, 453, 80, 20), new PlatformData(260, 413, 80, 20),
            new PlatformData(370, 373, 80, 20), new PlatformData(480, 333, 80, 20),
            // Smooth descent linking to mid-height section
            new PlatformData(620, 373, 90, 20), new PlatformData(770, 403, 90, 20),
            // Gap challenge at consistent mid height
            new PlatformData(930, 383, 90, 20), new PlatformData(1090, 383, 90, 20),
            // Zigzag alternating heights
            new PlatformData(1250, 423, 75, 20), new PlatformData(1355, 383, 75, 20), new PlatformData(1460, 353, 75, 20),
            new PlatformData(1565, 383, 75, 20), new PlatformData(1670, 423, 75, 20),
            // High suspended section – peak challenge
            new PlatformData(1830, 303, 100, 20), new PlatformData(1980, 263, 100, 20), new PlatformData(2130, 303, 100, 20),
            // Descending steps back to ground level
            new PlatformData(2290, 353, 90, 20), new PlatformData(2430, 403, 90, 20), new PlatformData(2570, 433, 100, 20),
            // Final stretch to goal
            new PlatformData(2720, 393, 170, 20)
        };

        // ─────────────────────────────────────────────────────────────────────
        public mainWin()
        {
            InitializeComponent();
            InitializeGame();
        }

        // ════════════════════════════════════════════════════════════════════
        //  BACKGROUND PAINTING
        // ════════════════════════════════════════════════════════════════════
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int W = ClientSize.Width, H = ClientSize.Height;

            using (var sky = new LinearGradientBrush(new Point(0, 0), new Point(0, H),
                Color.FromArgb(92, 148, 252), Color.FromArgb(178, 218, 255)))
                g.FillRectangle(sky, 0, 0, W, H);

            DrawMountains(g, W, H, (int)(cameraX * 0.08));
            DrawHills(g, W, H, (int)(cameraX * 0.25));
            DrawClouds(g, W, H, (int)(cameraX * 0.12));
        }

        private void DrawMountains(Graphics g, int W, int H, int offset)
        {
            Color[] rows = { Color.FromArgb(120, 140, 185), Color.FromArgb(150, 170, 210) };
            int[][] peaks = {
                new[]{ 0,150, 200,350, 280,200, 450,370, 560,180, 730,360, 820,200, 1000,370, 1100,160, 1280,360, 1400,185, 1600,370, W+100,370 },
                new[]{ 0,200, 150,380, 260,230, 420,390, 530,210, 700,390, 800,230, 980,390, 1080,200, 1260,390, 1380,225, 1580,390, W+100,390 },
            };
            for (int r = 0; r < 2; r++)
            {
                var pts = new List<Point>();
                for (int i = 0; i < peaks[r].Length; i += 2)
                    pts.Add(new Point(peaks[r][i] - offset % (W + 200), peaks[r][i + 1]));
                for (int i = 0; i < peaks[r].Length; i += 2)
                    pts.Add(new Point(peaks[r][i] - offset % (W + 200) + W + 200, peaks[r][i + 1]));
                pts.Add(new Point(W + 300, H));
                pts.Add(new Point(-100, H));
                using (var b = new SolidBrush(rows[r]))
                    g.FillPolygon(b, pts.ToArray());
            }
        }

        private void DrawHills(Graphics g, int W, int H, int offset)
        {
            foreach (var (wx, hy, r) in HILLS)
            {
                int sx = wx - offset;
                for (int rep = -1; rep <= 1; rep++)
                {
                    int x = sx + rep * 4000;
                    if (x + r < -50 || x - r > W + 50) continue;
                    using (var b = new SolidBrush(Color.FromArgb(55, 130, 50)))
                        g.FillEllipse(b, x - r, hy - r / 2, r * 2, r);
                    using (var b = new SolidBrush(Color.FromArgb(80, 175, 65)))
                        g.FillEllipse(b, x - r + 10, hy - r / 2 - 8, r * 2 - 20, r - 5);
                }
            }
        }

        private void DrawClouds(Graphics g, int W, int H, int offset)
        {
            foreach (var (wx, cy, cw) in CLOUDS)
            {
                int sx = wx - offset;
                for (int rep = -1; rep <= 1; rep++)
                {
                    int x = sx + rep * 4000;
                    if (x + cw < -20 || x - 20 > W + 20) continue;
                    int ch = cw / 2;
                    using (var b = new SolidBrush(Color.FromArgb(50, 160, 180, 210)))
                        g.FillEllipse(b, x + 6, cy + 8, cw, ch);
                    using (var b = new SolidBrush(Color.FromArgb(245, 250, 255)))
                    {
                        g.FillEllipse(b, x, cy + ch / 3, cw * 6 / 10, ch * 2 / 3);
                        g.FillEllipse(b, x + cw / 3, cy, cw * 5 / 10, ch * 4 / 5);
                        g.FillEllipse(b, x + cw * 5 / 10, cy + ch / 4, cw * 5 / 10, ch * 2 / 3);
                    }
                    using (var b = new SolidBrush(Color.FromArgb(180, 255, 255, 255)))
                        g.FillEllipse(b, x + cw / 3 + 4, cy + 4, cw / 5, ch / 4);
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  GAME INIT
        // ════════════════════════════════════════════════════════════════════
        private void InitializeGame()
        {
            KeyPreview = true;
            DoubleBuffered = true;
            Focus();
            BackgroundImage = null;
            BackColor = Color.FromArgb(92, 148, 252);

            gameManager = new GameManager();
            allLevels = new PlatformData[][]
            {
                LEVEL_1, LEVEL_2,
                GenerateRandomLevel(6), GenerateRandomLevel(7), GenerateRandomLevel(8)
            };
            currentLevelNumber = 1;
            currentLevel = allLevels[0];

            if (picboxplayer != null)
            {
                picboxplayer.Image = null;
                picboxplayer.BackColor = Color.Transparent;
                picboxplayer.Size = originalPlayerSize;
                picboxplayer.SizeMode = PictureBoxSizeMode.Normal;
                picboxplayer.Paint += DrawPlayerSprite;
                picboxplayer.BringToFront();
            }

            player = new Player(new Point(100, 405), null);
            player.IsGrounded = true;
            player.Health = 3;
            player.OnDamageTaken = () => { BecomeNormal(); };

            picboxplayer.Location = player.Position;
            picboxplayer.BringToFront();

            InitHud();
            FormClosing += (s, e) => { gameTimer?.Stop(); questionAnimTimer?.Stop(); };

            questionAnimTimer = new Timer { Interval = 110 };
            questionAnimTimer.Tick += (s, e) =>
            {
                questionAnimFrame = (questionAnimFrame + 1) % 6;
                coinAnimFrame = (coinAnimFrame + 1) % 8;
                foreach (var b in animatedBlocks) b.Invalidate();
            };
            questionAnimTimer.Start();

            CreateLongLevel();

            gameTimer = new Timer { Interval = 8 };
            gameTimer.Tick += GameLoop;
            KeyDown += MainWin_KeyDown;
            KeyUp += MainWin_KeyUp;

            Text = $"Super Mario – Level {currentLevelNumber}";
        }

        // ════════════════════════════════════════════════════════════════════
        //  PLAYER SPRITE
        // ════════════════════════════════════════════════════════════════════
        private void DrawPlayerSprite(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;

            int w = picboxplayer.Width;
            int h = picboxplayer.Height;

            if (isInvincible && ((int)(invincibleTimer / 100f) % 2 == 0))
                return;

            var img = Properties.Resources.dcaeqy1_614416a8_3ae1_4448_94b4_e3ecefa3e53a;

            if (!facingRight)
            {
                g.TranslateTransform(w, 0);
                g.ScaleTransform(-1, 1);
            }

            g.DrawImage(img, 0, 0, w, h);
        }

        // ════════════════════════════════════════════════════════════════════
        //  GAME LOOP
        // ════════════════════════════════════════════════════════════════════
        private void GameLoop(object sender, EventArgs e)
        {
            if (!gameManager.IsGameRunning) return;

            long now = _stopwatch.ElapsedMilliseconds;
            long elapsed = Math.Min(now - _lastTickMs, 100);
            _lastTickMs = now;
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
                UpdateHud();
                CheckWinCondition();
                Invalidate(new Rectangle(0, 0, ClientSize.Width, 520));
            }
        }

        private void PhysicsStep()
        {
            if (isInvincible)
            {
                invincibleTimer += FIXED_STEP_MS;
                if (invincibleTimer >= INVINCIBLE_DURATION)
                {
                    isInvincible = false;
                    invincibleTimer = 0f;
                }
            }

            if (isDying) { HandleDeathAnimation(FIXED_STEP_MS); return; }

            // Pit detection – player fell off the bottom of the world
            if (player.Position.Y > 580)
            {
                isDying = true;
                deathTimer = 0f;
                return;
            }

            int dir = (moveRight ? 1 : 0) + (moveLeft ? -1 : 0);

            if (dir != 0) facingRight = (dir > 0);
            isWalking = dir != 0 && player.IsGrounded;

            if (isWalking)
            {
                walkFrameTimer++;
                if (walkFrameTimer >= 6) { walkFrameTimer = 0; walkFrame = (walkFrame + 1) % 3; }
            }
            else walkFrame = 0;

            player.Move(dir, jump);
            CheckPlatformCollisions();
            CheckQuestionBlockCollisions();
            HandleFallDamage();
            UpdateGoombas();
            UpdateKoopas();
            UpdateFastEnemies();
            UpdateMushrooms();
            UpdateCoins();

            picboxplayer.Invalidate();
        }

        // ════════════════════════════════════════════════════════════════════
        //  LEVEL BUILDING
        // ════════════════════════════════════════════════════════════════════
        private void CreateLongLevel()
        {
            CreateBrickGround();
            ClearPowerUps();
            foreach (var p in currentLevel)
                AddPlatform(p.X, p.Y, p.Width, p.Height);
            AddQuestionBlocks();
            AddCoins();
            AddFinishFlagpole();
            SpawnGoombas();
            SpawnKoopas();
            SpawnFastEnemies();
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
        private void AddQuestionBlocks()
        {
            // World-space positions; alternating mushroom/coin blocks
            int[] xPos = { 400, 700, 950, 1250, 1600, 1900, 2200, 2500 };
            int[] yPos = { 350, 380, 320, 360,  290,  340,  370,  310  };
            PowerUpType[] types = {
                PowerUpType.Mushroom, PowerUpType.Coin, PowerUpType.Mushroom, PowerUpType.Coin,
                PowerUpType.Mushroom, PowerUpType.Coin, PowerUpType.Mushroom, PowerUpType.Coin
            };

            for (int i = 0; i < xPos.Length; i++)
            {
                var box = new PictureBox
                {
                    Size = new Size(50, 50),
                    Location = new Point(xPos[i], yPos[i]),
                    BackColor = Color.Transparent,
                };

                var block = new QuestionBlock(box.Location, box, null, types[i]);

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

                        // Coin blocks show "C", mushroom blocks show "?"
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
        //  COINS
        // ════════════════════════════════════════════════════════════════════
        private void AddCoins()
        {
            // Rows of coins above platforms and floating in open areas
            var coinPositions = new List<Point>();

            // Rows above each platform from the current level layout
            foreach (var p in currentLevel)
            {
                int rowY = p.Y - 50;
                int count = Math.Max(1, p.Width / 40);
                for (int j = 0; j < count; j++)
                    coinPositions.Add(new Point(p.X + 10 + j * 38, rowY));
            }

            // Extra floating coins in the open areas
            int[] floatX = { 300, 500, 800, 1100, 1400, 1650, 1950, 2250, 2450, 2600 };
            int[] floatY = { 390, 360, 370, 350,  380,  360,  370,  350,  390,  380  };
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
                pb.Paint += DrawCoinSprite;
                Controls.Add(pb);
                pb.SendToBack();
                animatedBlocks.Add(pb);
                coins.Add(new Coin(pos, pb));
            }
        }

        private void DrawCoinSprite(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var pb = (PictureBox)sender;
            int w = pb.Width, h = pb.Height;

            // Animate coin by squishing horizontally
            float squeeze = 1f - 0.6f * Math.Abs((float)Math.Sin(coinAnimFrame * Math.PI / 4.0));
            int cw = Math.Max(4, (int)(w * squeeze));
            int cx = (w - cw) / 2;

            using (var lg = new LinearGradientBrush(
                new Point(cx, 0), new Point(cx + cw, h),
                Color.FromArgb(255, 230, 40), Color.FromArgb(200, 155, 0)))
                g.FillEllipse(lg, cx, 1, cw, h - 2);

            // Sheen
            if (cw > 6)
            {
                using (var sh = new SolidBrush(Color.FromArgb(120, 255, 255, 180)))
                    g.FillEllipse(sh, cx + 2, 3, cw / 3, h / 3);
            }

            using (var border = new Pen(Color.FromArgb(180, 130, 0), 1.5f))
                g.DrawEllipse(border, cx, 1, cw, h - 2);
        }

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
            pb.Paint += DrawMushroomSprite;
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
                if (newX < 0 || newX > 2960) m.VelocityX = -m.VelocityX;
                int newY = m.Position.Y + (int)m.VerticalVelocity;
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

        private void DrawMushroomSprite(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var pb = (PictureBox)sender;
            int w = pb.Width, h = pb.Height;

            // Stem / base (cream)
            using (var stem = new SolidBrush(Color.FromArgb(245, 230, 190)))
                g.FillEllipse(stem, 4, h / 2, w - 8, h / 2);

            // Cap (red with white spots)
            using (var cap = new LinearGradientBrush(
                new Point(0, 0), new Point(0, h / 2 + 4),
                Color.FromArgb(230, 50, 30), Color.FromArgb(170, 25, 10)))
                g.FillEllipse(cap, 0, 0, w, h / 2 + 8);

            // White spots
            using (var spot = new SolidBrush(Color.White))
            {
                g.FillEllipse(spot, 4,  5,  8, 8);
                g.FillEllipse(spot, w - 12, 5, 8, 8);
                g.FillEllipse(spot, w / 2 - 4, 2, 8, 8);
            }
            // Cap sheen
            using (var sh = new SolidBrush(Color.FromArgb(60, 255, 200, 200)))
                g.FillEllipse(sh, 3, 3, w / 3, h / 4);

            // Face – two small eyes
            int eyeY = h / 2 + 3;
            g.FillEllipse(Brushes.Black, w / 2 - 7, eyeY, 5, 5);
            g.FillEllipse(Brushes.Black, w / 2 + 2, eyeY, 5, 5);
        }

        // ════════════════════════════════════════════════════════════════════
        //  GOOMBA SPAWNING & UPDATE
        // ════════════════════════════════════════════════════════════════════
        private void SpawnGoombas()
        {
            int[] goombaX = { 600, 950, 1250, 1550, 1850, 2100 };
            foreach (int x in goombaX)
            {
                var goomba = new Goomba(new Point(x, 461));
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

                if (!goomba.IsGrounded)
                {
                    goomba.VerticalVelocity += 0.6f;
                    if (goomba.VerticalVelocity > 15f) goomba.VerticalVelocity = 15f;
                    goomba.Position = new Point(goomba.Position.X, goomba.Position.Y + (int)goomba.VerticalVelocity);
                }

                bool gGrounded = false;
                var gRect = goomba.Bounds;
                foreach (var plat in platforms)
                {
                    var pr = new Rectangle(
                        plat.PictureBox.Left + cameraX,
                        plat.Position.Y,
                        plat.PictureBox.Width,
                        plat.PictureBox.Height);

                    if (!gRect.IntersectsWith(pr)) continue;

                    int overlapTop    = gRect.Bottom - pr.Top;
                    int overlapBottom = pr.Bottom - gRect.Top;
                    int overlapLeft   = gRect.Right - pr.Left;
                    int overlapRight  = pr.Right - gRect.Left;
                    int minOverlap    = Math.Min(Math.Min(overlapTop, overlapBottom), Math.Min(overlapLeft, overlapRight));

                    if (minOverlap == overlapTop && overlapTop < 25)
                    {
                        goomba.Position = new Point(goomba.Position.X, pr.Top - goomba.Visual.Height);
                        goomba.VerticalVelocity = 0;
                        gGrounded = true;
                    }
                    else if (minOverlap == overlapLeft || minOverlap == overlapRight)
                    {
                        goomba.ReverseDirection();
                    }
                }
                goomba.IsGrounded = gGrounded;

                if (goomba.IsSquished)
                {
                    if (goomba.UpdateSquish(FIXED_STEP_MS)) goomba.Kill();
                    goomba.Visual.Location = new Point(goomba.Position.X - cameraX, goomba.Position.Y);
                    continue;
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

                if (fallingDown && playerAbove)
                {
                    goomba.Squish();
                    player.Bounce();
                    player.Score += 100;
                }
                else if (!isInvincible)
                {
                    player.TakeDamage(1);
                    isInvincible = true;
                    invincibleTimer = 0f;
                    if (player.Health <= 0) { isDying = true; deathTimer = 0f; }
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  KOOPA SPAWNING & UPDATE
        // ════════════════════════════════════════════════════════════════════
        private void SpawnKoopas()
        {
            int[] koopaX = { 750, 1350, 1700, 2300 };
            foreach (int x in koopaX)
            {
                var k = new Koopa(new Point(x, 457));
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

                // Gravity
                if (!k.IsGrounded)
                {
                    k.VerticalVelocity += 0.6f;
                    if (k.VerticalVelocity > 15f) k.VerticalVelocity = 15f;
                    k.Position = new Point(k.Position.X, k.Position.Y + (int)k.VerticalVelocity);
                }

                // Platform collision
                bool kGrounded = false;
                var kRect = k.Bounds;
                foreach (var plat in platforms)
                {
                    var pr = new Rectangle(
                        plat.PictureBox.Left + cameraX, plat.Position.Y,
                        plat.PictureBox.Width, plat.PictureBox.Height);
                    if (!kRect.IntersectsWith(pr)) continue;

                    int ot = kRect.Bottom - pr.Top, ob = pr.Bottom - kRect.Top;
                    int ol = kRect.Right - pr.Left, orr = pr.Right - kRect.Left;
                    int min = Math.Min(Math.Min(ot, ob), Math.Min(ol, orr));

                    if (min == ot && ot < 25)
                    {
                        k.Position = new Point(k.Position.X, pr.Top - k.Visual.Height);
                        k.VerticalVelocity = 0;
                        kGrounded = true;
                    }
                    else if (min == ol || min == orr) k.ReverseDirection();
                }
                k.IsGrounded = kGrounded;

                // Shell timer – remove after duration
                if (k.IsShell)
                {
                    if (k.UpdateShell(FIXED_STEP_MS)) k.Kill();
                    k.Visual.Location = new Point(k.Position.X - cameraX, k.Position.Y);
                    continue;
                }

                k.Update();
                k.Visual.Location = new Point(k.Position.X - cameraX, k.Position.Y);

                if (isDying) continue;
                var kWorld = new Rectangle(k.Position.X, k.Position.Y, k.Visual.Width, k.Visual.Height);
                if (!playerRect.IntersectsWith(kWorld)) continue;

                int pBottom = player.Position.Y + picboxplayer.Height;
                bool falling = pBottom - k.Position.Y < 24;
                bool above   = player.Position.Y < k.Position.Y + k.Visual.Height / 2;

                if (falling && above)
                {
                    k.Stomp();
                    player.Bounce();
                    player.Score += 150;
                }
                else if (!isInvincible)
                {
                    player.TakeDamage(1);
                    isInvincible = true;
                    invincibleTimer = 0f;
                    if (player.Health <= 0) { isDying = true; deathTimer = 0f; }
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  FAST ENEMY SPAWNING & UPDATE
        // ════════════════════════════════════════════════════════════════════
        private void SpawnFastEnemies()
        {
            int[] feX = { 1100, 1800, 2400 };
            foreach (int x in feX)
            {
                var fe = new FastEnemy(new Point(x, 465));
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

                // Gravity
                if (!fe.IsGrounded)
                {
                    fe.VerticalVelocity += 0.6f;
                    if (fe.VerticalVelocity > 15f) fe.VerticalVelocity = 15f;
                    fe.Position = new Point(fe.Position.X, fe.Position.Y + (int)fe.VerticalVelocity);
                }

                // Platform collision
                bool feGrounded = false;
                var feRect = fe.Bounds;
                foreach (var plat in platforms)
                {
                    var pr = new Rectangle(
                        plat.PictureBox.Left + cameraX, plat.Position.Y,
                        plat.PictureBox.Width, plat.PictureBox.Height);
                    if (!feRect.IntersectsWith(pr)) continue;

                    int ot = feRect.Bottom - pr.Top, ob = pr.Bottom - feRect.Top;
                    int ol = feRect.Right - pr.Left, orr = pr.Right - feRect.Left;
                    int min = Math.Min(Math.Min(ot, ob), Math.Min(ol, orr));

                    if (min == ot && ot < 25)
                    {
                        fe.Position = new Point(fe.Position.X, pr.Top - fe.Visual.Height);
                        fe.VerticalVelocity = 0;
                        feGrounded = true;
                    }
                    else if (min == ol || min == orr) fe.ReverseDirection();
                }
                fe.IsGrounded = feGrounded;

                if (fe.IsSquished)
                {
                    if (fe.UpdateSquish(FIXED_STEP_MS)) fe.Kill();
                    fe.Visual.Location = new Point(fe.Position.X - cameraX, fe.Position.Y);
                    continue;
                }

                fe.Update();
                fe.Visual.Location = new Point(fe.Position.X - cameraX, fe.Position.Y);

                if (isDying) continue;
                var feWorld = new Rectangle(fe.Position.X, fe.Position.Y, fe.Visual.Width, fe.Visual.Height);
                if (!playerRect.IntersectsWith(feWorld)) continue;

                int pBot = player.Position.Y + picboxplayer.Height;
                bool fall = pBot - fe.Position.Y < 24;
                bool abv  = player.Position.Y < fe.Position.Y + fe.Visual.Height / 2;

                if (fall && abv)
                {
                    fe.Squish();
                    player.Bounce();
                    player.Score += 200;
                }
                else if (!isInvincible)
                {
                    player.TakeDamage(1);
                    isInvincible = true;
                    invincibleTimer = 0f;
                    if (player.Health <= 0) { isDying = true; deathTimer = 0f; }
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  HUD  –  controls created once, text updated each tick
        // ════════════════════════════════════════════════════════════════════
        private void InitHud()
        {
            _hudLabel = new Label
            {
                Name = "hudLabel",
                AutoSize = false,
                Size = new Size(320, 38),
                Location = new Point(8, 8),
                BackColor = Color.FromArgb(160, 20, 20, 40),
                ForeColor = Color.White,
                Font = new Font("Courier New", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
            };
            Controls.Add(_hudLabel);
            _hudLabel.BringToFront();

            _scoreLabel = new Label
            {
                Name = "scoreLabel",
                AutoSize = false,
                Size = new Size(320, 30),
                Location = new Point(8, 48),
                BackColor = Color.FromArgb(160, 20, 20, 40),
                ForeColor = Color.FromArgb(255, 230, 80),
                Font = new Font("Courier New", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
            };
            Controls.Add(_scoreLabel);
            _scoreLabel.BringToFront();

            for (int i = 0; i < 3; i++)
            {
                _heartLabels[i] = new Label
                {
                    Name = "heartLabel",
                    Font = new Font("Arial", 20, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(180 + i * 36, 6),
                    BackColor = Color.Transparent,
                };
                Controls.Add(_heartLabels[i]);
                _heartLabels[i].BringToFront();
            }

            UpdateHud();
        }

        private void UpdateHud()
        {
            if (_hudLabel == null || _scoreLabel == null) return;

            if (currentLevelNumber != _lastHudLevel || isPlayerSuper != _lastHudSuper)
            {
                _lastHudLevel = currentLevelNumber;
                _lastHudSuper = isPlayerSuper;
                _hudLabel.Text = $"  LVL {currentLevelNumber}     {(isPlayerSuper ? "★ SUPER" : "")}";
                Text = $"Super Mario – Level {currentLevelNumber}{(isPlayerSuper ? "  ★ SUPER" : "")}";
            }

            if (player.Score != _lastHudScore || coinCount != _lastHudCoins)
            {
                _lastHudScore = player.Score;
                _lastHudCoins = coinCount;
                _scoreLabel.Text = $"  SCORE {player.Score:D6}   COINS {coinCount:D3}";
            }

            if (player.Health != _lastHudHealth)
            {
                _lastHudHealth = player.Health;
                for (int i = 0; i < 3; i++)
                {
                    if (_heartLabels[i] == null) continue;
                    bool filled = i < player.Health;
                    _heartLabels[i].Text = filled ? "❤" : "♡";
                    _heartLabels[i].ForeColor = filled
                        ? Color.FromArgb(255, 60, 80)
                        : Color.FromArgb(140, 100, 110);
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  INPUT
        // ════════════════════════════════════════════════════════════════════
        private void MainWin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) moveRight = true;
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) moveLeft = true;
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up || e.KeyCode == Keys.Space) jump = true;

            if (e.KeyCode == Keys.Enter && !gameManager.IsGameRunning)
            {
                _stopwatch.Restart(); _lastTickMs = 0; _accumulatedMs = 0;
                gameManager.StartGame();
                gameTimer.Start();
            }
            if (e.KeyCode == Keys.Escape)
            {
                gameTimer.Stop(); _stopwatch.Stop();
                gameManager.EndGame();
                Text = $"Super Mario – Level {currentLevelNumber} – PAUSED";
            }
        }

        private void MainWin_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) moveRight = false;
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) moveLeft = false;
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up || e.KeyCode == Keys.Space) jump = false;
        }

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
                    break;
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
            ClearCoins();
            ClearPowerUps();
        }

        // ════════════════════════════════════════════════════════════════════
        //  RANDOM LEVEL GENERATOR
        // ════════════════════════════════════════════════════════════════════
        private PlatformData[] GenerateRandomLevel(int numSections)
        {
            var openingPool = new[] { SECTION_STAIRS, SECTION_GAP_JUMPS, SECTION_WAVE, SECTION_LONG_RUN };
            List<PlatformData> result;
            int extra = 0;
            do
            {
                result = new List<PlatformData>();
                int xOff = 200, yBase = 483;
                int total = numSections + extra;
                for (int i = 0; i < total; i++)
                {
                    var sec = i == 0
                        ? openingPool[levelRandom.Next(openingPool.Length)]
                        : ALL_SECTIONS[levelRandom.Next(ALL_SECTIONS.Length)];
                    foreach (var p in sec)
                    {
                        int ny = yBase + p.Y;
                        if (ny >= 250 && ny <= 483)
                            result.Add(new PlatformData(xOff + p.X, ny, p.Width, p.Height));
                    }
                    if (sec.Length > 0)
                        xOff += sec.Max(p => p.X + p.Width) + 120;
                }
                extra++;
            } while (result.Count < 5 && extra < 20);

            return result.ToArray();
        }

        // ════════════════════════════════════════════════════════════════════
        //  LOAD EVENT
        // ════════════════════════════════════════════════════════════════════
        private void mainWin_Load(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Controls:\n" +
                "  W / ↑ / SPACE  – Jump\n" +
                "  A / ←           – Move Left\n" +
                "  D / →           – Move Right\n\n" +
                "⚠️  Press ENTER to START!\n\n" +
                $"Level {currentLevelNumber}: Reach the GOAL flagpole!\n\n" +
                "💡 Hit ? blocks for mushrooms!\n" +
                "💰 Hit C blocks for coins!\n" +
                "🍄 Collect mushrooms to grow SUPER!\n" +
                "👟 Jump ON goombas, koopas & red enemies!\n" +
                "❤ Fall damage applies for big drops!",
                "Super Mario", MessageBoxButtons.OK);
            Text = $"Super Mario – Level {currentLevelNumber}";
            _stopwatch.Start();
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  HELPER
    // ════════════════════════════════════════════════════════════════════════
    internal static class GraphicsExtensions
    {
        public static void FillRoundedRect(this Graphics g, Brush b, int x, int y, int w, int h, int r)
        {
            if (w <= 0 || h <= 0) return;
            r = Math.Min(r, Math.Min(w / 2, h / 2));
            using (var path = new GraphicsPath())
            {
                path.AddArc(x, y, r * 2, r * 2, 180, 90);
                path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
                path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
                path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
                path.CloseFigure();
                g.FillPath(b, path);
            }
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  DATA CLASSES
    // ════════════════════════════════════════════════════════════════════════
    public enum PowerUpType { Mushroom, Coin }

    public class Mushroom
    {
        public Point Position { get; set; }
        public PictureBox Visual { get; set; }
        public bool IsCollected { get; set; }
        public float VelocityX { get; set; }
        public float VerticalVelocity { get; set; }
        public bool IsGrounded { get; set; }

        public Mushroom(Point pos, PictureBox visual)
        {
            Position = pos;
            Visual = visual;
            IsCollected = false;
            VelocityX = 1.8f;
            VerticalVelocity = 0f;
            IsGrounded = false;
        }
    }

    public class Coin
    {
        public Point Position { get; set; }
        public PictureBox Visual { get; set; }
        public bool IsCollected { get; set; }
        public Coin(Point pos, PictureBox visual) { Position = pos; Visual = visual; IsCollected = false; }
    }

    public class QuestionBlock
    {
        public Point Position { get; set; }
        public PictureBox Visual { get; set; }
        public Label QuestionLabel { get; set; }
        public bool IsHit { get; set; }
        public PowerUpType PowerUpInside { get; set; }
        public QuestionBlock(Point pos, PictureBox visual, Label label, PowerUpType powerUp)
        { Position = pos; Visual = visual; QuestionLabel = label; IsHit = false; PowerUpInside = powerUp; }
    }
}
