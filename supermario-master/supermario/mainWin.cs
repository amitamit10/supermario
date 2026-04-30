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
        private List<Goomba> goombas = new List<Goomba>();   // ← Goombas list
        private PlatformData[] currentLevel;
        private int currentLevelNumber = 1;
        private PlatformData[][] allLevels;
        private Random levelRandom = new Random();

        private Stopwatch _stopwatch = new Stopwatch();
        private long _lastTickMs = 0;
        private long _accumulatedMs = 0;
        private const long FIXED_STEP_MS = 16;

        // ── Question-block animation ────────────────────────────────────────
        private Timer questionAnimTimer;
        private int questionAnimFrame = 0;
        private List<PictureBox> animatedBlocks = new List<PictureBox>();

        // ── Player direction / animation ────────────────────────────────────
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
        private const int LEVEL_PIXEL_WIDTH = 3000;  // matches CreateBrickGround brick span
        private const int FLAGPOLE_X = 2750;          // X coordinate of the finish flagpole
        private const int CAMERA_MAX = LEVEL_PIXEL_WIDTH - 982; // 982 = ClientSize.Width
        private bool isPlayerSuper = false;
        private Size originalPlayerSize = new Size(68, 68);
        private Size superPlayerSize = new Size(82, 82);
        private bool isDying = false;
        private float deathTimer = 0f;
        private const float DEATH_ANIMATION_DURATION = 2000f;
        private float maxFallStartY = 0;
        private bool wasGroundedLastFrame = false;
        private const float FALL_DAMAGE_THRESHOLD = 60f;
        private bool canTakeFallDamage = true;
        private bool _levelComplete = false;

        // ── Invincibility frames after damage ────────────────────────────────
        private bool isInvincible = false;
        private float invincibleTimer = 0f;
        private const float INVINCIBLE_DURATION = 1500f; // ms

        // ── Persistent HUD (created once in InitHud, updated each tick) ──────
        private Label _hudLabel;
        private readonly Label[] _heartLabels = new Label[3];

        // ── Level data ───────────────────────────────────────────────────────
        private static readonly PlatformData[] SECTION_STAIRS = { new PlatformData(0, 30, 120, 20), new PlatformData(150, -20, 120, 20), new PlatformData(300, -70, 120, 20) };
        private static readonly PlatformData[] SECTION_GAP_JUMPS = { new PlatformData(0, -20, 100, 20), new PlatformData(170, -20, 100, 20), new PlatformData(340, -20, 100, 20) };
        private static readonly PlatformData[] SECTION_WAVE = { new PlatformData(0, 0, 100, 20), new PlatformData(150, -40, 100, 20), new PlatformData(300, 0, 100, 20), new PlatformData(450, -40, 100, 20) };
        private static readonly PlatformData[] SECTION_HIGH = { new PlatformData(0, -80, 100, 20), new PlatformData(150, -130, 100, 20), new PlatformData(300, -80, 100, 20) };
        private static readonly PlatformData[] SECTION_CHALLENGE = { new PlatformData(0, -30, 80, 20), new PlatformData(130, -70, 80, 20), new PlatformData(260, -30, 80, 20), new PlatformData(390, -70, 80, 20) };
        private static readonly PlatformData[][] ALL_SECTIONS = { SECTION_STAIRS, SECTION_GAP_JUMPS, SECTION_WAVE, SECTION_HIGH, SECTION_CHALLENGE };

        private static readonly PlatformData[] LEVEL_1 = {
            new PlatformData(200,483,120,20), new PlatformData(350,433,120,20), new PlatformData(500,383,120,20),
            new PlatformData(700,433,100,20), new PlatformData(870,433,100,20), new PlatformData(1040,433,100,20),
            new PlatformData(1200,413,100,20), new PlatformData(1350,353,100,20), new PlatformData(1500,413,100,20),
            new PlatformData(1650,333,100,20), new PlatformData(1800,283,100,20), new PlatformData(1950,333,100,20),
            new PlatformData(2100,433,120,20), new PlatformData(2270,433,120,20), new PlatformData(2440,433,120,20),
            new PlatformData(2650,383,200,20)
        };
        private static readonly PlatformData[] LEVEL_2 = {
            new PlatformData(150,473,80,20), new PlatformData(300,433,80,20), new PlatformData(450,393,80,20),
            new PlatformData(600,353,80,20), new PlatformData(750,403,70,20), new PlatformData(880,353,70,20),
            new PlatformData(1010,403,70,20), new PlatformData(1140,353,70,20), new PlatformData(1270,303,100,20),
            new PlatformData(1420,253,100,20), new PlatformData(1570,303,100,20), new PlatformData(1720,253,100,20),
            new PlatformData(1870,353,100,20), new PlatformData(2020,403,100,20), new PlatformData(2170,433,100,20),
            new PlatformData(2300,373,80,20), new PlatformData(2430,333,80,20), new PlatformData(2560,373,80,20),
            new PlatformData(2700,383,200,20)
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

            // ── Player picture box ────────────────────────────────────────────
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
            // FIX: use = not += to prevent stacking on restarts
            player.OnDamageTaken = () => { BecomeNormal(); };

            picboxplayer.Location = player.Position;
            picboxplayer.BringToFront();

            InitHud();
            FormClosing += (s, e) => { gameTimer?.Stop(); questionAnimTimer?.Stop(); };

            // ── Question-block animation timer ───────────────────────────────
            questionAnimTimer = new Timer { Interval = 110 };
            questionAnimTimer.Tick += (s, e) =>
            {
                questionAnimFrame = (questionAnimFrame + 1) % 6;
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
        //  PLAYER SPRITE – uses resource image, flips when facing left
        // ════════════════════════════════════════════════════════════════════
        private void DrawPlayerSprite(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;

            int w = picboxplayer.Width;
            int h = picboxplayer.Height;

            // Flash when invincible
            if (isInvincible && ((int)(invincibleTimer / 100f) % 2 == 0))
                return; // skip draw = flicker effect

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
            // Tick invincibility
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
            AddFinishFlagpole();
            SpawnGoombas();          // ← spawn goombas after level geometry
            picboxplayer.BringToFront();
            // HUD must stay above all game objects including the player sprite
            _hudLabel?.BringToFront();
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
            int h = 200;  // flagpole PictureBox height; width (80) is embedded in draw coords below
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

        private void AddQuestionBlocks()
        {
            int[] xPos = { 400, 650, 950, 1300, 1700, 2150, 2500 };
            int[] yPos = { 350, 380, 320, 360, 280, 350, 380 };

            for (int i = 0; i < xPos.Length; i++)
            {
                var box = new PictureBox
                {
                    Size = new Size(50, 50),
                    Location = new Point(xPos[i], yPos[i]),
                    BackColor = Color.Transparent,
                };

                var block = new QuestionBlock(box.Location, box, null, PowerUpType.Mushroom);

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
                        using (var qFont = new Font("Arial", 24, FontStyle.Bold))
                        using (var qBrush = new SolidBrush(Color.FromArgb(100, 50, 0)))
                        using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                            g.DrawString("?", qFont, qBrush, new RectangleF(0, qOff, bw, bh), sf);
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
        }

        // ════════════════════════════════════════════════════════════════════
        //  GOOMBA SPAWNING
        // ════════════════════════════════════════════════════════════════════
        private void SpawnGoombas()
        {
            // Place goombas at varied positions across the level
            int[] goombaX = { 480, 850, 1150, 1450, 1750, 2050, 2350 };
            foreach (int x in goombaX)
            {
                var goomba = new Goomba(new Point(x, 461));
                Controls.Add(goomba.Visual);
                goomba.Visual.SendToBack();
                goombas.Add(goomba);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  GOOMBA UPDATE + COLLISION
        // ════════════════════════════════════════════════════════════════════
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
                    goomba.Visual.Dispose();   // release native control handle immediately
                    goombas.RemoveAt(i);
                    continue;
                }

                // ── Apply gravity to goomba ──────────────────────────────────
                if (!goomba.IsGrounded)
                {
                    goomba.VerticalVelocity += 0.6f;
                    if (goomba.VerticalVelocity > 15f) goomba.VerticalVelocity = 15f;
                    goomba.Position = new Point(
                        goomba.Position.X,
                        goomba.Position.Y + (int)goomba.VerticalVelocity);
                }

                // ── Goomba platform collision ────────────────────────────────
                bool gGrounded = false;
                var gRect = goomba.Bounds;
                foreach (var plat in platforms)
                {
                    var pr = new Rectangle(
                        plat.PictureBox.Left + cameraX,   // world coords
                        plat.Position.Y,
                        plat.PictureBox.Width,
                        plat.PictureBox.Height);

                    if (!gRect.IntersectsWith(pr)) continue;

                    int overlapTop = gRect.Bottom - pr.Top;
                    int overlapBottom = pr.Bottom - gRect.Top;
                    int overlapLeft = gRect.Right - pr.Left;
                    int overlapRight = pr.Right - gRect.Left;
                    int minOverlap = Math.Min(Math.Min(overlapTop, overlapBottom),
                                                 Math.Min(overlapLeft, overlapRight));

                    if (minOverlap == overlapTop && overlapTop < 25)
                    {
                        // Land on top
                        goomba.Position = new Point(goomba.Position.X, pr.Top - Goomba.NormalSize.Height);
                        goomba.VerticalVelocity = 0;
                        gGrounded = true;
                    }
                    else if (minOverlap == overlapLeft || minOverlap == overlapRight)
                    {
                        // Hit a wall – reverse
                        goomba.ReverseDirection();
                    }
                }
                goomba.IsGrounded = gGrounded;

                // ── Squish countdown ─────────────────────────────────────────
                if (goomba.IsSquished)
                {
                    if (goomba.UpdateSquish(FIXED_STEP_MS)) goomba.Kill();
                    // Still sync screen position while squished
                    goomba.Visual.Location = new Point(goomba.Position.X - cameraX, goomba.Position.Y);
                    continue;
                }

                // ── Normal walk update ───────────────────────────────────────
                goomba.Update();

                // ── Sync visual to screen ────────────────────────────────────
                goomba.Visual.Location = new Point(goomba.Position.X - cameraX, goomba.Position.Y);

                // ── Player vs Goomba collision ───────────────────────────────
                if (isDying) continue;
                var gWorldRect = new Rectangle(
                    goomba.Position.X, goomba.Position.Y,
                    goomba.Visual.Width, goomba.Visual.Height);

                if (!playerRect.IntersectsWith(gWorldRect)) continue;

                // Stomped? Player must be falling and feet overlap goomba top
                int playerBottom = player.Position.Y + picboxplayer.Height;
                int goombaTop = goomba.Position.Y;
                bool fallingDown = playerBottom - goombaTop < 24;
                bool playerAbove = player.Position.Y < goomba.Position.Y + goomba.Visual.Height / 2;

                if (fallingDown && playerAbove)
                {
                    // Stomp! Give the player a real upward bounce impulse
                    goomba.Squish();
                    player.Bounce();
                }
                else if (!isInvincible)
                {
                    // Side hit – damage with invincibility frames
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
                Size = new Size(300, 40),
                Location = new Point(8, 8),
                BackColor = Color.FromArgb(160, 20, 20, 40),
                ForeColor = Color.White,
                Font = new Font("Courier New", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
            };
            Controls.Add(_hudLabel);
            _hudLabel.BringToFront();

            for (int i = 0; i < 3; i++)
            {
                _heartLabels[i] = new Label
                {
                    Name = "heartLabel",
                    Font = new Font("Arial", 20, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(160 + i * 36, 6),
                    BackColor = Color.Transparent,
                };
                Controls.Add(_heartLabels[i]);
                _heartLabels[i].BringToFront();
            }

            UpdateHud();
        }

        private int _lastHudHealth = -1;
        private bool _lastHudSuper = false;
        private int _lastHudLevel = -1;

        private void UpdateHud()
        {
            if (_hudLabel == null) return;

            // Only push string updates when something actually changed (avoids repaint churn)
            if (currentLevelNumber != _lastHudLevel || isPlayerSuper != _lastHudSuper)
            {
                _lastHudLevel = currentLevelNumber;
                _lastHudSuper = isPlayerSuper;
                _hudLabel.Text = $"  LVL {currentLevelNumber}     {(isPlayerSuper ? "★ SUPER" : "")}";
                Text = $"Super Mario – Level {currentLevelNumber}{(isPlayerSuper ? "  ★ SUPER" : "")}";
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

                int overlapTop = playerRect.Bottom - platRect.Top;
                int overlapBottom = platRect.Bottom - playerRect.Top;
                int overlapLeft = playerRect.Right - platRect.Left;
                int overlapRight = platRect.Right - playerRect.Left;
                int minOverlap = Math.Min(Math.Min(overlapTop, overlapBottom),
                                             Math.Min(overlapLeft, overlapRight));

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

                if (!isPlayerSuper) BecomeSuper();
                else player.Health = Math.Min(player.Health + 1, 3);
            }
        }

        private void CheckWinCondition()
        {
            // _levelComplete prevents double-trigger (e.g. game loop fires again before MessageBox closes)
            if (_levelComplete || isDying || player.Position.X < FLAGPOLE_X) return;
            _levelComplete = true;
            gameTimer.Stop();
            if (currentLevelNumber < allLevels.Length)
            {
                MessageBox.Show($"Level {currentLevelNumber} Complete! 🎉", "Level Complete!", MessageBoxButtons.OK);
                LoadNextLevel();
            }
            else
            {
                MessageBox.Show("You completed ALL levels! 🏆", "YOU WIN!", MessageBoxButtons.OK);
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
        //  FALL DAMAGE  ← FIXED: was subtracting backwards
        // ════════════════════════════════════════════════════════════════════
        private void HandleFallDamage()
        {
            // Record Y the moment the player leaves the ground
            if (wasGroundedLastFrame && !player.IsGrounded)
            {
                maxFallStartY = player.Position.Y;
                canTakeFallDamage = true;
            }

            // On landing, measure how far we fell (Y increases downward)
            if (!wasGroundedLastFrame && player.IsGrounded && !isDying)
            {
                float fallDist = player.Position.Y - maxFallStartY;   // ← FIXED sign
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
            // Goombas are positioned in world space each frame – no scroll needed here
        }

        // ════════════════════════════════════════════════════════════════════
        //  LEVEL RESET / LOAD
        // ════════════════════════════════════════════════════════════════════
        private void RestartLevel() => DoLevelSetup(currentLevelNumber);

        private void LoadNextLevel() => DoLevelSetup(currentLevelNumber + 1);

        private void DoLevelSetup(int levelNum)
        {
            currentLevelNumber = levelNum;
            currentLevel = allLevels[currentLevelNumber - 1];
            gameManager.ResetGame();
            cameraX = 0; isDying = false; isInvincible = false; invincibleTimer = 0f;
            wasGroundedLastFrame = false; canTakeFallDamage = true; isPlayerSuper = false;
            _levelComplete = false;
            // Reset HUD dirty flags so UpdateHud() re-draws everything after level change
            _lastHudHealth = -1; _lastHudLevel = -1; _lastHudSuper = false;
            player.Respawn(new Point(100, 405));
            player.IsGrounded = true; player.Health = 3;
            // Use = not += to prevent handler stacking across restarts
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
            // Clear goombas
            foreach (var g in goombas) { Controls.Remove(g.Visual); g.Visual.Dispose(); }
            goombas.Clear();
            ClearPowerUps();
        }

        // ════════════════════════════════════════════════════════════════════
        //  RANDOM LEVEL GENERATOR
        // ════════════════════════════════════════════════════════════════════
        private PlatformData[] GenerateRandomLevel(int numSections)
        {
            List<PlatformData> result;
            int extra = 0;
            do
            {
                result = new List<PlatformData>();
                int xOff = 200, yBase = 483;
                int total = numSections + extra;
                for (int i = 0; i < total; i++)
                {
                    var sec = ALL_SECTIONS[levelRandom.Next(ALL_SECTIONS.Length)];
                    foreach (var p in sec)
                    {
                        int ny = yBase + p.Y;
                        if (ny >= 250 && ny <= 483)
                            result.Add(new PlatformData(xOff + p.X, ny, p.Width, p.Height));
                    }
                    if (sec.Length > 0)
                        xOff += sec.Max(p => p.X + p.Width) + 100;
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
                "💡 Hit ? blocks with your head for power-ups!\n" +
                "👟 Jump ON goombas to stomp them!\n" +
                "❤ Fall damage applies for big drops!",
                "Super Mario", MessageBoxButtons.OK);
            Text = $"Super Mario – Level {currentLevelNumber}";
            _stopwatch.Start();
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  HELPER: FillRoundedRect extension
    // ════════════════════════════════════════════════════════════════════════
    internal static class GraphicsExtensions
    {
        // Helper kept for future use — not currently called by game code
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
        { Position = pos; Visual = visual; QuestionLabel = label; IsHit = false; PowerUpInside = powerUp; }
    }
}