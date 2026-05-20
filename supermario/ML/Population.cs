using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace supermario.ML
{
    public class Population
    {
        public List<MarioAgent> Agents     { get; private set; }
        public int              Generation { get; set; }

        private readonly Point _startPos;

        public Population(Point startPos)
        {
            _startPos = startPos;
            Agents    = new List<MarioAgent>(NetParams.PopulationSize);
            for (int i = 0; i < NetParams.PopulationSize; i++)
                Agents.Add(new MarioAgent(new NeuralNetwork(NetParams.NetworkShape), startPos));
        }

        public int AliveCount => Agents.Count(a => a.IsAlive);

        public bool AllDead => Agents.All(a => !a.IsAlive);

        // Returns the top survivors sorted by fitness descending
        public List<MarioAgent> GetBestAgents()
        {
            int keep = Math.Max(2, (int)(NetParams.PopulationSize * NetParams.SurviveRate));
            return Agents.OrderByDescending(a => a.TotalFitness).Take(keep).ToList();
        }

        public MarioAgent BestAgent()
            => Agents.OrderByDescending(a => a.TotalFitness).FirstOrDefault();

        public void CreateNewGeneration()
        {
            var survivors = GetBestAgents();
            var next      = new List<MarioAgent>(NetParams.PopulationSize);

            // Degenerate case: no agents yet — fall back to a fresh random population.
            if (survivors.Count == 0)
            {
                for (int i = 0; i < NetParams.PopulationSize; i++)
                    next.Add(new MarioAgent(new NeuralNetwork(NetParams.NetworkShape), _startPos));
                Agents = next;
                Generation++;
                return;
            }

            // Elitism: keep the single best agent unchanged
            next.Add(new MarioAgent(survivors[0].Brain.Clone(), _startPos));

            while (next.Count < NetParams.PopulationSize)
            {
                // Pick two distinct parents
                int ai = NetParams.randomNum.Next(survivors.Count);
                int bi;
                do { bi = NetParams.randomNum.Next(survivors.Count); } while (bi == ai && survivors.Count > 1);

                double tilt = NetParams.randomNum.NextDouble() * 0.6 + 0.2;   // 0.2 – 0.8
                var brain   = NeuralNetwork.CrossOver(survivors[ai].Brain, survivors[bi].Brain, tilt);

                // Mutate each weight/bias according to MutationRate
                MutateNetwork(brain);

                next.Add(new MarioAgent(brain, _startPos));
            }

            Agents = next;
            Generation++;
        }

        private static void MutateNetwork(NeuralNetwork net)
        {
            for (int li = 1; li < net.Shape.Length; li++)
            {
                var layer = net.GetLayer(li);
                foreach (var neuron in layer.Neurons)
                    neuron.Mutate();
            }
        }
    }
}
