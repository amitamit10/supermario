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
        private List<JumpingEnemy> jumpingEnemies = new List<JumpingEnemy>();
        private List<PlatformPatrolEnemy> patrolEnemies = new List<PlatformPatrolEnemy>();
        private List<FlyingEnemy> flyingEnemies = new List<FlyingEnemy>();
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

        // ── Question-block animation ─────────────────────────────────────
        private int globalTick = 0;
        private int questionAnimFrame = 0;
        private int _animStepCount = 0;
        private List<PictureBox> animatedBlocks = new List<PictureBox>();

        // ── Coin spin animation ─────────────────────────────────────────
        private int coinAnimFrame = 0;

        // ── Player direction / animation ───────────────────────────────
        private bool facingRight = true;
        private bool isWalking = false;

        // ── Background cloud / scenery ────────────────────────────────
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

        private bool moveRight = false, moveLeft = false, jump = false, _prevJump = false;
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
        private const float FALL_DAMAGE_THRESHOLD = 120f;
        private bool canTakeFallDamage = true;
        private bool _levelComplete = false;

        // ── Invincibility frames ──────────────────────────────────────
        private bool isInvincible = false;
        private float invincibleTimer = 0f;
        private const float INVINCIBLE_DURATION = 1500f;

        // ── HUD ───────────────────────────────────────────────────────
        private Label _hudLabel;
        private Label _scoreLabel;
        private readonly Label[] _heartLabels = new Label[3];
        private int _lastHudHealth = -1;
        private bool _lastHudSuper = false;
        private int _lastHudLevel = -1;
        private int _lastHudScore = -1;
        private int _lastHudCoins = -1;

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
            g.SmoothingMode = SmoothingMode.None;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            int W = ClientSize.Width, H = ClientSize.Height;

            using (var sky = new LinearGradientBrush(new Point(0, 0), new Point(0, H),
                Color.FromArgb(92, 148, 252), Color.FromArgb(178, 218, 255)))
                g.FillRectangle(sky, 0, 0, W, H);

            if (TextureLoader.TryGetSheet("bg", out var bg))
            {
                int tileHeight = Math.Max(1, H);
                int tileWidth = Math.Max(1, bg.Width * tileHeight / bg.Height);
                int offset = tileWidth == 0 ? 0 : (int)(cameraX * 0.12) % tileWidth;
                for (int x = -offset; x < W; x += tileWidth)
                {
                    g.DrawImage(bg, new Rectangle(x, 0, tileWidth, tileHeight),
                        new Rectangle(0, 0, bg.Width, bg.Height), GraphicsUnit.Pixel);
                }
                return;
            }

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
            foreach (var hill in HILLS)
            {
                int wx = hill.wx; int hy = hill.y; int r = hill.r;
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
            foreach (var cloud in CLOUDS)
            {
                int wx = cloud.wx; int cy = cloud.y; int cw = cloud.w;
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
            TextureLoader.LoadAll();

            KeyPreview = true;
            DoubleBuffered = true;
            Focus();
            BackgroundImage = null;
            BackColor = Color.FromArgb(92, 148, 252);

            gameManager = new GameManager();
            allLevels = new PlatformData[][]
            {
                LEVEL_1, LEVEL_2, LEVEL_3,
                GenerateRandomLevel(7), GenerateRandomLevel(9)
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
            FormClosing += (s, e) => { gameTimer?.Stop(); };

            CreateLongLevel();

            gameTimer = new Timer { Interval = 16 };
            gameTimer.Tick += GameLoop;
            KeyDown += MainWin_KeyDown;
            KeyUp += MainWin_KeyUp;

            Text = $"Super Mario – Level {currentLevelNumber}";
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
                globalTick++;
                PhysicsStep();
                _accumulatedMs -= FIXED_STEP_MS;
                didStep = true;
            }

            if (didStep)
            {
                UpdateCamera();
                UpdateHud();
                CheckWinCondition();
                _animStepCount++;
                if (_animStepCount >= 7)
                {
                    _animStepCount = 0;
                    questionAnimFrame = (questionAnimFrame + 1) % 6;
                    coinAnimFrame = (coinAnimFrame + 1) % 8;
                    foreach (var b in animatedBlocks) b.Invalidate();
                }
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

            // Edge-detect the jump key so holding it down doesn't cause auto-jump on landing
            bool jumpEdge = jump && !_prevJump;
            _prevJump = jump;
            player.Move(dir, jumpEdge, jump);
            CheckPlatformCollisions();
            CheckQuestionBlockCollisions();
            HandleFallDamage();
            UpdateGoombas();
            UpdateKoopas();
            UpdateFastEnemies();
            UpdateJumpingEnemies();
            UpdatePatrolEnemies();
            UpdateFlyingEnemies();
            UpdateMushrooms();
            UpdateCoins();

            picboxplayer.Invalidate();
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
        //  LOAD EVENT
        // ════════════════════════════════════════════════════════════════════
        private void mainWin_Load(object sender, EventArgs e)
        {
            Text = $"Super Mario – Level {currentLevelNumber}";
            _stopwatch.Restart();
            _lastTickMs = 0;
            _accumulatedMs = 0;
            gameManager.StartGame();
            gameTimer.Start();
        }
    }
}
