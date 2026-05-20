namespace supermario.ML
{
    public class Layer
    {
        public int     NumInputs  { get; }
        public Neuron[] Neurons   { get; }

        public Layer(int numNeurons, int numInputs)
        {
            NumInputs = numInputs;
            Neurons   = new Neuron[numNeurons];
            for (int i = 0; i < numNeurons; i++)
                Neurons[i] = new Neuron(numInputs);
        }

        public double[] Forward(double[] inputs)
        {
            var outputs = new double[Neurons.Length];
            for (int i = 0; i < Neurons.Length; i++)
                outputs[i] = Neurons[i].Forward(inputs);
            return outputs;
        }

        public static Layer CrossOver(Layer a, Layer b, double tilt)
        {
            var child = new Layer(a.Neurons.Length, a.NumInputs);
            for (int i = 0; i < a.Neurons.Length; i++)
                child.Neurons[i] = Neuron.CrossOver(a.Neurons[i], b.Neurons[i], tilt);
            return child;
        }

        public Layer Clone()
        {
            var c = new Layer(Neurons.Length, NumInputs);
            for (int i = 0; i < Neurons.Length; i++)
                c.Neurons[i] = Neurons[i].Clone();
            return c;
        }
    }
}
