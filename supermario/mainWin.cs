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
        private int walkFrame = 0;       // 0,1,2 walk cycle
        private int walkFrameTimer = 0;
        private bool isWalking = false;

        // ── Background cloud / scenery ───────────────────────────────────────
        // Each cloud: (worldX, y, width) – world-space, parallax 0.15
        private static readonly (int wx, int y, int w)[] CLOUDS = {
            (200, 55, 130), (550, 35, 90), (900, 65, 160), (1200, 45, 110),
            (1500, 70, 140), (1800, 40, 100), (2100, 60, 130), (2400, 50, 90),
            (2700, 75, 120), (3000, 45, 110), (3300, 65, 150), (3600, 55, 95),
        };
        // Hills: (worldX, y, radius) – parallax 0.3
        private static readonly (int wx, int y, int r)[] HILLS = {
            (300,  520, 180), (700,  530, 140), (1100, 515, 220),
            (1500, 525, 160), (1900, 510, 240), (2300, 520, 170),
            (2700, 515, 200), (3100, 530, 150), (3500, 520, 190),
        };

        private bool moveRight = false, moveLeft = false, jump = false;
        private int cameraX = 0;
        private const int SCROLL_THRESHOLD = 400;
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

        // ── Level data ───────────────────────────────────────────────────────
        private static readonly PlatformData[] SECTION_STAIRS = { new PlatformData(0, 30, 120, 20), new PlatformData(150, -20, 120, 20), new PlatformData(300, -70, 120, 20) };
        private static readonly PlatformData[] SECTION_GAP_JUMPS = { new PlatformData(0, -20, 100, 20), new PlatformData(170, -20, 100, 20), new PlatformData(340, -20, 100, 20) };
        private static readonly PlatformData[] SECTION_WAVE = { new PlatformData(0, 0, 100, 20), new PlatformData(150, -40, 100, 20), new PlatformData(300, 0, 100, 20), new PlatformData(450, -40, 100, 20) };
        private static readonly PlatformData[] SECTION_HIGH = { new PlatformData(0, -80, 100, 20), new PlatformData(150, -130, 100, 20), new PlatformData(300, -80, 100, 20) };
        private static readonly PlatformData[] SECTION_CHALLENGE = { new PlatformData(0, -30, 80, 20), new PlatformData(130, -70, 80, 20), new PlatformData(260, -30, 80, 20), new PlatformData(390, -70, 80, 20) };
        private static readonly PlatformData[][] ALL_SECTIONS = { SECTION_STAIRS, SECTION_GAP_JUMPS, SECTION_WAVE, SECTION_HIGH, SECTION_CHALLENGE };

        private static readonly PlatformData[] LEVEL_1 = { new PlatformData(200, 483, 120, 20), new PlatformData(350, 433, 120, 20), new PlatformData(500, 383, 120, 20), new PlatformData(700, 433, 100, 20), new PlatformData(870, 433, 100, 20), new PlatformData(1040, 433, 100, 20), new PlatformData(1200, 413, 100, 20), new PlatformData(1350, 353, 100, 20), new PlatformData(1500, 413, 100, 20), new PlatformData(1650, 333, 100, 20), new PlatformData(1800, 283, 100, 20), new PlatformData(1950, 333, 100, 20), new PlatformData(2100, 433, 120, 20), new PlatformData(2270, 433, 120, 20), new PlatformData(2440, 433, 120, 20), new PlatformData(2650, 383, 200, 20) };
        private static readonly PlatformData[] LEVEL_2 = { new PlatformData(150, 473, 80, 20), new PlatformData(300, 433, 80, 20), new PlatformData(450, 393, 80, 20), new PlatformData(600, 353, 80, 20), new PlatformData(750, 403, 70, 20), new PlatformData(880, 353, 70, 20), new PlatformData(1010, 403, 70, 20), new PlatformData(1140, 353, 70, 20), new PlatformData(1270, 303, 100, 20), new PlatformData(1420, 253, 100, 20), new PlatformData(1570, 303, 100, 20), new PlatformData(1720, 253, 100, 20), new PlatformData(1870, 353, 100, 20), new PlatformData(2020, 403, 100, 20), new PlatformData(2170, 433, 100, 20), new PlatformData(2300, 373, 80, 20), new PlatformData(2430, 333, 80, 20), new PlatformData(2560, 373, 80, 20), new PlatformData(2700, 383, 200, 20) };

        // ─────────────────────────────────────────────────────────────────────
        public mainWin()
        {
            InitializeComponent();
            InitializeGame();
        }

        // ════════════════════════════════════════════════════════════════════
        //  BACKGROUND PAINTING  (runs every time the form repaints)
        // ════════════════════════════════════════════════════════════════════
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Do NOT call base – we own the entire background
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int W = ClientSize.Width;
            int H = ClientSize.Height;

            // ── Sky gradient ────────────────────────────────────────────────
            using (var sky = new LinearGradientBrush(
                new Point(0, 0), new Point(0, H),
                Color.FromArgb(92, 148, 252),
                Color.FromArgb(178, 218, 255)))
                g.FillRectangle(sky, 0, 0, W, H);

            // ── Distant mountains (parallax 0.08) ───────────────────────────
            int mOff = (int)(cameraX * 0.08);
            DrawMountains(g, W, H, mOff);

            // ── Rolling hills (parallax 0.25) ───────────────────────────────
            int hOff = (int)(cameraX * 0.25);
            DrawHills(g, W, H, hOff);

            // ── Clouds (parallax 0.12) ───────────────────────────────────────
            int cOff = (int)(cameraX * 0.12);
            DrawClouds(g, W, H, cOff);
        }

        private void DrawMountains(Graphics g, int W, int H, int offset)
        {
            // Two rows of overlapping triangle mountains
            Color[] rows = {
                Color.FromArgb(120, 140, 185),
                Color.FromArgb(150, 170, 210),
            };
            int[][] peaks = {
                new[]{ 0,150, 200,350, 280,200, 450,370, 560,180, 730,360, 820,200, 1000,370, 1100,160, 1280,360, 1400,185, 1600,370, W+100,370 },
                new[]{ 0,200, 150,380, 260,230, 420,390, 530,210, 700,390, 800,230, 980,390, 1080,200, 1260,390, 1380,225, 1580,390, W+100,390 },
            };

            for (int r = 0; r < 2; r++)
            {
                var pts = new List<Point>();
                for (int i = 0; i < peaks[r].Length; i += 2)
                    pts.Add(new Point(peaks[r][i] - offset % (W + 200), peaks[r][i + 1]));
                // Wrap – add a second copy shifted right
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
                // Draw hill twice to handle seam
                for (int rep = -1; rep <= 1; rep++)
                {
                    int x = sx + rep * 4000;
                    if (x + r < -50 || x - r > W + 50) continue;

                    // Shadow/dark base
                    using (var b = new SolidBrush(Color.FromArgb(55, 130, 50)))
                        g.FillEllipse(b, x - r, hy - r / 2, r * 2, r);

                    // Bright top
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
                    // Shadow
                    using (var b = new SolidBrush(Color.FromArgb(50, 160, 180, 210)))
                        g.FillEllipse(b, x + 6, cy + 8, cw, ch);

                    // Main puffs
                    using (var b = new SolidBrush(Color.FromArgb(245, 250, 255)))
                    {
                        g.FillEllipse(b, x, cy + ch / 3, cw * 6 / 10, ch * 2 / 3);
                        g.FillEllipse(b, x + cw / 3, cy, cw * 5 / 10, ch * 4 / 5);
                        g.FillEllipse(b, x + cw * 5 / 10, cy + ch / 4, cw * 5 / 10, ch * 2 / 3);
                    }
                    // Bright highlight
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

            // Remove the designer's BackgroundImage so our OnPaintBackground takes over
            BackgroundImage = null;
            BackColor = Color.FromArgb(92, 148, 252); // fallback sky colour

            gameManager = new GameManager();
            allLevels = new PlatformData[][]
            {
                LEVEL_1, LEVEL_2,
                GenerateRandomLevel(6), GenerateRandomLevel(7), GenerateRandomLevel(8)
            };
            currentLevelNumber = 1;
            currentLevel = allLevels[0];

            // ── Player picture box: pure GDI+ sprite ─────────────────────────
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
            player.OnDamageTaken += () => { BecomeNormal(); };

            picboxplayer.Location = player.Position;
            picboxplayer.BringToFront();

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
        //  PLAYER SPRITE  (GDI+, direction-aware)
        // ════════════════════════════════════════════════════════════════════
        private void DrawPlayerSprite(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = picboxplayer.Width;
            int h = picboxplayer.Height;

            // Flip horizontally when facing left
            if (!facingRight)
            {
                g.TranslateTransform(w, 0);
                g.ScaleTransform(-1, 1);
            }

            bool big = isPlayerSuper;

            // ── Colours ──────────────────────────────────────────────────────
            Color skin = Color.FromArgb(252, 196, 140);
            Color capRed = Color.FromArgb(200, 40, 30);
            Color capDark = Color.FromArgb(140, 20, 15);
            Color overall = Color.FromArgb(40, 80, 180);
            Color shirtRed = Color.FromArgb(200, 40, 30);
            Color mustache = Color.FromArgb(90, 50, 20);
            Color shoe = Color.FromArgb(90, 50, 20);
            Color shoeHi = Color.FromArgb(130, 80, 40);
            Color eyeWhite = Color.White;
            Color eyePupil = Color.FromArgb(40, 20, 0);

            // ── Walk cycle offsets ───────────────────────────────────────────
            int legOff = isWalking ? (walkFrame == 1 ? -3 : walkFrame == 2 ? 3 : 0) : 0;
            int bodyBob = (player.IsGrounded && isWalking) ? (walkFrame == 1 || walkFrame == 2 ? -1 : 0) : 0;

            // ── Proportional layout ──────────────────────────────────────────
            int cx = w / 2;
            int headY = big ? 4 : 8;
            int headR = big ? 22 : 18;
            int bodyY = headY + headR * 2 - 4;
            int bodyH = big ? 22 : 18;
            int legY = bodyY + bodyH;
            int legH = big ? 16 : 13;
            int shoeH = big ? 8 : 6;

            // ── Cap ──────────────────────────────────────────────────────────
            // Brim
            g.FillEllipse(new SolidBrush(capDark), cx - headR - 2, headY + headR - 4, (headR + 2) * 2 + 4, 8);
            // Top dome
            var capPath = new GraphicsPath();
            capPath.AddArc(cx - headR, headY, headR * 2, headR * 2, 180, 180);
            capPath.CloseFigure();
            g.FillPath(new SolidBrush(capRed), capPath);
            // Cap highlight
            g.FillEllipse(new SolidBrush(Color.FromArgb(80, 255, 200, 200)), cx - headR / 2 - 3, headY + 3, headR / 2, headR / 3);

            // ── Head / face ──────────────────────────────────────────────────
            g.FillEllipse(new SolidBrush(skin), cx - headR, headY + headR / 2, headR * 2, headR + bodyBob);
            // Face highlight
            g.FillEllipse(new SolidBrush(Color.FromArgb(60, 255, 230, 210)), cx - headR / 2, headY + headR / 2 + 2, headR - 2, headR / 2);

            // Eye
            int eyeX = cx + headR / 3;
            int eyeY = headY + headR - 2;
            g.FillEllipse(new SolidBrush(eyeWhite), eyeX - 5, eyeY - 4, 10, 8);
            g.FillEllipse(new SolidBrush(eyePupil), eyeX - 2, eyeY - 2, 5, 5);
            // Eyebrow
            using (var p = new Pen(mustache, 2.5f))
                g.DrawArc(p, eyeX - 6, eyeY - 8, 12, 8, 210, 120);

            // Nose
            g.FillEllipse(new SolidBrush(Color.FromArgb(240, 170, 110)), cx + headR / 5, headY + headR + 2, 9, 7);

            // Mustache (two curved strokes)
            using (var p = new Pen(mustache, 3.5f) { EndCap = LineCap.Round, StartCap = LineCap.Round })
            {
                int my = headY + headR * 2 - 5;
                g.DrawArc(p, cx - headR / 2 - 2, my - 4, headR / 2 + 2, 10, 0, 180);
                g.DrawArc(p, cx + 4, my - 4, headR / 2, 10, 0, 180);
            }

            // ── Shirt (arms) ─────────────────────────────────────────────────
            g.FillRectangle(new SolidBrush(shirtRed), cx - headR, bodyY + bodyBob, headR * 2, bodyH);
            // Left arm
            g.FillRoundedRect(new SolidBrush(shirtRed), cx - headR - 8, bodyY + 2 + bodyBob, 10, bodyH - 4, 4);
            // Right arm
            g.FillRoundedRect(new SolidBrush(shirtRed), cx + headR - 2, bodyY + 2 + bodyBob, 10, bodyH - 4, 4);
            // Shirt highlight
            g.FillRectangle(new SolidBrush(Color.FromArgb(50, 255, 200, 200)), cx - headR + 2, bodyY + 2 + bodyBob, headR - 2, 5);

            // ── Overalls ─────────────────────────────────────────────────────
            int ovW = (int)(headR * 1.4);
            int ovX = cx - ovW / 2;
            g.FillRectangle(new SolidBrush(overall), ovX, bodyY + bodyBob + 5, ovW, bodyH - 4);
            // Overall buttons
            g.FillEllipse(new SolidBrush(Color.FromArgb(220, 220, 255)), cx - ovW / 4 - 2, bodyY + bodyBob + 7, 5, 5);
            g.FillEllipse(new SolidBrush(Color.FromArgb(220, 220, 255)), cx + ovW / 4 - 2, bodyY + bodyBob + 7, 5, 5);

            // ── Legs ─────────────────────────────────────────────────────────
            int legW = (int)(headR * 0.8);
            // Left leg (animated)
            g.FillRoundedRect(new SolidBrush(overall),
                cx - legW - 3, legY + bodyBob, legW, legH - legOff, 3);
            // Right leg (animated)
            g.FillRoundedRect(new SolidBrush(overall),
                cx + 3, legY + bodyBob, legW, legH + legOff, 3);

            // ── Shoes ────────────────────────────────────────────────────────
            // Left shoe
            int lShoeY = legY + bodyBob + legH - legOff;
            g.FillRoundedRect(new SolidBrush(shoe), cx - legW - 6, lShoeY, legW + 6, shoeH, 3);
            g.FillRoundedRect(new SolidBrush(shoeHi), cx - legW - 4, lShoeY + 1, legW, 3, 2);
            // Right shoe
            int rShoeY = legY + bodyBob + legH + legOff;
            g.FillRoundedRect(new SolidBrush(shoe), cx + 1, rShoeY, legW + 6, shoeH, 3);
            g.FillRoundedRect(new SolidBrush(shoeHi), cx + 3, rShoeY + 1, legW, 3, 2);
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
                DrawHearts();
                CheckWinCondition();
                // Invalidate form background for parallax redraw
                Invalidate(new Rectangle(0, 0, ClientSize.Width, 520));
                Text = $"Super Mario – Level {currentLevelNumber}  ♥ {player.Health}  {(isPlayerSuper ? "★ SUPER" : "")}";
            }
        }

        private void PhysicsStep()
        {
            if (isDying) { HandleDeathAnimation(FIXED_STEP_MS); return; }

            int dir = (moveRight ? 1 : 0) + (moveLeft ? -1 : 0);

            // Track facing & walk animation
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

            picboxplayer.Invalidate(); // redraw sprite every physics step
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
            picboxplayer.BringToFront();
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

            // Base colour – earthy orange-brown
            g.FillRectangle(new SolidBrush(Color.FromArgb(185, 100, 40)), 0, 0, w, h);

            // Mortar grid
            using (var m = new Pen(Color.FromArgb(120, 60, 15), 2f))
            {
                g.DrawLine(m, 0, h / 2, w, h / 2);         // horizontal split
                g.DrawLine(m, w / 2, 0, w / 2, h / 2);     // top brick seam
                g.DrawLine(m, w / 4, h / 2, w / 4, h);     // bottom-left seam
                g.DrawLine(m, 3 * w / 4, h / 2, 3 * w / 4, h); // bottom-right seam
            }

            // Top highlight
            g.FillRectangle(new SolidBrush(Color.FromArgb(70, 255, 210, 150)), 0, 0, w, 3);
            // Left highlight
            g.FillRectangle(new SolidBrush(Color.FromArgb(40, 255, 210, 150)), 0, 0, 2, h);

            // Outer border
            using (var b = new Pen(Color.FromArgb(90, 40, 5), 1f))
                g.DrawRectangle(b, 0, 0, w - 1, h - 1);
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

            // Slightly cooler tan – distinct from ground
            g.FillRectangle(new SolidBrush(Color.FromArgb(210, 140, 65)), 0, 0, w, h);

            // Brick seams scaled to tile size
            int bw = 40, bh = 20;
            using (var m = new Pen(Color.FromArgb(150, 85, 25), 1.5f))
            {
                for (int by = 0; by < h; by += bh)
                {
                    g.DrawLine(m, 0, by, w, by);
                    int offset = ((by / bh) % 2 == 0) ? 0 : bw / 2;
                    for (int bx = offset; bx < w + bw; bx += bw)
                        g.DrawLine(m, bx, by, bx, Math.Min(by + bh, h));
                }
            }

            // Top glow
            g.FillRectangle(new SolidBrush(Color.FromArgb(80, 255, 220, 160)), 0, 0, w, 4);
            // Border
            using (var b = new Pen(Color.FromArgb(110, 60, 10), 2f))
                g.DrawRectangle(b, 1, 1, w - 2, h - 2);
        }

        private void AddFinishFlagpole()
        {
            // Flag pole using a PictureBox
            var flag = new PictureBox
            {
                Size = new Size(80, 200),
                Location = new Point(2750, 313),
                BackColor = Color.Transparent,
            };
            flag.Paint += DrawFlagpole;
            Controls.Add(flag);
            flag.SendToBack();
            // Collision box is the base block
            platforms.Add(new GameObjectS(flag, flag.Location, "finish"));
        }

        private void DrawFlagpole(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = 80, h = 200;

            // Pole
            using (var lg = new LinearGradientBrush(new Point(30, 0), new Point(46, 0),
                Color.FromArgb(200, 200, 210), Color.FromArgb(90, 90, 110)))
                g.FillRectangle(lg, 34, 0, 12, h);

            // Pole shine
            g.FillRectangle(new SolidBrush(Color.FromArgb(80, 255, 255, 255)), 36, 0, 3, h);

            // Ball on top
            g.FillEllipse(new SolidBrush(Color.FromArgb(255, 210, 30)), 26, 0, 28, 28);
            g.FillEllipse(new SolidBrush(Color.FromArgb(180, 255, 255, 150)), 30, 3, 10, 10);

            // Flag (green banner)
            var flagPts = new PointF[]
            {
                new PointF(46, 18),
                new PointF(46, 70),
                new PointF(78, 44),
            };
            g.FillPolygon(new SolidBrush(Color.FromArgb(60, 180, 60)), flagPts);
            g.FillPolygon(new SolidBrush(Color.FromArgb(80, 255, 80, 80)), new PointF[]
            {
                new PointF(48, 20), new PointF(58, 30), new PointF(50, 38)
            });

            // Base block
            g.FillRectangle(new SolidBrush(Color.FromArgb(80, 80, 90)), 14, h - 30, 50, 30);
            g.FillRectangle(new SolidBrush(Color.FromArgb(110, 110, 125)), 14, h - 30, 50, 8);
            using (var p = new Pen(Color.FromArgb(50, 50, 60), 2f))
                g.DrawRectangle(p, 14, h - 30, 50, 30);

            // "GOAL" text
            using (var f = new Font("Courier New", 7f, FontStyle.Bold))
            using (var b = new SolidBrush(Color.White))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("GOAL", f, b, new RectangleF(14, h - 22, 50, 14), sf);
            }
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
                        // ── Used block ────────────────────────────────────────
                        g.FillRectangle(new SolidBrush(Color.FromArgb(181, 116, 56)), 0, 0, bw, bh);
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
                        // ── Animated active block ─────────────────────────────
                        Color[] palette = {
                            Color.FromArgb(255, 210,   0),
                            Color.FromArgb(255, 230,  60),
                            Color.FromArgb(255, 255, 100),
                            Color.FromArgb(255, 230,  60),
                            Color.FromArgb(255, 210,   0),
                            Color.FromArgb(220, 160,   0),
                        };
                        g.FillRectangle(new SolidBrush(palette[questionAnimFrame % 6]), 0, 0, bw, bh);
                        g.FillRectangle(new SolidBrush(Color.FromArgb(100, 255, 255, 200)), 3, 3, bw - 6, 6);

                        using (var border = new Pen(Color.FromArgb(140, 80, 0), 3f))
                            g.DrawRectangle(border, 1, 1, bw - 3, bh - 3);

                        int qOff = (questionAnimFrame % 2 == 0) ? 0 : -2;
                        using (var qFont = new Font("Arial", 24, FontStyle.Bold))
                        using (var qBrush = new SolidBrush(Color.FromArgb(100, 50, 0)))
                        {
                            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                            g.DrawString("?", qFont, qBrush, new RectangleF(0, qOff, bw, bh), sf);
                        }
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
        //  HUD
        // ════════════════════════════════════════════════════════════════════
        private void DrawHearts()
        {
            // Remove old HUD labels
            foreach (var l in Controls.OfType<Label>().Where(l => l.Name == "heartLabel" || l.Name == "hudLabel").ToList())
            { Controls.Remove(l); l.Dispose(); }

            // HUD background strip
            var hud = new Label
            {
                Name = "hudLabel",
                AutoSize = false,
                Size = new Size(300, 40),
                Location = new Point(8, 8),
                BackColor = Color.FromArgb(160, 20, 20, 40),
                ForeColor = Color.White,
                Font = new Font("Courier New", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Text = $"  LVL {currentLevelNumber}     {(isPlayerSuper ? "★ SUPER" : "")}",
            };
            Controls.Add(hud);
            hud.BringToFront();

            // Hearts
            for (int i = 0; i < player.Health; i++)
            {
                var h = new Label
                {
                    Name = "heartLabel",
                    Text = "❤",
                    Font = new Font("Arial", 20, FontStyle.Bold),
                    ForeColor = Color.FromArgb(255, 60, 80),
                    AutoSize = true,
                    Location = new Point(160 + i * 36, 6),
                    BackColor = Color.Transparent,
                };
                Controls.Add(h);
                h.BringToFront();
            }
            // Empty heart slots
            for (int i = player.Health; i < 3; i++)
            {
                var h = new Label
                {
                    Name = "heartLabel",
                    Text = "♡",
                    Font = new Font("Arial", 20, FontStyle.Bold),
                    ForeColor = Color.FromArgb(140, 100, 110),
                    AutoSize = true,
                    Location = new Point(160 + i * 36, 6),
                    BackColor = Color.Transparent,
                };
                Controls.Add(h);
                h.BringToFront();
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
            var playerRect = new Rectangle(player.Position.X, player.Position.Y, picboxplayer.Width, picboxplayer.Height);
            bool foundGround = false;

            foreach (var plat in platforms)
            {
                var platRect = new Rectangle(plat.Position.X, plat.Position.Y, plat.PictureBox.Width, plat.PictureBox.Height);
                if (!playerRect.IntersectsWith(platRect)) continue;

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
            if (!foundGround) player.IsGrounded = false;
        }

        private void CheckQuestionBlockCollisions()
        {
            var headRect = new Rectangle(player.Position.X, player.Position.Y, picboxplayer.Width, 30);
            foreach (var block in questionBlocks)
            {
                if (block.IsHit) continue;
                var blockRect = new Rectangle(block.Position.X, block.Position.Y, block.Visual.Width, block.Visual.Height);
                if (!headRect.IntersectsWith(blockRect)) continue;

                block.IsHit = true;
                block.Visual.Invalidate();

                if (!isPlayerSuper) BecomeSuper();
                else player.Health = Math.Min(player.Health + 1, 3);
            }
        }

        private void CheckWinCondition()
        {
            if (player.Position.X < 2750) return;
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

        private void HandleFallDamage()
        {
            if (wasGroundedLastFrame && !player.IsGrounded) { maxFallStartY = player.Position.Y; canTakeFallDamage = true; }
            if (!wasGroundedLastFrame && player.IsGrounded && !isDying)
            {
                float fallDist = maxFallStartY - player.Position.Y;
                if (fallDist > FALL_DAMAGE_THRESHOLD && canTakeFallDamage)
                {
                    player.TakeDamage(1); canTakeFallDamage = false;
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
                picboxplayer.Location = new Point(picboxplayer.Location.X, (int)(player.Position.Y - (deathTimer / 500f) * 100));
            else if (deathTimer < DEATH_ANIMATION_DURATION)
                picboxplayer.Location = new Point(picboxplayer.Location.X, (int)(player.Position.Y + ((deathTimer - 500) / (DEATH_ANIMATION_DURATION - 500)) * 300));
            else { isDying = false; player.Health = 3; isPlayerSuper = false; RestartLevel(); }
        }

        // ════════════════════════════════════════════════════════════════════
        //  CAMERA
        // ════════════════════════════════════════════════════════════════════
        private void UpdateCamera()
        {
            int screenX = player.Position.X - cameraX;
            if (screenX > SCROLL_THRESHOLD && player.Position.X > SCROLL_THRESHOLD)
            {
                int newCam = Math.Min(player.Position.X - SCROLL_THRESHOLD, 2000);
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
            picboxplayer.BringToFront();
        }

        private void ScrollObjects(int scroll)
        {
            foreach (var p in platforms) p.PictureBox.Left -= scroll;
            foreach (var b in questionBlocks)
            {
                b.Visual.Left -= scroll;
                if (b.QuestionLabel != null) b.QuestionLabel.Left -= scroll;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  LEVEL RESET / LOAD
        // ════════════════════════════════════════════════════════════════════
        private void RestartLevel()
        {
            gameManager.ResetGame(); cameraX = 0; isDying = false;
            wasGroundedLastFrame = false; canTakeFallDamage = true; isPlayerSuper = false;
            player.Respawn(new Point(100, 405));
            player.IsGrounded = true; player.Health = 3;
            player.OnDamageTaken += () => { BecomeNormal(); };
            picboxplayer.Size = originalPlayerSize;
            picboxplayer.Location = player.Position;
            ClearPlatforms(); CreateLongLevel();
            _stopwatch.Restart(); _lastTickMs = 0; _accumulatedMs = 0;
            gameManager.StartGame(); gameTimer.Start();
        }

        private void LoadNextLevel()
        {
            currentLevelNumber++;
            currentLevel = allLevels[currentLevelNumber - 1];
            gameManager.ResetGame(); cameraX = 0; isDying = false;
            wasGroundedLastFrame = false; canTakeFallDamage = true; isPlayerSuper = false;
            player.Respawn(new Point(100, 405));
            player.IsGrounded = true; player.Health = 3;
            player.OnDamageTaken += () => { BecomeNormal(); };
            picboxplayer.Size = originalPlayerSize;
            picboxplayer.Location = player.Position;
            ClearPlatforms(); CreateLongLevel();
            _stopwatch.Restart(); _lastTickMs = 0; _accumulatedMs = 0;
            Text = $"Super Mario – Level {currentLevelNumber}";
            gameManager.StartGame(); gameTimer.Start();
        }

        private void ClearPlatforms()
        {
            foreach (var p in platforms) { Controls.Remove(p.PictureBox); p.PictureBox.Dispose(); }
            platforms.Clear();
            ClearPowerUps();
        }

        // ════════════════════════════════════════════════════════════════════
        //  RANDOM LEVEL GENERATOR
        // ════════════════════════════════════════════════════════════════════
        private PlatformData[] GenerateRandomLevel(int numSections)
        {
            var result = new List<PlatformData>();
            int xOff = 200, yBase = 483;
            var sections = Enumerable.Range(0, numSections)
                .Select(_ => ALL_SECTIONS[levelRandom.Next(ALL_SECTIONS.Length)]).ToList();

            foreach (var sec in sections)
            {
                foreach (var p in sec)
                {
                    int ny = yBase + p.Y;
                    if (ny >= 250 && ny <= 483) result.Add(new PlatformData(xOff + p.X, ny, p.Width, p.Height));
                }
                if (sec.Length > 0)
                {
                    int maxX = sec.Max(p => p.X + p.Width);
                    xOff += maxX + 100;
                }
            }
            return result.Count < 5 ? GenerateRandomLevel(numSections + 1) : result.ToArray();
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
                "💡 Hit ? blocks with your head for power-ups!",
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
        public static void FillRoundedRect(this Graphics g, Brush b, int x, int y, int w, int h, int r)
        {
            if (w <= 0 || h <= 0) return;
            r = Math.Min(r, Math.Min(w / 2, h / 2));
            var path = new GraphicsPath();
            path.AddArc(x, y, r * 2, r * 2, 180, 90);
            path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
            path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            g.FillPath(b, path);
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