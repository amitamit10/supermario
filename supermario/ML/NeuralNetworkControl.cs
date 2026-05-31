using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace supermario.ML
{
    // ════════════════════════════════════════════════════════════════════════
    //  ויזואליזציה של הרשת הנוירונית / Neural-network visualiser
    // ------------------------------------------------------------------------
    //  זהו החריג היחיד למודל "כל אובייקט הוא תמונה" של המשחק: כאן מציירים
    //  *תרשים נתונים חי* — קווי משקלים וצמתים שצבעם משתנה בכל פריים לפי
    //  ערכי ההפעלה (activations) והמשקלים של הרשת הטובה ביותר. אי-אפשר
    //  להחליף תרשים דינמי כזה בתמונת PNG סטטית, ולכן הוא נשאר GDI+.
    //
    //  This is the ONLY exception to the game's "every object is an image"
    //  rule: it renders a *live data chart* — weight lines and nodes whose
    //  colours change every frame from the best network's activations and
    //  weights. A dynamic chart like this cannot be a static PNG, so it stays
    //  GDI+ on purpose.
    // ════════════════════════════════════════════════════════════════════════
    // Read-only visualiser: draws the network topology with activation colours.
    public class NeuralNetworkControl : UserControl
    {
        private NeuralNetwork _net;
        private double[]      _inputs;

        public NeuralNetworkControl()
        {
            DoubleBuffered = true;
            BackColor      = Color.FromArgb(20, 20, 35);
        }

        public void SetNetwork(NeuralNetwork net, double[] inputs = null)
        {
            _net    = net;
            _inputs = inputs;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_net == null) return;

            var g  = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int W  = Width, H = Height;
            int[] shape = _net.Shape;
            int numCols  = shape.Length;
            int colW     = W / numCols;
            int maxN     = 0;
            foreach (int n in shape) if (n > maxN) maxN = n;
            int nodeR = Math.Max(4, Math.Min(12, (H / (maxN + 2)) / 2));

            // Pre-run forward pass so we can colour activations
            double[][] activations = new double[numCols][];
            activations[0] = _inputs ?? new double[shape[0]];
            for (int li = 1; li < numCols; li++)
            {
                var layer = _net.GetLayer(li);
                var prev  = activations[li - 1];
                activations[li] = new double[shape[li]];
                for (int ni = 0; ni < shape[li]; ni++)
                    activations[li][ni] = layer.Neurons[ni].Forward(prev);
            }

            // Compute pixel centres
            var centres = new PointF[numCols][];
            for (int col = 0; col < numCols; col++)
            {
                centres[col] = new PointF[shape[col]];
                float cx = colW * col + colW / 2f;
                float rowH = (float)H / (shape[col] + 1);
                for (int row = 0; row < shape[col]; row++)
                    centres[col][row] = new PointF(cx, rowH * (row + 1));
            }

            // Draw weights (lines)
            for (int col = 1; col < numCols; col++)
            {
                var layer = _net.GetLayer(col);
                for (int ni = 0; ni < shape[col]; ni++)
                {
                    for (int pi = 0; pi < shape[col - 1]; pi++)
                    {
                        double w = layer.Neurons[ni].Weights.Length > pi
                            ? layer.Neurons[ni].Weights[pi] : 0;
                        int alpha = (int)(Math.Abs(w) * 120);
                        alpha = Math.Min(200, Math.Max(20, alpha));
                        Color lc = w >= 0
                            ? Color.FromArgb(alpha, 80, 140, 255)
                            : Color.FromArgb(alpha, 255, 80, 80);
                        using (var pen = new Pen(lc, 1f))
                            g.DrawLine(pen, centres[col - 1][pi], centres[col][ni]);
                    }
                }
            }

            // Draw nodes
            for (int col = 0; col < numCols; col++)
            {
                for (int row = 0; row < shape[col]; row++)
                {
                    double act = activations[col].Length > row ? activations[col][row] : 0;
                    // tanh output is in [-1,1]; map to brightness
                    int brightness = (int)((act + 1.0) / 2.0 * 255);
                    brightness = Math.Max(30, Math.Min(255, brightness));
                    var nodeColor = Color.FromArgb(brightness, brightness, 80);   // yellow→dark
                    var pt  = centres[col][row];
                    var rct = new RectangleF(pt.X - nodeR, pt.Y - nodeR, nodeR * 2, nodeR * 2);
                    using (var b = new SolidBrush(nodeColor))
                        g.FillEllipse(b, rct);
                    using (var p = new Pen(Color.FromArgb(180, 180, 180), 1f))
                        g.DrawEllipse(p, rct);
                }
            }
        }
    }
}
