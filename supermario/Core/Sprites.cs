using System;
using System.Drawing;
using System.IO;

namespace supermario
{
    /// ════════════════════════════════════════════════════════════════════
    ///  ספריית התמונות / Sprite library
    /// --------------------------------------------------------------------
    ///  טוענת פעם אחת את כל קובצי ה-PNG מהתיקייה assets/textures/sprites
    ///  ומחזיקה אותם בשדות בעלי שם. כל שאר הקוד פשוט קורא, למשל,
    ///  Sprites.Goomba[0] ומציב אותו ב-PictureBox.Image — אין ציור GDI+.
    ///
    ///  Loads every PNG from assets/textures/sprites once and keeps them in
    ///  named fields. The rest of the game just reads e.g. Sprites.Goomba[0]
    ///  and assigns it to a PictureBox.Image — no GDI+ drawing anywhere.
    /// ════════════════════════════════════════════════════════════════════
    internal static class Sprites
    {
        // ── שחקן / Player (ימינה + עותקים הפוכים לשמאל) ──────────────────
        public static Image MarioIdle, MarioJump;
        public static Image[] MarioWalk;                 // [0], [1]
        public static Image MarioIdleLeft, MarioJumpLeft;
        public static Image[] MarioWalkLeft;             // pre-flipped copies

        // ── לואיג'י / Luigi (סוכני מסך האימון — מריו בירוק) ──────────────
        public static Image LuigiIdle, LuigiJump;
        public static Image[] LuigiWalk;                 // [0], [1] — אנימציית הליכה

        // ── אויבים / Enemies (כל אחד: שני פריימי הליכה) ──────────────────
        public static Image[] Goomba, Koopa, Fast, Jumper, Patrol, Flyer;
        public static Image KoopaShell;                  // קליפת קואפה
        public static Image Squished;                    // אויב מעוך (גנרי)

        // ── פריטים / Items ───────────────────────────────────────────────
        public static Image[] Coin;                      // [0..2] אנימציית סיבוב
        public static Image Mushroom;

        // ── בלוקים ועולם / Blocks & world ────────────────────────────────
        public static Image[] Question;                  // [0], [1] הבהוב
        public static Image EmptyBlock, Brick, Pipe, Flag, Background;

        // ── HUD ──────────────────────────────────────────────────────────
        public static Image HeartFull, HeartEmpty;       // לבבות חיים / life hearts

        private static bool _loaded;

        // ════════════════════════════════════════════════════════════════
        //  טעינה / Loading
        // ════════════════════════════════════════════════════════════════
        public static void LoadAll()
        {
            if (_loaded) return;
            _loaded = true;                              // never retry, even on failure

            string dir;
            try { dir = FindSpritesDirectory(); }
            catch { return; }                            // assets missing -> images stay null

            // שחקן / player
            MarioIdle = Load(dir, "mario_idle");
            MarioJump = Load(dir, "mario_jump");
            MarioWalk = new[] { Load(dir, "mario_walk1"), Load(dir, "mario_walk2") };
            MarioIdleLeft = FlipX(MarioIdle);
            MarioJumpLeft = FlipX(MarioJump);
            MarioWalkLeft = new[] { FlipX(MarioWalk[0]), FlipX(MarioWalk[1]) };

            // לואיג'י (סוכני האימון) / Luigi (training agents)
            LuigiIdle = Load(dir, "luigi_idle");
            LuigiJump = Load(dir, "luigi_jump");
            LuigiWalk = new[] { Load(dir, "luigi_walk1"), Load(dir, "luigi_walk2") };

            // אויבים / enemies
            Goomba = new[] { Load(dir, "goomba_1"), Load(dir, "goomba_2") };
            Koopa  = new[] { Load(dir, "koopa_1"),  Load(dir, "koopa_2") };
            Fast   = new[] { Load(dir, "fast_1"),   Load(dir, "fast_2") };
            Jumper = new[] { Load(dir, "jumper_1"), Load(dir, "jumper_2") };
            Patrol = new[] { Load(dir, "patrol_1"), Load(dir, "patrol_2") };
            Flyer  = new[] { Load(dir, "flyer_1"),  Load(dir, "flyer_2") };
            KoopaShell = Load(dir, "koopa_shell");
            Squished   = Load(dir, "squished");

            // פריטים / items
            Coin = new[] { Load(dir, "coin_1"), Load(dir, "coin_2"), Load(dir, "coin_3") };
            Mushroom = Load(dir, "mushroom");

            // בלוקים ועולם / blocks & world
            Question = new[] { Load(dir, "question_1"), Load(dir, "question_2") };
            EmptyBlock = Load(dir, "empty_block");
            Brick = Load(dir, "brick");
            Pipe = Load(dir, "pipe");
            Flag = Load(dir, "flag");
            Background = Load(dir, "world_bg");

            // HUD
            HeartFull  = Load(dir, "heart_full");
            HeartEmpty = Load(dir, "heart_empty");
        }

        // ════════════════════════════════════════════════════════════════
        //  עזרים / Helpers
        // ════════════════════════════════════════════════════════════════

        // טוען PNG בודד לתוך Bitmap עצמאי (משחרר את הקובץ/הזרם מיד)
        // Load a single PNG into a standalone Bitmap (frees the file/stream now).
        private static Image Load(string directory, string name)
        {
            string path = Path.Combine(directory, name + ".png");
            try
            {
                if (!File.Exists(path)) return null;
                var bytes = File.ReadAllBytes(path);
                using (var ms = new MemoryStream(bytes))
                using (var loaded = Image.FromStream(ms))
                    return new Bitmap(loaded);
            }
            catch { return null; }                        // missing/corrupt -> null (blank sprite)
        }

        // יוצר עותק הפוך אופקית (לשחקן שפונה שמאלה) / horizontal mirror copy
        private static Image FlipX(Image src)
        {
            if (src == null) return null;
            var copy = new Bitmap(src);
            copy.RotateFlip(RotateFlipType.RotateNoneFlipX);
            return copy;
        }

        // מחפש את התיקייה assets/textures/sprites כלפי מעלה מתיקיית ההרצה
        // Search upward from the run directory for assets/textures/sprites.
        private static string FindSpritesDirectory()
        {
            foreach (string root in new[] { AppDomain.CurrentDomain.BaseDirectory, Directory.GetCurrentDirectory() })
            {
                var directory = new DirectoryInfo(root);
                while (directory != null)
                {
                    string candidate = Path.Combine(directory.FullName, "assets", "textures", "sprites");
                    if (Directory.Exists(candidate)) return candidate;
                    directory = directory.Parent;
                }
            }
            throw new DirectoryNotFoundException("Could not find assets/textures/sprites.");
        }
    }
}
