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

עקרונות עיצוב / design principles:
- כל אויב בעל צללית ייחודית משלו (לא רק שינוי צבע).
- לכל דמות הולכת יש שני פריימים (walk1/walk2) עם רגליים/כנפיים מתחלפות
  כדי שייראה שהיא זזה. הקוד מחליף ביניהם אוטומטית.
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
# הצבעים המקוריים נשמרים כדי לא לשנות ספרייטים קיימים; נוספו רק צבעים חדשים
# (אפור לקפיץ/אנטנה, ורוד ללב, ירוק-כהה לכנף). / original colours preserved;
# only new colours added (greys, heart pink, wing dark-green).
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
    # גוונים לאויבים הצבעוניים / shades for the coloured enemies
    'r': (220, 70, 40, 255),    # אדום בהיר (Fast)
    'x': (150, 35, 20, 255),    # אדום כהה  (Fast)
    'j': (80, 100, 210, 255),   # כחול בהיר (Jumping)
    'k': (40, 55, 140, 255),    # כחול כהה  (Jumping)
    'o': (235, 140, 40, 255),   # כתום בהיר (Patrol)
    'v': (170, 80, 10, 255),    # כתום כהה  (Patrol)
    'd': (15, 85, 15, 255),     # ירוק כהה (צינור/קליפה)
    'w': (140, 220, 140, 255),  # ירוק בהיר (הדגשה)
    # ── צבעים חדשים לשלב זה / new colours ──
    'g': (24, 120, 40, 255),    # ירוק-כהה לדגם הכנף/שריון / wing & shell pattern
    'a': (180, 180, 190, 255),  # אפור בהיר — קפיץ/אנטנה / light grey (spring/antenna)
    'm': (100, 100, 110, 255),  # אפור כהה — גבעול אנטנה / dark grey (antenna stalk)
    'h': (235, 70, 90, 255),    # ורוד-אדום ללב מלא / heart red
    'n': (120, 40, 50, 255),    # אדום כהה למתאר לב ריק / heart outline
}

SCALE = 4
OUTDIR = "assets/textures/sprites"

# ════════════════════════════════════════════════════════════════════
#  השחקנים — מריו ולואיג'י (16x16)  /  Players
#  הגוף נשמר כמקור; רק הרגליים (שורות 14–15) מתחלפות כדי שייראה שהוא הולך.
#  Body kept as the original; only the legs (rows 14–15) swap so it walks.
# ════════════════════════════════════════════════════════════════════
mario_idle = [
    "TTTTTRRRRRTTTTTT", "TTTTRRRRRRRRRTTT", "TTTTDDSDSSSTTTTT", "TTTDSDDSSDSSTTTT",
    "TTTDSDSSDSSSSTTT", "TTTDDSSSSDDDDTTT", "TTTTTTSSSSSTTTTT", "TTTTRRUUURRTTTTT",
    "TTTRRRUUURRRTTTT", "TTRRRRUUUURRRRTT", "TTSSYRUYURYSSTTT", "TTSSSOUUOUUSSTTT",
    "TTTSSUUUUUUSTTTT", "TTTTUUUUUUUTTTTT", "TTTDDDTTTDDTTTTT", "TTDDDDTTTDDDTTTT",
]
mario_jump = [
    "TTTTTRRRRRTTTTTT", "TTTTRRRRRRRRRTTT", "TTTTDDSDSSSTTTTT", "TTTDSDDSSDSSTTTT",
    "TTTDSDSSDSSSSTTT", "TTTDDSSSSDDDDTTT", "TTTTTTSSSSSTTTTT", "TTTTRRRUUTTTTTTT",
    "TTRRRRRUUURRRTTT", "TRRRRRRUUUURRRTT", "TTSSYRUYUYURSSTT", "TTSSSOUUOUUUSSTT",
    "TTTSSUUUUUUUSTTT", "TTTTUUUUUUUTTTTT", "TTTDDDTTTDDDTTTT", "TTDDDDTTTDDDDTTT",
]
# גוף משותף (שורות 0–13) לפריימי ההליכה / shared body for the walk frames
_mario_body = mario_idle[:14]
# רגלי הליכה: רגל אחת בולטת קדימה ומתחלפת בין הפריימים / a clear alternating stride
_legs_walk1 = ["TTDDDDTTTDDDTTTT", "TDDDDDTTTDDDDTTT"]   # רגל שמאל בולטת קדימה
_legs_walk2 = ["TTTDDDTTTDDDDTTT", "TTDDDDTTTDDDDDTT"]   # רגל ימין בולטת קדימה
mario_walk1 = _mario_body + _legs_walk1
mario_walk2 = _mario_body + _legs_walk2


def to_luigi(frame):
    """לואיג'י = מריו בירוק: כובע/חולצה אדומים -> ירוקים / Luigi is green Mario."""
    return [row.replace('R', 'G') for row in frame]


luigi_idle  = to_luigi(mario_idle)
luigi_jump  = to_luigi(mario_jump)
luigi_walk1 = to_luigi(mario_walk1)
luigi_walk2 = to_luigi(mario_walk2)

# ════════════════════════════════════════════════════════════════════
#  אויבים  /  Enemies
#  Goomba/Koopa נשמרים כמקור; Fast/Jumper/Patrol/Flyer קיבלו עיצוב ייחודי.
# ════════════════════════════════════════════════════════════════════
# ── Goomba — פטרייה חומה קלאסית (מקור) / classic goomba (original) ───
goomba_1 = [
    "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT", "TTTTTTDDDDTTTTTT", "TTTTDDLLLLDDTTTT",
    "TTDDLLLLLLLLDDTT", "TDLLBBLLLLBBLLDT", "TDLWWBLLLLBWWLDT", "TDLWWLLLLLLWWLDT",
    "TDLLLLDDDDLLLLDT", "TDLLLLLLLLLLLLDT", "TTDDLLLLLLLLDDTT", "TTTTDDDDDDDDTTTT",
    "TTTTTSDDDSSTTTTT", "TTTTSSDDDSSSTTTT", "TTTBBBDDDBBBTTTT", "TBBBBBTTTBBBBBTT",
]
goomba_2 = [
    "TTTTTTTTTTTTTTTT", "TTTTTTTTTTTTTTTT", "TTTTTTDDDDTTTTTT", "TTTTDDLLLLDDTTTT",
    "TTDDLLLLLLLLDDTT", "TDLLBBLLLLBBLLDT", "TDLWWBLLLLBWWLDT", "TDLWWLLLLLLWWLDT",
    "TDLLLLDDDDLLLLDT", "TDLLLLLLLLLLLLDT", "TTDDLLLLLLLLDDTT", "TTTTDDDDDDDDTTTT",
    "TTTTTSDDDSSTTTTT", "TTTTSSDDDSSSTTTT", "TTBBBDDDDDDBBTTT", "TBBBBBBTTTBBBTTT",
]
# ── Koopa — צב ירוק (מקור) / green koopa (original) ──────────────────
koopa_1 = [
    "TTTTTGGGGGTTTTTT", "TTTGGYYYYYGGTTTT", "TTGYYYBBBYYYGTTT", "TGYYYWWWWBYYYGTT",
    "TGYYYYWWWWYYYGTT", "TTGYYYYYYYYYGTTT", "TTTGGYYYYYGGTTTT", "TTTTGGBBBGGTTTTT",
    "TTTGGGDDDDGGGTTT", "TTGGGGDDDGGGGGTT", "TGGGGDDDDDDGGGGT", "TGGYGGDDDDGGYGGT",
    "TGGYGGDDDDGGYGGT", "TGGYYGDDDDGYYGGT", "TTGGYYDDDDYYGGTT", "TTTGGYDDDDYGGTTT",
    "TTTTGGGGGGGGTTTT", "TTTTGGGGGGGGTTTT", "TTTTTGGTTGGTTTTT", "TTTTTGGTTGGTTTTT",
    "TTTLLLLTTLLLLTTT", "TTLLLLLLTLLLLLLT",
]
koopa_2 = [
    "TTTTTGGGGGTTTTTT", "TTTGGYYYYYGGTTTT", "TTGYYYBBBYYYGTTT", "TGYYYWWWWBYYYGTT",
    "TGYYYYWWWWYYYGTT", "TTGYYYYYYYYYGTTT", "TTTGGYYYYYGGTTTT", "TTTTGGBBBGGTTTTT",
    "TTTGGGDDDDGGGTTT", "TTGGGGDDDGGGGGTT", "TGGGGDDDDDDGGGGT", "TGGYGGDDDDGGYGGT",
    "TGGYGGDDDDGGYGGT", "TGGYYGDDDDGYYGGT", "TTGGYYDDDDYYGGTT", "TTTGGYDDDDYGGTTT",
    "TTTTGGGGGGGGTTTT", "TTTTGGGGGGGGTTTT", "TTTTTGGTTGGTTTTT", "TTTTTGGTTGGTTTTT",
    "TTTTLLLLLLLLTTTT", "TTTLLLLLLLLLLTTT",
]
# קליפת קואפה (מקור) / koopa shell (original)
koopa_shell = [
    "TTTTGGGGGGGGTTTT", "TTGGGGGGGGGGGGTT", "TGGGGddddGGGGGGT", "GGGddGGGGddGGGGG",
    "GGdGGGGGGGGddGGG", "GGdGGGGGGGGddGGG", "GGGddGGGGddGGGGG", "TGGGGddddGGGGGGT",
    "TTGGGGGGGGGGGGTT", "TTTTGGGGGGGGTTTT",
]

# ── FastEnemy — חיפושית אדומה מהירה עם אנטנות / fast red beetle ──────
_beetle_body = [
    "TTTTTxxxxxxTTTTT",  # כיפת שריון כהה / dark shell dome
    "TTTxxrrrrrrxxTTT",
    "TTxrrrrrrrrrrxTT",
    "TxrrWWrrrrWWrrxT",  # נקודות לבנות / white spots
    "xrrrrrrrrrrrrrrx",
    "xrrBBrrrrrrBBrrx",  # עיניים / eyes
    "xrrrrrrrrrrrrrrx",
    "TxrrrrxxxxrrrrxT",  # תפר השריון / shell seam
    "TTxxrrrrrrrrxxTT",
    "TTTxxxxxxxxxxTTT",
]
fast_1 = ["TTTBTTTTTTTTBTTT", "TTTTBTTTTTTBTTTT"] + _beetle_body + ["TTBBTTBBBBTTBBTT"]
fast_2 = ["TTBTTTTTTTTTTBTT", "TTTBTTTTTTTTBTTT"] + _beetle_body + ["TBBTTTBBBBTTTBBT"]

# ── JumpingEnemy — קופץ כחול עם רגלי קפיץ / blue spring-legged jumper ─
_jumper_body = [
    "TTTTTjjjjjjTTTTT",  # ראש עגול / round head
    "TTTjjjjjjjjjjTTT",
    "TTjjjjjjjjjjjjTT",
    "TjjWWWjjjjWWWjjT",  # עיניים גדולות / big eyes
    "TjjWBWjjjjWBWjjT",  # אישונים / pupils
    "TjjWWWjjjjWWWjjT",
    "TjjjjjjjjjjjjjjT",
    "TjjjjjkkkkjjjjjT",  # פה / mouth
    "TTjjjjjjjjjjjjTT",
    "TTTkkkkkkkkkkTTT",  # בסיס הגוף / body base
]
jumper_1 = _jumper_body + [
    "TTTaTTTTTTTTaTTT", "TTTTaTTTTTTaTTTT", "TTTaTTTTTTTTaTTT", "TTaaTTTTTTTTaaTT",
]
jumper_2 = _jumper_body + [
    "TTTaaTTTTTTaaTTT", "TTaaTTTTTTTTaaTT", "TTTaaTTTTTTaaTTT", "TTaaaTTTTTTaaaTT",
]

# ── PlatformPatrolEnemy — סייר כתום עם אנטנה / orange patroller w/ antenna ─
_patrol_body = [
    "TTTTTTTaaTTTTTTT",  # כדור האנטנה / antenna ball
    "TTTTTTTmmTTTTTTT",  # גבעול האנטנה / antenna stalk
    "TTTTTooooooTTTTT",
    "TTTToooooooooTTT",
    "TTToooooooooooTT",
    "TToWWooooooWWoTT",  # עיניים / eyes
    "TToWBooooooBWoTT",  # אישונים / pupils
    "TTooooooooooooTT",
    "TTooovvvvvvooTTT",  # פה נחוש / determined mouth
    "TTToooooooooTTTT",
    "TTTTvvvvvvvvTTTT",  # בסיס / base
]
patrol_1 = _patrol_body + ["TTToooTTToooTTTT", "TTvvvvTTTvvvvTTT"]
patrol_2 = _patrol_body + ["TTTToooTToooTTTT", "TTTvvvvTvvvvTTTT"]

# ── FlyingEnemy — קואפה מעופפת עם כנפיים / winged parakoopa ──────────
_flyer_core = [
    "TTTTTTGGGGGTTTTT",  # ראש / head
    "TTTTTGGGGGGGTTTT",
    "TTTTTGWBGGBWGTTT",  # עיניים / eyes
    "TTTTTGGGGGGGTTTT",
    "TTTTGGYYYYYYGTTT",  # שריון / shell
    "TTTGGYwggggwYGTT",
    "TTTGGYgggggwYGTT",
    "TTTTGGYYYYYYGTTT",
    "TTTTTGGGTTGGGTTT",  # רגליים / feet
]
# כנפיים: פריים1 מורמות, פריים2 מונמכות / wings up vs down
flyer_1 = [
    "TWWTTTTTTTTTTWWT",
    "WWWWTTGGGGGTWWWW",
    "TWWWTGGGGGGGTWWW",
] + ["TT" + r[2:14] + "TT" for r in _flyer_core[1:]]
flyer_2 = [
    "TTTTTTGGGGGTTTTT",
    "TTTTTGGGGGGGTTTT",
] + _flyer_core[2:] + [
    "TWWWTTTTTTTTWWWT",
    "WWWWTTTTTTTTWWWW",
]

# אויב מעוך (גנרי לכל הסוגים, מקור) / generic squished enemy (original)
squished = [
    "TTTTTTTTTTTTTTTT", "TTTTDDDDDDDDTTTT", "TTDDLLLLLLLLDDTT", "TDLLWLLLLLLWLLDT",
    "TDLLLLLLLLLLLLDT", "TTDDLLLLLLLLDDTT", "TTTTDDDDDDDDTTTT",
]

# ════════════════════════════════════════════════════════════════════
#  פריטים ובלוקים (16x16, מקור)  /  Items & blocks (original)
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
pipe = [
    "dddddddddddddddd", "dwwGGGGGGGGGGGGd", "dGGGGGGGGGGGGGGd", "dddddddddddddddd",
    "dGwGGGGGGGGGGGGd", "dGwGGGGGGGGGGGGd", "dGwGGGGGGGGGGGGd", "dGwGGGGGGGGGGGGd",
    "dGwGGGGGGGGGGGGd", "dGwGGGGGGGGGGGGd", "dGwGGGGGGGGGGGGd", "dGwGGGGGGGGGGGGd",
    "dGwGGGGGGGGGGGGd", "dGwGGGGGGGGGGGGd", "dGwGGGGGGGGGGGGd", "dddddddddddddddd",
]

# ── לבבות ל-HUD (חדש) / HUD hearts (new) ─────────────────────────────
heart_full = [
    "TTTTTTTTTTTTTTTT", "TTThhhTTTThhhTTT", "TThhhhhTThhhhhTT", "ThhhhhhhhhhhhhhT",
    "hhhhhhhhhhhhhhhh", "hhhhhhhhhhhhhhhh", "hhhhhhhhhhhhhhhh", "ThhhhhhhhhhhhhhT",
    "TThhhhhhhhhhhhTT", "TTThhhhhhhhhhTTT", "TTTThhhhhhhhTTTT", "TTTTThhhhhhTTTTT",
    "TTTTTThhhhTTTTTT", "TTTTTTThhTTTTTTT",
]
heart_empty = [
    "TTTTTTTTTTTTTTTT", "TTTnnnTTTTnnnTTT", "TTnTTTnTTnTTTnTT", "TnTTTTTnnTTTTTnT",
    "nTTTTTTTTTTTTTTn", "nTTTTTTTTTTTTTTn", "nTTTTTTTTTTTTTTn", "TnTTTTTTTTTTTTnT",
    "TTnTTTTTTTTTTnTT", "TTTnTTTTTTTTnTTT", "TTTTnTTTTTTnTTTT", "TTTTTnTTTTnTTTTT",
    "TTTTTTnTTnTTTTTT", "TTTTTTTnnTTTTTTT",
]

# ════════════════════════════════════════════════════════════════════
#  רקע פרוס (64x32, מקור)  /  Tiled background (original)
# ════════════════════════════════════════════════════════════════════
world_bg = [
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
def _check(frame, name):
    """מוודא שכל שורה היא בדיוק 16 תווים / assert every row is exactly 16 chars."""
    for i, row in enumerate(frame):
        if len(row) != 16:
            raise ValueError(f"{name}: row {i} has {len(row)} chars (expected 16): {row!r}")


def save_sprite(frame, name):
    """שומר פריים 16-רחב כקובץ PNG מוגדל פי SCALE / save a 16-wide frame as PNG."""
    _check(frame, name)
    # מורידים שורות שקופות בתחתית כדי שהדמות תשב בתחתית התיבה ולא "תרחף"
    # strip trailing transparent rows so the sprite sits at the bottom of its box
    frame = list(frame)
    while len(frame) > 1 and all(ch == "T" for ch in frame[-1]):
        frame = frame[:-1]
    _render(frame, 16, name)


def save_wide(frame, name):
    """שומר פריים ברוחב משתנה (רקע 64) — מרפד שורות קצרות בשקוף / variable-width frame."""
    frame = list(frame)
    while len(frame) > 1 and all(ch == "T" for ch in frame[-1]):
        frame = frame[:-1]
    _render(frame, max(len(row) for row in frame), name)


def _render(frame, w, name):
    """מצייר את הפריים ל-PNG מוגדל פי SCALE / rasterise a frame to a scaled PNG."""
    h = len(frame)
    img = Image.new("RGBA", (w * SCALE, h * SCALE), (0, 0, 0, 0))
    for y in range(h):
        for x in range(w):
            ch    = frame[y][x] if x < len(frame[y]) else "T"   # שורה קצרה -> שקוף
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

# שחקנים / players
save_sprite(mario_idle,  "mario_idle")
save_sprite(mario_walk1, "mario_walk1")
save_sprite(mario_walk2, "mario_walk2")
save_sprite(mario_jump,  "mario_jump")
save_sprite(luigi_idle,  "luigi_idle")
save_sprite(luigi_walk1, "luigi_walk1")
save_sprite(luigi_walk2, "luigi_walk2")
save_sprite(luigi_jump,  "luigi_jump")

# אויבים (כל אחד עיצוב ייחודי) / enemies (each a unique design)
save_sprite(goomba_1, "goomba_1")
save_sprite(goomba_2, "goomba_2")
save_sprite(koopa_1, "koopa_1")
save_sprite(koopa_2, "koopa_2")
save_sprite(koopa_shell, "koopa_shell")
save_sprite(fast_1, "fast_1")
save_sprite(fast_2, "fast_2")
save_sprite(jumper_1, "jumper_1")
save_sprite(jumper_2, "jumper_2")
save_sprite(patrol_1, "patrol_1")
save_sprite(patrol_2, "patrol_2")
save_sprite(flyer_1, "flyer_1")
save_sprite(flyer_2, "flyer_2")
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
save_wide(world_bg, "world_bg")

# HUD
save_sprite(heart_full, "heart_full")
save_sprite(heart_empty, "heart_empty")

print("\nכל הספרייטים נוצרו בהצלחה! / All sprites generated successfully.")
