using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlappyBirdCSharp.ANN
{
    /// <summary>
    /// Represents a population of players (birds) for the genetic algorithm.
    /// Handles population initialization, selection, crossover, and new generation creation.
    /// </summary>
    public class Population
    {
        Random rnd;

        /// <summary>
        /// List of all players in the population.
        /// </summary>
        public List<Player> players { get; set; }

        /// <summary>
        /// Initializes a new population and fills it with players.
        /// </summary>
        public Population()
        {
            rnd = new Random();
            players = new List<Player>();
            initPopulation();
        }

        /// <summary>
        /// Initializes the population with new players.
        /// </summary>
        private void initPopulation()
        {
            for (int i = 0; i < NetParams.POPULATION_SIZE; i++)
            {
                Player p = new Player(NetParams.NETWORK_SHAPE);
                players.Add(p);
            }
        }

        /// <summary>
        /// Adds a player to the population.
        /// </summary>
        public void addPlyer(Player p)
        {
            players.Add(p);
        }

        /// <summary>
        /// Removes all players from the population and disposes their visual representations.
        /// </summary>
        public void removeAll()
        {
            foreach(Player p in players)
            {
                p.getBird().Dispose();
            }
            players.Clear();
        }

        /// <summary>
        /// Returns a list of the best players based on their scores.
        /// </summary>
        public List<Player> getBestPlayers()
        {
            int size = (int)(NetParams.SURVIVE_RATE * NetParams.POPULATION_SIZE);
            players = players.OrderByDescending(x => x.score).ToList();
            List<Player> result = new List<Player>();
            for (int i=0;i<size; i++)
            {
                result.Add(players[i].clone());
            }
            return result;
        }

        /// <summary>
        /// Returns the number of players that are still alive.
        /// </summary>
        public int numOfAlive()
        {
            return players.Where(x => x.isAlive).Count();
        }

        /// <summary>
        /// Creates a new generation of players using crossover from the best players.
        /// </summary>
        public void CreateNewGeneration()
        {
            List<Player> best = getBestPlayers();
            removeAll();
            Player parent1, parent2;
            
            for (int i = 0; i < NetParams.POPULATION_SIZE; i++)
            {
                (parent1, parent2) = getParents(best);
                players.Add(crossOver(parent1, parent2));
                players[i].getBird().Visible = true;
            }
        }

        /// <summary>
        /// Selects two distinct parents from the best players for crossover.
        /// </summary>
        private (Player parent1, Player parent2) getParents(List<Player> best)
        {
            int idx1, idx2;

            idx1 = rnd.Next(0, best.Count);
            idx2 = rnd.Next(0, best.Count);
            while (idx1 == idx2)
                idx2 = rnd.Next(0, best.Count);

            return (best[idx1], best[idx2]);
        }

        /// <summary>
        /// Adds a player to the population.
        /// </summary>
        public void addPlayer(Player player)
        {
            players.Add(player);
        }

        /// <summary>
        /// Performs crossover between two parent players to produce a new player.
        /// </summary>
        private Player crossOver(Player parentA, Player parentB)
        {
            Player child = new Player(parentA.networkShape);
            child = child.CrossOver(parentA, parentB);
            return child;
        }
    }
}
