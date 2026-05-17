using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using supermario.ML;

namespace supermario
{
    public sealed class TrainingForm : Form
    {
        // ── Layout ───────────────────────────────────────────────────────────────
        private const int DASHBOARD_W = 320;
        private const int AGENT_W     = 36;
        private const int AGENT_H     = 48;

        // ── Game canvas ──────────────────────────────────────────────────────────
        private Panel    _canvas;
        private Timer    _simTimer;
        private int      _cameraX;
        private List<Rectangle> _platforms = new List<Rectangle>();

        // ── ML state ─────────────────────────────────────────────────────────────
        private Population _pop;
        private bool       _running;
        private int        _bestEver;

        // ── Dashboard controls ───────────────────────────────────────────────────
        private Label  _lblGen, _lblAlive, _lblBest, _lblBestEver;
        private NeuralNetworkControl _netVis;

        // Settings inputs
        private NumericUpDown _nudPop, _nudMutRate, _nudSurvive;
        private TextBox       _tbShape;
        private Button        _btnApply;

        // Control buttons
        private Button _btnStartPause, _btnReset, _btnBack;

        // ── Level geometry (a flat representative strip for training) ────────────
        private static readonly (int x, int y, int w, int h)[] TRAIN_PLATFORMS = {
            (0,   450, 500, 40),  (520, 430, 240, 40), (800, 390, 180, 40),
            (1010,450, 300, 40),  (1350,410, 220, 40), (1610,370, 200, 40),
            (1850,450, 250, 40),  (2140,430, 200, 40), (2380,390, 200, 40),
            (2620,450, 380, 40),
        };

        private static readonly Point SPAWN = new Point(30, 350);

        public TrainingForm()
        {
            Text            = "Luigi AI Trainer";
            FormBorderStyle = FormBorderStyle.None;
            WindowState     = FormWindowState.Maximized;
            DoubleBuffered  = true;
            BackColor       = Color.FromArgb(15, 15, 25);
            KeyPreview      = true;
            KeyDown        += (s, e) => { if (e.KeyCode == Keys.Escape) GoBack(); };

            BuildPlatforms();
            BuildUI();

            _simTimer          = new Timer { Interval = 16 };
            _simTimer.Tick    += SimTick;

            Shown += (s, e) => ResetTraining();
        }

        // ════════════════════════════════════════════════════════════════════════
        //  UI construction
        // ════════════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            // ── Game canvas (left portion) ──────────────────────────────────────
            _canvas = new Panel
            {
                BackColor = Color.FromArgb(92, 148, 252),
                Dock      = DockStyle.None,
            };
            _canvas.Paint += CanvasPaint;
            Controls.Add(_canvas);

            // ── Dashboard panel (right) ─────────────────────────────────────────
            var dash = new Panel
            {
                BackColor = Color.FromArgb(22, 22, 38),
                Dock      = DockStyle.None,
            };
            Controls.Add(dash);

            SizeChanged += (s, e) => LayoutPanels(dash);
            Shown       += (s, e) => LayoutPanels(dash);

            // Title
            dash.Controls.Add(MakeLabel("LUIGI  AI  TRAINER", "Impact", 17, Color.FromArgb(80,200,80),
                new Rectangle(0, 14, DASHBOARD_W, 32), true));

            // Separator
            var sep = new Panel { BackColor = Color.FromArgb(50,50,70), Height = 2 };
            sep.Location = new Point(10, 52);
            sep.Width    = DASHBOARD_W - 20;
            dash.Controls.Add(sep);

            // ── Stats ───────────────────────────────────────────────────────────
            int y = 64;
            dash.Controls.Add(MakeLabel("GENERATION", "Courier New", 9, Color.FromArgb(140,180,140),
                new Rectangle(12, y, 150, 18)));
            _lblGen = MakeLabel("0", "Courier New", 14, Color.White, new Rectangle(160, y - 2, 140, 22));
            dash.Controls.Add(_lblGen);

            y += 28;
            dash.Controls.Add(MakeLabel("ALIVE", "Courier New", 9, Color.FromArgb(140,180,140),
                new Rectangle(12, y, 150, 18)));
            _lblAlive = MakeLabel("0 / 0", "Courier New", 14, Color.FromArgb(100,220,100),
                new Rectangle(160, y - 2, 140, 22));
            dash.Controls.Add(_lblAlive);

            y += 28;
            dash.Controls.Add(MakeLabel("BEST THIS GEN", "Courier New", 9, Color.FromArgb(140,180,140),
                new Rectangle(12, y, 150, 18)));
            _lblBest = MakeLabel("0", "Courier New", 14, Color.FromArgb(255,230,60),
                new Rectangle(160, y - 2, 140, 22));
            dash.Controls.Add(_lblBest);

            y += 28;
            dash.Controls.Add(MakeLabel("ALL-TIME BEST", "Courier New", 9, Color.FromArgb(140,180,140),
                new Rectangle(12, y, 150, 18)));
            _lblBestEver = MakeLabel("0", "Courier New", 14, Color.FromArgb(255,160,60),
                new Rectangle(160, y - 2, 140, 22));
            dash.Controls.Add(_lblBestEver);

            // ── Network visualiser ───────────────────────────────────────────────
            y += 40;
            dash.Controls.Add(MakeLabel("BEST NETWORK", "Courier New", 8, Color.FromArgb(120,140,120),
                new Rectangle(12, y, 150, 16)));
            y += 20;
            _netVis = new NeuralNetworkControl
            {
                Location = new Point(8, y),
                Size     = new Size(DASHBOARD_W - 16, 140),
            };
            dash.Controls.Add(_netVis);
            y += 150;

            // Separator
            var sep2 = new Panel { BackColor = Color.FromArgb(50,50,70), Height = 2,
                Location = new Point(10, y), Width = DASHBOARD_W - 20 };
            dash.Controls.Add(sep2);
            y += 10;

            // ── Settings ─────────────────────────────────────────────────────────
            dash.Controls.Add(MakeLabel("SETTINGS", "Impact", 12, Color.FromArgb(200,200,255),
                new Rectangle(12, y, DASHBOARD_W - 24, 22)));
            y += 26;

            // Population size
            dash.Controls.Add(MakeLabel("Population", "Courier New", 9, Color.FromArgb(160,160,200),
                new Rectangle(12, y + 2, 120, 18)));
            _nudPop = new NumericUpDown { Location = new Point(138, y), Size = new Size(80, 22),
                Minimum = 10, Maximum = 500, Value = NetParams.PopulationSize,
                BackColor = Color.FromArgb(35, 35, 55), ForeColor = Color.White };
            dash.Controls.Add(_nudPop);
            y += 28;

            // Mutation rate
            dash.Controls.Add(MakeLabel("Mutation %", "Courier New", 9, Color.FromArgb(160,160,200),
                new Rectangle(12, y + 2, 120, 18)));
            _nudMutRate = new NumericUpDown { Location = new Point(138, y), Size = new Size(80, 22),
                Minimum = 1, Maximum = 50, DecimalPlaces = 0,
                Value = (decimal)(NetParams.MutationRate * 100),
                BackColor = Color.FromArgb(35, 35, 55), ForeColor = Color.White };
            dash.Controls.Add(_nudMutRate);
            y += 28;

            // Survive rate
            dash.Controls.Add(MakeLabel("Survive %", "Courier New", 9, Color.FromArgb(160,160,200),
                new Rectangle(12, y + 2, 120, 18)));
            _nudSurvive = new NumericUpDown { Location = new Point(138, y), Size = new Size(80, 22),
                Minimum = 5, Maximum = 80, DecimalPlaces = 0,
                Value = (decimal)(NetParams.SurviveRate * 100),
                BackColor = Color.FromArgb(35, 35, 55), ForeColor = Color.White };
            dash.Controls.Add(_nudSurvive);
            y += 28;

            // Network shape
            dash.Controls.Add(MakeLabel("Net shape", "Courier New", 9, Color.FromArgb(160,160,200),
                new Rectangle(12, y + 2, 120, 18)));
            _tbShape = new TextBox { Location = new Point(138, y), Size = new Size(150, 22),
                Text = ShapeToString(NetParams.NetworkShape),
                BackColor = Color.FromArgb(35, 35, 55), ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle };
            dash.Controls.Add(_tbShape);
            y += 28;

            // Shape hint
            dash.Controls.Add(MakeLabel("e.g.  4,6,4,2  (inputs,hidden...,outputs)", "Courier New", 7,
                Color.FromArgb(100,100,140), new Rectangle(12, y, DASHBOARD_W - 24, 14)));
            y += 22;

            // Apply button
            _btnApply = MakeDashButton("APPLY", Color.FromArgb(55, 80, 140), new Rectangle(12, y, DASHBOARD_W - 24, 30));
            _btnApply.Click += (s, e) => ApplySettings();
            dash.Controls.Add(_btnApply);
            y += 42;

            // ── Control buttons ──────────────────────────────────────────────────
            _btnStartPause = MakeDashButton("▶  START", Color.FromArgb(40, 140, 40), new Rectangle(12, y, DASHBOARD_W - 24, 36));
            _btnStartPause.Click += (s, e) => ToggleRunning();
            dash.Controls.Add(_btnStartPause);
            y += 46;

            _btnReset = MakeDashButton("⟳  RESET", Color.FromArgb(120, 80, 30), new Rectangle(12, y, DASHBOARD_W - 24, 32));
            _btnReset.Click += (s, e) => ResetTraining();
            dash.Controls.Add(_btnReset);
            y += 42;

            _btnBack = MakeDashButton("← BACK TO MENU", Color.FromArgb(90, 30, 30), new Rectangle(12, y, DASHBOARD_W - 24, 32));
            _btnBack.Click += (s, e) => GoBack();
            dash.Controls.Add(_btnBack);
        }

        private void LayoutPanels(Panel dash)
        {
            int W = ClientSize.Width, H = ClientSize.Height;
            dash.Location = new Point(W - DASHBOARD_W, 0);
            dash.Size     = new Size(DASHBOARD_W, H);
            _canvas.Location = new Point(0, 0);
            _canvas.Size     = new Size(W - DASHBOARD_W, H);
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Training logic
        // ════════════════════════════════════════════════════════════════════════
        private void BuildPlatforms()
        {
            _platforms.Clear();
            foreach (var (px, py, pw, ph) in TRAIN_PLATFORMS)
                _platforms.Add(new Rectangle(px, py, pw, ph));
        }

        private void ResetTraining()
        {
            _simTimer.Stop();
            _running  = false;
            _cameraX  = 0;
            _bestEver = 0;
            _pop      = new Population(SPAWN);
            UpdateDashboard();
            _btnStartPause.Text = "▶  START";
            _canvas.Invalidate();
        }

        private void ApplySettings()
        {
            // Validate shape string
            int[] shape = ParseShape(_tbShape.Text);
            if (shape == null || shape.Length < 2 || shape[0] < 1 || shape[shape.Length - 1] < 1)
            {
                MessageBox.Show("Invalid network shape.\nUse comma-separated integers, e.g.  4,6,4,2",
                    "Shape Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            NetParams.PopulationSize = (int)_nudPop.Value;
            NetParams.MutationRate   = (double)_nudMutRate.Value / 100.0;
            NetParams.SurviveRate    = (double)_nudSurvive.Value / 100.0;
            NetParams.NetworkShape   = shape;

            ResetTraining();
        }

        private void ToggleRunning()
        {
            _running = !_running;
            if (_running)
            {
                _simTimer.Start();
                _btnStartPause.Text = "⏸  PAUSE";
            }
            else
            {
                _simTimer.Stop();
                _btnStartPause.Text = "▶  RESUME";
            }
        }

        private void SimTick(object sender, EventArgs e)
        {
            if (_pop == null) return;

            // Step every living agent
            foreach (var agent in _pop.Agents)
            {
                if (!agent.IsAlive) continue;

                // Kill if fallen off world
                if (agent.Position.Y > 560) { agent.IsAlive = false; continue; }

                // Think
                var inputs         = MarioAgent.ComputeInputs(
                    agent.Position, AGENT_W, AGENT_H, _platforms, new List<Rectangle>(), agent.IsGrounded);
                var (dir, jump)    = agent.Think(inputs);

                agent.Step(dir, jump);
                ApplyPlatformCollisions(agent);
            }

            // Advance camera to track the lead agent
            var leader = _pop.Agents.Where(a => a.IsAlive).OrderByDescending(a => a.Position.X).FirstOrDefault();
            if (leader != null)
                _cameraX = Math.Max(0, leader.Position.X - (_canvas.Width / 3));

            // Evolve when all dead
            if (_pop.AllDead)
            {
                int genBest = _pop.BestAgent()?.Fitness ?? 0;
                if (genBest > _bestEver) _bestEver = genBest;
                _pop.CreateNewGeneration();
            }

            UpdateDashboard();
            _canvas.Invalidate();
        }

        private void ApplyPlatformCollisions(MarioAgent agent)
        {
            bool foundGround = false;
            var ar = new Rectangle(agent.Position.X, agent.Position.Y, AGENT_W, AGENT_H);

            foreach (var plat in _platforms)
            {
                if (!ar.IntersectsWith(plat)) continue;

                int ot = ar.Bottom - plat.Top;
                int ob = plat.Bottom - ar.Top;
                int ol = ar.Right   - plat.Left;
                int orr= plat.Right - ar.Left;
                int min = Math.Min(Math.Min(ot, ob), Math.Min(ol, orr));

                if (min == ot && ot < 20)
                {
                    agent.LandOn(plat.Top, AGENT_H);
                    foundGround = true;
                    ar = new Rectangle(agent.Position.X, agent.Position.Y, AGENT_W, AGENT_H);
                }
                else if (min == ob) { agent.HitCeiling(plat.Bottom); }
                else if (min == ol || min == orr) { agent.BlockHorizontal(min == ol ? plat.Left - AGENT_W : plat.Right); }
            }

            if (!foundGround) agent.LeaveGround();
        }

        private void UpdateDashboard()
        {
            if (_pop == null) return;
            int alive   = _pop.AliveCount;
            int total   = _pop.Agents.Count;
            int genBest = _pop.Agents.Max(a => a.Fitness);

            _lblGen.Text      = _pop.Generation.ToString();
            _lblAlive.Text    = $"{alive} / {total}";
            _lblBest.Text     = genBest.ToString();
            _lblBestEver.Text = _bestEver.ToString();

            var best = _pop.BestAgent();
            if (best != null)
            {
                var inputs = MarioAgent.ComputeInputs(
                    best.Position, AGENT_W, AGENT_H, _platforms, new List<Rectangle>(), best.IsGrounded);
                _netVis.SetNetwork(best.Brain, inputs);
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Canvas paint
        // ════════════════════════════════════════════════════════════════════════
        private void CanvasPaint(object sender, PaintEventArgs e)
        {
            var g  = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            int W  = _canvas.Width, H = _canvas.Height;

            // Sky
            using (var sky = new LinearGradientBrush(Point.Empty, new Point(0, H),
                Color.FromArgb(92,148,252), Color.FromArgb(178,218,255)))
                g.FillRectangle(sky, 0, 0, W, H);

            // Platforms
            foreach (var plat in _platforms)
            {
                int sx = plat.X - _cameraX;
                if (sx + plat.Width < 0 || sx > W) continue;
                using (var b = new SolidBrush(Color.FromArgb(185,100,40)))
                    g.FillRectangle(b, sx, plat.Y, plat.Width, plat.Height);
                using (var p = new Pen(Color.FromArgb(120,60,15), 2f))
                    g.DrawRectangle(p, sx, plat.Y, plat.Width, plat.Height);
                using (var top = new SolidBrush(Color.FromArgb(70,255,210,150)))
                    g.FillRectangle(top, sx, plat.Y, plat.Width, 4);
            }

            // Agents (Luigi = green figure)
            if (_pop == null) return;
            foreach (var agent in _pop.Agents)
            {
                if (!agent.IsAlive) continue;
                int sx = agent.Position.X - _cameraX;
                DrawLuigiAgent(g, sx, agent.Position.Y, agent == _pop.BestAgent());
            }
        }

        // Minimal Luigi sprite: same proportions as DrawMario but green/white
        private static void DrawLuigiAgent(Graphics g, int x, int y, bool isBest)
        {
            int alpha = isBest ? 255 : 130;

            // Hat (green)
            using (var b = new SolidBrush(Color.FromArgb(alpha, 30, 140, 30)))
            {
                g.FillRectangle(b, x + 1,  y + 2,  22, 5);
                g.FillRectangle(b, x + 5,  y - 4,  14, 8);
            }
            // Face
            using (var b = new SolidBrush(Color.FromArgb(alpha, 240, 190, 140)))
                g.FillEllipse(b, x + 4, y + 6, 14, 11);
            // Mustache
            using (var b = new SolidBrush(Color.FromArgb(alpha, 60, 30, 5)))
            {
                g.FillEllipse(b, x + 4,  y + 13, 6, 4);
                g.FillEllipse(b, x + 11, y + 13, 6, 4);
            }
            // Overalls (white/light for Luigi)
            using (var b = new SolidBrush(Color.FromArgb(alpha, 200, 210, 200)))
                g.FillRectangle(b, x + 2, y + 17, 18, 10);
            // Shirt (green)
            using (var b = new SolidBrush(Color.FromArgb(alpha, 30, 140, 30)))
            {
                g.FillRectangle(b, x,      y + 17, 4, 7);
                g.FillRectangle(b, x + 18, y + 17, 4, 7);
            }
            // Legs
            using (var b = new SolidBrush(Color.FromArgb(alpha, 200, 210, 200)))
            {
                g.FillRectangle(b, x + 3,  y + 27,  7, 7);
                g.FillRectangle(b, x + 12, y + 27, 7, 7);
            }
            // Shoes (brown)
            using (var b = new SolidBrush(Color.FromArgb(alpha, 75, 42, 8)))
            {
                g.FillRectangle(b, x + 1,  y + 33, 10, 4);
                g.FillRectangle(b, x + 11, y + 33, 10, 4);
            }

            // Gold ring around the best agent
            if (isBest)
                using (var p = new Pen(Color.FromArgb(255, 255, 200, 0), 2f))
                    g.DrawRectangle(p, x - 2, y - 6, AGENT_W + 4, AGENT_H + 6);
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Navigation
        // ════════════════════════════════════════════════════════════════════════
        private void GoBack()
        {
            _simTimer.Stop();
            var menu = new MainMenuForm();
            menu.Show();
            Close();
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Helpers
        // ════════════════════════════════════════════════════════════════════════
        private static Label MakeLabel(string text, string font, float size, Color color,
            Rectangle bounds, bool centered = false)
        {
            var lbl = new Label
            {
                Text      = text,
                Font      = new Font(font, size, FontStyle.Bold),
                ForeColor = color,
                BackColor = Color.Transparent,
                Location  = new Point(bounds.X, bounds.Y),
                Size      = new Size(bounds.Width, bounds.Height),
                TextAlign = centered ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft,
            };
            return lbl;
        }

        private static Button MakeDashButton(string text, Color back, Rectangle bounds)
        {
            var b = new Button
            {
                Text      = text,
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = Color.White,
                Font      = new Font("Courier New", 9f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                Location  = new Point(bounds.X, bounds.Y),
                Size      = new Size(bounds.Width, bounds.Height),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            b.FlatAppearance.BorderColor = ControlPaint.Dark(back, 0.3f);
            b.FlatAppearance.BorderSize  = 2;
            return b;
        }

        private static string ShapeToString(int[] shape)
            => string.Join(",", shape);

        private static int[] ParseShape(string s)
        {
            try
            {
                var parts = s.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var result = new int[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                    result[i] = int.Parse(parts[i].Trim());
                return result;
            }
            catch { return null; }
        }

        // C# 7 compat: _tbShape indexer [^1] replaced with Length-1
        private static int LastElement(int[] arr) => arr[arr.Length - 1];
    }
}
