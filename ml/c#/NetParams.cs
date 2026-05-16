using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlappyBirdCSharp.ANN
{
    /// <summary>
    /// Contains static parameters and utility functions for the neural network and genetic algorithm.
    /// </summary>
    public static class NetParams
    {

        public static Random randomNum = new Random();
        public  enum GAME_MODE  { TRAINING, PLAYING, LOADING,EXIT };
        /// <summary>
        /// Maximum number of generations for evolution.
        /// </summary>
        public static int MAX_GENERATIONS = 1000;

        /// <summary>
        /// Number of players in each population.
        /// </summary>
        public static int POPULATION_SIZE = 60;

        /// <summary>
        /// Probability of mutation during crossover.
        /// </summary>
        public static double MUTATION_RATE = 0.05;

        /// <summary>
        /// Fraction of the population that survives to the next generation.
        /// </summary>
        public static double SURVIVE_RATE = 0.3;

        /// <summary>
        /// Shape of the neural network (neurons per layer).
        /// </summary>
        public static int [] NETWORK_SHAPE = { 3, 4,2, 1 };

        /// <summary>
        /// Sigmoid activation function.
        /// </summary>
        public static double Sigmoid(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }

        /// <summary>
        /// Hyperbolic tangent activation function.
        /// </summary>
        public static double tanh(double x)
        {
            return Math.Tanh(x);
        }
    }
}
