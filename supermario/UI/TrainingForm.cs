using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using supermario.ML;

namespace supermario
{
    public sealed class TrainingForm : Form
    {
        // Panel that double-buffers its own painting. The host Form's DoubleBuffered
        // flag does not apply to child controls, so without this the simulation
        // canvas (repainted every tick) flickers badly.
        private sealed class BufferedPanel : Panel
        {
            public BufferedPanel() { DoubleBuffered = true; }
        }

        // ── Layout ───────────────────────────────────────────────────────────────
        private const int DASHBOARD_W = 320;
        private const int AGENT_W     = 36;
        private const int AGENT_H     = 48;
        private const int COIN_BONUS  = 50;   // fitness per coin collected
        private const int COIN_R      = 10;   // coin draw radius

        // The simulation feeds a fixed-size input vector (ComputeInputs) and reads
        // a fixed number of outputs (Think), so any network shape must match this
        // contract or Forward / Think will index out of range.
        private const int NET_INPUTS  = 4;
        private const int NET_OUTPUTS = 2;

        // ── Game canvas ──────────────────────────────────────────────────────────
        private Panel            _canvas;
        private Timer            _simTimer;
        private int              _cameraX;
        private List<Rectangle>  _platforms = new List<Rectangle>();
        private Rectangle[]      _coinRects;

        // ── תצוגת הסצנה כתמונות (PictureBox) במקום ציור GDI+ ─────────────────────
        // Scene rendered with PictureBoxes (no GDI+). Platforms/coins are created
        // once and only repositioned; agents reuse a pool so we never create or
        // destroy controls per frame (which would stutter the training).
        private readonly List<PictureBox> _platformViews = new List<PictureBox>();
        private PictureBox[]              _coinViews;
        private readonly List<PictureBox> _agentPool = new List<PictureBox>();
        private int                       _simFrame;   // מונה פריימים לאנימציית הליכה

        // ── ML state ─────────────────────────────────────────────────────────────
        private Population _pop;
        private bool       _running;
        private int        _bestEver;

        // ── Dashboard controls ───────────────────────────────────────────────────
        private Label _lblGen, _lblAlive, _lblBest, _lblBestEver;
        private NeuralNetworkControl _netVis;

        // Settings inputs
        private NumericUpDown _nudPop, _nudMutRate, _nudSurvive;
        private TextBox       _tbShape;
        private Button        _btnApply;

        // Control buttons
        private Button _btnStartPause, _btnReset, _btnSave, _btnLoad, _btnBack;

        // ── Training level geometry ──────────────────────────────────────────────
        private static readonly (int x, int y, int w, int h)[] TRAIN_PLATFORMS = {
            (0,   450, 500, 40),   (520, 430, 240, 40),   (800, 390, 180, 40),
            (1010,450, 300, 40),   (1350,410, 220, 40),   (1610,370, 200, 40),
            (1850,450, 250, 40),   (2140,430, 200, 40),   (2380,390, 200, 40),
            (2620,450, 380, 40),
        };

        // Coins placed to reward crossing gaps and reaching higher platforms.
        // Each coin gives +COIN_BONUS to the collecting agent's fitness.
        private static readonly Point[] TRAIN_COINS = {
            // Forward momentum on first platform
            new Point(100,380), new Point(200,380), new Point(350,380), new Point(460,380),
            // Gap 1 (500-520): high coin forces a jump
            new Point(510,350),
            // Platform 1
            new Point(580,360), new Point(660,360), new Point(730,360),
            // Gap 2 (760-800): jump up
            new Point(782,310),
            // Platform 2 (higher)
            new Point(840,320), new Point(900,320), new Point(960,320),
            // Gap 3 (980-1010): drop down
            new Point(994,360),
            // Platform 3
            new Point(1070,380), new Point(1170,380), new Point(1270,380),
            // Gap 4 (1310-1350): jump up
            new Point(1332,340),
            // Platform 4
            new Point(1400,340), new Point(1490,340), new Point(1555,340),
            // Gap 5 (1570-1610): jump up
            new Point(1592,290),
            // Platform 5 (highest so far)
            new Point(1650,300), new Point(1740,300),
            // Gap 6 (1810-1850): drop down
            new Point(1832,370),
            // Platform 6
            new Point(1920,380), new Point(2010,380), new Point(2070,380),
            // Gap 7 + Platform 7
            new Point(2120,355), new Point(2220,360), new Point(2300,360),
            // Gap 8 (2340-2380): jump up
            new Point(2362,310),
            // Platform 8
            new Point(2430,320), new Point(2520,320),
            // Final stretch
            new Point(2660,380), new Point(2770,380), new Point(2880,380), new Point(2970,380),
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

            Sprites.LoadAll();          // ודא שהתמונות זמינות גם אם נכנסים ישר לאימון

            BuildPlatforms();
            BuildCoins();
            BuildUI();

            _simTimer       = new Timer { Interval = 16 };
            _simTimer.Tick += SimTick;

            FormClosed += (s, e) => { _simTimer.Stop(); _simTimer.Dispose(); };

            Shown += (s, e) => ResetTraining();
        }

        // ════════════════════════════════════════════════════════════════════════
        //  UI construction
        // ════════════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            _canvas = new BufferedPanel { BackColor = Color.FromArgb(92, 148, 252) };
            if (Sprites.Background != null)
            {
                _canvas.BackgroundImage       = Sprites.Background;
                _canvas.BackgroundImageLayout = ImageLayout.Tile;   // רקע פרוס כאריחים
            }
            Controls.Add(_canvas);
            BuildLevelViews();          // יוצר PictureBox לכל פלטפורמה/מטבע (פעם אחת)

            var dash = new Panel { BackColor = Color.FromArgb(22, 22, 38) };
            Controls.Add(dash);

            SizeChanged += (s, e) => LayoutPanels(dash);
            Shown       += (s, e) => LayoutPanels(dash);

            // Title
            dash.Controls.Add(MakeLabel("LUIGI  AI  TRAINER", "Impact", 17,
                Color.FromArgb(80, 200, 80), new Rectangle(0, 14, DASHBOARD_W, 32), true));
            AddSep(dash, 52);

            // Stats
            int y = 64;
            AddStatRow(dash, ref y, "GENERATION",    Color.White,                  out _lblGen);
            AddStatRow(dash, ref y, "ALIVE",         Color.FromArgb(100,220,100),  out _lblAlive);
            AddStatRow(dash, ref y, "BEST THIS GEN", Color.FromArgb(255,230,60),   out _lblBest);
            AddStatRow(dash, ref y, "ALL-TIME BEST", Color.FromArgb(255,160,60),   out _lblBestEver);

            // Network visualiser
            y += 12;
            dash.Controls.Add(MakeLabel("BEST NETWORK", "Courier New", 8,
                Color.FromArgb(120,140,120), new Rectangle(12, y, 150, 16)));
            y += 18;
            _netVis = new NeuralNetworkControl { Location = new Point(8, y),
                Size = new Size(DASHBOARD_W - 16, 140) };
            dash.Controls.Add(_netVis);
            y += 150;

            AddSep(dash, y); y += 10;

            // Settings
            dash.Controls.Add(MakeLabel("SETTINGS", "Impact", 12, Color.FromArgb(200,200,255),
                new Rectangle(12, y, DASHBOARD_W - 24, 22)));
            y += 26;

            AddNudRow(dash, ref y, "Population",  10, 500, NetParams.PopulationSize, 0, out _nudPop);
            AddNudRow(dash, ref y, "Mutation %",   1,  50, (decimal)(NetParams.MutationRate * 100), 0, out _nudMutRate);
            AddNudRow(dash, ref y, "Survive %",    5,  80, (decimal)(NetParams.SurviveRate * 100),  0, out _nudSurvive);

            dash.Controls.Add(MakeLabel("Net shape", "Courier New", 9, Color.FromArgb(160,160,200),
                new Rectangle(12, y + 2, 120, 18)));
            _tbShape = new TextBox { Location = new Point(138, y), Size = new Size(150, 22),
                Text = ShapeToString(NetParams.NetworkShape),
                BackColor = Color.FromArgb(35,35,55), ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle };
            dash.Controls.Add(_tbShape);
            y += 28;

            dash.Controls.Add(MakeLabel("e.g.  4,6,4,2  (inputs,hidden...,outputs)",
                "Courier New", 7, Color.FromArgb(100,100,140),
                new Rectangle(12, y, DASHBOARD_W - 24, 14)));
            y += 20;

            _btnApply = MakeDashButton("APPLY SETTINGS", Color.FromArgb(55,80,140),
                new Rectangle(12, y, DASHBOARD_W - 24, 28));
            _btnApply.Click += (s, e) => ApplySettings();
            dash.Controls.Add(_btnApply);
            y += 40;

            AddSep(dash, y); y += 10;

            // Control buttons
            _btnStartPause = MakeDashButton("▶  START", Color.FromArgb(40,140,40),
                new Rectangle(12, y, DASHBOARD_W - 24, 36));
            _btnStartPause.Click += (s, e) => ToggleRunning();
            dash.Controls.Add(_btnStartPause);
            y += 46;

            _btnReset = MakeDashButton("⟳  RESET", Color.FromArgb(100, 70, 20),
                new Rectangle(12, y, DASHBOARD_W - 24, 30));
            _btnReset.Click += (s, e) => ResetTraining();
            dash.Controls.Add(_btnReset);
            y += 40;

            // Save / Load side-by-side
            int hw = (DASHBOARD_W - 24 - 6) / 2;
            _btnSave = MakeDashButton("💾 SAVE", Color.FromArgb(35,80,110),
                new Rectangle(12, y, hw, 30));
            _btnSave.Click += (s, e) => SaveBestNetwork();
            dash.Controls.Add(_btnSave);

            _btnLoad = MakeDashButton("📂 LOAD", Color.FromArgb(55,90,45),
                new Rectangle(12 + hw + 6, y, hw, 30));
            _btnLoad.Click += (s, e) => LoadNetwork();
            dash.Controls.Add(_btnLoad);
            y += 40;

            _btnBack = MakeDashButton("← BACK TO MENU", Color.FromArgb(90,30,30),
                new Rectangle(12, y, DASHBOARD_W - 24, 30));
            _btnBack.Click += (s, e) => GoBack();
            dash.Controls.Add(_btnBack);
        }

        // ── Panel layout ─────────────────────────────────────────────────────────
        private void LayoutPanels(Panel dash)
        {
            int W = ClientSize.Width, H = ClientSize.Height;
            dash.Location = new Point(W - DASHBOARD_W, 0);
            dash.Size     = new Size(DASHBOARD_W, H);
            _canvas.Location = Point.Empty;
            _canvas.Size     = new Size(W - DASHBOARD_W, H);
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Level setup
        // ════════════════════════════════════════════════════════════════════════
        private void BuildPlatforms()
        {
            _platforms.Clear();
            foreach (var (px, py, pw, ph) in TRAIN_PLATFORMS)
                _platforms.Add(new Rectangle(px, py, pw, ph));
        }

        private void BuildCoins()
        {
            _coinRects = new Rectangle[TRAIN_COINS.Length];
            for (int i = 0; i < TRAIN_COINS.Length; i++)
                _coinRects[i] = new Rectangle(TRAIN_COINS[i].X - COIN_R, TRAIN_COINS[i].Y - COIN_R,
                    COIN_R * 2, COIN_R * 2);
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Training logic
        // ════════════════════════════════════════════════════════════════════════
        private void ResetTraining()
        {
            _simTimer.Stop();
            _running  = false;
            _cameraX  = 0;
            _bestEver = 0;
            _pop      = new Population(SPAWN);
            UpdateDashboard();
            _btnStartPause.Text = "▶  START";
            RenderScene();
        }

        private void ApplySettings()
        {
            int[] shape = ParseShape(_tbShape.Text);
            if (!IsShapeCompatible(shape))
            {
                MessageBox.Show(
                    $"Invalid network shape.\nThe first layer must be {NET_INPUTS} (inputs) and the last "
                    + $"at least {NET_OUTPUTS} (outputs).\nExample:  4,6,4,2",
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
            if (_running) { _simTimer.Start(); _btnStartPause.Text = "⏸  PAUSE"; }
            else          { _simTimer.Stop();  _btnStartPause.Text = "▶  RESUME"; }
        }

        private void SimTick(object sender, EventArgs e)
        {
            if (_pop == null) return;
            _simFrame++;                 // קצב אנימציית ההליכה של הסוכנים

            foreach (var agent in _pop.Agents)
            {
                if (!agent.IsAlive) continue;
                if (agent.Position.Y > 560) { agent.IsAlive = false; continue; }

                var inputs      = MarioAgent.ComputeInputs(
                    agent.Position, AGENT_W, AGENT_H, _platforms, _emptyRects, agent.IsGrounded);
                var (dir, jump) = agent.Think(inputs);
                agent.Step(dir, jump);
                ApplyPlatformCollisions(agent);
                CheckCoinCollections(agent);
            }

            // Camera follows the lead agent
            var leader = _pop.Agents.Where(a => a.IsAlive)
                                    .OrderByDescending(a => a.Position.X)
                                    .FirstOrDefault();
            if (leader != null)
                _cameraX = Math.Max(0, leader.Position.X - _canvas.Width / 3);

            // Evolve when all agents are dead
            if (_pop.AllDead)
            {
                int genBest = _pop.BestAgent()?.TotalFitness ?? 0;
                if (genBest > _bestEver) _bestEver = genBest;
                _pop.CreateNewGeneration();
                // Snap camera back to spawn so the first frame of the new generation
                // isn't drawn with the stale far-right scroll position.
                _cameraX = 0;
            }

            UpdateDashboard();
            RenderScene();
        }

        private static readonly List<Rectangle> _emptyRects = new List<Rectangle>();

        private void CheckCoinCollections(MarioAgent agent)
        {
            var ar = new Rectangle(agent.Position.X, agent.Position.Y, AGENT_W, AGENT_H);
            for (int ci = 0; ci < _coinRects.Length; ci++)
            {
                if (agent.CollectedCoins.Contains(ci)) continue;
                if (!ar.IntersectsWith(_coinRects[ci])) continue;
                agent.CollectedCoins.Add(ci);
                agent.Score += COIN_BONUS;
            }
        }

        private void ApplyPlatformCollisions(MarioAgent agent)
        {
            bool foundGround = false;
            var ar = new Rectangle(agent.Position.X, agent.Position.Y, AGENT_W, AGENT_H);

            foreach (var plat in _platforms)
            {
                if (!ar.IntersectsWith(plat)) continue;

                int ot  = ar.Bottom - plat.Top;
                int ob  = plat.Bottom - ar.Top;
                int ol  = ar.Right   - plat.Left;
                int orr = plat.Right - ar.Left;
                int min = Math.Min(Math.Min(ot, ob), Math.Min(ol, orr));

                if (min == ot)
                {
                    // Land on top of the platform. The original `ot < 20` threshold
                    // let fast-falling agents (gravity up to ~15.5/frame) phase through;
                    // dropping it preserves the original "snap-up on top" behavior even
                    // for deep overlaps.
                    agent.LandOn(plat.Top, AGENT_H);
                    foundGround = true;
                    ar = new Rectangle(agent.Position.X, agent.Position.Y, AGENT_W, AGENT_H);
                }
                else if (min == ob && agent.VerticalVelocity < 0) { agent.HitCeiling(plat.Bottom); }
                else if (min == ob && agent.VerticalVelocity >= 0)
                {
                    // Descended past the platform top in one frame – ground-snap up
                    // instead of silently falling through.
                    agent.LandOn(plat.Top, AGENT_H);
                    foundGround = true;
                    ar = new Rectangle(agent.Position.X, agent.Position.Y, AGENT_W, AGENT_H);
                }
                else if (min == ol || min == orr)
                {
                    agent.BlockHorizontal(min == ol ? plat.Left - AGENT_W : plat.Right);
                    ar = new Rectangle(agent.Position.X, agent.Position.Y, AGENT_W, AGENT_H);
                }
            }

            if (!foundGround) agent.LeaveGround();
        }

        private void UpdateDashboard()
        {
            if (_pop == null) return;
            int alive   = _pop.AliveCount;
            int total   = _pop.Agents.Count;
            int genBest = total == 0 ? 0 : _pop.Agents.Max(a => a.TotalFitness);

            _lblGen.Text      = _pop.Generation.ToString();
            _lblAlive.Text    = $"{alive} / {total}";
            _lblBest.Text     = genBest.ToString();
            _lblBestEver.Text = _bestEver.ToString();

            var best = _pop.BestAgent();
            if (best != null)
            {
                var inputs = MarioAgent.ComputeInputs(
                    best.Position, AGENT_W, AGENT_H, _platforms, _emptyRects, best.IsGrounded);
                _netVis.SetNetwork(best.Brain, inputs);
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Save / Load
        // ════════════════════════════════════════════════════════════════════════
        private void SaveBestNetwork()
        {
            var best = _pop?.BestAgent();
            if (best == null) return;

            using (var dlg = new SaveFileDialog
            {
                Title      = "Save Best Luigi Network",
                Filter     = "Luigi Network|*.smnet|All files|*.*",
                DefaultExt = "smnet",
                FileName   = $"luigi_gen{_pop.Generation}_fit{best.TotalFitness}.smnet",
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    NetworkSerializer.Save(best.Brain, _pop.Generation, best.TotalFitness, dlg.FileName);
                    MessageBox.Show($"Saved gen {_pop.Generation}, fitness {best.TotalFitness}.",
                        "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Save failed:\n" + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadNetwork()
        {
            using (var dlg = new OpenFileDialog
            {
                Title  = "Load Luigi Network",
                Filter = "Luigi Network|*.smnet|All files|*.*",
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    var (net, generation, fitness) = NetworkSerializer.Load(dlg.FileName);

                    if (!IsShapeCompatible(net.Shape))
                    {
                        MessageBox.Show(
                            $"Loaded network is incompatible with this trainer.\nIt must have {NET_INPUTS} "
                            + $"inputs and at least {NET_OUTPUTS} outputs (shape was {ShapeToString(net.Shape)}).",
                            "Load failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Update network shape UI to match the loaded file
                    NetParams.NetworkShape = net.Shape;
                    _tbShape.Text = ShapeToString(net.Shape);

                    // Restart training with loaded brain injected as the elite agent
                    _simTimer.Stop();
                    _running = false;
                    _cameraX = 0;
                    _pop     = new Population(SPAWN);
                    _pop.Generation = generation;

                    // Replace first agent with the loaded elite brain
                    _pop.Agents[0] = new ML.MarioAgent(net, SPAWN);

                    UpdateDashboard();
                    _btnStartPause.Text = "▶  RESUME";
                    RenderScene();

                    MessageBox.Show(
                        $"Loaded gen {generation}, fitness {fitness}.\nPress RESUME to continue training.",
                        "Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Load failed:\n" + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  תצוגת הסצנה כתמונות / Scene rendering with PictureBoxes
        // ════════════════════════════════════════════════════════════════════════
        // נוצרים פעם אחת: PictureBox לכל פלטפורמה (אריח לבנה) ולכל מטבע.
        // Created once: one PictureBox per platform (brick tile) and per coin.
        private void BuildLevelViews()
        {
            foreach (var plat in _platforms)
            {
                var pb = new PictureBox
                {
                    BackColor = Color.FromArgb(185, 100, 40),   // גיבוי אם אין תמונה
                    Size      = new Size(plat.Width, plat.Height),
                    Location  = new Point(plat.X - _cameraX, plat.Y),
                };
                if (Sprites.Brick != null)
                {
                    pb.BackgroundImage       = Sprites.Brick;
                    pb.BackgroundImageLayout = ImageLayout.Tile;
                }
                _canvas.Controls.Add(pb);
                _platformViews.Add(pb);
            }

            _coinViews = new PictureBox[_coinRects.Length];
            for (int i = 0; i < _coinRects.Length; i++)
            {
                var cr = _coinRects[i];
                var pb = new PictureBox
                {
                    BackColor = Color.Transparent,
                    SizeMode  = PictureBoxSizeMode.StretchImage,
                    Size      = new Size(cr.Width, cr.Height),
                    Location  = new Point(cr.X - _cameraX, cr.Y),
                    Image     = Sprites.Coin != null ? Sprites.Coin[0] : null,
                };
                _canvas.Controls.Add(pb);
                _coinViews[i] = pb;
            }
        }

        // מגדיל את מאגר ה-PictureBox של הסוכנים לפי הצורך (יצירה חד-פעמית).
        // Grow the agent PictureBox pool on demand (created once, never per frame).
        private void EnsureAgentPool(int needed)
        {
            while (_agentPool.Count < needed)
            {
                var pb = new PictureBox
                {
                    BackColor = Color.Transparent,
                    SizeMode  = PictureBoxSizeMode.StretchImage,
                    Size      = new Size(AGENT_W, AGENT_H),
                    Visible   = false,
                };
                _canvas.Controls.Add(pb);
                pb.BringToFront();            // סוכנים מעל פלטפורמות/מטבעות
                _agentPool.Add(pb);
            }
        }

        // ממקם מחדש את כל התמונות לפי המצלמה ומצב הסוכנים. נקרא כל פריים במקום ציור.
        // Repositions every view from the camera & agent state. Called each frame
        // instead of painting. Off-screen views are hidden so they aren't drawn.
        private void RenderScene()
        {
            int W = _canvas.Width;

            for (int i = 0; i < _platformViews.Count; i++)
                _platformViews[i].Left = _platforms[i].X - _cameraX;

            if (_coinViews != null)
                for (int i = 0; i < _coinViews.Length; i++)
                {
                    int sx = _coinRects[i].X - _cameraX;
                    _coinViews[i].Left    = sx;
                    _coinViews[i].Visible = sx + _coinRects[i].Width >= 0 && sx <= W;
                }

            if (_pop == null) return;

            EnsureAgentPool(_pop.Agents.Count);
            var best     = _pop.BestAgent();
            int frame    = (_simFrame / 6) % 2;                       // אנימציית הליכה
            var luigiImg = Sprites.LuigiWalk != null ? Sprites.LuigiWalk[frame] : null;

            for (int i = 0; i < _agentPool.Count; i++)
            {
                var pb = _agentPool[i];
                if (i >= _pop.Agents.Count) { pb.Visible = false; continue; }

                var agent = _pop.Agents[i];
                int sx    = agent.Position.X - _cameraX;
                if (!agent.IsAlive || sx + AGENT_W < 0 || sx > W) { pb.Visible = false; continue; }

                pb.Image    = luigiImg;
                // המוביל מודגש בהילה זהובה מאחוריו / leader highlighted with a gold halo
                pb.BackColor = agent == best ? Color.FromArgb(90, 255, 205, 0) : Color.Transparent;
                pb.Location  = new Point(sx, agent.Position.Y);
                pb.Visible   = true;
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Navigation
        // ════════════════════════════════════════════════════════════════════════
        private void GoBack()
        {
            _simTimer.Stop();
            // Closing this form triggers the FormClosed handler the launching
            // MainMenuForm attached, which re-shows the original (hidden) menu.
            Close();
        }

        // ════════════════════════════════════════════════════════════════════════
        //  UI helpers
        // ════════════════════════════════════════════════════════════════════════
        private void AddSep(Panel dash, int y)
        {
            dash.Controls.Add(new Panel
            {
                BackColor = Color.FromArgb(50, 50, 70),
                Location  = new Point(10, y),
                Size      = new Size(DASHBOARD_W - 20, 2),
            });
        }

        private void AddStatRow(Panel dash, ref int y, string caption, Color valColor, out Label valLbl)
        {
            dash.Controls.Add(MakeLabel(caption, "Courier New", 9, Color.FromArgb(140,180,140),
                new Rectangle(12, y, 148, 18)));
            valLbl = MakeLabel("0", "Courier New", 13, valColor, new Rectangle(162, y - 1, 140, 20));
            dash.Controls.Add(valLbl);
            y += 26;
        }

        private void AddNudRow(Panel dash, ref int y, string caption,
            decimal min, decimal max, decimal val, int decimals, out NumericUpDown nud)
        {
            dash.Controls.Add(MakeLabel(caption, "Courier New", 9, Color.FromArgb(160,160,200),
                new Rectangle(12, y + 2, 120, 18)));
            nud = new NumericUpDown
            {
                Location = new Point(138, y), Size = new Size(80, 22),
                Minimum = min, Maximum = max, Value = val, DecimalPlaces = decimals,
                BackColor = Color.FromArgb(35,35,55), ForeColor = Color.White,
            };
            dash.Controls.Add(nud);
            y += 28;
        }

        private static Label MakeLabel(string text, string font, float size, Color color,
            Rectangle bounds, bool centered = false)
        {
            return new Label
            {
                Text      = text,
                Font      = new Font(font, size, FontStyle.Bold),
                ForeColor = color,
                BackColor = Color.Transparent,
                Location  = new Point(bounds.X, bounds.Y),
                Size      = new Size(bounds.Width, bounds.Height),
                TextAlign = centered ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft,
            };
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

        private static bool IsShapeCompatible(int[] shape)
            => shape != null && shape.Length >= 2
               && shape[0] == NET_INPUTS && shape[shape.Length - 1] >= NET_OUTPUTS;

        private static string ShapeToString(int[] shape) => string.Join(",", shape);

        private static int[] ParseShape(string s)
        {
            try
            {
                var parts  = s.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var result = new int[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                    result[i] = int.Parse(parts[i].Trim());
                return result;
            }
            catch { return null; }
        }
    }
}
