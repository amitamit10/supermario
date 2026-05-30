"""
====================================================================
  מחולל הספרייטים של המשחק  /  Sprite generator
====================================================================
הסקריפט מצייר כל אובייקט במשחק כתמונת PNG בודדת ושומר אותה בתיקייה
assets/textures/sprites/ . הקוד ב-C# פשוט טוען כל תמונה ומציב אותה
ב-PictureBox (אין ציור GDI+ ידני).

This script renders every game object as a single PNG into
assets/textures/sprites/. The C# code just loads each image and puts
it into a PictureBox (no hand-written GDI+ drawing).

הספרייטים מצוירים כ"פיקסל ארט" באמצעות מפת תווים (כל תו = צבע),
ומוגדלים פי SCALE כדי שייראו חדים.
====================================================================
"""

import os
import sys

try:
    from PIL import Image, ImageDraw
except ImportError:
    import subprocess
    subprocess.check_call([sys.executable, "-m", "pip", "install", "Pillow"])
    from PIL import Image, ImageDraw

# ════════════════════════════════════════════════════════════════════
#  פלטת הצבעים  /  Colour palette  (תו -> RGBA)
# ════════════════════════════════════════════════════════════════════
COLORS = {
    'T': (0, 0, 0, 0),          # שקוף / transparent
    'B': (0, 0, 0, 255),        # שחור / black
    'R': (255, 0, 0, 255),      # אדום / red
    'O': (255, 128, 0, 255),    # כתום / orange
    'Y': (255, 255, 0, 255),    # צהוב / yellow
    'S': (255, 204, 153, 255),  # עור / skin
    'U': (0, 0, 255, 255),      # כחול / blue
    'W': (255, 255, 255, 255),  # לבן / white
    'G': (0, 180, 0, 255),      # ירוק / green
    'D': (139, 69, 19, 255),    # חום כהה / dark brown
    'L': (205, 133, 63, 255),   # חום בהיר / light brown
    'C': (92, 148, 252, 255),   # תכלת שמיים / sky blue
    'P': (252, 152, 56, 255),   # אפרסק / peach
    'E': (200, 76, 12, 255),    # אדום לבנה / brick red
    # גוונים לאויבים הצבעוניים / shades for the recoloured enemies
    'r': (220, 70, 40, 255),    # אדום בהיר (Fast)
    'x': (150, 35, 20, 255),    # אדום כהה  (Fast)
    'j': (80, 100, 210, 255),   # כחול בהיר (Jumping)
    'k': (40, 55, 140, 255),    # כחול כהה  (Jumping)
    'o': (235, 140, 40, 255),   # כתום בהיר (Patrol)
    'v': (170, 80, 10, 255),    # כתום כהה  (Patrol)
    'd': (15, 85, 15, 255),     # ירוק כהה (צינור/קליפה)
    'w': (140, 220, 140, 255),  # ירוק בהיר (הדגשה)
}

SCALE = 4
OUTDIR = "assets/textures/sprites"

# ════════════════════════════════════════════════════════════════════
#  פריימים של השחקן (16x16)  /  Player frames
# ════════════════════════════════════════════════════════════════════
mario_idle = [
    "TTTTTRRRRRTTTTTT", "TTTTRRRRRRRRRTTT", "TTTTDDSDSSSTTTTT", "TTTDSDDSSDSSTTTT",
    "TTTDSDSSDSSSSTTT", "TTTDDSSSSDDDDTTT", "TTTTTTSSSSSTTTTT", "TTTTRRUUURRTTTTT",
    "TTTRRRUUURRRTTTT", "TTRRRRUUUURRRRTT", "TTSSYRUYURYSSTTT", "TTSSSOUUOUUSSTTT",
    "TTTSSUUUUUUSTTTT", "TTTTUUUUUUUTTTTT", "TTTDDDTTTDDTTTTT", "TTDDDDTTTDDDTTTT",
]
mario_walk1 = [
    "TTTTTRRRRRTTTTTT", "TTTTRRRRRRRRRTTT", "TTTTDDSDSSSTTTTT", "TTTDSDDSSDSSTTTT",
    "TTTDSDSSDSSSSTTT", "TTTDDSSSSDDDDTTT", "TTTTTTSSSSSTTTTT", "TTTTRRUUUUTTTTTT",
    "TTTRRRUUUUURRTTT", "TTRRRRUUUUURRRTT", "TTSSYRUYUUUUSSTT", "TTSSSOUUUUUSSTTT",
    "TTTSSUUUUUUTTTTT", "TTTTTUUUUTTTTTTT", "TTTDDDTTDDTTTTTT", "TTDDDDTTTDDDTTTT",
]
mario_walk2 = [
    "TTTTTRRRRRTTTTTT", "TTTTRRRRRRRRRTTT", "TTTTDDSDSSSTTTTT", "TTTDSDDSSDSSTTTT",
    "TTTDSDSSDSSSSTTT", "TTTDDSSSSDDDDTTT", "TTTTTTSSSSSTTTTT", "TTTTTRRUUURRTTTT",
    "TTTTRRRUUURRRTTT", "TTTRRRRUUUURRRRT", "TTTSSYRUYURYSSST", "TTTSSSOUUOUUSSST",
    "TTTTSSUUUUUUSTTT", "TTTTTUUUUUUUTTTT", "TTTTTDDTTTDDDTTT", "TTTTDDDTTTDDDDTT",
]
mario_jump = [
    "TTTTTRRRRRTTTTTT", "TTTTRRRRRRRRRTTT", "TTTTDDSDSSSTTTTT", "TTTDSDDSSDSSTTTT",
    "TTTDSDSSDSSSSTTT", "TTTDDSSSSDDDDTTT", "TTTTTTSSSSSTTTTT", "TTTTRRRUUTTTTTTT",
    "TTRRRRRUUURRRTTT", "TRRRRRRUUUURRRTT", "TTSSYRUYUYURSSTT", "TTSSSOUUOUUUSSTT",
    "TTTSSUUUUUUUSTTT", "TTTTUUUUUUUTTTTT", "TTTDDDTTTDDDTTTT", "TTDDDDTTTDDDDTTT",
]

# ════════════════════════════════════════════════════════════════════
#  פריימים של אויבים (16x24)  /  Enemy frames
# ════════════════════════════════════════════════════════════════════
goomba_1 = [
    "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT", "TTTTTTDDDDTTTTTT", "TTTTDDLLLLDDTTTT",
    "TTDDLLLLLLLLDDTT", "TDLLBBLLLLBBLLDT", "TDLWWBLLLLBWWLDT", "TDLWWLLLLLLWWLDT",
    "TDLLLLDDDDLLLLDT", "TDLLLLLLLLLLLLDT", "TTDDLLLLLLLLDDTT", "TTTTDDDDDDDDTTTT",
    "TTTTTSDDDSSTTTTT", "TTTTSSDDDSSSTTTT", "TTTBBBDDDBBBTTTT", "TBBBBBTTTBBBBBTT",
    "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT",
    "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT",
]
goomba_2 = [
    "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT", "TTTTTTDDDDTTTTTT", "TTTTDDLLLLDDTTTT",
    "TTDDLLLLLLLLDDTT", "TDLLBBLLLLBBLLDT", "TDLWWBLLLLBWWLDT", "TDLWWLLLLLLWWLDT",
    "TDLLLLDDDDLLLLDT", "TDLLLLLLLLLLLLDT", "TTDDLLLLLLLLDDTT", "TTTTDDDDDDDDTTTT",
    "TTTTTSDDDSSTTTTT", "TTTTSSDDDSSSTTTT", "TTBBBDDDDDDBBTTT", "TBBBBBBTTTBBBTTT",
    "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT",
    "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT",
]
koopa_1 = [
    "TTTTTGGGGGTTTTTT", "TTTGGYYYYYGGTTTT", "TTGYYYBBBYYYGTTT", "TGYYYWWWWBYYYGTT",
    "TGYYYYWWWWYYYGTT", "TTGYYYYYYYYYGTTT", "TTTGGYYYYYGGTTTT", "TTTTGGBBBGGTTTTT",
    "TTTGGGDDDDGGGTTT", "TTGGGGDDDGGGGGTT", "TGGGGDDDDDDGGGGT", "TGGYGGDDDDGGYGGT",
    "TGGYGGDDDDGGYGGT", "TGGYYGDDDDGYYGGT", "TTGGYYDDDDYYGGTT", "TTTGGYDDDDYGGTTT",
    "TTTTGGGGGGGGTTTT", "TTTTGGGGGGGGTTTT", "TTTTTGGTTGGTTTTT", "TTTTTGGTTGGTTTTT",
    "TTTLLLLTTLLLLTTT", "TTLLLLLLTLLLLLLT", "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT",
]
koopa_2 = [
    "TTTTTGGGGGTTTTTT", "TTTGGYYYYYGGTTTT", "TTGYYYBBBYYYGTTT", "TGYYYWWWWBYYYGTT",
    "TGYYYYWWWWYYYGTT", "TTGYYYYYYYYYGTTT", "TTTGGYYYYYGGTTTT", "TTTTGGBBBGGTTTTT",
    "TTTGGGDDDDGGGTTT", "TTGGGGDDDGGGGGTT", "TGGGGDDDDDDGGGGT", "TGGYGGDDDDGGYGGT",
    "TGGYGGDDDDGGYGGT", "TGGYYGDDDDGYYGGT", "TTGGYYDDDDYYGGTT", "TTTGGYDDDDYGGTTT",
    "TTTTGGGGGGGGTTTT", "TTTTGGGGGGGGTTTT", "TTTTTGGTTGGTTTTT", "TTTLLLLTTLLLLTTT",
    "TTLLLLLLTLLLLLLT", "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT",
]
# קליפת קואפה (16x12) / koopa shell
koopa_shell = [
    "TTTTGGGGGGGGTTTT", "TTGGGGGGGGGGGGTT", "TGGGGddddGGGGGGT", "GGGddGGGGddGGGGG",
    "GGdGGGGGGGGddGGG", "GGdGGGGGGGGddGGG", "GGGddGGGGddGGGGG", "TGGGGddddGGGGGGT",
    "TTGGGGGGGGGGGGTT", "TTTTGGGGGGGGTTTT", "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT",
]

# ════════════════════════════════════════════════════════════════════
#  פריטים ובלוקים (16x16)  /  Items & blocks
# ════════════════════════════════════════════════════════════════════
coin_1 = [
    "TTTTTTYYYYTTTTTT", "TTTTYYYYYYYYTTTT", "TTTYYTTYYTTYYTTT", "TTYYTTTYYTTTYYTT",
    "TTYYTTTYYTTTYYTT", "TYYTTTTYYTTTTYYT", "TYYTTTTYYTTTTYYT", "TYYTTTTYYTTTTYYT",
    "TYYTTTTYYTTTTYYT", "TYYTTTTYYTTTTYYT", "TYYTTTTYYTTTTYYT", "TTYYTTTYYTTTYYTT",
    "TTYYTTTYYTTTYYTT", "TTTYYTTYYTTYYTTT", "TTTTYYYYYYYYTTTT", "TTTTTTYYYYTTTTTT",
]
coin_2 = [
    "TTTTTTTYYTTTTTTT", "TTTTTTYYYYTTTTTT", "TTTTTYYTYYTTTTTT", "TTTTYYTTTYYTTTTT",
    "TTTTYYTTTYYTTTTT", "TTTYYTTTTTYYTTTT", "TTTYYTTTTTYYTTTT", "TTTYYTTTTTYYTTTT",
    "TTTYYTTTTTYYTTTT", "TTTYYTTTTTYYTTTT", "TTTYYTTTTTYYTTTT", "TTTTYYTTTYYTTTTT",
    "TTTTYYTTTYYTTTTT", "TTTTTYYTYYTTTTTT", "TTTTTTYYYYTTTTTT", "TTTTTTTYYTTTTTTT",
]
coin_3 = [
    "TTTTTTTTTTTTTTTT", "TTTTTTTYYTTTTTTT", "TTTTTTYYYYTTTTTT", "TTTTTTYYYYTTTTTT",
    "TTTTTTYYYYTTTTTT", "TTTTTTYYYYTTTTTT", "TTTTTTYYYYTTTTTT", "TTTTTTYYYYTTTTTT",
    "TTTTTTYYYYTTTTTT", "TTTTTTYYYYTTTTTT", "TTTTTTYYYYTTTTTT", "TTTTTTYYYYTTTTTT",
    "TTTTTTYYYYTTTTTT", "TTTTTTTYYTTTTTTT", "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT",
]
mushroom = [
    "TTTTTTBBBBTTTTTT", "TTTTBBRRRRBBTTTT", "TTTBRRRRRRRRBTTT", "TTBRRWWWWWWWRBTT",
    "TBRRWWWWWWWWWRBT", "TBRRWWWWWWWWWRBT", "TBRRWWWWWWWWWRBT", "TBRRRWWWWWWWRRBT",
    "TBRRRRRRRRRRRRBT", "TTBRRRRRRRRRRBTT", "TTTBBBBBBBBBBTTT", "TTTTBSSSSSBBTTTT",
    "TTTTBSSSSSBBTTTT", "TTTTTBBBBBBTTTTT", "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT",
]
question_1 = [
    "YPYPYPYPYPYPYPYP", "PYPYPYPYPYPYPYPY", "YPYPYYDDDDYYPYPY", "PYPYDDYYYYDDYPYP",
    "YPYPDDYYYYDDYPYP", "PYPYYYYYDDYYYPYP", "YPYPYYYDDYYYPYPY", "PYPYYDDYYYYYPYPY",
    "YPYPYDDYYYYYPYPY", "PYPYPYPYPYPYPYPY", "YPYPYDDYYYYYPYPY", "PYPYYDDYYYYYPYPY",
    "YPYPYPYPYPYPYPYP", "PYBBBYPYPYPYBBBY", "YBBBYPYPYPYYBBBY", "PYPYPYPYPYPYPYPY",
]
question_2 = [
    "YPYPYPYPYPYPYPYP", "PYPYPYPYPYPYPYPY", "YPYPYYWWWWYYPYPY", "PYPYWWYYYYWWYPYP",
    "YPYPWWYYYYWWYPYP", "PYPYYYYYWWYYYPYP", "YPYPYYYWWYYYPYPY", "PYPYYWWYYYYYPYPY",
    "YPYPYWWYYYYYPYPY", "PYPYPYPYPYPYPYPY", "YPYPYWWYYYYYPYPY", "PYPYYWWYYYYYPYPY",
    "YPYPYPYPYPYPYPYP", "PYBBBYPYPYPYBBBY", "YBBBYPYPYPYYBBBY", "PYPYPYPYPYPYPYPY",
]
empty_block = [
    "DDDDDDDDDDDDDDDD", "DLLLLLLLLLLLLLLD", "DLLDDDDDDDDDDLLD", "DLDDDDDDDDDDDDLD",
    "DLDDDDDDDDDDDDLD", "DLDDDDDDDDDDDDLD", "DLDDDDDDDDDDDDLD", "DLDDDDDDDDDDDDLD",
    "DLDDDDDDDDDDDDLD", "DLDDDDDDDDDDDDLD", "DLDDDDDDDDDDDDLD", "DLDDDDDDDDDDDDLD",
    "DLDDDDDDDDDDDDLD", "DLLDDDDDDDDDDLLD", "DLLLLLLLLLLLLLLD", "DDDDDDDDDDDDDDDD",
]
brick = [
    "EEEEEEEEEEEEEEEE", "ELLLLLLEELLLLLLE", "ELLLLLLEELLLLLLE", "ELLLLLLEELLLLLLE",
    "EEEEEEEEEEEEEEEE", "LLLEELLLLLLEELLL", "LLLEELLLLLLEELLL", "LLLEELLLLLLEELLL",
    "EEEEEEEEEEEEEEEE", "ELLLLLLEELLLLLLE", "ELLLLLLEELLLLLLE", "ELLLLLLEELLLLLLE",
    "EEEEEEEEEEEEEEEE", "LLLEELLLLLLEELLL", "LLLEELLLLLLEELLL", "EEEEEEEEEEEEEEEE",
]
# צינור (16x16) / pipe — נמתח לגובה הצינור / stretched to the pipe height
pipe = [
    "dddddddddddddddd", "dwwGGGGGGGGGGGGd", "dGGGGGGGGGGGGGGd", "dddddddddddddddd",
    "dGwGGGGGGGGGGGGd", "dGwGGGGGGGGGGGGd", "dGwGGGGGGGGGGGGd", "dGwGGGGGGGGGGGGd",
    "dGwGGGGGGGGGGGGd", "dGwGGGGGGGGGGGGd", "dGwGGGGGGGGGGGGd", "dGwGGGGGGGGGGGGd",
    "dGwGGGGGGGGGGGGd", "dGwGGGGGGGGGGGGd", "dGwGGGGGGGGGGGGd", "dddddddddddddddd",
]
# אויב מעוך (16x8) — תמונה גנרית לכל האויבים / generic squished sprite
squished = [
    "TTTTTTTTTTTTTTTT", "TTTTDDDDDDDDTTTT", "TTDDLLLLLLLLDDTT", "TDLLWLLLLLLWLLDT",
    "TDLLLLLLLLLLLLDT", "TTDDLLLLLLLLDDTT", "TTTTDDDDDDDDTTTT", "TTTTTTTTTTTTTTTT",
]

# ════════════════════════════════════════════════════════════════════
#  רקע (64x32)  /  Background tile
# ════════════════════════════════════════════════════════════════════
bg = [
    "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
    "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
    "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
    "CCCCCCCCCCWWWWCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
    "CCCCCCCWWWWWWWWWWCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
    "CCCCCCCWWWWWWWWWWCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
    "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCWWWWCCCCCCCCCCCCCCCCCCC",
    "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCWWWWWWWWWWCCCCCCCCCCCCCCCC",
    "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCWWWWWWWWWWCCCCCCCCCCCCCCCC",
    "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
    "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
    "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
    "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
    "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
    "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
    "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
    "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
    "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
    "CCCCCCCCCCCCCCCCGCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
    "CCCCCCCCCCCCCCGGGGGCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
    "CCCCCCCCCCCCCGGYYYGGCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCGCCCCCCCC",
    "CCCCCCCCCCCGGGYYYYYGGGCCCCCCCCCCCCCCCCCCCCCCCCCCCCCGGGGGCCCCCCC",
    "CCCCCCCCCGGGYYYYYYYYYGGGCCCCCCCCCCCCCCCCCCCCCCCCGGYGGYYGGCCCCCC",
    "CCCCCCCGGGYYYYYYYYYYYYYGGGCCCCCCCCCCCCCCCCCCCCGGYYYYYYYYYGGCCCC",
    "CCCCCGGGYYYYYYYYYYYYYYYYYGGGCCCCCCCCCCCCCCCGGYYYYYYYYYYYYYGGCCC",
    "CCCGGGYYYYYYYYYYYYYYYYYYYYYGGGCCCCCCCCCCCGGYYYYYYYYYYYYYYYGGCCC",
    "GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG",
    "GLLGLLGLLGLLGLLGLLGLLGLLGLLGLLGLLGLLGLLGLLGLLGLLGLLGLLGLLGLLGLLG",
    "LLDDLLDDLLDDLLDDLLDDLLDDLLDDLLDDLLDDLLDDLLDDLLDDLLDDLLDDLLDDLLDD",
    "DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD",
    "DLLLLDDDDLLLLDDDDLLLLDDDDLLLLDDDDLLLLDDDDLLLLDDDDLLLLDDDDLLLLDDD",
    "DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD",
]


# ════════════════════════════════════════════════════════════════════
#  פונקציות עזר  /  Helpers
# ════════════════════════════════════════════════════════════════════
def recolor(frame, mapping):
    """מחזיר עותק של הפריים עם החלפת תווי צבע לפי המפה / recolour a frame."""
    return ["".join(mapping.get(ch, ch) for ch in row) for row in frame]


def save_sprite(frame, name):
    """שומר פריים יחיד כקובץ PNG מוגדל פי SCALE / save one frame as a PNG."""
    # מורידים שורות שקופות בתחתית כדי שהדמות תשב בתחתית התיבה ולא "תרחף"
    # strip trailing transparent rows so the sprite sits at the bottom of its box
    frame = list(frame)
    while len(frame) > 1 and all(ch == "T" for ch in frame[-1]):
        frame = frame[:-1]
    h = len(frame)
    w = max(len(row) for row in frame)   # רוחב = השורה הארוכה ביותר / width = longest row
    img = Image.new("RGBA", (w * SCALE, h * SCALE), (0, 0, 0, 0))
    for y in range(h):
        for x in range(w):
            ch = frame[y][x] if x < len(frame[y]) else "T"   # שורה קצרה -> שקוף / pad short rows
            color = COLORS.get(ch, (0, 0, 0, 0))
            for sy in range(SCALE):
                for sx in range(SCALE):
                    img.putpixel((x * SCALE + sx, y * SCALE + sy), color)
    path = os.path.join(OUTDIR, name + ".png")
    img.save(path)
    print("saved", path)


def save_flagpole(name):
    """מצייר עמוד דגל (עמוד + כדור + דגל + בסיס) ושומר אותו / draw the goal flagpole."""
    w, h = 16 * SCALE, 40 * SCALE
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle([7 * SCALE, 3 * SCALE, 9 * SCALE, 38 * SCALE], fill=(200, 200, 210, 255))   # עמוד / pole
    d.ellipse([5 * SCALE, 0, 11 * SCALE, 4 * SCALE], fill=(255, 210, 30, 255))              # כדור / ball
    d.polygon([(9 * SCALE, 5 * SCALE), (9 * SCALE, 13 * SCALE), (15 * SCALE, 9 * SCALE)],
              fill=(60, 180, 60, 255))                                                       # דגל / flag
    d.rectangle([3 * SCALE, 37 * SCALE, 13 * SCALE, 40 * SCALE - 1], fill=(90, 90, 100, 255))  # בסיס / base
    path = os.path.join(OUTDIR, name + ".png")
    img.save(path)
    print("saved", path)


# ════════════════════════════════════════════════════════════════════
#  יצירת כל הספרייטים  /  Generate every sprite
# ════════════════════════════════════════════════════════════════════
os.makedirs(OUTDIR, exist_ok=True)

# שחקן / player
save_sprite(mario_idle,  "mario_idle")
save_sprite(mario_walk1, "mario_walk1")
save_sprite(mario_walk2, "mario_walk2")
save_sprite(mario_jump,  "mario_jump")

# אויבים / enemies
save_sprite(goomba_1, "goomba_1")
save_sprite(goomba_2, "goomba_2")
save_sprite(koopa_1, "koopa_1")
save_sprite(koopa_2, "koopa_2")
save_sprite(koopa_shell, "koopa_shell")
# וריאציות צבע על Goomba/Koopa / colour variants
save_sprite(recolor(goomba_1, {"L": "r", "D": "x"}), "fast_1")     # אדום / red
save_sprite(recolor(goomba_2, {"L": "r", "D": "x"}), "fast_2")
save_sprite(recolor(goomba_1, {"L": "j", "D": "k"}), "jumper_1")   # כחול / blue
save_sprite(recolor(goomba_2, {"L": "j", "D": "k"}), "jumper_2")
save_sprite(recolor(goomba_1, {"L": "o", "D": "v"}), "patrol_1")   # כתום / orange
save_sprite(recolor(goomba_2, {"L": "o", "D": "v"}), "patrol_2")
save_sprite(koopa_1, "flyer_1")                                    # ירוק (קואפה מעופפת) / green
save_sprite(koopa_2, "flyer_2")
save_sprite(squished, "squished")

# פריטים / items
save_sprite(coin_1, "coin_1")
save_sprite(coin_2, "coin_2")
save_sprite(coin_3, "coin_3")
save_sprite(mushroom, "mushroom")

# בלוקים ועולם / blocks & world
save_sprite(question_1, "question_1")
save_sprite(question_2, "question_2")
save_sprite(empty_block, "empty_block")
save_sprite(brick, "brick")
save_sprite(pipe, "pipe")
save_flagpole("flag")
save_sprite(bg, "world_bg")

print("\nכל הספרייטים נוצרו בהצלחה! / All sprites generated successfully.")
