using System;
using System.Collections.Generic;
using System.Text;

namespace doko
{
    public class Game
    {
        public Player[] Winner { get; private set; }

        public int Points { get; private set; }

        public bool Bock { get; private set; }

        public Game(Player[] winner, int points, bool bock) {
            if (!(winner.Length > 0 && winner.Length < 4)) {
                throw new ArgumentException("The amount of winner is not logical");
            }

            Winner = winner;
            Points = points;
            Bock = bock;
        }
                                  
    }


}
