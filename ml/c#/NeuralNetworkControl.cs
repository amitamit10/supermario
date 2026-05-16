using FlappyBirdCSharp.ANN;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlappyBirdCSharp.UCUtils
{

    public partial class NeuralNetworkControl : UserControl
    {
        private NeuralNetwork _network;
        private List<PointF[]> _layerPositions;
        private double[][] _activations;   // neuron outputs

        private readonly int _neuronRadius = 18;
        private readonly int _hSpacing = 120;
        private readonly int _vSpacing = 60;

        public NeuralNetworkControl()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            _layerPositions = new List<PointF[]>();
        }

        public void SetNetwork(NeuralNetwork network, double[] inputs)
        {
            _network = network;
            CalculatePositions();

            if (inputs != null)
                _activations = ComputeActivations(inputs);

            Invalidate(true);
        }

        private void CalculatePositions()
        {
            _layerPositions.Clear();
            if (_network == null) return;

            int[] shape = _network.networkShape;
            int x = 60;

            foreach (int neuronCount in shape)
            {
                PointF[] layer = new PointF[neuronCount];
                int totalHeight = (neuronCount - 1) * _vSpacing;
                int yStart = (Height / 2) - (totalHeight / 2);

                for (int i = 0; i < neuronCount; i++)
                {
                    layer[i] = new PointF(x, yStart + i * _vSpacing);
                }

                _layerPositions.Add(layer);
                x += _hSpacing;
            }
        }

        private double[][] ComputeActivations(double[] inputs)
        {
            var values = new List<double[]>();
            values.Add(inputs);

            for (int l = 1; l < _network.Layers.Length; l++)
            {
                inputs = _network.Layers[l].Forward(inputs);
                values.Add(inputs);
            }

            return values.ToArray();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_network == null || _layerPositions.Count == 0)
                return;

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (var font = new Font("Arial", 8))
            using (var textBrush = new SolidBrush(Color.Black))
            {
                // Draw connections + weight values
                for (int l = 1; l < _network.Layers.Length; l++)
                {
                    var layer = _network.Layers[l];
                    var fromPositions = _layerPositions[l - 1];
                    var toPositions = _layerPositions[l];

                    for (int j = 0; j < layer.Neurons.Length; j++)
                    {
                        var neuron = layer.Neurons[j];
                        for (int i = 0; i < neuron.Weights.Length; i++)
                        {
                            double weight = neuron.Weights[i];
                            Color c = WeightToColor(weight);
                            using (var pen = new Pen(c, 1))
                            {
                                g.DrawLine(pen, fromPositions[i], toPositions[j]);
                            }

                            // Draw weight text at midpoint
                            float midX = (fromPositions[i].X + toPositions[j].X) / 2;
                            float midY = (fromPositions[i].Y + toPositions[j].Y) / 2;
                            g.DrawString(weight.ToString("0.00"), font, textBrush, midX, midY);
                        }
                    }
                }

                // Draw neurons + activation values
                for (int l = 0; l < _layerPositions.Count; l++)
                {
                    for (int i = 0; i < _layerPositions[l].Length; i++)
                    {
                        double value = 0;
                         if (_activations != null && l < _activations.Length && i < _activations[l].Length)
                            value = _activations[l][i];

                        DrawNeuron(g, _layerPositions[l][i], value);

                        // Draw neuron value in the center
                        string text = value.ToString("0.00");
                        SizeF textSize = g.MeasureString(text, font);
                        g.DrawString(text, font, textBrush,
                            _layerPositions[l][i].X - textSize.Width / 2,
                            _layerPositions[l][i].Y - textSize.Height / 2);
                    }
                }
            }
        }

        private void DrawNeuron(Graphics g, PointF center, double value)
        {
            RectangleF rect = new RectangleF(
                center.X - _neuronRadius,
                center.Y - _neuronRadius,
                _neuronRadius * 2,
                _neuronRadius * 2);

            Color c = NeuronToColor(value);
            using (var brush = new SolidBrush(c))
            using (var pen = new Pen(Color.Black, 1))
            {
                g.FillEllipse(brush, rect);
                g.DrawEllipse(pen, rect);
            }
        }

        private Color NeuronToColor(double value)
        {
            // tanh outputs [-1, 1]
            value = Math.Max(-1, Math.Min(1, value));
            double norm = (value + 1) / 2.0; // normalize to [0,1]

            int r = 255;
            int g = 255;
            int b = (int)(255 * (1 - norm));
            return Color.FromArgb(r, g, b);
        }

        private Color WeightToColor(double weight)
        {
            double v = Math.Min(1, Math.Abs(weight));
            int r = (int)(255 * (1 - v));
            int g = (int)(255 * (1 - v));
            int b = 255;
            return Color.FromArgb(r, g, b);
        }
    }
   
}

