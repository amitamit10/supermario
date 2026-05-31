# CLAUDE.md — מדריך הפרויקט / Project guide

> קובץ זה נטען אוטומטית ע"י Claude Code ונועד לתת התמצאות מהירה בפרויקט.
> This file is auto-loaded by Claude Code and gives a fast orientation to the project.

## מה זה / What this is
משחק פלטפורמה בסגנון Super Mario, כתוב ב‑**C# / WinForms** (.NET Framework 4.7.2).
כולל מצב משחק רגיל ומצב **אימון AI** (נוירו‑אבולוציה) נפרד.

A Super Mario–style platformer written in **C# / WinForms** (.NET Framework 4.7.2),
with a normal game mode and a separate **AI training** mode (neuroevolution).

## בנייה והרצה / Build & run
- נבנה ורץ ב‑**Visual Studio על Windows** (פותחים `supermario.sln`, F5).
- ‏**לא** ניתן לבנות על Linux/macOS — זה WinForms על .NET Framework.
- נקודת כניסה: `supermario/Program.cs` → `MainMenuForm` → `mainWin` (משחק) או `TrainingForm` (AI).

## מבנה הקוד / Code layout
```
supermario/
├── Program.cs                כניסה / entry point
├── Core/
│   ├── Sprites.cs            ספריית התמונות — טוען PNG בודדים / image library
│   ├── Animator.cs           מחזור פריימים לאנימציה / frame cycler
│   ├── Player.cs             מצב + פיזיקת השחקן (פשוטה) / player state & physics
│   ├── GameData.cs           Coin / Mushroom / QuestionBlock (נתונים בלבד)
│   └── GameManager.cs        דגל "המשחק רץ" / running flag
├── Enemies/
│   ├── Enemy.cs              מחלקת בסיס משותפת / shared base class
│   └── Goomba/Koopa/FastEnemy/JumpingEnemy/PlatformPatrolEnemy/FlyingEnemy.cs
├── World/GameObjectS.cs      עטיפת פלטפורמה/צינור / platform & pipe wrapper
├── UI/
│   ├── mainWin*.cs           חלון המשחק (partial class) — לולאה, קלט, פיזיקה,
│   │                         בניית רמות, אספנים, אויבים, HUD
│   ├── MainMenuForm.cs       מסך פתיחה בסיסי / basic start screen
│   └── TrainingForm.cs       מסך אימון ה‑AI / AI training screen  (לא לגעת)
└── ML/                       רשת נוירונים + אבולוציה / neural net + GA  (לא לגעת)
generate_spritesheets.py      מחולל הספרייטים / sprite generator
assets/textures/sprites/      קובצי ה‑PNG שנוצרים / generated PNGs
```

## מודל הרינדור / Rendering model  ★ חשוב
**כל אובייקט במשחק הוא `PictureBox` עם תמונה** (`Image` או `BackgroundImage`).
אין שום ציור GDI+ ידני בקוד המשחק (אין `Graphics`/`Paint`/מכחולים). כדי לשנות מראה,
משנים את ה‑PNG בסקריפט הפייתון — לא את הקוד.

**Every game object is a `PictureBox` with an image.** There is **no hand-written
GDI+ drawing** in the game code (no `Graphics`/`Paint`/brushes). To change how
something looks, edit the PNG via the Python script — not the code.

- שחקן: `mainWin.HUD.cs → UpdatePlayerSprite()` מציב `picboxplayer.Image`.
- אויבים: `Enemy` מציב `Visual.Image` (אנימציה דרך `Animator`).
- מטבעות/בלוקים: `mainWin.UpdateAnimatedSprites()` מחליף תמונות.
- קרקע/פלטפורמות: `BackgroundImage` + `BackgroundImageLayout = Tile`.
- רקע: `BackgroundImage` של הטופס (פרוס).
- HUD: לבבות החיים הם `PictureBox` (`Sprites.HeartFull/HeartEmpty`); הניקוד/רמה נשארים טקסט.
- **מסך האימון (`TrainingForm`) גם הוא תמונות**: רקע/פלטפורמות/מטבעות וסוכני לואיג'י הם
  `PictureBox` (הסוכנים ממאגר משוכפל). ראה `RenderScene()`.

★ **החריג היחיד ל‑GDI+**: `ML/NeuralNetworkControl.cs` — *תרשים נתונים חי* של הרשת
(קווים/צמתים שצבעם משתנה כל פריים). תרשים דינמי כזה אי‑אפשר להפוך ל‑PNG, ולכן הוא נשאר GDI+.

**Hearts** are `PictureBox`es; the **AI training screen also uses images** (background,
platforms, coins, Luigi agents via a pooled set of `PictureBox`es — see `RenderScene()`).
The **only** remaining GDI+ is `ML/NeuralNetworkControl.cs`, a live data chart that
cannot be a static PNG.

## צינור הנכסים / Asset pipeline
1. עורכים את הספרייט (פיקסל‑ארט כ‑ASCII) ב‑`generate_spritesheets.py`.
2. מריצים: `python3 generate_spritesheets.py`  → נוצרים PNG ב‑`assets/textures/sprites/`.
3. ה‑`.csproj` מעתיק את כל `assets/textures/sprites/*.png` לתיקיית הפלט ליד ה‑EXE.
4. `Sprites.LoadAll()` טוען אותם פעם אחת בהרצה.

**מוסכמות עיצוב הספרייטים / sprite design rules:**
- כל סוג אויב הוא **עיצוב ייחודי** (לא רק שינוי צבע): גומבה(פטרייה), קואפה(צב),
  fast(חיפושית אדומה), jumper(כחול עם רגלי קפיץ), patrol(כתום עם אנטנה), flyer(קואפה עם כנפיים).
- לכל דמות הולכת **שני פריימי הליכה** (`*_1/*_2`, ולשחקן `*_walk1/walk2`) עם רגליים/כנפיים
  מתחלפות — הקוד מחליף ביניהם אוטומטית כך שייראה שהיא זזה. שני פריימי זוג חייבים אותו גובה.
- ספרייטים חדשים בשלב זה: `luigi_idle/walk1/walk2/jump`, `heart_full`, `heart_empty`.

## מוסכמות / Conventions
- הערות: **עברית** לכותרות/הסברים, **אנגלית** להערות טכניות קצרות.
- כל קובץ מחולק ל"פרקים" עם כותרות באנר (`// ═══ ... ═══`).
- פיזיקה ולולאת משחק **פשוטות בכוונה** (טיימר 16ms, מהירות/כבידה קבועות).

## מה לא לגעת / Do NOT touch
- `ML/` — **לוגיקת** ה‑AI (רשת/אבולוציה); עצמאית ולא תלויה בגרפיקת המשחק.
  (התצוגה ב‑`TrainingForm` כן הומרה לתמונות, אבל אין לגעת בלוגיקת הסוכן/האבולוציה.)
- `UI/*.Designer.cs` — קוד שנוצר אוטומטית.
- `UI/mainWin.LevelData.cs` — נתוני הרמות.

## עוד תיעוד / More docs
תיעוד מעמיק לפי נושאים תחת `docs/branches/` (ARCHITECTURE, ENEMIES, RENDERING,
ASSET_PIPELINE, PLAYER, WORLD, HUD_AND_MENU, LUIGI_AI ועוד).
