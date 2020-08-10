using System;
using System.Collections.Generic;
using System.Text;

namespace doko
{
    public class Game
    {
        public Player[] Winner { get; private set; }

        public Player[] Players { get; private set; }

        public int Points { get; private set; }

        public bool BockRound { get; private set; }

        private Game(Player[] players, String[] winner, int points, bool bock) {
            if (players.Length != 4) {
                throw new ArgumentException("The amount of players is not logical");
            }

            Players = players;
            Winner = players.GetPlayers(winner);
            Points = points;
            BockRound = bock;
        }

        /*
         * Public static factory methods to make semantic checks easier: 2 winners in standard game; 1 or 3 winner in a solo. Last one means lost solo for the forth one.
         */

        public static Game StandardGame(Player[] playing, int points, bool bock, params String[] winner) {
            if (winner.Length != 2)
            {
                throw new ArgumentException("The amount of winner is not logical");
            }
            return new Game(playing, winner, points, bock);
        }

        public static Game Solo(Player[] playing, int points, bool bock, params String[] winner) {
            if (winner.Length != 1 && winner.Length != 3)
            {
                throw new ArgumentException("The amount of winner is not logical");
            }
            return new Game(playing, winner, points, bock);
        }
        
    }

}
