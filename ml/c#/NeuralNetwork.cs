using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlappyBirdCSharp.ANN
{
    [Serializable]
    public class NeuralNetwork
    {
        // Defines the shape of the neural network (number of neurons per layer)
        public int[] networkShape   { get; set; }

        // Array of layers in the neural network
        public Layer[] Layers { get; set; }

        /// <summary>
        /// Initializes a new instance of the NeuralNetwork class with the specified network shape.
        /// Each layer (except the input layer) is initialized with the given number of neurons and inputs.
        /// </summary>
        /// <param name="networkShape">An array representing the number of neurons in each layer.</param>
        public NeuralNetwork(int[] networkShape)
        {
            this.networkShape = networkShape;
            this.Layers = new Layer[networkShape.Length];
            for (int i = 1; i < Layers.Length; i++)
            {
                Layers[i] = new Layer(networkShape[i],networkShape[i-1]);
            }
        }

        /// <summary>
        /// Feeds the input through the network and returns the output.
        /// Each layer processes the input and passes its output to the next layer.
        /// </summary>
        /// <param name="inputs">Input values for the network.</param>
        /// <returns>Output values from the network.</returns>
        public double[] Forward(double[] inputs)
        {
            double[] tmp;
            for (int i = 1; i < Layers.Length; i++)
            {
                inputs = Layers[i].Forward(inputs);                
            }            
            return inputs;
        }

        /// <summary>
        /// Creates a new NeuralNetwork by crossing over two parent networks.
        /// Each layer is crossed over using the specified tilt parameter.
        /// </summary>
        /// <param name="parentA">First parent neural network.</param>
        /// <param name="parentB">Second parent neural network.</param>
        /// <param name="tilt">Tilt parameter for crossover.</param>
        /// <returns>A new NeuralNetwork instance resulting from the crossover.</returns>
        public NeuralNetwork CrossOver(NeuralNetwork parentA,NeuralNetwork parentB, double tilt)
        {
            NeuralNetwork result = new NeuralNetwork(networkShape);
            for (int i = 1; i < Layers.Length; i++)
            {
                result.Layers[i] = result.Layers[i].CrossOver(parentA.Layers[i],parentB.Layers[i],tilt);
            }
            return result;
        }
    }
}
