
namespace supermario
{
    partial class mainWin
    {
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
        // 5-step descending staircase – mirrors STAIR_UP coming back down
        private static readonly PlatformData[] SECTION_DESCENT_STAIRS = {
            new PlatformData(0, -140, 80, 20), new PlatformData(90, -110, 80, 20),
            new PlatformData(180, -80, 80, 20), new PlatformData(270, -50, 80, 20),
            new PlatformData(360, -20, 80, 20)
        };
        // Two wide platforms flanking a 155 px pit – commitment running jump
        private static readonly PlatformData[] SECTION_BIG_GAP = {
            new PlatformData(0, -50, 110, 20), new PlatformData(265, -50, 110, 20)
        };
        // Five narrow cloud platforms at altitude – precise footing required
        private static readonly PlatformData[] SECTION_CLOUD_WALK = {
            new PlatformData(0, -150, 60, 20), new PlatformData(110, -175, 60, 20),
            new PlatformData(220, -150, 60, 20), new PlatformData(330, -175, 60, 20),
            new PlatformData(440, -150, 60, 20)
        };
        // Castle battlements – four equal platforms at uniform height with 30 px gaps
        private static readonly PlatformData[] SECTION_BATTLEMENTS = {
            new PlatformData(0, -65, 70, 20), new PlatformData(100, -65, 70, 20),
            new PlatformData(200, -65, 70, 20), new PlatformData(300, -65, 70, 20)
        };

        private static readonly PlatformData[][] ALL_SECTIONS = {
            SECTION_LONG_RUN, SECTION_BRIDGE, SECTION_GENTLE_HOP,
            SECTION_STAIRS, SECTION_STAIR_UP, SECTION_TRIPLE_JUMP, SECTION_PYRAMID,
            SECTION_DESCEND, SECTION_DESCENT_STAIRS,
            SECTION_GAP_JUMPS, SECTION_WIDE_GAPS, SECTION_DOUBLE_GAP, SECTION_BIG_GAP,
            SECTION_WAVE, SECTION_ZIGZAG, SECTION_VALLEY,
            SECTION_HIGH, SECTION_CHALLENGE, SECTION_ARCH, SECTION_MULTI_LEVEL,
            SECTION_LEDGE_HOP, SECTION_CASTLE, SECTION_BATTLEMENTS,
            SECTION_SUSPENDED, SECTION_CLOUD_WALK
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
            // Row of 3 above Y=433 platform (440-580): block_Y = 433-158 = 275
            new QBlockDef(460, 275, PowerUpType.Coin),
            new QBlockDef(510, 275, PowerUpType.Coin),
            new QBlockDef(560, 275, PowerUpType.Mushroom),
            // Above Y=393 raised platform (850-1050): block_Y = 393-158 = 235
            new QBlockDef(970, 235, PowerUpType.Mushroom),
            // Row of 3 above Y=393 elevated run (1440-1700): block_Y = 235
            new QBlockDef(1500, 235, PowerUpType.Coin),
            new QBlockDef(1550, 235, PowerUpType.Coin),
            new QBlockDef(1600, 235, PowerUpType.Coin),
            // Above Y=373 challenge platform (2070-2200): block_Y = 373-158 = 215
            new QBlockDef(2120, 215, PowerUpType.Mushroom),
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
            // Above Y=433 platform (200-340): block_Y = 433-158 = 275
            new QBlockDef(290,  275, PowerUpType.Mushroom),
            // Above Y=373 platform (700-860): block_Y = 373-158 = 215
            new QBlockDef(760,  215, PowerUpType.Coin),
            // Above Y=393 platform (1070-1210): block_Y = 393-158 = 235
            new QBlockDef(1110, 235, PowerUpType.Coin),
            // Above Y=413 platform (1370-1490): block_Y = 413-158 = 255
            new QBlockDef(1420, 255, PowerUpType.Mushroom),
            // Row above Y=353 deep traverse (1560-1760): block_Y = 353-158 = 195
            new QBlockDef(1610, 195, PowerUpType.Coin),
            new QBlockDef(1660, 195, PowerUpType.Coin),
            // Above pipe top at Y=353 (2100-2180): block_Y = 195
            new QBlockDef(2165, 195, PowerUpType.Mushroom),
            // Above Y=433 platform (2540-2620), accessible from nearby: block_Y = 275
            new QBlockDef(2530, 275, PowerUpType.Coin),
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
            // Above Y=373 platform (160-280): block_Y = 373-158 = 215
            new QBlockDef(240,  215, PowerUpType.Coin),
            // Above Y=353 landing (990-1130): block_Y = 353-158 = 195
            new QBlockDef(1080, 195, PowerUpType.Mushroom),
            // Row above Y=273 high platform (1390-1490): block_Y = 273-158 = 115
            new QBlockDef(1440, 115, PowerUpType.Coin),
            new QBlockDef(1490, 115, PowerUpType.Coin),
            // Above Y=313 sky traverse (1560-1680): block_Y = 313-158 = 155
            new QBlockDef(1640, 155, PowerUpType.Mushroom),
            // Between platforms over Y=393 zone, reachable from ground: block_Y = 235
            new QBlockDef(2000, 235, PowerUpType.Coin),
            // Row above Y=273 reward platform (2270-2410): block_Y = 115
            new QBlockDef(2310, 115, PowerUpType.Coin),
            new QBlockDef(2360, 115, PowerUpType.Mushroom),
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
    }
}
