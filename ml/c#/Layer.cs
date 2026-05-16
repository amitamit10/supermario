using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlappyBirdCSharp.ANN
{
    [Serializable]
    /// <summary>
    /// Represents a single layer in a neural network, containing multiple neurons.
    /// </summary>
    public class Layer
    {
        /// <summary>
        /// Number of neurons in this layer.
        /// </summary>
        public int numOfNeurons { get; set; }

        /// <summary>
        /// Number of inputs each neuron in this layer receives.
        /// </summary>
        public int numOfInputs { get; set; }

        /// <summary>
        /// Array of neurons in this layer.
        /// </summary>
        public Neuron[] Neurons { get; set; }

        /// <summary>
        /// Initializes a new layer with the specified number of neurons and inputs per neuron.
        /// </summary>
        public Layer(int numOfNeurons, int numOfInputs)
        {
            this.numOfNeurons = numOfNeurons;
            this.numOfInputs = numOfInputs;
            Neurons = new Neuron[numOfNeurons];
            for (int i = 0; i < Neurons.Length; i++)
            {
                Neurons[i] = new Neuron(numOfInputs);
            }
        }

        /// <summary>
        /// Activates the layer by feeding inputs through all neurons and returning their outputs.
        /// </summary>
        public double[] Forward(double[] inputs)
        {
            double[] output = new double[numOfNeurons];
            for (int i = 0; i < Neurons.Length; i++)
            {
                output[i] = Neurons[i].Forward(inputs);
            }
            return output;
        }

        /// <summary>
        /// Performs crossover between two parent layers to produce a new layer.
        /// </summary>
        public Layer CrossOver(Layer parentA,Layer parentB, double tilt)
        {
            Layer result = new Layer(numOfNeurons, numOfInputs);
            for(int i = 0; i < result.Neurons.Length; i++)
            {
                result.Neurons[i] = result.Neurons[i].CrossOver(parentA.Neurons[i], parentB.Neurons[i],tilt);
            }
            return result;
        }
    }
}

