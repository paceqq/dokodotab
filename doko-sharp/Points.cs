using System;
using System.Collections.Generic;
using System.Text;

namespace doko
{
    public class Points
    {
        public Dictionary<Player, Tuple<int, int>> GamePoints { get; private set; }
        public int LastValue { get; private set; }
        public int BockCounter { get; private set; }

        readonly Player[] player;

        public Points(Player[] player) {
            this.player = player;
            GamePoints = new Dictionary<Player, Tuple<int,int>>();
            LastValue = 0;
            BockCounter = 0;

            foreach (var person in this.player)
            {
                GamePoints.Add(person, new Tuple<int, int>(0,0));
            }

        }

        public void AddGame(Game game) {
            AddGame(game, BockCounter != 0);
        }

        public void AddGame(Game game, bool bock) {
            var winner = game.Winner;
            var loser = game.Players.Invert(winner);
            int bockFactor = 1;
            if (game.BockRound) {
                BockCounter += 4;
            }

            if (bock) {
                bockFactor = 2;
                BockCounter--;
            }
            LastValue = game.Points * bockFactor;
            int winnerPoints = LastValue;
            int loserPoints = LastValue;
            switch (winner.Length)
            {
                case 1:
                    winnerPoints *= 3;
                    break;
                case 3:
                    loserPoints *= 3;
                    break;
                case 2:
                    break;
                default:
                    throw new ArgumentException("Amount of winner is not logical");
            }

            foreach (var winningPerson in winner)
            {
                GamePoints[winningPerson] = new Tuple<int, int>(winnerPoints, GamePoints[winningPerson].Item2 + winnerPoints);
                
            }
            foreach (var losingPerson in loser)
            {
                GamePoints[losingPerson] = new Tuple<int, int>(-loserPoints, GamePoints[losingPerson].Item2 - loserPoints);
            }
        }

    }
}
