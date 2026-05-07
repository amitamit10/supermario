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
        //  LEVEL SECTION TEMPLATES  –  Y offsets applied onto yBase at runtime
        // ════════════════════════════════════════════════════════════════════

        // ── Opening / recovery sections ───────────────────────────────────────
        private static readonly PlatformData[] SECTION_LONG_RUN = {
            new PlatformData(0, -55, 340, 20)
        };
        private static readonly PlatformData[] SECTION_BRIDGE = {
            new PlatformData(80, -60, 280, 20)
        };
        private static readonly PlatformData[] SECTION_GENTLE_HOP = {
            new PlatformData(0, -40, 120, 20), new PlatformData(190, -60, 110, 20),
            new PlatformData(370, -40, 120, 20), new PlatformData(560, -60, 110, 20)
        };

        // ── Ascending / stair sections ────────────────────────────────────────
        private static readonly PlatformData[] SECTION_STAIRS = {
            new PlatformData(0, -20, 100, 20), new PlatformData(140, -60, 100, 20),
            new PlatformData(280, -100, 100, 20), new PlatformData(420, -60, 100, 20)
        };
        private static readonly PlatformData[] SECTION_STAIR_UP = {
            new PlatformData(0, -30, 80, 20), new PlatformData(110, -70, 80, 20),
            new PlatformData(220, -110, 80, 20), new PlatformData(330, -70, 80, 20)
        };
        private static readonly PlatformData[] SECTION_TRIPLE_JUMP = {
            new PlatformData(0, -40, 80, 20), new PlatformData(120, -80, 80, 20),
            new PlatformData(240, -120, 80, 20), new PlatformData(360, -80, 80, 20)
        };
        private static readonly PlatformData[] SECTION_PYRAMID = {
            new PlatformData(0, -30, 100, 20), new PlatformData(140, -70, 90, 20),
            new PlatformData(270, -110, 80, 20), new PlatformData(390, -70, 90, 20),
            new PlatformData(520, -30, 100, 20)
        };
        private static readonly PlatformData[] SECTION_DESCEND = {
            new PlatformData(0, -120, 100, 20), new PlatformData(160, -80, 100, 20),
            new PlatformData(320, -50, 110, 20), new PlatformData(490, -25, 120, 20)
        };

        // ── Gap jump sections ─────────────────────────────────────────────────
        private static readonly PlatformData[] SECTION_GAP_JUMPS = {
            new PlatformData(0, -50, 110, 20), new PlatformData(190, -50, 110, 20),
            new PlatformData(380, -50, 110, 20)
        };
        private static readonly PlatformData[] SECTION_WIDE_GAPS = {
            new PlatformData(0, -50, 130, 20), new PlatformData(250, -40, 130, 20),
            new PlatformData(500, -50, 130, 20)
        };
        private static readonly PlatformData[] SECTION_DOUBLE_GAP = {
            new PlatformData(0, -55, 100, 20), new PlatformData(180, -75, 90, 20),
            new PlatformData(360, -55, 100, 20), new PlatformData(540, -75, 90, 20)
        };

        // ── Wave / rhythm sections ─────────────────────────────────────────────
        private static readonly PlatformData[] SECTION_WAVE = {
            new PlatformData(0, -40, 110, 20), new PlatformData(180, -80, 100, 20),
            new PlatformData(350, -40, 110, 20), new PlatformData(530, -80, 100, 20)
        };
        private static readonly PlatformData[] SECTION_ZIGZAG = {
            new PlatformData(0, -50, 80, 20), new PlatformData(120, -90, 75, 20),
            new PlatformData(240, -50, 80, 20), new PlatformData(360, -90, 75, 20),
            new PlatformData(480, -50, 80, 20)
        };
        private static readonly PlatformData[] SECTION_VALLEY = {
            new PlatformData(0, -100, 110, 20), new PlatformData(180, -40, 110, 20),
            new PlatformData(360, -100, 110, 20)
        };

        // ── High / challenge sections ──────────────────────────────────────────
        private static readonly PlatformData[] SECTION_HIGH = {
            new PlatformData(0, -100, 110, 20), new PlatformData(180, -150, 100, 20),
            new PlatformData(360, -100, 110, 20)
        };
        private static readonly PlatformData[] SECTION_CHALLENGE = {
            new PlatformData(0, -50, 80, 20), new PlatformData(130, -90, 80, 20),
            new PlatformData(260, -50, 80, 20), new PlatformData(390, -90, 80, 20)
        };
        private static readonly PlatformData[] SECTION_ARCH = {
            new PlatformData(0, -40, 100, 20), new PlatformData(160, -90, 100, 20),
            new PlatformData(320, -140, 90, 20), new PlatformData(480, -90, 100, 20),
            new PlatformData(640, -40, 100, 20)
        };
        private static readonly PlatformData[] SECTION_MULTI_LEVEL = {
            new PlatformData(0, -30, 100, 20), new PlatformData(160, -80, 90, 20),
            new PlatformData(300, -130, 80, 20), new PlatformData(430, -80, 90, 20),
            new PlatformData(570, -30, 100, 20)
        };

        // ── Precision / narrow sections ────────────────────────────────────────
        private static readonly PlatformData[] SECTION_LEDGE_HOP = {
            new PlatformData(0, -40, 70, 20), new PlatformData(110, -70, 65, 20),
            new PlatformData(220, -40, 70, 20), new PlatformData(330, -70, 65, 20),
            new PlatformData(440, -40, 70, 20)
        };
        private static readonly PlatformData[] SECTION_CASTLE = {
            new PlatformData(0, -60, 90, 20), new PlatformData(120, -100, 50, 20),
            new PlatformData(190, -60, 90, 20), new PlatformData(310, -100, 50, 20),
            new PlatformData(380, -60, 90, 20)
        };
        private static readonly PlatformData[] SECTION_SUSPENDED = {
            new PlatformData(0, -130, 100, 20), new PlatformData(170, -170, 90, 20),
            new PlatformData(330, -130, 100, 20)
        };

        private static readonly PlatformData[][] ALL_SECTIONS = {
            SECTION_LONG_RUN, SECTION_BRIDGE, SECTION_GENTLE_HOP,
            SECTION_STAIRS, SECTION_STAIR_UP, SECTION_TRIPLE_JUMP, SECTION_PYRAMID, SECTION_DESCEND,
            SECTION_GAP_JUMPS, SECTION_WIDE_GAPS, SECTION_DOUBLE_GAP,
            SECTION_WAVE, SECTION_ZIGZAG, SECTION_VALLEY,
            SECTION_HIGH, SECTION_CHALLENGE, SECTION_ARCH, SECTION_MULTI_LEVEL,
            SECTION_LEDGE_HOP, SECTION_CASTLE, SECTION_SUSPENDED
        };

        // ════════════════════════════════════════════════════════════════════
        //  HAND-CRAFTED LEVELS  –  authentic Mario-style architecture
        // ════════════════════════════════════════════════════════════════════

        // ── LEVEL 1: Overworld Plains (SMB 1-1 inspired) ─────────────────────
        private static readonly PlatformData[] LEVEL_1 = {
            // Intro zone: wide forgiving platforms close to ground
            new PlatformData(180, 453, 200, 20),
            new PlatformData(440, 433, 140, 20),
            // Small step before first pipe
            new PlatformData(640, 413, 80, 20),
            // Raised combat platform after pipe 1 (enemies patrol here)
            new PlatformData(850, 393, 200, 20),
            // Step-down bridge linking to pipe zone
            new PlatformData(1090, 433, 90, 20),
            // Long elevated run after pipes 2 + 3
            new PlatformData(1440, 393, 260, 20),
            // Descent section
            new PlatformData(1740, 413, 120, 20),
            new PlatformData(1910, 433, 120, 20),
            // Challenge zone
            new PlatformData(2070, 373, 130, 20),
            new PlatformData(2250, 393, 100, 20),
            // Rest platform before staircase
            new PlatformData(2390, 453, 80, 20),
            // ── Iconic 8-step ascending staircase (block heights fill to ground) ──
            new PlatformData(2490, 473, 40, 40),
            new PlatformData(2530, 433, 40, 80),
            new PlatformData(2570, 393, 40, 120),
            new PlatformData(2610, 353, 40, 160),
            new PlatformData(2640, 313, 40, 200),
            new PlatformData(2660, 273, 40, 240),
            new PlatformData(2690, 233, 40, 280),
            new PlatformData(2710, 193, 40, 320),
        };
        private static readonly PlatformData[] LEVEL_1_PIPES = {
            new PlatformData(750,  433, 80,  80),  // Pipe 1: 2 blocks tall
            new PlatformData(1200, 393, 80, 120),  // Pipe 2: 3 blocks tall
            new PlatformData(1340, 353, 80, 160),  // Pipe 3: 4 blocks tall
        };
        private static readonly QBlockDef[] LEVEL_1_QBLOCKS = {
            new QBlockDef(460, 353, PowerUpType.Coin),     // Row of 3 question blocks
            new QBlockDef(510, 353, PowerUpType.Coin),
            new QBlockDef(560, 353, PowerUpType.Mushroom), // Third block = mushroom!
            new QBlockDef(970, 313, PowerUpType.Mushroom), // Above raised platform
            new QBlockDef(1500, 273, PowerUpType.Coin),    // Elevated run triple row
            new QBlockDef(1550, 273, PowerUpType.Coin),
            new QBlockDef(1600, 273, PowerUpType.Coin),
            new QBlockDef(2120, 333, PowerUpType.Mushroom),// Challenge reward
        };
        // Goomba h=52: ground Y=461; platform@Y=393 → Y=341
        private static readonly EnemyDef[] LEVEL_1_GOOMBAS = {
            new EnemyDef(520,  461), // Ground near QBs
            new EnemyDef(670,  461), // Ground before pipe 1
            new EnemyDef(910,  341), // On raised platform at Y=393
            new EnemyDef(980,  341), // Paired goomba on raised platform
            new EnemyDef(1550, 341), // On elevated run at Y=393
            new EnemyDef(2140, 461), // Ground, pre-staircase
        };
        // Koopa h=56: ground Y=457; platform@Y=393 → Y=337
        private static readonly EnemyDef[] LEVEL_1_KOOPAS = {
            new EnemyDef(875,  457), // Ground right of pipe 1
            new EnemyDef(1460, 337), // On elevated run at Y=393
            new EnemyDef(2000, 457), // Ground in challenge zone
        };
        // FastEnemy h=48: ground Y=465; platform@Y=393 → Y=345
        private static readonly EnemyDef[] LEVEL_1_FAST_ENEMIES = {
            new EnemyDef(1620, 345), // On elevated run at Y=393
            new EnemyDef(2290, 465), // Ground before staircase
        };
        private static readonly int[] LEVEL_1_FLOAT_COIN_X = { 350, 580, 730, 790, 900, 1270, 1480, 1545, 2085, 2280 };
        private static readonly int[] LEVEL_1_FLOAT_COIN_Y = { 400, 370, 378, 362, 358, 382,  358,  358,  348,  382  };

        // ── LEVEL 2: Underground Cavern (SMB 1-2 inspired) ───────────────────
        private static readonly PlatformData[] LEVEL_2 = {
            // Cavern entry – descending feeling
            new PlatformData(200, 433, 140, 20),
            new PlatformData(400, 393, 100, 20),
            new PlatformData(560, 353, 80,  20),
            // Cavern floor traversal
            new PlatformData(700, 373, 160, 20),
            new PlatformData(930, 353, 80,  20),
            new PlatformData(1070, 393, 140, 20),
            // Post-pipe 1 platform
            new PlatformData(1370, 413, 120, 20),
            // Deep cavern long traverse
            new PlatformData(1560, 353, 200, 20),
            // Cavern descent
            new PlatformData(1820, 373, 90,  20),
            new PlatformData(1970, 393, 100, 20),
            // Post-pipe 3 underground challenge
            new PlatformData(2240, 333, 130, 20),
            new PlatformData(2420, 373, 100, 20),
            // Pre-staircase rest
            new PlatformData(2540, 433, 80,  20),
            // ── Underground exit: 6-step ascending staircase ─────────────────
            new PlatformData(2570, 473, 40,  40),
            new PlatformData(2610, 433, 40,  80),
            new PlatformData(2640, 393, 40, 120),
            new PlatformData(2670, 353, 40, 160),
            new PlatformData(2700, 313, 40, 200),
            new PlatformData(2720, 273, 40, 240),
        };
        private static readonly PlatformData[] LEVEL_2_PIPES = {
            new PlatformData(1230, 433, 80,  80), // Pipe 1: 2 blocks
            new PlatformData(1730, 393, 80, 120), // Pipe 2: 3 blocks
            new PlatformData(2100, 353, 80, 160), // Pipe 3: 4 blocks
        };
        private static readonly QBlockDef[] LEVEL_2_QBLOCKS = {
            new QBlockDef(290,  313, PowerUpType.Mushroom), // Hard early reward
            new QBlockDef(760,  293, PowerUpType.Coin),
            new QBlockDef(1110, 313, PowerUpType.Coin),
            new QBlockDef(1420, 313, PowerUpType.Mushroom),
            new QBlockDef(1610, 293, PowerUpType.Coin),
            new QBlockDef(1660, 293, PowerUpType.Coin),
            new QBlockDef(2165, 273, PowerUpType.Mushroom),
            new QBlockDef(2530, 373, PowerUpType.Coin),
        };
        // Goomba: ground Y=461; Y=373→321, Y=353→301, Y=433→401
        private static readonly EnemyDef[] LEVEL_2_GOOMBAS = {
            new EnemyDef(480,  461),
            new EnemyDef(760,  321), // On Y=373 platform
            new EnemyDef(1610, 301), // On Y=353 deep traverse
            new EnemyDef(2330, 461),
        };
        // Koopa: ground Y=457; Y=393→337, Y=353→297, Y=333→277, Y=433→377
        private static readonly EnemyDef[] LEVEL_2_KOOPAS = {
            new EnemyDef(440,  337), // On Y=393 platform
            new EnemyDef(1000, 457),
            new EnemyDef(1590, 297), // On Y=353 deep traverse
            new EnemyDef(2265, 277), // On Y=333 challenge platform
            new EnemyDef(2555, 377), // On Y=433 pre-staircase
        };
        // FastEnemy: ground Y=465; Y=353→305
        private static readonly EnemyDef[] LEVEL_2_FAST_ENEMIES = {
            new EnemyDef(1650, 305), // On Y=353 deep traverse
            new EnemyDef(1980, 465),
        };
        private static readonly int[] LEVEL_2_FLOAT_COIN_X = { 330, 510, 800, 1000, 1430, 1645, 1700, 2190, 2365, 2510 };
        private static readonly int[] LEVEL_2_FLOAT_COIN_Y = { 390, 358, 338,  358,  378,  318,  318,  308,  352,  398  };

        // ── LEVEL 3: Sky Fortress (higher, faster, tougher) ──────────────────
        private static readonly PlatformData[] LEVEL_3 = {
            // Sky opening – elevated from the start
            new PlatformData(160, 373, 120, 20),
            new PlatformData(360, 353, 100, 20),
            new PlatformData(530, 313, 80,  20),
            // Post pipe-gauntlet landing
            new PlatformData(990, 353, 140, 20),
            // Sky high traverse
            new PlatformData(1200, 313, 120, 20),
            new PlatformData(1390, 273, 100, 20),
            new PlatformData(1560, 313, 120, 20),
            // Precision narrow ledge sequence
            new PlatformData(1740, 353, 75, 20),
            new PlatformData(1880, 393, 75, 20),
            new PlatformData(2020, 353, 75, 20),
            new PlatformData(2160, 313, 75, 20),
            // High reward platform
            new PlatformData(2270, 273, 140, 20),
            // ── Iconic 8-step ascending staircase ────────────────────────────
            new PlatformData(2450, 473, 40,  40),
            new PlatformData(2490, 433, 40,  80),
            new PlatformData(2530, 393, 40, 120),
            new PlatformData(2570, 353, 40, 160),
            new PlatformData(2610, 313, 40, 200),
            new PlatformData(2650, 273, 40, 240),
            new PlatformData(2680, 233, 40, 280),
            new PlatformData(2720, 193, 40, 320),
        };
        private static readonly PlatformData[] LEVEL_3_PIPES = {
            new PlatformData(660, 433, 80,  80), // Pipe 1: 2 blocks (stepping stone)
            new PlatformData(800, 393, 80, 120), // Pipe 2: 3 blocks (step up)
        };
        private static readonly QBlockDef[] LEVEL_3_QBLOCKS = {
            new QBlockDef(240,  313, PowerUpType.Coin),
            new QBlockDef(1080, 273, PowerUpType.Mushroom),
            new QBlockDef(1440, 233, PowerUpType.Coin),
            new QBlockDef(1490, 233, PowerUpType.Coin),
            new QBlockDef(1640, 273, PowerUpType.Mushroom),
            new QBlockDef(2000, 293, PowerUpType.Coin),
            new QBlockDef(2310, 233, PowerUpType.Coin),
            new QBlockDef(2360, 233, PowerUpType.Mushroom),
        };
        // Goomba: Y=373→321, Y=353→301, Y=313→261
        private static readonly EnemyDef[] LEVEL_3_GOOMBAS = {
            new EnemyDef(220,  321), // On Y=373 platform
            new EnemyDef(1050, 301), // On Y=353 landing
            new EnemyDef(1260, 261), // On Y=313
            new EnemyDef(1760, 301), // On Y=353 narrow
            new EnemyDef(2400, 461),
        };
        // Koopa: Y=313→257, Y=273→217
        private static readonly EnemyDef[] LEVEL_3_KOOPAS = {
            new EnemyDef(575,  257), // On Y=313 intro
            new EnemyDef(1600, 257), // On Y=313 sky run
            new EnemyDef(2320, 217), // On Y=273 reward platform
        };
        // FastEnemy: Y=273→225, Y=353→305, Y=313→265
        private static readonly EnemyDef[] LEVEL_3_FAST_ENEMIES = {
            new EnemyDef(1440, 225), // On Y=273 high platform
            new EnemyDef(2060, 305), // On Y=353 narrow
            new EnemyDef(2340, 225), // On Y=273 reward platform
        };
        private static readonly int[] LEVEL_3_FLOAT_COIN_X = { 255, 445, 695, 840, 1290, 1470, 1825, 1995, 2315, 2445 };
        private static readonly int[] LEVEL_3_FLOAT_COIN_Y = { 338, 318, 400, 358, 278,  248,  328,  318,  248,  413  };

        // ── JumpingEnemy  h=50: ground Y=463; platform@Y=393→343, @Y=373→323, @Y=313→263, @Y=273→223
        // Level 1: two ground positions that don't conflict with elevated enemies
        private static readonly EnemyDef[] LEVEL_1_JUMPING = {
            new EnemyDef(600,  463), // Ground, just after intro platforms (clear ground)
            new EnemyDef(2050, 463), // Ground, challenge zone approach (clear of overhead platform)
        };
        // Level 2: ground + one on the long traverse (1560-1760, Y=353→303)
        private static readonly EnemyDef[] LEVEL_2_JUMPING = {
            new EnemyDef(550,  463), // Ground entry zone
            new EnemyDef(1140, 343), // On Y=393 platform (1070-1210) – away from other enemies
            new EnemyDef(2260, 463), // Ground challenge zone
        };
        // Level 3: verified positions inside actual platform bounds
        private static readonly EnemyDef[] LEVEL_3_JUMPING = {
            new EnemyDef(210,  323), // On Y=373 intro platform (160-280) – centred ✓
            new EnemyDef(1430, 223), // On Y=273 high platform (1390-1490) ✓
            new EnemyDef(2050, 303), // On Y=353 narrow (2020-2095) – centred ✓
        };

        // ── PlatformPatrolEnemy  h=50: placed on platforms wide enough to show edge-turn behaviour
        // Level 1: step-down bridge (1090-1180, Y=433→383) and pre-staircase rest (2390-2470, Y=453→403)
        private static readonly EnemyDef[] LEVEL_1_PATROL = {
            new EnemyDef(1120, 383), // On step-down bridge (1090-1180, Y=433) ✓
            new EnemyDef(2420, 403), // On rest platform (2390-2470, Y=453) ✓
        };
        // Level 2: deep traverse (1560-1760, Y=353→303) far from others; ground past elevated section
        private static readonly EnemyDef[] LEVEL_2_PATROL = {
            new EnemyDef(1720, 303), // On traverse (1560-1760, Y=353) – rightmost, spacing ✓
            new EnemyDef(2080, 463), // Ground, just past elevated platform at 1970-2070 ✓
        };
        // Level 3: sky traverse (1200-1320, Y=313→263) and narrow ledge (1740-1815, Y=353→303)
        private static readonly EnemyDef[] LEVEL_3_PATROL = {
            new EnemyDef(1260, 263), // On Y=313 sky traverse (1200-1320) ✓
            new EnemyDef(1760, 303), // On Y=353 narrow ledge (1740-1815) ✓
        };

        // ── FlyingEnemy  h=48: baseY chosen so amplitude±22 stays clear of all platform tops/bottoms
        // Level 1: over pipe gap (clear air between pipes 1 and 2) + challenge zone
        private static readonly EnemyDef[] LEVEL_1_FLYING = {
            new EnemyDef(1060, 405), // Over pipe zone – oscillates 383-427, clear of all platforms
            new EnemyDef(2340, 400), // Challenge zone – oscillates 378-422, clear of ground (513)
        };
        // Level 2: shifted past platform edge + deep section
        private static readonly EnemyDef[] LEVEL_2_FLYING = {
            new EnemyDef(880,  420), // Just past platform (700-860, Y=373) – oscillates 398-442 ✓
            new EnemyDef(1490, 395), // Deep traverse – oscillates 373-417, below platforms ✓
        };
        // Level 3: Y raised so oscillation stays below all platform bottoms (lowest platform bottom = 333)
        private static readonly EnemyDef[] LEVEL_3_FLYING = {
            new EnemyDef(700,  420), // Pipe gauntlet – oscillates 398-442, below platform Y=313 ✓
            new EnemyDef(1700, 425), // Sky traverse – oscillates 403-447, below Y=313 platforms ✓
            new EnemyDef(2190, 410), // High reward section – oscillates 388-432, below Y=273 plat ✓
        };

        // ── Per-level data helpers ────────────────────────────────────────────
        private struct QBlockDef
        {
            public readonly int X, Y;
            public readonly PowerUpType Type;
            public QBlockDef(int x, int y, PowerUpType t) { X = x; Y = y; Type = t; }
        }
        private struct EnemyDef
        {
            public readonly int X, Y;
            public EnemyDef(int x, int y) { X = x; Y = y; }
        }

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
            UpdateJumpingEnemies();
            UpdatePatrolEnemies();
            UpdateFlyingEnemies();
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
        //  COINS
        // ════════════════════════════════════════════════════════════════════
        private void AddCoins()
        {
            var coinPositions = new List<Point>();

            // Rows of coins above each platform in the level
            foreach (var p in currentLevel)
            {
                // Skip staircase steps (very narrow blocks at specific Y values)
                if (p.Width == 40 && p.Height >= 40) continue;
                int rowY = p.Y - 50;
                int count = Math.Max(1, p.Width / 40);
                for (int j = 0; j < count; j++)
                    coinPositions.Add(new Point(p.X + 10 + j * 38, rowY));
            }

            // Per-level floating coins guiding the player through key sections
            int[] floatX, floatY;
            switch (currentLevelNumber)
            {
                case 1: floatX = LEVEL_1_FLOAT_COIN_X; floatY = LEVEL_1_FLOAT_COIN_Y; break;
                case 2: floatX = LEVEL_2_FLOAT_COIN_X; floatY = LEVEL_2_FLOAT_COIN_Y; break;
                case 3: floatX = LEVEL_3_FLOAT_COIN_X; floatY = LEVEL_3_FLOAT_COIN_Y; break;
                default:
                    floatX = new[] { 300, 500, 800, 1100, 1400, 1650, 1950, 2250, 2450, 2600 };
                    floatY = new[] { 390, 360, 370,  350,  380,  360,  370,  350,  390,  380 };
                    break;
            }
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
            var playerRect = new Rectangle(
                player.Position.X, player.Position.Y,
                picboxplayer.Width, picboxplayer.Height);

            for (int i = jumpingEnemies.Count - 1; i >= 0; i--)
            {
                var je = jumpingEnemies[i];

                if (!je.IsAlive)
                {
                    Controls.Remove(je.Visual);
                    je.Visual.Dispose();
                    jumpingEnemies.RemoveAt(i);
                    continue;
                }

                // Gravity
                if (!je.IsGrounded)
                {
                    je.VerticalVelocity += 0.6f;
                    if (je.VerticalVelocity > 15f) je.VerticalVelocity = 15f;
                    je.Position = new Point(je.Position.X, je.Position.Y + (int)je.VerticalVelocity);
                }

                bool jeGrounded = false;
                var jeRect = je.Bounds;
                foreach (var plat in platforms)
                {
                    var pr = new Rectangle(
                        plat.PictureBox.Left + cameraX, plat.Position.Y,
                        plat.PictureBox.Width, plat.PictureBox.Height);
                    if (!jeRect.IntersectsWith(pr)) continue;

                    int ot = jeRect.Bottom - pr.Top, ob = pr.Bottom - jeRect.Top;
                    int ol = jeRect.Right - pr.Left, orr = pr.Right - jeRect.Left;
                    int min = Math.Min(Math.Min(ot, ob), Math.Min(ol, orr));

                    if (min == ot && ot < 25)
                    {
                        je.Position = new Point(je.Position.X, pr.Top - je.Visual.Height);
                        je.VerticalVelocity = 0;
                        jeGrounded = true;
                    }
                    else if (min == ol || min == orr) je.ReverseDirection();
                }
                je.IsGrounded = jeGrounded;

                if (je.IsSquished)
                {
                    if (je.UpdateSquish(FIXED_STEP_MS)) je.Kill();
                    je.Visual.Location = new Point(je.Position.X - cameraX, je.Position.Y);
                    continue;
                }

                je.Update();
                je.Visual.Location = new Point(je.Position.X - cameraX, je.Position.Y);

                if (isDying) continue;
                var jeWorld = new Rectangle(je.Position.X, je.Position.Y, je.Visual.Width, je.Visual.Height);
                if (!playerRect.IntersectsWith(jeWorld)) continue;

                int pBottom = player.Position.Y + picboxplayer.Height;
                bool falling = pBottom - je.Position.Y < 24;
                bool above   = player.Position.Y < je.Position.Y + je.Visual.Height / 2;

                if (falling && above)
                {
                    je.Squish();
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
            var playerRect = new Rectangle(
                player.Position.X, player.Position.Y,
                picboxplayer.Width, picboxplayer.Height);

            for (int i = patrolEnemies.Count - 1; i >= 0; i--)
            {
                var pe = patrolEnemies[i];

                if (!pe.IsAlive)
                {
                    Controls.Remove(pe.Visual);
                    pe.Visual.Dispose();
                    patrolEnemies.RemoveAt(i);
                    continue;
                }

                // Gravity
                if (!pe.IsGrounded)
                {
                    pe.VerticalVelocity += 0.6f;
                    if (pe.VerticalVelocity > 15f) pe.VerticalVelocity = 15f;
                    pe.Position = new Point(pe.Position.X, pe.Position.Y + (int)pe.VerticalVelocity);
                }

                bool peGrounded = false;
                var peRect = pe.Bounds;
                foreach (var plat in platforms)
                {
                    var pr = new Rectangle(
                        plat.PictureBox.Left + cameraX, plat.Position.Y,
                        plat.PictureBox.Width, plat.PictureBox.Height);
                    if (!peRect.IntersectsWith(pr)) continue;

                    int ot = peRect.Bottom - pr.Top, ob = pr.Bottom - peRect.Top;
                    int ol = peRect.Right - pr.Left, orr = pr.Right - peRect.Left;
                    int min = Math.Min(Math.Min(ot, ob), Math.Min(ol, orr));

                    if (min == ot && ot < 25)
                    {
                        pe.Position = new Point(pe.Position.X, pr.Top - pe.Visual.Height);
                        pe.VerticalVelocity = 0;
                        peGrounded = true;
                    }
                    else if (min == ol || min == orr) pe.ReverseDirection();
                }
                pe.IsGrounded = peGrounded;

                // Edge detection – turn around before walking off a platform
                if (peGrounded && !pe.IsSquished)
                {
                    int frontX = pe.Direction > 0 ? pe.Position.X + pe.Visual.Width : pe.Position.X;
                    int probeY  = pe.Position.Y + pe.Visual.Height + 4;
                    var probe   = new Rectangle(frontX - 2, probeY, 8, 6);
                    bool groundAhead = false;
                    foreach (var plat in platforms)
                    {
                        var pr = new Rectangle(
                            plat.PictureBox.Left + cameraX, plat.Position.Y,
                            plat.PictureBox.Width, plat.PictureBox.Height);
                        if (probe.IntersectsWith(pr)) { groundAhead = true; break; }
                    }
                    if (!groundAhead) pe.ReverseDirection();
                }

                if (pe.IsSquished)
                {
                    if (pe.UpdateSquish(FIXED_STEP_MS)) pe.Kill();
                    pe.Visual.Location = new Point(pe.Position.X - cameraX, pe.Position.Y);
                    continue;
                }

                pe.Update();
                pe.Visual.Location = new Point(pe.Position.X - cameraX, pe.Position.Y);

                if (isDying) continue;
                var peWorld = new Rectangle(pe.Position.X, pe.Position.Y, pe.Visual.Width, pe.Visual.Height);
                if (!playerRect.IntersectsWith(peWorld)) continue;

                int pBot2 = player.Position.Y + picboxplayer.Height;
                bool fall2 = pBot2 - pe.Position.Y < 24;
                bool abv2  = player.Position.Y < pe.Position.Y + pe.Visual.Height / 2;

                if (fall2 && abv2)
                {
                    pe.Squish();
                    player.Bounce();
                    player.Score += 175;
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
            var playerRect = new Rectangle(
                player.Position.X, player.Position.Y,
                picboxplayer.Width, picboxplayer.Height);

            for (int i = flyingEnemies.Count - 1; i >= 0; i--)
            {
                var fl = flyingEnemies[i];

                if (!fl.IsAlive)
                {
                    Controls.Remove(fl.Visual);
                    fl.Visual.Dispose();
                    flyingEnemies.RemoveAt(i);
                    continue;
                }

                if (fl.HasWings)
                {
                    // Sine-wave flight – Update() manages both axes
                    fl.Update();
                    fl.Visual.Location = new Point(fl.Position.X - cameraX, fl.Position.Y);
                }
                else
                {
                    // Wings stripped – apply gravity and platform collision
                    if (!fl.IsGrounded)
                    {
                        fl.VerticalVelocity += 0.6f;
                        if (fl.VerticalVelocity > 15f) fl.VerticalVelocity = 15f;
                        fl.Position = new Point(fl.Position.X, fl.Position.Y + (int)fl.VerticalVelocity);
                    }

                    bool flGrounded = false;
                    var flRect = fl.Bounds;
                    foreach (var plat in platforms)
                    {
                        var pr = new Rectangle(
                            plat.PictureBox.Left + cameraX, plat.Position.Y,
                            plat.PictureBox.Width, plat.PictureBox.Height);
                        if (!flRect.IntersectsWith(pr)) continue;

                        int ot = flRect.Bottom - pr.Top, ob = pr.Bottom - flRect.Top;
                        int ol = flRect.Right - pr.Left, orr = pr.Right - flRect.Left;
                        int min = Math.Min(Math.Min(ot, ob), Math.Min(ol, orr));

                        if (min == ot && ot < 25)
                        {
                            fl.Position = new Point(fl.Position.X, pr.Top - fl.Visual.Height);
                            fl.VerticalVelocity = 0;
                            flGrounded = true;
                        }
                        else if (min == ol || min == orr) fl.ReverseDirection();
                    }
                    fl.IsGrounded = flGrounded;

                    if (fl.IsSquished)
                    {
                        if (fl.UpdateSquish(FIXED_STEP_MS)) fl.Kill();
                        fl.Visual.Location = new Point(fl.Position.X - cameraX, fl.Position.Y);
                        continue;
                    }

                    fl.Update();
                    fl.Visual.Location = new Point(fl.Position.X - cameraX, fl.Position.Y);
                }

                if (isDying) continue;
                var flWorld = new Rectangle(fl.Position.X, fl.Position.Y, fl.Visual.Width, fl.Visual.Height);
                if (!playerRect.IntersectsWith(flWorld)) continue;

                int pBot3 = player.Position.Y + picboxplayer.Height;
                bool fall3 = pBot3 - fl.Position.Y < 24;
                bool abv3  = player.Position.Y < fl.Position.Y + fl.Visual.Height / 2;

                if (fall3 && abv3)
                {
                    bool hadWings = fl.HasWings;
                    fl.Stomp();
                    player.Bounce();
                    player.Score += hadWings ? 200 : 300;
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
                    // Continue checking pipes – don't break early
                }
                else if (plat.Type == "pipe")
                {
                    // Pipes block horizontal movement when the player approaches from the side
                    bool feetBelowRim = player.Position.Y + picboxplayer.Height > platRect.Top + 10;
                    if (feetBelowRim && overlapLeft > 0 && overlapLeft <= 22 && overlapLeft < overlapTop)
                    {
                        player.Position = new Point(platRect.Left - picboxplayer.Width, player.Position.Y);
                    }
                    else if (feetBelowRim && overlapRight > 0 && overlapRight <= 22 && overlapRight < overlapTop)
                    {
                        player.Position = new Point(platRect.Right, player.Position.Y);
                    }
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
            foreach (var je in jumpingEnemies) { Controls.Remove(je.Visual); je.Visual.Dispose(); }
            jumpingEnemies.Clear();
            foreach (var pe in patrolEnemies) { Controls.Remove(pe.Visual); pe.Visual.Dispose(); }
            patrolEnemies.Clear();
            foreach (var fl in flyingEnemies) { Controls.Remove(fl.Visual); fl.Visual.Dispose(); }
            flyingEnemies.Clear();
            ClearCoins();
            ClearPowerUps();
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
            };
            var hardPool = new[] {
                SECTION_HIGH, SECTION_ARCH, SECTION_SUSPENDED, SECTION_LEDGE_HOP, SECTION_DESCEND,
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
