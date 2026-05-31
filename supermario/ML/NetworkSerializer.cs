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
                int wi;
                for (wi = 0; wi < neuron.Weights.Length && wi + 3 < parts.Length; wi++)
                    neuron.Weights[wi] = double.Parse(parts[wi + 3], INV);
                for (; wi < neuron.Weights.Length; wi++)
                    neuron.Weights[wi] = 0.0;
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

        // ── JSON save / load (simple manual format, no external library) ──────────

        public static void SaveJson(NeuralNetwork net, int generation, int fitness, string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"generation\": " + generation + ",");
            sb.AppendLine("  \"fitness\": " + fitness + ",");
            sb.AppendLine("  \"shape\": [" + string.Join(", ", net.Shape) + "],");
            sb.AppendLine("  \"neurons\": [");

            bool firstNeuron = true;
            for (int li = 1; li < net.Shape.Length; li++)
            {
                var layer = net.GetLayer(li);
                for (int ni = 0; ni < layer.Neurons.Length; ni++)
                {
                    var n = layer.Neurons[ni];
                    if (!firstNeuron) sb.AppendLine(",");
                    firstNeuron = false;

                    var weights = string.Join(", ", Array.ConvertAll(n.Weights, w => w.ToString("R", INV)));
                    sb.Append("    {\"layer\": " + li + ", \"neuron\": " + ni
                        + ", \"bias\": " + n.Bias.ToString("R", INV)
                        + ", \"weights\": [" + weights + "]}");
                }
            }
            sb.AppendLine();
            sb.AppendLine("  ]");
            sb.AppendLine("}");

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        public static (NeuralNetwork net, int generation, int fitness) LoadJson(string path)
        {
            string text = File.ReadAllText(path, Encoding.UTF8);

            int generation = ParseJsonInt(text, "generation");
            int fitness    = ParseJsonInt(text, "fitness");
            int[] shape    = ParseJsonShape(text);

            if (shape == null || shape.Length < 2)
                throw new InvalidDataException("Missing or invalid shape in JSON.");

            var net = new NeuralNetwork(shape);

            int neuronsStart = text.IndexOf("\"neurons\"", StringComparison.Ordinal);
            if (neuronsStart < 0) throw new InvalidDataException("No neurons array in JSON.");
            int arrayStart = text.IndexOf('[', neuronsStart);
            int arrayEnd   = text.LastIndexOf(']');
            string neuronsBlock = text.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);

            var neuronObjects = neuronsBlock.Split(new[] { "},{" }, StringSplitOptions.None);
            foreach (var obj in neuronObjects)
            {
                int li = ParseJsonInt(obj, "layer");
                int ni = ParseJsonInt(obj, "neuron");
                if (li < 1 || li >= shape.Length) continue;

                var layer = net.GetLayer(li);
                if (ni < 0 || ni >= layer.Neurons.Length) continue;
                var neuron = layer.Neurons[ni];

                neuron.Bias = ParseJsonDouble(obj, "bias");

                int wStart = obj.IndexOf("\"weights\"", StringComparison.Ordinal);
                if (wStart < 0) continue;
                int wArr = obj.IndexOf('[', wStart);
                int wEnd = obj.IndexOf(']', wArr);
                string wBlock = obj.Substring(wArr + 1, wEnd - wArr - 1).Trim();
                if (wBlock.Length == 0) continue;

                var wParts = wBlock.Split(',');
                for (int wi = 0; wi < neuron.Weights.Length && wi < wParts.Length; wi++)
                    neuron.Weights[wi] = double.Parse(wParts[wi].Trim(), INV);
            }

            return (net, generation, fitness);
        }

        private static int ParseJsonInt(string json, string key)
        {
            string search = "\"" + key + "\"";
            int idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return 0;
            int colon = json.IndexOf(':', idx + search.Length);
            int start = colon + 1;
            while (start < json.Length && (json[start] == ' ' || json[start] == '\t')) start++;
            int end = start;
            while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '-')) end++;
            return end > start ? int.Parse(json.Substring(start, end - start), INV) : 0;
        }

        private static double ParseJsonDouble(string json, string key)
        {
            string search = "\"" + key + "\"";
            int idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return 0.0;
            int colon = json.IndexOf(':', idx + search.Length);
            int start = colon + 1;
            while (start < json.Length && (json[start] == ' ' || json[start] == '\t')) start++;
            int end = start;
            while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '-' || json[end] == '.' || json[end] == 'E' || json[end] == 'e' || json[end] == '+')) end++;
            return end > start ? double.Parse(json.Substring(start, end - start), INV) : 0.0;
        }

        private static int[] ParseJsonShape(string json)
        {
            int idx = json.IndexOf("\"shape\"", StringComparison.Ordinal);
            if (idx < 0) return null;
            int arrStart = json.IndexOf('[', idx);
            int arrEnd   = json.IndexOf(']', arrStart);
            if (arrStart < 0 || arrEnd < 0) return null;
            return ParseInts(json.Substring(arrStart + 1, arrEnd - arrStart - 1));
        }
    }
}
