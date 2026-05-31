# Super Mario — Sprite Integration Guide (individual images)

> **עודכן:** הפרויקט עבר מ‑sprite‑sheets + ציור GDI+ ל**תמונות בודדות + `PictureBox.Image`**.
> **Updated:** the project moved from sprite sheets + GDI+ drawing to **individual images + `PictureBox.Image`**.
> ראו גם `CLAUDE.md` בשורש. / See also `CLAUDE.md` at the repo root.

## 1. הנכסים / The assets
כל ספרייט הוא קובץ PNG בודד תחת `assets/textures/sprites/` (מוגדל פי 4: אריח 16×16 → 64×64).
הם נוצרים ע"י `generate_spritesheets.py`. דוגמאות:

| קובץ / file | תוכן / contents |
|---|---|
| `mario_idle/walk1/walk2/jump.png` | פריימי השחקן (2 פריימי הליכה) / player frames |
| `luigi_idle/walk1/walk2/jump.png` | סוכני מסך האימון (מריו בירוק) / Luigi AI agents |
| `goomba_1/2`, `koopa_1/2`, `koopa_shell` | גומבה / קואפה / קליפה |
| `fast_1/2`, `jumper_1/2`, `patrol_1/2`, `flyer_1/2` | 4 האויבים הנוספים — **כל אחד עיצוב ייחודי** (חיפושית/קפיץ/אנטנה/כנפיים) |
| `squished` | אויב מעוך (גנרי) / generic squished enemy |
| `coin_1/2/3`, `mushroom` | פריטים / items |
| `question_1/2`, `empty_block`, `brick`, `pipe`, `flag` | בלוקים ועולם / blocks & world |
| `world_bg` | רקע פרוס / tiled background |
| `heart_full`, `heart_empty` | לבבות חיים ל‑HUD / HUD life hearts |

לכל דמות/אויב שני פריימי הליכה (`*_1/*_2`) שמתחלפים אוטומטית כדי להיראות "הולכים".
Each character/enemy has two walk frames that auto-alternate so it looks like it's moving.

## 2. כיצד הקוד משתמש בהם / How the code uses them
אין `Graphics`/`Paint`/חיתוך sheet. הכול דרך תמונות:

```csharp
// טעינה פעם אחת בהרצה / load once at runtime
Sprites.LoadAll();

// אובייקט = PictureBox עם תמונה / an object is a PictureBox with an image
var pb = new PictureBox { SizeMode = PictureBoxSizeMode.StretchImage };
pb.Image = Sprites.Mushroom;                 // פריט בודד / single item

// קרקע/פלטפורמה = תמונה פרוסה / tiled image
pb.BackgroundImage = Sprites.Brick;
pb.BackgroundImageLayout = ImageLayout.Tile;
```

- `Core/Sprites.cs` — טוען את כל ה‑PNG לשדות בעלי שם (`Sprites.Goomba[0]`, `Sprites.Coin[2]`...).
- `Core/Animator.cs` — מחליף פריימים לאנימציה (אויבים/מטבעות).
- אנימציה = החלפת `PictureBox.Image`, לא ציור.

## 3. הוספת/שינוי ספרייט / Adding or changing a sprite
1. ערכו את ה‑ASCII בפלטה/פריים ב‑`generate_spritesheets.py` (או הוסיפו `save_sprite(frame, "name")`).
2. הריצו `python3 generate_spritesheets.py`.
3. הוסיפו שדה תואם ב‑`Sprites.cs` (וה‑`.csproj` כבר מעתיק את כל `assets/textures/sprites/*.png` לפלט).

## 4. למה זה פשוט יותר / Why this is simpler
- אין קוד GDI+ במשחק (אין מכחולים, נתיבים, חיתוך מלבנים).
- "מראה" נקבע ע"י קבצי תמונה, לא ע"י קוד — קל להחליף ולהציג.
- כל אובייקט מתנהג אותו דבר: PictureBox עם תמונה.

## 5. מסך האימון וה‑HUD / Training screen & HUD
- **מסך האימון** (`UI/TrainingForm.cs`) הומר גם הוא לתמונות: רקע/פלטפורמות/מטבעות הם `PictureBox`
  שנוצרים פעם אחת, וסוכני לואיג'י משתמשים ב**מאגר** `PictureBox` משוכפל (`RenderScene()`),
  כדי לא ליצור/למחוק פקדים בכל פריים.
- **לבבות ה‑HUD** הם `PictureBox` (`Sprites.HeartFull/HeartEmpty`).
- ★ **החריג היחיד שנשאר עם GDI+**: `ML/NeuralNetworkControl.cs` — תרשים חי של הרשת
  (קווים/צמתים שצבעם משתנה כל פריים). תרשים דינמי כזה אי‑אפשר להמיר ל‑PNG סטטי.
