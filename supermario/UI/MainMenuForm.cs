using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace supermario
{
    public sealed class MainMenuForm : Form
    {
        private Timer  _animTimer;
        private float  _cloudOffset;
        private float  _bobPhase;
        private bool   _showHowTo;

        private Button _btnStart;
        private Button _btnHowTo;
        private Button _btnExit;

        private static readonly (float relX, int y, int w)[] CLOUDS = {
            (0.05f, 50, 140), (0.23f, 28, 100), (0.45f, 62, 160),
            (0.65f, 38, 115), (0.84f, 56, 130), (1.06f, 44, 95),
        };

        // ════════════════════════════════════════════════════════════════════
        //  CONSTRUCTION
        // ════════════════════════════════════════════════════════════════════
        public MainMenuForm()
        {
            Text             = "Super Mario";
            FormBorderStyle  = FormBorderStyle.None;
            WindowState      = FormWindowState.Maximized;
            DoubleBuffered   = true;
            BackColor        = Color.FromArgb(92, 148, 252);
            KeyPreview       = true;
            KeyDown         += OnMenuKeyDown;
            FormClosed      += (s, e) => { _animTimer?.Stop(); _animTimer?.Dispose(); };

            _btnStart = MakeButton("▶   START GAME",  Color.FromArgb(48, 176, 48),  Color.FromArgb(22, 120, 22),  16f);
            _btnHowTo = MakeButton("?   HOW TO PLAY", Color.FromArgb(210, 165, 10), Color.FromArgb(155, 105, 0),  13f);
            _btnExit  = MakeButton("✕   EXIT",         Color.FromArgb(185, 38, 28),  Color.FromArgb(130, 18, 10), 13f);

            _btnStart.Click += (s, e) => LaunchGame();
            _btnHowTo.Click += (s, e) => { _showHowTo = !_showHowTo; Invalidate(); };
            _btnExit.Click  += (s, e) => Application.Exit();

            Controls.Add(_btnStart);
            Controls.Add(_btnHowTo);
            Controls.Add(_btnExit);

            Shown      += (s, e) => PositionButtons();
            SizeChanged += (s, e) => PositionButtons();

            _animTimer          = new Timer { Interval = 33 };   // ~30 fps
            _animTimer.Tick    += AnimTick;
            _animTimer.Start();
        }

        // ── Input ─────────────────────────────────────────────────────────────
        private void OnMenuKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Application.Exit();
            if ((e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) && !_showHowTo)
                LaunchGame();
        }

        // ── Animation tick ────────────────────────────────────────────────────
        private void AnimTick(object sender, EventArgs e)
        {
            _cloudOffset = (_cloudOffset + 0.5f) % (ClientSize.Width + 320f);
            _bobPhase   += 0.035f;
            if (_bobPhase > 6.2832f) _bobPhase -= 6.2832f;
            Invalidate();
        }

        // ── Button layout (called on Shown / SizeChanged) ─────────────────────
        private void PositionButtons()
        {
            int cx   = ClientSize.Width  / 2;
            int midY = ClientSize.Height / 2 + 52;
            const int BW_L = 280, BW_S = 256, BH = 54, GAP = 14;

            _btnStart.Location = new Point(cx - BW_L / 2, midY);
            _btnStart.Size     = new Size(BW_L, BH);

            _btnHowTo.Location = new Point(cx - BW_S / 2, midY + BH + GAP);
            _btnHowTo.Size     = new Size(BW_S, BH - 4);

            _btnExit.Location  = new Point(cx - BW_S / 2, midY + (BH + GAP) + (BH - 4) + GAP);
            _btnExit.Size      = new Size(BW_S, BH - 4);
        }

        // ── Button factory ────────────────────────────────────────────────────
        private static Button MakeButton(string text, Color back, Color dark, float fontSize)
        {
            var b = new Button
            {
                Text      = text,
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = Color.White,
                Font      = new Font("Arial", fontSize, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            b.FlatAppearance.BorderColor        = dark;
            b.FlatAppearance.BorderSize         = 3;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(back, 0.18f);
            b.FlatAppearance.MouseDownBackColor = dark;
            return b;
        }

        // ── Game launch ───────────────────────────────────────────────────────
        private void LaunchGame()
        {
            _animTimer.Stop();
            var game = new mainWin();
            game.FormClosed += (s, e) =>
            {
                _cloudOffset = 0f;
                Show();
                BringToFront();
                _animTimer.Start();
            };
            // Show game first so there is no desktop flash when the fullscreen menu hides
            game.Show();
            Hide();
        }

        // ════════════════════════════════════════════════════════════════════
        //  PAINT
        // ════════════════════════════════════════════════════════════════════
        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            int W = ClientSize.Width, H = ClientSize.Height;

            PaintSky(g, W, H);
            PaintMountains(g, W, H);
            PaintClouds(g, W);
            PaintHills(g, W, H);
            PaintGround(g, W, H);
            PaintDecorations(g, W, H);
            PaintTitle(g, W, H);

            if (_showHowTo) PaintHowToPlay(g, W, H);
        }

        // ── Sky ───────────────────────────────────────────────────────────────
        private static void PaintSky(Graphics g, int W, int H)
        {
            using (var sky = new LinearGradientBrush(Point.Empty, new Point(0, H),
                Color.FromArgb(92, 148, 252), Color.FromArgb(178, 218, 255)))
                g.FillRectangle(sky, 0, 0, W, H);
        }

        // ── Mountains (parallax back layer) ───────────────────────────────────
        private static void PaintMountains(Graphics g, int W, int H)
        {
            int gy = H - 100;
            Color[] cols = { Color.FromArgb(115, 135, 182), Color.FromArgb(148, 168, 208) };
            int[][] raw = {
                new[] { 0,gy,  W/7,gy-155,  W*2/7,gy-75,  W/2,gy-195,
                        W*4/7,gy-85,  W*5/7,gy-165,  W*6/7,gy-95,  W,gy },
                new[] { 0,gy,  W/6,gy-105,  W*2/6,gy-55,  W/2,gy-145,
                        W*4/6,gy-65,  W*5/6,gy-115,  W,gy },
            };
            for (int r = 0; r < 2; r++)
            {
                var pts = new Point[raw[r].Length / 2];
                for (int i = 0; i < pts.Length; i++)
                    pts[i] = new Point(raw[r][i * 2], raw[r][i * 2 + 1]);
                using (var b = new SolidBrush(cols[r]))
                    g.FillPolygon(b, pts);
            }
        }

        // ── Animated clouds ───────────────────────────────────────────────────
        private void PaintClouds(Graphics g, int W)
        {
            foreach (var (rx, cy, cw) in CLOUDS)
            {
                int sx = (int)(rx * (W + 320) - _cloudOffset);
                while (sx + cw + 50 < 0) sx += W + 370;
                int ch = cw / 2;
                using (var shadow = new SolidBrush(Color.FromArgb(35, 80, 110, 165)))
                    g.FillEllipse(shadow, sx + 9, cy + 13, cw, ch);
                using (var b = new SolidBrush(Color.FromArgb(245, 250, 255)))
                {
                    g.FillEllipse(b, sx,               cy + ch / 3,  cw * 6 / 10, ch * 2 / 3);
                    g.FillEllipse(b, sx + cw / 3,      cy,           cw * 5 / 10, ch * 4 / 5);
                    g.FillEllipse(b, sx + cw * 5 / 10, cy + ch / 4,  cw * 5 / 10, ch * 2 / 3);
                }
                using (var hi = new SolidBrush(Color.FromArgb(160, 255, 255, 255)))
                    g.FillEllipse(hi, sx + cw / 3 + 4, cy + 4, cw / 5, ch / 4);
            }
        }

        // ── Green hills ───────────────────────────────────────────────────────
        private static void PaintHills(Graphics g, int W, int H)
        {
            int gy = H - 100;
            (float rx, int r)[] hills = {
                (0.05f,145), (0.22f,108), (0.42f,170),
                (0.62f,125), (0.80f,155), (0.97f,120),
            };
            foreach (var (rx, r) in hills)
            {
                int hx = (int)(rx * W);
                using (var b = new SolidBrush(Color.FromArgb(55, 130, 50)))
                    g.FillEllipse(b, hx - r, gy - r / 2, r * 2, r);
                using (var b = new SolidBrush(Color.FromArgb(80, 175, 65)))
                    g.FillEllipse(b, hx - r + 16, gy - r / 2 - 10, r * 2 - 32, r - 8);
            }
        }

        // ── Brick ground ──────────────────────────────────────────────────────
        private static void PaintGround(Graphics g, int W, int H)
        {
            int gy = H - 100;
            const int BW = 40, BH = 40;
            using (var fill = new SolidBrush(Color.FromArgb(185, 100, 40)))
                g.FillRectangle(fill, 0, gy, W, H - gy);
            using (var mortar = new Pen(Color.FromArgb(120, 60, 15), 1.5f))
            {
                for (int row = 0; row * BH < H - gy + BH; row++)
                {
                    int y = gy + row * BH;
                    g.DrawLine(mortar, 0, y, W, y);
                    int xOff = (row % 2 == 0) ? 0 : BW / 2;
                    for (int x = -BW + xOff; x <= W; x += BW)
                        g.DrawLine(mortar, x, y, x, Math.Min(y + BH, H));
                }
            }
            using (var hi = new SolidBrush(Color.FromArgb(70, 255, 210, 150)))
                g.FillRectangle(hi, 0, gy, W, 4);
        }

        // ════════════════════════════════════════════════════════════════════
        //  DECORATIVE ELEMENTS
        // ════════════════════════════════════════════════════════════════════
        private void PaintDecorations(Graphics g, int W, int H)
        {
            int gy = H - 100;
            int cx = W / 2;

            // Mario standing left of centre
            DrawMario(g, cx / 3, gy - 88);

            // Two Goombas patrolling right side
            DrawGoomba(g, cx + cx * 2 / 3 - 32, gy - 52);
            DrawGoomba(g, cx + cx * 2 / 3 + 36, gy - 52);

            // Bobbing question blocks flanking the title
            int qBobL = (int)(Math.Sin(_bobPhase * 0.8)       * 4);
            int qBobR = (int)(Math.Sin(_bobPhase * 0.8 + 1.1) * 4);
            DrawQuestionBlock(g, cx - 302, H / 5 + 158 + qBobL);
            DrawQuestionBlock(g, cx - 250, H / 5 + 170 + qBobL);
            DrawQuestionBlock(g, cx + 210, H / 5 + 162 + qBobR);
            DrawQuestionBlock(g, cx + 262, H / 5 + 150 + qBobR);

            // Animated coin row above Mario
            for (int i = 0; i < 5; i++)
            {
                float cb = (float)(Math.Sin(_bobPhase * 1.8 + i * 0.7) * 5);
                DrawCoin(g, cx / 3 - 52 + i * 32, gy - 148 + (int)cb);
            }

            // Flagpole decoration on far right
            DrawFlagpole(g, W - 90, gy);
        }

        private void DrawQuestionBlock(Graphics g, int x, int y)
        {
            const int S = 46;
            int frame = (int)(_bobPhase * 2.5f) % 6;
            Color[] pal = {
                Color.FromArgb(255,210,0),   Color.FromArgb(255,230,60),
                Color.FromArgb(255,255,100), Color.FromArgb(255,230,60),
                Color.FromArgb(255,210,0),   Color.FromArgb(220,160,0),
            };
            using (var fill = new SolidBrush(pal[frame]))
                g.FillRectangle(fill, x, y, S, S);
            using (var shine = new SolidBrush(Color.FromArgb(100, 255, 255, 200)))
                g.FillRectangle(shine, x + 3, y + 3, S - 6, 7);
            using (var border = new Pen(Color.FromArgb(140, 80, 0), 3f))
                g.DrawRectangle(border, x + 1, y + 1, S - 3, S - 3);
            int qOff = (frame % 2 == 0) ? 0 : -2;
            using (var f = new Font("Arial", 22, FontStyle.Bold))
            using (var b = new SolidBrush(Color.FromArgb(100, 50, 0)))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString("?", f, b, new RectangleF(x + qOff, y + qOff, S, S), sf);
        }

        private static void DrawMario(Graphics g, int x, int y)
        {
            // Hat
            using (var b = new SolidBrush(Color.FromArgb(200, 30, 10)))
            {
                g.FillRectangle(b, x - 28, y + 6,  56, 13);
                g.FillRectangle(b, x - 19, y - 12, 38, 20);
            }
            using (var b = new SolidBrush(Color.FromArgb(75, 42, 8)))
                g.FillRectangle(b, x - 28, y + 6, 11, 6);
            // Face
            using (var b = new SolidBrush(Color.FromArgb(240, 190, 140)))
                g.FillEllipse(b, x - 19, y + 11, 38, 30);
            g.FillEllipse(Brushes.Black, x - 9, y + 16, 8, 8);
            g.FillEllipse(Brushes.White, x - 6, y + 17, 3, 3);
            using (var b = new SolidBrush(Color.FromArgb(205, 148, 96)))
                g.FillEllipse(b, x - 3, y + 25, 10, 7);
            // Mustache
            using (var b = new SolidBrush(Color.FromArgb(75, 42, 8)))
            {
                g.FillEllipse(b, x - 16, y + 30, 15, 9);
                g.FillEllipse(b, x + 2,  y + 30, 15, 9);
            }
            // Overalls
            using (var b = new SolidBrush(Color.FromArgb(28, 76, 196)))
                g.FillRectangle(b, x - 23, y + 41, 46, 24);
            // Shirt
            using (var b = new SolidBrush(Color.FromArgb(200, 30, 10)))
            {
                g.FillRectangle(b, x - 27, y + 41, 8, 18);
                g.FillRectangle(b, x + 19, y + 41, 8, 18);
            }
            // Hands
            using (var b = new SolidBrush(Color.FromArgb(240, 190, 140)))
            {
                g.FillEllipse(b, x - 30, y + 55, 12, 12);
                g.FillEllipse(b, x + 18, y + 55, 12, 12);
            }
            // Legs
            using (var b = new SolidBrush(Color.FromArgb(28, 76, 196)))
            {
                g.FillRectangle(b, x - 18, y + 63, 17, 17);
                g.FillRectangle(b, x + 1,  y + 63, 17, 17);
            }
            // Shoes
            using (var b = new SolidBrush(Color.FromArgb(75, 42, 8)))
            {
                g.FillRectangle(b, x - 22, y + 78, 24, 10);
                g.FillRectangle(b, x + 2,  y + 78, 24, 10);
            }
        }

        private static void DrawGoomba(Graphics g, int x, int y)
        {
            const int GW = 50, GH = 52;
            // Feet
            using (var b = new SolidBrush(Color.FromArgb(90, 40, 5)))
            {
                g.FillRectangle(b, x + 3,       y + GH - 14, 18, 14);
                g.FillRectangle(b, x + GW - 21, y + GH - 14, 18, 14);
            }
            // Body
            using (var br = new LinearGradientBrush(
                new Point(x, y + 6), new Point(x + GW, y + GH - 14),
                Color.FromArgb(175, 95, 35), Color.FromArgb(120, 55, 10)))
                g.FillEllipse(br, x, y + 6, GW, GH - 18);
            // Dark cap
            using (var cp = new GraphicsPath())
            using (var cb = new SolidBrush(Color.FromArgb(85, 35, 5)))
            {
                cp.AddArc(x, y + 6, GW, GH - 18, 180, 180);
                cp.CloseFigure();
                g.FillPath(cb, cp);
            }
            // Angry brows
            int ey = y + (GH - 18) / 2 - 2;
            using (var pen = new Pen(Color.FromArgb(50, 15, 0), 2.5f)
                { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(pen, x + 6,       ey + 1, x + 18,      ey + 5);
                g.DrawLine(pen, x + GW - 18, ey + 5, x + GW - 6,  ey + 1);
            }
            // Eyes
            g.FillEllipse(Brushes.White, x + 5,       ey + 3, 13, 13);
            g.FillEllipse(Brushes.White, x + GW - 18, ey + 3, 13, 13);
            using (var pu = new SolidBrush(Color.FromArgb(20, 10, 0)))
            {
                g.FillEllipse(pu, x + 9,       ey + 6, 6, 6);
                g.FillEllipse(pu, x + GW - 15, ey + 6, 6, 6);
            }
        }

        private void DrawCoin(Graphics g, int cx, int cy)
        {
            const int R = 11;
            float sq = 1f - 0.55f * Math.Abs((float)Math.Sin(_bobPhase * 2.2));
            int cw = Math.Max(3, (int)(R * 2 * sq));
            int ox = (R * 2 - cw) / 2;
            using (var lg = new LinearGradientBrush(
                new Point(cx + ox, cy), new Point(cx + ox + cw, cy + R * 2),
                Color.FromArgb(255, 230, 40), Color.FromArgb(200, 155, 0)))
                g.FillEllipse(lg, cx + ox, cy, cw, R * 2);
            if (cw > 5)
            {
                using (var sh = new SolidBrush(Color.FromArgb(120, 255, 255, 180)))
                    g.FillEllipse(sh, cx + ox + 2, cy + 2, cw / 3 + 1, R / 2);
            }
            using (var pen = new Pen(Color.FromArgb(180, 130, 0), 1.5f))
                g.DrawEllipse(pen, cx + ox, cy, cw, R * 2);
        }

        private static void DrawFlagpole(Graphics g, int x, int gy)
        {
            const int PH = 155;
            int pX = x + 12;
            using (var lg = new LinearGradientBrush(
                new Point(pX, 0), new Point(pX + 12, 0),
                Color.FromArgb(200, 200, 210), Color.FromArgb(90, 90, 110)))
                g.FillRectangle(lg, pX, gy - PH, 12, PH);
            using (var sh = new SolidBrush(Color.FromArgb(80, 255, 255, 255)))
                g.FillRectangle(sh, pX + 2, gy - PH, 3, PH);
            using (var b = new SolidBrush(Color.FromArgb(255, 210, 30)))
                g.FillEllipse(b, pX - 6, gy - PH - 16, 24, 24);
            using (var b = new SolidBrush(Color.FromArgb(180, 255, 255, 150)))
                g.FillEllipse(b, pX - 2, gy - PH - 12, 8, 8);
            var flagPts = new PointF[] {
                new PointF(pX + 12, gy - PH + 6),
                new PointF(pX + 12, gy - PH + 50),
                new PointF(pX + 50, gy - PH + 28),
            };
            using (var b = new SolidBrush(Color.FromArgb(60, 185, 60)))
                g.FillPolygon(b, flagPts);
            using (var b = new SolidBrush(Color.FromArgb(80, 80, 90)))
                g.FillRectangle(b, pX - 12, gy - 24, 36, 24);
            using (var b = new SolidBrush(Color.FromArgb(110, 110, 125)))
                g.FillRectangle(b, pX - 12, gy - 24, 36, 8);
        }

        // ════════════════════════════════════════════════════════════════════
        //  TITLE
        // ════════════════════════════════════════════════════════════════════
        private void PaintTitle(Graphics g, int W, int H)
        {
            int cx  = W / 2;
            int bob = (int)(Math.Sin(_bobPhase * 0.6) * 5);
            int ty  = H / 6 + bob;

            // Drop shadow (single pass, no stroke)
            DrawStrokedText(g, "SUPER", "Impact", 48,  FontStyle.Bold,
                Color.FromArgb(55, 0, 0, 0), Color.Empty, 0, cx + 4, ty + 4);
            DrawStrokedText(g, "MARIO", "Impact", 100, FontStyle.Bold,
                Color.FromArgb(55, 0, 0, 0), Color.Empty, 0, cx + 5, ty + 50);

            // "SUPER" – white with dark outline
            DrawStrokedText(g, "SUPER", "Impact", 48, FontStyle.Bold,
                Color.FromArgb(255, 255, 255), Color.FromArgb(60, 30, 0), 4f, cx, ty);

            // "MARIO" – red with very dark outline
            DrawStrokedText(g, "MARIO", "Impact", 100, FontStyle.Bold,
                Color.FromArgb(218, 28, 8), Color.FromArgb(70, 0, 0), 7f, cx, ty + 46);

            // Gold stars
            using (var sb = new SolidBrush(Color.FromArgb(255, 240, 0)))
            {
                DrawStar(g, sb, cx - 290, ty + 108 + bob / 2, 22);
                DrawStar(g, sb, cx + 278, ty + 108 + bob / 2, 22);
                DrawStar(g, sb, cx - 225, ty + 62  + bob / 2, 13);
                DrawStar(g, sb, cx + 215, ty + 62  + bob / 2, 13);
            }

            // Footer hint
            using (var f  = new Font("Courier New", 10f, FontStyle.Bold))
            using (var b  = new SolidBrush(Color.FromArgb(200, 255, 255, 180)))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center })
                g.DrawString("Reach the  GOAL  flag to win each level!", f, b, cx, H - 24, sf);
        }

        // ── Outlined text via GraphicsPath ────────────────────────────────────
        private static void DrawStrokedText(Graphics g, string text,
            string family, float ptSize, FontStyle style,
            Color fill, Color stroke, float strokeThick, int cx, int y)
        {
            using (var font = new Font(family, ptSize, style))
            using (var path = new GraphicsPath())
            using (var sf   = new StringFormat
            {
                Alignment     = StringAlignment.Center,
                LineAlignment = StringAlignment.Near,
            })
            {
                float em = g.DpiY * font.SizeInPoints / 72f;
                path.AddString(text, font.FontFamily, (int)font.Style, em, new Point(cx, y), sf);

                if (strokeThick > 0f)
                {
                    using (var pen = new Pen(stroke, strokeThick * 2f) { LineJoin = LineJoin.Round })
                        g.DrawPath(pen, path);
                }
                using (var br = new SolidBrush(fill))
                    g.FillPath(br, path);
            }
        }

        // ── 5-pointed star ────────────────────────────────────────────────────
        private static void DrawStar(Graphics g, Brush b, int cx, int cy, int r)
        {
            var pts = new PointF[10];
            for (int i = 0; i < 10; i++)
            {
                double angle = i * Math.PI / 5 - Math.PI / 2;
                float  rad   = (i % 2 == 0) ? r : r * 0.38f;
                pts[i] = new PointF(
                    cx + rad * (float)Math.Cos(angle),
                    cy + rad * (float)Math.Sin(angle));
            }
            g.FillPolygon(b, pts);
        }

        // ════════════════════════════════════════════════════════════════════
        //  HOW TO PLAY OVERLAY
        // ════════════════════════════════════════════════════════════════════
        private static void PaintHowToPlay(Graphics g, int W, int H)
        {
            const int PW = 450, PH = 280;
            int px = (W - PW) / 2;
            int py = (H - PH) / 2 - 10;

            // Panel background + border
            using (var b = new SolidBrush(Color.FromArgb(238, 14, 14, 38)))
                g.FillRectangle(b, px, py, PW, PH);
            using (var pen = new Pen(Color.FromArgb(255, 210, 0), 4f))
                g.DrawRectangle(pen, px, py, PW, PH);
            using (var pen = new Pen(Color.FromArgb(80, 255, 255, 180), 1.5f))
                g.DrawRectangle(pen, px + 5, py + 5, PW - 10, PH - 10);

            // Header
            using (var f  = new Font("Impact", 20f))
            using (var b  = new SolidBrush(Color.FromArgb(255, 220, 0)))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center })
                g.DrawString("HOW  TO  PLAY", f, b, px + PW / 2, py + 14, sf);
            using (var pen = new Pen(Color.FromArgb(180, 200, 160, 0), 2f))
                g.DrawLine(pen, px + 20, py + 48, px + PW - 20, py + 48);

            // Controls table
            string[,] rows = {
                { "W / ↑ / SPACE",  "Jump"                           },
                { "A / ←",          "Move Left"                      },
                { "D / →",          "Move Right"                     },
                { "ESC",            "Pause"                          },
                { "",               ""                               },
                { "? blocks",       "Mushroom (grow SUPER) or Coin"  },
                { "Jump on enemy",  "Stomp to defeat"                },
                { "Mushroom",       "Grow SUPER – absorb extra hit"  },
                { "GOAL flag",      "Reach it to complete the level!" },
            };

            using (var fKey = new Font("Courier New", 9.5f, FontStyle.Bold))
            using (var fAct = new Font("Courier New", 9.5f))
            using (var bKey = new SolidBrush(Color.FromArgb(255, 230, 80)))
            using (var bAct = new SolidBrush(Color.White))
            {
                for (int i = 0; i < rows.GetLength(0); i++)
                {
                    if (rows[i, 0].Length == 0) continue;
                    int ry = py + 58 + i * 22;
                    g.DrawString(rows[i, 0], fKey, bKey, px + 14,  ry);
                    g.DrawString("→  " + rows[i, 1], fAct, bAct, px + 198, ry);
                }
            }

            // Close hint
            using (var f  = new Font("Arial", 8.5f, FontStyle.Italic))
            using (var b  = new SolidBrush(Color.FromArgb(140, 200, 200, 255)))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center })
                g.DrawString("click  ? HOW TO PLAY  again to close", f, b, px + PW / 2, py + PH - 20, sf);
        }
    }
}
