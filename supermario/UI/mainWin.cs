using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace supermario
{
    public partial class mainWin : Form
    {
        // ════════════════════════════════════════════════════════════════════
        //  שדות / Fields
        // ════════════════════════════════════════════════════════════════════
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
        private List<Mushroom> spawnedMushrooms = new List<Mushroom>();
        private List<Coin> coins = new List<Coin>();
        private int coinCount = 0;

        private List<Goomba> goombas = new List<Goomba>();
        private List<Koopa> koopas = new List<Koopa>();
        private List<FastEnemy> fastEnemies = new List<FastEnemy>();
        private List<JumpingEnemy> jumpingEnemies = new List<JumpingEnemy>();
        private List<PlatformPatrolEnemy> patrolEnemies = new List<PlatformPatrolEnemy>();
        private List<FlyingEnemy> flyingEnemies = new List<FlyingEnemy>();

        private PlatformData[] currentLevel;
        private int currentLevelNumber = 1;
        private PlatformData[][] allLevels;
        private Random levelRandom = new Random();

        private const long FIXED_STEP_MS = 16;

        private int globalTick = 0;
        private int questionAnimFrame = 0;
        private int _animStepCount = 0;
        private int coinAnimFrame = 0;

        private bool facingRight = true;
        private bool isWalking = false;

        // קלט / input
        private bool moveRight = false, moveLeft = false, jump = false, _prevJump = false;

        // מצב AI / AI mode
        private ML.NeuralNetwork _aiNet = null;
        private bool _aiMode = false;
        private static readonly List<Rectangle> _emptyRects = new List<Rectangle>();
        private List<Rectangle> _platformWorldRects = new List<Rectangle>();

        // מצלמה / camera
        private int cameraX = 0;
        private const int SCROLL_THRESHOLD = 400;
        private const int LEVEL_PIXEL_WIDTH = 3000;
        private const int FLAGPOLE_X = 2750;
        private int CameraMax => Math.Max(0, LEVEL_PIXEL_WIDTH - ClientSize.Width);
        private const int PLAYER_START_X = 100;
        private const int GROUND_TOP_Y = 513;

        private bool isPlayerSuper = false;
        private Size originalPlayerSize = new Size(68, 68);
        private Size superPlayerSize = new Size(82, 82);

        private bool isDying = false;
        private float deathTimer = 0f;
        private const float DEATH_ANIMATION_DURATION = 2000f;
        private float maxFallStartY = 0;
        private bool wasGroundedLastFrame = true;
        private const int PIT_DEATH_Y     = 580;
        private const int ENEMY_DESPAWN_Y = 620;
        private const int ENEMY_FELL_Y    = 600;
        private const float FALL_DAMAGE_THRESHOLD = 220f;
        private bool canTakeFallDamage = true;
        private bool _levelComplete = false;

        private bool isInvincible = false;
        private float invincibleTimer = 0f;
        private const float INVINCIBLE_DURATION = 1500f;

        private Label _hudLabel;
        private Label _scoreLabel;
        private readonly PictureBox[] _hearts = new PictureBox[3];
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

        public mainWin(ML.NeuralNetwork ai)
        {
            _aiNet = ai;
            _aiMode = ai != null;
            InitializeComponent();
            InitializeGame();
        }

        // ════════════════════════════════════════════════════════════════════
        //  אתחול המשחק / Game init
        // ════════════════════════════════════════════════════════════════════
        private void InitializeGame()
        {
            Sprites.LoadAll();

            KeyPreview = true;
            DoubleBuffered = true;
            Focus();

            if (Sprites.Background != null)
            {
                BackgroundImage = Sprites.Background;
                BackgroundImageLayout = ImageLayout.Tile;
            }
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
                picboxplayer.BackColor = Color.Transparent;
                picboxplayer.Size = originalPlayerSize;
                picboxplayer.SizeMode = PictureBoxSizeMode.StretchImage;
                picboxplayer.Image = Sprites.MarioIdle;
                picboxplayer.BringToFront();
            }

            player = new Player(GetPlayerStartPosition(), null);
            player.IsGrounded = true;
            player.Health = 3;
            player.MaxX = LEVEL_PIXEL_WIDTH - picboxplayer.Width;
            player.OnDamageTaken = () => { BecomeNormal(); };

            picboxplayer.Location = player.Position;
            picboxplayer.BringToFront();

            InitHud();
            FormClosing += (s, e) => { gameTimer?.Stop(); };

            CreateLongLevel();
            BuildPlatformWorldRects();

            gameTimer = new Timer { Interval = (int)FIXED_STEP_MS };
            gameTimer.Tick += GameLoop;
            KeyDown += MainWin_KeyDown;
            KeyUp += MainWin_KeyUp;

            Text = _aiMode
                ? "Super Mario – AI Mode"
                : $"Super Mario – Level {currentLevelNumber}";
        }

        // ════════════════════════════════════════════════════════════════════
        //  לולאת המשחק / Game loop
        // ════════════════════════════════════════════════════════════════════
        private void GameLoop(object sender, EventArgs e)
        {
            if (!gameManager.IsGameRunning) return;

            globalTick = (globalTick + 1) % 168;

            PhysicsStep();
            UpdateCamera();
            UpdateHud();
            CheckWinCondition();

            _animStepCount++;
            if (_animStepCount >= 7)
            {
                _animStepCount = 0;
                questionAnimFrame = (questionAnimFrame + 1) % 6;
                coinAnimFrame = (coinAnimFrame + 1) % 8;
                UpdateAnimatedSprites();
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

            if (isDying)
            {
                isWalking = false;
                HandleDeathAnimation(FIXED_STEP_MS);
                return;
            }

            if (player.Position.Y > PIT_DEATH_Y)
            {
                player.Position = new Point(player.Position.X, PIT_DEATH_Y);
                isDying = true;
                deathTimer = 0f;
                return;
            }

            int dir;
            bool jumpEdge;

            if (_aiMode && _aiNet != null)
            {
                var inputs = ML.MarioAgent.ComputeInputs(
                    player.Position, picboxplayer.Width, picboxplayer.Height,
                    _platformWorldRects, _emptyRects, player.IsGrounded);
                double[] outputs = _aiNet.Forward(inputs);
                dir = outputs[0] > 0.33 ? 1 : outputs[0] < -0.33 ? -1 : 0;
                bool aiJump = outputs[1] > 0.5;
                jumpEdge = aiJump && !_prevJump;
                _prevJump = aiJump;
            }
            else
            {
                dir = (moveRight ? 1 : 0) + (moveLeft ? -1 : 0);
                jumpEdge = jump && !_prevJump;
                _prevJump = jump;
            }

            if (dir != 0) facingRight = (dir > 0);
            isWalking = dir != 0 && player.IsGrounded;

            player.Move(dir, jumpEdge);
            CheckPlatformCollisions();
            isWalking = dir != 0 && player.IsGrounded;
            HandleFallDamage();

            SuspendLayout();
            UpdateGoombas();
            UpdateKoopas();
            UpdateFastEnemies();
            UpdateJumpingEnemies();
            UpdatePatrolEnemies();
            UpdateFlyingEnemies();
            UpdateMushrooms();
            UpdateCoins();
            ResumeLayout(false);

            UpdatePlayerSprite();
        }

        private void BuildPlatformWorldRects()
        {
            _platformWorldRects.Clear();
            foreach (var gos in platforms)
                _platformWorldRects.Add(gos.Bounds);
        }

        private void UpdateAnimatedSprites()
        {
            if (Sprites.Coin != null && Sprites.Coin.Length > 0)
            {
                Image coinImg = Sprites.Coin[coinAnimFrame % Sprites.Coin.Length];
                foreach (var c in coins)
                    if (!c.IsCollected && c.Visual != null && c.Visual.Image != coinImg)
                        c.Visual.Image = coinImg;
            }

            foreach (var qb in questionBlocks)
            {
                if (qb.Visual == null) continue;
                Image qi = qb.IsHit
                    ? Sprites.EmptyBlock
                    : (Sprites.Question != null && Sprites.Question.Length > 0
                        ? Sprites.Question[questionAnimFrame % Sprites.Question.Length]
                        : null);
                if (qi != null && qb.Visual.Image != qi) qb.Visual.Image = qi;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  קלט / Input
        // ════════════════════════════════════════════════════════════════════
        private void MainWin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) moveRight = true;
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) moveLeft = true;
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up || e.KeyCode == Keys.Space) jump = true;

            if (e.KeyCode == Keys.Enter && !gameManager.IsGameRunning)
            {
                gameManager.StartGame();
                gameTimer.Start();
            }
            if (e.KeyCode == Keys.Escape && !isDying)
            {
                gameTimer.Stop();
                gameManager.EndGame();
                moveRight = moveLeft = jump = false;
                _prevJump = false;
                Text = $"Super Mario – Level {currentLevelNumber} – PAUSED  [Enter to Resume]";
            }
        }

        private void MainWin_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) moveRight = false;
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) moveLeft = false;
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up || e.KeyCode == Keys.Space) jump = false;
        }

        // ════════════════════════════════════════════════════════════════════
        //  אירוע טעינה / Load event
        // ════════════════════════════════════════════════════════════════════
        private void mainWin_Load(object sender, EventArgs e)
        {
            if (!_aiMode)
                Text = $"Super Mario – Level {currentLevelNumber}";
            gameManager.StartGame();
            gameTimer.Start();
        }
    }
}
