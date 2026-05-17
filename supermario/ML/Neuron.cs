namespace supermario.ML
{
    public class Neuron
    {
        public double[] Weights { get; private set; }
        public double   Bias    { get; set; }
        public double   Output  { get; private set; }

        public Neuron(int numInputs)
        {
            Weights = new double[numInputs];
            // Shared RNG avoids identical seeds when neurons are created in a tight loop
            Bias = NetParams.randomNum.NextDouble() * 2 - 1;
            InitRandomWeights();
        }

        private void InitRandomWeights()
        {
            for (int i = 0; i < Weights.Length; i++)
                Weights[i] = NetParams.randomNum.NextDouble() * 2 - 1;  // [-1, 1]
        }

        public double Forward(double[] inputs)
        {
            double sum = Bias;  // bias is added here (was missing before)
            for (int i = 0; i < Weights.Length; i++)
                sum += inputs[i] * Weights[i];
            Output = NetParams.Tanh(sum);
            return Output;
        }

        public void Mutate()
        {
            for (int i = 0; i < Weights.Length; i++)
            {
                if (NetParams.randomNum.NextDouble() < NetParams.MutationRate)
                    Weights[i] = NetParams.randomNum.NextDouble() * 2 - 1;
            }
            if (NetParams.randomNum.NextDouble() < NetParams.MutationRate)
                Bias = NetParams.randomNum.NextDouble() * 2 - 1;
        }

        public static Neuron CrossOver(Neuron a, Neuron b, double tilt)
        {
            var child = new Neuron(a.Weights.Length);
            for (int i = 0; i < a.Weights.Length; i++)
                child.Weights[i] = (NetParams.randomNum.NextDouble() < tilt) ? a.Weights[i] : b.Weights[i];
            child.Bias = (NetParams.randomNum.NextDouble() < tilt) ? a.Bias : b.Bias;
            return child;
        }

        public Neuron Clone()
        {
            var c = new Neuron(Weights.Length);
            System.Array.Copy(Weights, c.Weights, Weights.Length);
            c.Bias = Bias;
            return c;
        }
    }
}
