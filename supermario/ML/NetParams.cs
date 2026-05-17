using System;

namespace supermario.ML
{
    public static class NetParams
    {
        public static readonly Random randomNum = new Random();

        // Mutable so TrainingForm can change them before starting a new run
        public static int    PopulationSize = 60;
        public static double MutationRate   = 0.05;
        public static double SurviveRate    = 0.30;

        // 4 inputs: gap distance, enemy distance, platform-height-diff, is-grounded
        // 2 outputs: horizontal-dir (-1/0/+1 via tanh), jump (>0 = jump)
        public static int[] NetworkShape = { 4, 6, 4, 2 };

        public static double Tanh(double x) => Math.Tanh(x);
    }
}
