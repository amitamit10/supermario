using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace supermario.ML
{
    // Simple line-based format — no external JSON library needed.
    //
    // File layout:
    //   SMNET1
    //   generation=<int>
    //   fitness=<int>
    //   shape=<n0>,<n1>,...
    //   <layerIdx>,<neuronIdx>,<bias>,<w0>,<w1>,...
    //   (one line per neuron, starting at layer 1)
    public static class NetworkSerializer
    {
        private static readonly CultureInfo INV = CultureInfo.InvariantCulture;

        public static void Save(NeuralNetwork net, int generation, int fitness, string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SMNET1");
            sb.AppendLine("generation=" + generation);
            sb.AppendLine("fitness=" + fitness);
            sb.AppendLine("shape=" + string.Join(",", net.Shape));

            for (int li = 1; li < net.Shape.Length; li++)
            {
                var layer = net.GetLayer(li);
                for (int ni = 0; ni < layer.Neurons.Length; ni++)
                {
                    var n = layer.Neurons[ni];
                    sb.Append(li).Append(',').Append(ni).Append(',')
                      .Append(n.Bias.ToString("R", INV));
                    foreach (double w in n.Weights)
                        sb.Append(',').Append(w.ToString("R", INV));
                    sb.AppendLine();
                }
            }

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        // Returns null net on failure.
        public static (NeuralNetwork net, int generation, int fitness) Load(string path)
        {
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            if (lines.Length < 4 || lines[0].Trim() != "SMNET1")
                throw new InvalidDataException("Not a valid SMNET1 file.");

            int generation = 0, fitness = 0;
            int[] shape = null;

            foreach (string line in lines)
            {
                if (line.StartsWith("generation="))
                    int.TryParse(line.Substring(11), out generation);
                else if (line.StartsWith("fitness="))
                    int.TryParse(line.Substring(8), out fitness);
                else if (line.StartsWith("shape="))
                    shape = ParseInts(line.Substring(6));
            }

            if (shape == null || shape.Length < 2)
                throw new InvalidDataException("Missing or invalid shape.");

            var net = new NeuralNetwork(shape);

            foreach (string line in lines)
            {
                if (line.Length == 0 || !char.IsDigit(line[0])) continue;
                var parts = line.Split(',');
                if (parts.Length < 3) continue;

                int li = int.Parse(parts[0], INV);
                int ni = int.Parse(parts[1], INV);
                if (li < 1 || li >= shape.Length) continue;

                var layer = net.GetLayer(li);
                if (ni < 0 || ni >= layer.Neurons.Length) continue;
                var neuron = layer.Neurons[ni];

                neuron.Bias = double.Parse(parts[2], INV);
                for (int wi = 0; wi < neuron.Weights.Length && wi + 3 < parts.Length; wi++)
                    neuron.Weights[wi] = double.Parse(parts[wi + 3], INV);
            }

            return (net, generation, fitness);
        }

        private static int[] ParseInts(string s)
        {
            try
            {
                var parts = s.Split(',');
                var result = new int[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                    result[i] = int.Parse(parts[i].Trim(), INV);
                return result;
            }
            catch { return null; }
        }
    }
}
