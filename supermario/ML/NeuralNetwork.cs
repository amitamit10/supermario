namespace supermario.ML
{
    public class NeuralNetwork
    {
        // layers[0] = null (input layer — no neurons, just values passed through)
        private Layer[] layers;

        public int[] Shape { get; }

        public NeuralNetwork(int[] networkShape)
        {
            Shape  = networkShape;
            layers = new Layer[networkShape.Length];
            // Start at index 1; index 0 is the input layer (no Layer object needed)
            for (int i = 1; i < networkShape.Length; i++)
                layers[i] = new Layer(networkShape[i], networkShape[i - 1]);
        }

        public double[] Forward(double[] inputs)
        {
            double[] current = inputs;
            for (int i = 1; i < layers.Length; i++)
                current = layers[i].Forward(current);
            return current;
        }

        public static NeuralNetwork CrossOver(NeuralNetwork a, NeuralNetwork b, double tilt)
        {
            var child = new NeuralNetwork(a.Shape);
            for (int i = 1; i < a.layers.Length; i++)
                child.layers[i] = Layer.CrossOver(a.layers[i], b.layers[i], tilt);
            return child;
        }

        public NeuralNetwork Clone()
        {
            var c = new NeuralNetwork(Shape);
            for (int i = 1; i < layers.Length; i++)
                c.layers[i] = layers[i].Clone();
            return c;
        }

        public Layer GetLayer(int index) => layers[index];
    }
}
