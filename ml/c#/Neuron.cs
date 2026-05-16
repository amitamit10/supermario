using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlappyBirdCSharp.ANN
{
    [Serializable]
    /// <summary>
    /// Represents a single neuron in a neural network, with weights and a bias.
    /// </summary>
    public class Neuron
    {
        Random rnd = new Random();

        /// <summary>
        /// The bias value for this neuron.
        /// </summary>
        public double Bias { get; set; }

        /// <summary>
        /// The weights for the neuron's inputs.
        /// </summary>
        public double[] Weights { get; set; } 

        public Neuron() { }

        /// <summary>
        /// Initializes a neuron with a specified number of weights, randomly assigned.
        /// </summary>
        public Neuron(int numOfWeights)
        {
            Weights = new double[numOfWeights];
            initRandomWeights();
        }

        /// <summary>
        /// Initializes a neuron with given weights and bias.
        /// </summary>
        public Neuron(double [] weights,double bais)
        {
            Weights = new double[weights.Length];
            Array.Copy(weights, Weights, weights.Length);
            this.Bias = bais;
        }

        /// <summary>
        /// Initializes the neuron's weights with random values.
        /// </summary>
        private void initRandomWeights()
        {
            for (int i = 0; i< Weights.Length; i++)
            {
                Weights[i] = rnd.NextDouble();
            }
        }

        /// <summary>
        /// Calculates the neuron's output by applying the activation function to the weighted sum of inputs.
        /// </summary>
        public double Forward(double[] inputs)
        {
            double sum = 0;
            for (int i = 0; i< Weights.Length; i++)
            {
                sum += Weights[i] * inputs[i];
            }
            sum = NetParams.tanh(sum);
            return sum;
        }
        
        /// <summary>
        /// Performs crossover between two parent neurons to produce a new neuron, with possible mutation.
        /// </summary>
        public Neuron CrossOver(Neuron parentA,Neuron parentB, double tilt)
        {
            Neuron result = new Neuron(parentA.Weights.Length);
            for (int i = 0; i < Weights.Length; i++)
            {
                if (rnd.NextDouble() > tilt)
                {
                    result.Weights[i] = parentB.Weights[i];
                }
                else
                {
                    result.Weights[i] = parentA.Weights[i];
                }
            }
            if (rnd.NextDouble() > 0.5)
                result.Bias = parentA.Bias;
            else
                result.Bias = parentB.Bias;
            result = applyMutation(result);
            return result;
        }

        /// <summary>
        /// Applies mutation to the neuron's weights based on the mutation rate.
        /// </summary>
        private Neuron applyMutation(Neuron result)
        {
            if (rnd.NextDouble() < NetParams.MUTATION_RATE)
            {
                for (int i = 0; i < result.Weights.Length; i++)
                {
                    result.Weights[i] = rnd.NextDouble() * 4 - 2;
                }
            }
            return result;
        }
    }
}
