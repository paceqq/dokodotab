using System;
using System.Collections.Generic;
using System.Text;

namespace doko
{
    public class Session
    {
        public Players Players { get; private set; }

        public List<Game> gameHistory { get; private set; }

        public Session(Players player) {
            gameHistory = new List<Game>();
            Players = player;    
        }

        public Session(Player[] player) {
            gameHistory = new List<Game>();
            Players = new Players(player);
        }

        public Session(String[] player) {
            gameHistory = new List<Game>();
            Players = new Players(player);
        }

        public void AddGame(Game game) {
            gameHistory.Add(game);
        }

        private void PrintGame(Points point, int i, bool printSingle) {
            Console.Write(" | ");
            foreach (var player in Players.Names)
            {
                String format = String.Format("{{0,{0}}}", player.Name.Length);
                int points;
                if (printSingle) {
                    points = point.GamePoints[player].Item1;
                } else
                {
                    points = point.GamePoints[player].Item2;
                }
                Console.Write(format+" | ", points);
            }
            Console.WriteLine("{0,5} | {1,4} |", point.LastValue, point.BockCounter);
        }

        private void PrintHeader() {
            Console.Write(" | ");
            foreach (var player in Players.Names) {
                Console.Write("{0} | ", player.Name);
            }
            Console.WriteLine("Spiel | Bock |");
        }

        public void PrintLastGame() {
            Print(true);
        }

        public void PrintSession() {
            Print(false);
        }

        private void Print(bool onlyLast) {
            PrintHeader();
            Points point = new Points(Players);
            for (int i = 0; i < gameHistory.Count; i++)
            {
                point.AddGame(gameHistory[i]);
                if (!onlyLast || (i == gameHistory.Count -1 ) )
                {
                    PrintGame(point, i, onlyLast);
                }
            }
        }

    }
}
