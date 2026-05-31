"""
generate_project_book.py  —  יוצר תיק פרויקט DOCX למשחק Super Mario
"""

from docx import Document
from docx.shared import Pt, RGBColor, Cm
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml.ns import qn
from docx.oxml import OxmlElement

OUTPUT_PATH = "תיק_פרויקט_סופר_מריו.docx"
FONT = "David"
BODY_SIZE = 12
PLACEHOLDER_COLOR = RGBColor(0x99, 0x66, 0x00)
DIAGRAM_COLOR = RGBColor(0x33, 0x66, 0x99)

# ─── helpers ─────────────────────────────────────────────────────────────────

def _set_rtl(para, align=WD_ALIGN_PARAGRAPH.RIGHT):
    pPr = para._p.get_or_add_pPr()
    for old in pPr.findall(qn("w:bidi")):
        pPr.remove(old)
    pPr.append(OxmlElement("w:bidi"))
    para.alignment = align


def _rtl_run(para, text, bold=False, italic=False, size=BODY_SIZE,
             font=FONT, color=None):
    run = para.add_run(text)
    run.bold = bold
    run.italic = italic
    run.font.size = Pt(size)
    run.font.name = font
    if color:
        run.font.color.rgb = color
    rPr = run._r.get_or_add_rPr()
    rPr.append(OxmlElement("w:rtl"))
    rf = rPr.find(qn("w:rFonts"))
    if rf is None:
        rf = OxmlElement("w:rFonts")
        rPr.insert(0, rf)
    for attr in ("w:ascii", "w:hAnsi", "w:cs"):
        rf.set(qn(attr), font)
    return run


def H1(doc, text):
    p = doc.add_paragraph()
    _set_rtl(p)
    _rtl_run(p, text, bold=True, size=18)
    p.paragraph_format.space_before = Pt(18)
    p.paragraph_format.space_after = Pt(6)
    return p


def H2(doc, text):
    p = doc.add_paragraph()
    _set_rtl(p)
    _rtl_run(p, text, bold=True, size=14)
    p.paragraph_format.space_before = Pt(12)
    p.paragraph_format.space_after = Pt(4)
    return p


def H3(doc, text):
    p = doc.add_paragraph()
    _set_rtl(p)
    _rtl_run(p, text, bold=True, size=12)
    p.paragraph_format.space_before = Pt(8)
    p.paragraph_format.space_after = Pt(2)
    return p


def para(doc, text, bold=False, italic=False, size=BODY_SIZE, color=None):
    p = doc.add_paragraph()
    _set_rtl(p)
    if text:
        _rtl_run(p, text, bold=bold, italic=italic, size=size, color=color)
    return p


def bullet(doc, text, level=0):
    p = doc.add_paragraph(style="List Bullet")
    _set_rtl(p)
    for r in p.runs:
        r.text = ""
    _rtl_run(p, text)
    return p


def placeholder(doc, text):
    """Orange italic placeholder for student to fill."""
    return para(doc, text, italic=True, color=PLACEHOLDER_COLOR)


def screenshot_box(doc, description):
    """Grey bordered box representing a screenshot the student will insert."""
    p = doc.add_paragraph()
    _set_rtl(p, WD_ALIGN_PARAGRAPH.CENTER)
    pPr = p._p.get_or_add_pPr()
    sp = OxmlElement("w:spacing")
    sp.set(qn("w:before"), "160")
    sp.set(qn("w:after"), "160")
    pPr.append(sp)
    bd = OxmlElement("w:pBdr")
    for side in ("top", "left", "bottom", "right"):
        b = OxmlElement(f"w:{side}")
        b.set(qn("w:val"), "single")
        b.set(qn("w:sz"), "6")
        b.set(qn("w:space"), "4")
        b.set(qn("w:color"), "AAAAAA")
        bd.append(b)
    pPr.append(bd)
    shd = OxmlElement("w:shd")
    shd.set(qn("w:val"), "clear")
    shd.set(qn("w:color"), "auto")
    shd.set(qn("w:fill"), "F2F2F2")
    pPr.append(shd)
    ind = OxmlElement("w:ind")
    ind.set(qn("w:left"), "567")
    ind.set(qn("w:right"), "567")
    pPr.append(ind)
    _rtl_run(p, f"[ צילום מסך: {description} ]",
             italic=True, color=RGBColor(0x66, 0x66, 0x66), size=11)
    doc.add_paragraph()
    return p


def diagram_box(doc, description):
    """Blue bordered placeholder for a diagram."""
    p = doc.add_paragraph()
    _set_rtl(p, WD_ALIGN_PARAGRAPH.CENTER)
    pPr = p._p.get_or_add_pPr()
    sp = OxmlElement("w:spacing")
    sp.set(qn("w:before"), "160")
    sp.set(qn("w:after"), "160")
    pPr.append(sp)
    bd = OxmlElement("w:pBdr")
    for side in ("top", "left", "bottom", "right"):
        b = OxmlElement(f"w:{side}")
        b.set(qn("w:val"), "dashed")
        b.set(qn("w:sz"), "6")
        b.set(qn("w:space"), "4")
        b.set(qn("w:color"), "336699")
        bd.append(b)
    pPr.append(bd)
    shd = OxmlElement("w:shd")
    shd.set(qn("w:val"), "clear")
    shd.set(qn("w:color"), "auto")
    shd.set(qn("w:fill"), "EEF3FB")
    pPr.append(shd)
    ind = OxmlElement("w:ind")
    ind.set(qn("w:left"), "567")
    ind.set(qn("w:right"), "567")
    pPr.append(ind)
    _rtl_run(p, f"[ תרשים: {description} ]",
             italic=True, color=DIAGRAM_COLOR, size=11)
    doc.add_paragraph()
    return p


def page_break(doc):
    p = doc.add_paragraph()
    run = p.add_run()
    br = OxmlElement("w:br")
    br.set(qn("w:type"), "page")
    run._r.append(br)


def divider(doc):
    p = doc.add_paragraph()
    _set_rtl(p, WD_ALIGN_PARAGRAPH.CENTER)
    pPr = p._p.get_or_add_pPr()
    bd = OxmlElement("w:pBdr")
    b = OxmlElement("w:bottom")
    b.set(qn("w:val"), "single")
    b.set(qn("w:sz"), "4")
    b.set(qn("w:space"), "1")
    b.set(qn("w:color"), "CCCCCC")
    bd.append(b)
    pPr.append(bd)
    return p


# ─── header / footer ─────────────────────────────────────────────────────────

def setup_header_footer(doc):
    section = doc.sections[0]

    # Header
    hdr = section.header
    hdr.is_linked_to_previous = False
    hp = hdr.paragraphs[0] if hdr.paragraphs else hdr.add_paragraph()
    _set_rtl(hp)
    _rtl_run(hp, "[שם התלמיד]  |  פיתוח משחק פלטפורמה בסגנון Super Mario",
             size=9, color=RGBColor(0x55, 0x55, 0x55))

    # Footer with page number
    ftr = section.footer
    ftr.is_linked_to_previous = False
    fp = ftr.paragraphs[0] if ftr.paragraphs else ftr.add_paragraph()
    fp.alignment = WD_ALIGN_PARAGRAPH.CENTER
    run = fp.add_run()
    run.font.size = Pt(9)
    fld_begin = OxmlElement("w:fldChar")
    fld_begin.set(qn("w:fldCharType"), "begin")
    run._r.append(fld_begin)
    instr = OxmlElement("w:instrText")
    instr.text = " PAGE "
    run._r.append(instr)
    fld_end = OxmlElement("w:fldChar")
    fld_end.set(qn("w:fldCharType"), "end")
    run._r.append(fld_end)


# ─── page margins ────────────────────────────────────────────────────────────

def setup_page(doc):
    s = doc.sections[0]
    s.page_height = Cm(29.7)
    s.page_width = Cm(21)
    s.left_margin = Cm(2.5)
    s.right_margin = Cm(2.5)
    s.top_margin = Cm(2.5)
    s.bottom_margin = Cm(2.0)


# ═══════════════════════════════════════════════════════════════════════════════
#  SECTIONS
# ═══════════════════════════════════════════════════════════════════════════════

def cover_page(doc):
    """שער"""
    for _ in range(4):
        doc.add_paragraph()

    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    _rtl_run(p, "[ לוגו בית הספר ]", italic=True,
             color=PLACEHOLDER_COLOR, size=14)

    doc.add_paragraph()

    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    _rtl_run(p, "[ שם בית הספר ]", italic=True,
             color=PLACEHOLDER_COLOR, size=16)

    for _ in range(3):
        doc.add_paragraph()

    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    _rtl_run(p, "תיק פרויקט בהנדסת תוכנה", bold=True, size=20)

    doc.add_paragraph()

    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    _rtl_run(p, "פיתוח משחק פלטפורמה בסגנון Super Mario", bold=True, size=16)

    for _ in range(4):
        doc.add_paragraph()

    for label, value in [
        ("שם התלמיד", "[שם התלמיד]"),
        ("ת.ז.", "[ת.ז. התלמיד]"),
        ("שם המנחה", "[שם המנחה]"),
        ("שם החלופה", "שירותי אינטרנט, תכנות אסינכרוני ומסדי נתונים — הנדסת תוכנה 883589"),
        ("תאריך ההגשה", "[תאריך ההגשה]"),
    ]:
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        _rtl_run(p, f"{label}: ", bold=True, size=12)
        color = PLACEHOLDER_COLOR if value.startswith("[") else None
        _rtl_run(p, value, italic=value.startswith("["), size=12, color=color)

    page_break(doc)


def toc_section(doc):
    """תוכן עניינים"""
    H1(doc, "תוכן עניינים")
    placeholder(doc, "יש ליצור תוכן עניינים אוטומטי בוורד לאחר השלמת המסמך.")
    para(doc, "הוראות ליצירת תוכן עניינים אוטומטי בוורד:")
    bullet(doc, "סמן את כל הכותרות בסגנון 'כותרת 1', 'כותרת 2' וכו'")
    bullet(doc, "לחץ על הכרטיסייה References ← Table of Contents ← Automatic Table")
    para(doc, "")
    placeholder(doc, "[ כאן יופיע תוכן העניינים האוטומטי ]")
    page_break(doc)


def intro_section(doc):
    """מבוא"""
    H1(doc, "מבוא")

    H2(doc, "הרקע לפרויקט")
    para(doc, "שם הפרויקט: SuperMario — משחק פלטפורמה")
    para(doc,
         "תיאור קצר: פרויקט זה הוא משחק מחשב דו-ממדי בסגנון Nintendo Super Mario הקלאסי, "
         "הבנוי מאפס בשפת C# עם מסגרת WinForms. המשחק כולל 5 רמות (3 ידניות ו-2 נוצרות "
         "אלגוריתמית), 6 סוגי אויבים עם התנהגויות שונות, מנוע פיזיקה, מערכת ספרייטים, "
         "ומצב אימון AI מבוסס neuroevolution.")
    para(doc, "קהל היעד: חובבי משחקי וידאו ומשחקי פלטפורמה קלאסיים, גילאים 10 ומעלה.")
    placeholder(doc, "הסיבות לבחירת הנושא: [יש להשלים בנוסח אישי]")

    H2(doc, "מטרות המערכת")
    H3(doc, "מטרות על")
    bullet(doc, "פיתוח משחק פלטפורמה מלא, פונקציונלי וניתן לשחק ב-Windows")
    bullet(doc, "מימוש מודול אימון AI המסוגל ללמוד לשחק במשחק באופן אוטונומי")

    H3(doc, "מטרות נלוות")
    bullet(doc, "יישום עקרונות תכנות מונחה עצמים: ירושה, פולימורפיזם, הפשטה")
    bullet(doc, "פיתוח מנוע פיזיקה (כבידה, קפיצה, זיהוי התנגשויות)")
    bullet(doc, "ניהול מצב משחק, אנימציות, וממשק משתמש גרפי")
    bullet(doc, "מימוש אלגוריתם neuroevolution (רשת נוירונים + אלגוריתם גנטי)")

    H2(doc, "תיאור המערכת")
    para(doc,
         "המערכת בנויה כיישום שולחני ל-Windows הכולל שתי מצבי פעולה: מצב משחק רגיל "
         "ומצב אימון AI. המשחק מחולק ל-5 שכבות לוגיות: Core (ליבת המשחק), Enemies (אויבים), "
         "World (עצמי עולם), UI (ממשק משתמש), ו-ML (בינה מלאכותית).")

    H2(doc, "גבולות המערכת")
    bullet(doc, "פלטפורמה: Windows Desktop בלבד")
    bullet(doc, "אין תמיכה ב-Linux / macOS (WinForms תלוי ב-.NET Framework)")
    bullet(doc, "אין תמיכה מרובת שחקנים")
    bullet(doc, "אין שמירת התקדמות משחק")

    H2(doc, "סביבת פיתוח")
    bullet(doc, "סביבת פיתוח: Microsoft Visual Studio 2022")
    bullet(doc, "מסגרת: .NET Framework 4.7.2")
    bullet(doc, "מערכת גרסאות: Git / GitHub")

    H2(doc, "שפת תכנות")
    para(doc, "C# — שפה מונחית עצמים הנתמכת מלאית על ידי .NET Framework ו-WinForms.")

    H2(doc, "שכבות")
    for layer, desc in [
        ("Core", "מחלקות ליבה: שחקן, אנימציה, נתוני משחק, מנהל משחק"),
        ("Enemies", "היררכיית אויבים עם ירושה"),
        ("World", "פלטפורמות, צינורות וקצה הרמה"),
        ("UI", "חלונות, לולאת משחק, קלט, HUD"),
        ("ML", "רשת נוירונים, אלגוריתם גנטי, סוכן AI"),
    ]:
        bullet(doc, f"{layer}: {desc}")

    H2(doc, "פלטפורמות הלקוח")
    para(doc,
         "יישום Windows Desktop (WinForms) — נבחר בשל תמיכה מלאה ב-.NET Framework 4.7.2, "
         "כלי GUI מובנים, ויכולת ציור גרפי מתקדמת.")

    H2(doc, "אתגרים מרכזיים")
    bullet(doc,
           "פיתוח מנוע פיזיקה: מימוש כבידה, קפיצה, ואלגוריתם זיהוי התנגשויות "
           "הנכון גם עבור תנועה מהירה")
    bullet(doc,
           "ניהול 6 סוגי אויבים שונים עם התנהגויות מגוונות — "
           "פתרון: היררכיית ירושה עם Enemy כמחלקת בסיס מופשטת")
    bullet(doc,
           "מערכת ספרייטים: יצירה, טעינה ואנימציה של תמונות לכל אובייקט "
           "מבלי לגרום לבעיות ביצועים")
    bullet(doc,
           "מימוש Neuroevolution: שילוב רשת נוירונים עם אלגוריתם גנטי לאימון "
           "סוכן AI אוטונומי — תוך שמירה על ביצועי זמן-אמת")
    placeholder(doc, "אתגרים אישים נוספים: [יש להשלים]")

    H2(doc, "חידושים ועדכונים")
    bullet(doc, "כל ספרייט עוצב כפיקסל-ארט מקורי בסקריפט Python")
    bullet(doc, "ספרייטים נטענים דינמית ומוצגים דרך PictureBox — ללא ציור GDI+ ידני")
    bullet(doc, "רמות 4-5 נבנות אלגוריתמית מ-templates מוגדרים מראש")

    page_break(doc)


def system_analysis_section(doc):
    """ניתוח מערכת"""
    H1(doc, "ניתוח מערכת")

    H2(doc, "מסמך ייזום")
    placeholder(doc, "[יש להשלים את מסמך הייזום: שם הפרויקט, מזמין העבודה, תיאור הצורך]")
    diagram_box(doc, "מסמך ייזום — להשלים")

    H2(doc, "מצב קיים")
    para(doc,
         "לפני פיתוח הפרויקט לא קיים כלי דומה בשימוש. המשחק נבנה מאפס "
         "ואינו מחליף מערכת קיימת.")

    H2(doc, "מטרות המערכת העתידית")
    placeholder(doc, "[יש להשלים: מה המערכת אמורה לספק לאחר הפיתוח]")

    H2(doc, "מצב עתידי")
    placeholder(doc, "[יש להשלים: תיאור המצב הרצוי לאחר יישום הפרויקט]")

    H2(doc, "עץ תהליכים")
    para(doc, "עץ התהליכים הראשי של המערכת:")
    for line in [
        "Main Menu",
        "    ├── משחק רגיל ← בחירת רמה (1–5) ← משחק ← {ניצחון / מוות / רמה הבאה}",
        "    ├── אימון AI ← הגדרת פרמטרים ← הרצת סוכנים ← אבולוציה דורית",
        "    └── יציאה",
    ]:
        p = doc.add_paragraph()
        _set_rtl(p)
        _rtl_run(p, line, font="Courier New", size=10)
    diagram_box(doc, "עץ תהליכים — ניתן לייצא כתרשים ויזואלי ולהדביק כאן")

    H2(doc, "Use Case — תהליכי המערכת")
    para(doc, "שחקן רגיל:")
    bullet(doc, "מפעיל משחק → בוחר רמה → שולט בדמות → אוסף מטבעות ופטריות")
    bullet(doc, "קופץ על אויבים / נפגע מאויבים → מאבד חיים")
    bullet(doc, "מגיע לקצה הרמה → עובר לרמה הבאה")
    para(doc, "מאמן AI:")
    bullet(doc, "פותח מסך אימון → מגדיר פרמטרים (גודל אוכלוסייה, קצב מוטציה)")
    bullet(doc, "מריץ / מפסיק אימון → צופה בהתקדמות הסוכנים")
    bullet(doc, "שומר את הרשת הטובה ביותר לקובץ")
    diagram_box(doc, "תרשים Use Case — יש לשרטט ולהדביק כאן")

    H2(doc, "DFD-0 — תרשים זרימת נתונים")
    diagram_box(doc, "תרשים DFD-0 — יש לשרטט ולהדביק כאן")

    para(doc,
         "הערה: ERD אינו רלוונטי לפרויקט זה — המשחק אינו משתמש במסד נתונים רלציוני. "
         "כל הנתונים מנוהלים בזיכרון. ראה פרק בסיס נתונים.")

    page_break(doc)


def database_section(doc):
    """בסיס נתונים"""
    H1(doc, "בסיס נתונים")

    para(doc,
         "פרויקט זה אינו עושה שימוש במסד נתונים רלציוני חיצוני. "
         "כל נתוני המשחק מנוהלים בזיכרון (in-memory) במהלך ריצת התוכנה.")

    H2(doc, "מבני נתונים עיקריים בזיכרון")
    para(doc, "המחלקות הבאות מגדירות את מבני הנתונים של המשחק:")
    for name, desc in [
        ("Player", "מצב השחקן: מיקום, מהירות, בריאות, ניקוד"),
        ("Enemy (abstract)", "מצב אויב בסיסי: מיקום, כיוון, מהירות, חי/מת"),
        ("Coin", "מטבע: מיקום, האם נאסף"),
        ("Mushroom", "פטרייה: מיקום, האם פעילה, כיוון תנועה"),
        ("QuestionBlock", "בלוק שאלה: מיקום, האם נפתח"),
        ("PlatformData", "נתוני פלטפורמה: X, Y, רוחב, גובה"),
    ]:
        bullet(doc, f"{name} — {desc}")

    H2(doc, "שמירת נתונים לקובץ")
    para(doc,
         "הנתון היחיד הנשמר לדיסק הוא מצב הרשת הנוירונית של הסוכן הטוב ביותר, "
         "באמצעות המחלקה NetworkSerializer.")

    H3(doc, "פורמט קובץ הרשת הנוירונית")
    para(doc, "הקובץ הוא קובץ טקסט רגיל עם המבנה הבא:")
    for line in [
        "SMNET1           ← סוג הקובץ (header)",
        "generation=42    ← מספר הדור",
        "fitness=1850     ← ביצועי הסוכן הטוב",
        "shape=5,8,8,3    ← צורת הרשת (שכבות ונוירונים)",
        "<layerIdx>,<neuronIdx>,<bias>,<w0>,<w1>,...  ← נתוני כל נוירון",
    ]:
        p = doc.add_paragraph()
        _set_rtl(p)
        _rtl_run(p, line, font="Courier New", size=10)

    para(doc, "")
    para(doc,
         "מבנה זה מאפשר שמירה וטעינה מהירה של הרשת הנוירונית בין הפעלות התוכנה, "
         "ללא תלות בספריות חיצוניות.")

    page_break(doc)


def implementation_section(doc):
    """מימוש הפרויקט"""
    H1(doc, "מימוש הפרויקט")

    # ── Core ──────────────────────────────────────────────────────────────
    H2(doc, "שכבת Core — ליבת המשחק")
    diagram_box(doc, "תרשים UML של מחלקות Core")

    H3(doc, "Player")
    para(doc,
         "מחלקת Player מנהלת את כל מצב השחקן ופיזיקתו. "
         "הפיזיקה פשוטה ובסיסית: תנועה אופקית במהירות קבועה, "
         "כבידה קבועה לכל פריים, וקפיצה המציבה מהירות אנכית שלילית.")
    bullet(doc, "מהירות הליכה: 5 פיקסלים/פריים")
    bullet(doc, "כבידה: 0.6 פיקסלים/פריים²")
    bullet(doc, "עוצמת קפיצה: -13 (מעלה)")
    bullet(doc, "מהירות נפילה מרבית: 15 פיקסלים/פריים")
    bullet(doc, "מצב: בריאות (3 לבבות), ניקוד, IsGrounded")

    H3(doc, "Animator")
    para(doc,
         "מחלקת Animator מנהלת מחזור פריימים לאנימציה. "
         "מקבלת מערך של תמונות ומחזירה את התמונה הנוכחית לפי קצב מוגדר.")

    H3(doc, "GameData")
    para(doc,
         "מגדירה מבני נתונים לפריטי המשחק: Coin, Mushroom, QuestionBlock. "
         "כל אחד מכיל מיקום, גודל ומצב.")

    H3(doc, "GameManager")
    para(doc,
         "דגל גלובלי IsRunning המאפשר לרכיבים שונים לדעת האם המשחק פעיל.")

    H3(doc, "Sprites")
    para(doc,
         "טוען ומאחסן את כל תמונות המשחק (PNG) פעם אחת בהפעלה. "
         "כל האובייקטים מקבלים את תמונתם מאוסף זה — "
         "אין ציור GDI+ ידני בשכבת המשחק.")

    # ── Enemies ──────────────────────────────────────────────────────────
    H2(doc, "שכבת Enemies — מערכת האויבים")
    para(doc,
         "מערכת האויבים בנויה על היררכיית ירושה. "
         "כל ההתנהגות המשותפת מרוכזת במחלקות הבסיס, "
         "ותת-מחלקה מגדירה רק מהירות, ספרייטים והתנהגות מיוחדת.")

    diagram_box(doc, "תרשים ירושה — Enemy hierarchy")

    para(doc, "מבנה הירושה:")
    for line in [
        "Enemy (abstract)  ←  מחלקת בסיס לכל האויבים",
        "  ├── SquishableEnemy (abstract)  ←  אויבים הניתנים למעיכה",
        "  │     ├── Goomba       — פטרייה, הולכת לאט",
        "  │     ├── FastEnemy    — חיפושית אדומה, מהירה פי 2",
        "  │     ├── JumpingEnemy — קופצת מדי 2 שניות, כחולה",
        "  │     └── PlatformPatrolEnemy — עוצרת בקצה פלטפורמה, כתומה",
        "  ├── Koopa       — צב בקליפה, מפריד למצב קליפה",
        "  └── FlyingEnemy — קואפה מעופפת, 2 מכות להריגה",
    ]:
        p = doc.add_paragraph()
        _set_rtl(p)
        _rtl_run(p, line, font="Courier New", size=10)

    para(doc, "")

    for name, desc in [
        ("Goomba", "אויב בסיסי — הולך שמאלה, מת ממעיכה"),
        ("FastEnemy", "פי 2 מהיר מגומבה — עיצוב חיפושית אדומה"),
        ("JumpingEnemy", "קופץ אוטומטית כל 2 שניות — עיצוב כחול עם רגלי קפיץ"),
        ("PlatformPatrolEnemy",
         "זיהוי קצה פלטפורמה — מתהפך לפני נפילה. עיצוב כתום עם אנטנה"),
        ("Koopa", "מת ממכה ראשונה ועובר למצב קליפה גולשת"),
        ("FlyingEnemy", "טיסה בדפוס סינוס, דורש 2 מכות להריגה"),
    ]:
        bullet(doc, f"{name}: {desc}")

    # ── World ─────────────────────────────────────────────────────────────
    H2(doc, "שכבת World")
    para(doc,
         "המחלקה GameObjectS עוטפת כל פלטפורמה או צינור בעולם המשחק. "
         'היא מכילה PictureBox עם BackgroundImage משובץ (tile), ותחום (Type) שיכול להיות '
         '"ground", "pipe", או "finish".')

    # ── UI ────────────────────────────────────────────────────────────────
    H2(doc, "שכבת UI — ממשק המשתמש ולולאת המשחק")
    diagram_box(doc, "תרשים זרימה בין מסכי המשחק")

    H3(doc, "MainMenuForm — מסך פתיחה")
    para(doc,
         "מסך הפתיחה מציג 4 כפתורים: משחק רגיל, אימון AI, עזרה, ויציאה. "
         "מפעיל את mainWin או TrainingForm בהתאם לבחירה.")

    H3(doc, "mainWin — חלון המשחק הראשי (partial class, 8 קבצים)")
    para(doc, "לולאת המשחק פועלת על Timer בתדר 16ms (≈60fps):")
    for line in [
        "Timer.Tick → ReadInput() → Player.Move() → UpdateEnemies()",
        "         → CheckCollisions() → UpdateAnimatedSprites()",
        "         → ScrollCamera() → UpdateHUD()",
    ]:
        p = doc.add_paragraph()
        _set_rtl(p)
        _rtl_run(p, line, font="Courier New", size=10)

    para(doc, "")
    bullet(doc, "mainWin.cs — שדות, לולאה, קלט")
    bullet(doc, "mainWin.Physics.cs — זיהוי התנגשויות, מצלמה, מוות")
    bullet(doc, "mainWin.EnemyUpdates.cs — עדכון כל סוגי האויבים")
    bullet(doc, "mainWin.EnemyPhysics.cs — פיזיקת אויבים")
    bullet(doc, "mainWin.Collectibles.cs — איסוף מטבעות ופטריות")
    bullet(doc, "mainWin.HUD.cs — HUD: לבבות, ניקוד, ספרייטים")
    bullet(doc, "mainWin.LevelBuilder.cs — בניית / ניקוי רמה")
    bullet(doc, "mainWin.LevelData.cs — נתוני הרמות הסטטיים")

    H3(doc, "TrainingForm — מסך אימון AI")
    para(doc,
         "מסך מפוצל: קנבס שמאל לסימולציה חיה של עד 60 סוכנים, "
         "ולוח בקרה ימני עם: מספר דור, כמה חיים, ביצועי ניצחון, "
         "הגדרות פרמטרים, ושליטה (Start/Pause/Reset).")

    # ── ML ────────────────────────────────────────────────────────────────
    H2(doc, "שכבת ML — בינה מלאכותית (Neuroevolution)")
    para(doc,
         "שכבת ה-ML מורכבת ממנוע רשת נוירונים ואלגוריתם גנטי. "
         "חשוב לציין: המחלקות Neuron, Layer, NeuralNetwork, "
         "NeuralNetworkControl, Population ו-NetParams "
         "הותאמו מפרויקט Flappy Bird קיים (ראה ביבליוגרפיה).")
    diagram_box(doc, "ארכיטקטורת מודול ה-ML")

    H3(doc, "MarioAgent — מחלקה מקורית")
    para(doc,
         "MarioAgent היא המחלקה המקורית שנכתבה לפרויקט זה. "
         "כל סוכן הוא לואיג'י עצמאי עם פיזיקה ורשת נוירונים משלו.")
    bullet(doc, "קלטים (5): מרחק לאויב הקרוב, גובה השחקן, מהירות, האם על הקרקע, מרחק לבור")
    bullet(doc, "פלטים (3): קפיצה, תנועה שמאלה, תנועה ימינה")
    bullet(doc, "פיטנס: מרחק X שהגיע + 50 נקודות לכל מטבע שנאסף")

    H3(doc, "Population — אלגוריתם גנטי (מותאם)")
    para(doc,
         "מנהל את כלל הסוכנים: בוחר את הטובים ביותר, "
         "מבצע crossover בין רשתות, מוסיף מוטציות אקראיות.")

    page_break(doc)


def user_guide_section(doc):
    """מדריך למשתמש"""
    H1(doc, "מדריך למשתמש")

    para(doc,
         "המערכת תומכת בשני סוגי משתמשים: שחקן רגיל ומאמן AI. "
         "לכל סוג ממשק מותאם עם אפשרויות שונות.")
    para(doc, "הפרויקט נבדק על: Windows 10 / Windows 11 (64-bit), .NET Framework 4.7.2 ומעלה.")

    H2(doc, "שחקן רגיל")

    H3(doc, "מסך ראשי")
    screenshot_box(doc, "מסך Main Menu — 4 כפתורי ניווט")
    para(doc, "מהמסך הראשי ניתן:")
    bullet(doc, "לחיצה על 'שחק' — כניסה למשחק")
    bullet(doc, "לחיצה על 'אימון AI' — מעבר למסך אימון הסוכן")
    bullet(doc, "לחיצה על 'עזרה' — הצגת הוראות")
    bullet(doc, "לחיצה על 'יציאה' — סגירת התוכנה")

    H3(doc, "מסך המשחק")
    screenshot_box(doc, "מסך משחק — שחקן, אויבים, פלטפורמות ו-HUD עם לבבות וניקוד")
    para(doc, "שליטה:")
    bullet(doc, "חץ שמאל / ימין — תנועה")
    bullet(doc, "רווח או חץ למעלה — קפיצה (רק כשעל הקרקע)")
    bullet(doc, "ESC — יציאה מהמשחק")
    para(doc, "HUD (ראש הדף):")
    bullet(doc, "3 לבבות — מייצגים את החיים שנותרו")
    bullet(doc, "ניקוד — מתעדכן בכל מטבע שנאסף")
    bullet(doc, "מספר רמה נוכחית")
    para(doc,
         "הערה: אם השחקן נגמרים לו החיים — מוצגת הודעה ומתחיל משחק חדש. "
         "אם מגיע לדגל הסיום — עובר לרמה הבאה.")

    H3(doc, "מסך סיום")
    screenshot_box(doc, "מסך סיום רמה או מוות — הודעה וחזרה לתפריט")

    H2(doc, "מאמן AI")

    H3(doc, "מסך אימון AI")
    screenshot_box(doc, "מסך אימון — קנבס סימולציה שמאל, לוח בקרה ורשת נוירונים ימין")
    para(doc, "פרמטרים ניתנים להגדרה:")
    bullet(doc, "גודל אוכלוסייה — כמה סוכנים בכל דור")
    bullet(doc, "אחוז מוטציה — כמה שינוי אקראי בין דורות")
    bullet(doc, "אחוז שרידים — כמה מהטובים עוברים לדור הבא")
    bullet(doc, "צורת הרשת — מספר הנוירונים בכל שכבה")
    para(doc, "כפתורי שליטה:")
    bullet(doc, "Start / Pause — הפעלה / השהייה")
    bullet(doc, "Reset — איפוס האימון")
    bullet(doc, "Apply Settings — החלת הגדרות חדשות")
    bullet(doc, "שמירת הרשת הטובה לקובץ .smnet")

    page_break(doc)


def developer_guide_section(doc):
    """מדריך למפתח"""
    H1(doc, "מדריך למפתח")

    H2(doc, "הגדרת סביבת הפיתוח")
    bullet(doc, "דרישות: Windows 10/11, Visual Studio 2022, .NET Framework 4.7.2, Python 3.x")
    bullet(doc, "פתיחת הפרויקט: supermario.sln → F5 להרצה")
    bullet(doc, "יצירת ספרייטים: python generate_spritesheets.py → PNG נוצרים ב-assets/textures/sprites/")

    H2(doc, "מסך ראשי — MainMenuForm")
    screenshot_box(doc, "מסך Main Menu — מנקודת מבט מפתח")
    para(doc,
         "קובץ: UI/MainMenuForm.cs — מחלקה: MainMenuForm.")
    bullet(doc, "4 כפתורים: btnPlay, btnTrain, btnHelp, btnExit")
    bullet(doc, "לחיצה על btnPlay: new mainWin().Show(), סגירת MainMenuForm")
    bullet(doc, "לחיצה על btnTrain: new TrainingForm().Show(), סגירת MainMenuForm")
    bullet(doc, "הוספת כפתור חדש: גרור Button ב-Designer, הוסף EventHandler")

    H2(doc, "מסך המשחק — mainWin")
    screenshot_box(doc, "מסך משחק — מנקודת מבט מפתח: PictureBox של שחקן ואויבים")
    para(doc,
         "mainWin היא partial class המחולקת ל-8 קבצים. "
         "הרכיב המרכזי הוא gameTimer (Timer, 16ms) המפעיל את כל הלוגיקה.")
    para(doc, "כיצד להוסיף רמה חדשה:")
    bullet(doc, "פתח mainWin.LevelData.cs")
    bullet(doc, "הוסף מערך PlatformData[] חדש עם קואורדינטות")
    bullet(doc, "הוסף רשומה ב-switch של BuildLevel() ב-mainWin.LevelBuilder.cs")

    H2(doc, "מערכת האויבים")
    screenshot_box(doc, "6 סוגי האויבים בפעולה — גומבה, קואפה, fast, jumper, patrol, flyer")
    para(doc,
         "כיצד להוסיף סוג אויב חדש:")
    bullet(doc,
           "צור מחלקה חדשה היורשת מ-SquishableEnemy (אם ניתן למעיכה) "
           "או מ-Enemy (אחרת)")
    bullet(doc, "הגדר מהירות, ספרייטים, וגודל בבנאי")
    bullet(doc, "דרוס את Update() להתנהגות מיוחדת (קפיצה, טיסה)")
    bullet(doc, "הוסף List<NewEnemy> ב-mainWin.cs")
    bullet(doc, "הוסף ספאווון ב-mainWin.LevelBuilder.cs ועדכון ב-mainWin.EnemyUpdates.cs")
    para(doc, "כיצד לשנות ספרייט:")
    bullet(doc, "ערוך את generate_spritesheets.py")
    bullet(doc, "הרץ: python generate_spritesheets.py")
    bullet(doc, "הפרויקט יטען את ה-PNG החדש בהרצה הבאה")

    H2(doc, "מסך אימון AI — TrainingForm + ML")
    screenshot_box(doc, "מסך אימון — תצוגת הרשת הנוירונית ולוח הבקרה")
    para(doc,
         "TrainingForm מנהלת את הציור (RenderScene) ואת ה-Population. "
         "MarioAgent — המחלקה המקורית — מגדיר את קלטי הרשת והפיזיקה של הסוכן.")
    para(doc, "כיצד לשנות את מספר הקלטים של הרשת:")
    bullet(doc, "ערוך את MarioAgent.GetInputs() ב-ML/MarioAgent.cs")
    bullet(doc, "עדכן את NetParams.DefaultShape[0] ב-ML/NetParams.cs")
    bullet(doc, "איפוס האימון נדרש לאחר שינוי")

    H2(doc, "מחלקות שנבנו על ידי המפתח")
    para(doc, "המחלקות הבאות נכתבו במקור לפרויקט זה:")
    for cls in [
        "Core/Player.cs", "Core/Animator.cs", "Core/GameData.cs",
        "Core/GameManager.cs", "Core/Sprites.cs",
        "Enemies/Enemy.cs", "Enemies/SquishableEnemy.cs",
        "Enemies/Goomba.cs", "Enemies/Koopa.cs", "Enemies/FastEnemy.cs",
        "Enemies/JumpingEnemy.cs", "Enemies/PlatformPatrolEnemy.cs", "Enemies/FlyingEnemy.cs",
        "World/GameObjectS.cs",
        "UI/mainWin.cs (+ 7 partial files)", "UI/MainMenuForm.cs", "UI/TrainingForm.cs",
        "ML/MarioAgent.cs",
    ]:
        bullet(doc, cls)

    page_break(doc)


def reflection_section(doc):
    """סיכום אישי / רפלקציה"""
    H1(doc, "סיכום אישי / רפלקציה")
    para(doc,
         "חלק זה מיועד לתיאור אישי של תהליך הפיתוח. "
         "יש להשלים לפחות חצי עמוד עד עמוד שלם.")

    H2(doc, "תיאור תהליך העבודה")
    placeholder(doc,
                "[תאר את תהליך העבודה על הפרויקט: מה עשית ראשון, "
                "כיצד התקדמת, מה שינית בדרך]")

    H2(doc, "אתגרים ודרכי פתרון")
    placeholder(doc,
                "[תאר את האתגרים המרכזיים שנתקלת בהם ואיך פתרת אותם. "
                "לדוגמה: בעיות בפיזיקה, באויבים, בביצועים]")

    H2(doc, "תהליך למידה")
    placeholder(doc,
                "[מה למדת באופן עצמאי שלא נלמד בכיתה? "
                "אילו מושגים, טכנולוגיות או כלים גילית בכוחות עצמך?]")

    H2(doc, "כלים שאקח להמשך")
    placeholder(doc,
                "[אילו כלים, עקרונות או ידע שרכשת תשתמש בהם בפרויקטים הבאים?]")

    H2(doc, "בראייה לאחור")
    placeholder(doc,
                "[אם היית מתחיל מחדש, מה היית עושה אחרת? "
                "אילו החלטות עיצוביות היית משנה?]")

    H2(doc, "שיפורים אפשריים")
    placeholder(doc,
                "[אם היו לך יותר זמן ומשאבים, כיצד היית משפר את הפרויקט? "
                "לדוגמה: רמות נוספות, סוגי אויבים, מצב multiplayer]")

    page_break(doc)


def bibliography_section(doc):
    """ביבליוגרפיה"""
    H1(doc, "ביבליוגרפיה")
    para(doc, "מקורות המידע שנעשה בהם שימוש בפרויקט, לפי כללי APA:")

    bullet(doc,
           "[מחבר פרויקט Flappy Bird]. ([שנה]). [שם פרויקט Flappy Bird עם neuroevolution]. "
           "GitHub. [יש להשלים את הקישור המדויק]")
    bullet(doc,
           "Microsoft. (2024). Windows Forms documentation. Microsoft Docs. "
           "https://docs.microsoft.com/en-us/dotnet/desktop/winforms/")
    bullet(doc,
           "Microsoft. (2024). C# language reference. Microsoft Docs. "
           "https://docs.microsoft.com/en-us/dotnet/csharp/")
    bullet(doc,
           "Nintendo. (1985). Super Mario Bros. [Video game]. Nintendo.")
    placeholder(doc, "[יש להוסיף מקורות נוספים שהשתמשת בהם]")

    page_break(doc)


def appendix_section(doc):
    """נספחים"""
    H1(doc, "נספחים")

    H2(doc, "נספח א׳ — קוד מחלקות הפרויקט")
    para(doc,
         "יש לצרף כאן את קוד כל מחלקות הפרויקט. "
         "הקוד יוצג כתדפיס מסודר (לא צילום מסך) עם הערות בכל המקומות הרלוונטיים.")
    para(doc, "מחלקות לצירוף (לפי סדר):")
    for cls in [
        "Core/Player.cs", "Core/Animator.cs", "Core/GameData.cs",
        "Core/GameManager.cs", "Core/Sprites.cs",
        "Enemies/Enemy.cs", "Enemies/SquishableEnemy.cs",
        "Enemies/Goomba.cs", "Enemies/Koopa.cs", "Enemies/FastEnemy.cs",
        "Enemies/JumpingEnemy.cs", "Enemies/PlatformPatrolEnemy.cs", "Enemies/FlyingEnemy.cs",
        "World/GameObjectS.cs",
        "UI/mainWin.cs", "UI/mainWin.LevelBuilder.cs", "UI/mainWin.LevelData.cs",
        "UI/mainWin.Physics.cs", "UI/mainWin.EnemyUpdates.cs",
        "UI/mainWin.EnemyPhysics.cs", "UI/mainWin.Collectibles.cs", "UI/mainWin.HUD.cs",
        "UI/MainMenuForm.cs", "UI/TrainingForm.cs",
        "ML/MarioAgent.cs",
    ]:
        bullet(doc, cls)

    H2(doc, "נספח ב׳ — הסבר על הטכנולוגיות")
    placeholder(doc,
                "[ניתן להוסיף כאן הסברים על טכנולוגיות ששימשו בפרויקט: "
                "WinForms, GDI+, Neuroevolution, Genetic Algorithm וכו']")


# ═══════════════════════════════════════════════════════════════════════════════
#  MAIN
# ═══════════════════════════════════════════════════════════════════════════════

def main():
    doc = Document()
    setup_page(doc)
    setup_header_footer(doc)

    cover_page(doc)
    toc_section(doc)
    intro_section(doc)
    system_analysis_section(doc)
    database_section(doc)
    implementation_section(doc)
    user_guide_section(doc)
    developer_guide_section(doc)
    reflection_section(doc)
    bibliography_section(doc)
    appendix_section(doc)

    doc.save(OUTPUT_PATH)
    print(f"✓ נשמר: {OUTPUT_PATH}")


if __name__ == "__main__":
    main()
