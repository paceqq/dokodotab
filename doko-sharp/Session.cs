using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace doko
{
    public class Session
    {
        public Player[] CurrentPlayers { get; private set; }
        public Player[] ActivePlayers { 
            get {
                Player[] buf = new Player[4];
                int len = CurrentPlayers.Length - roundNumber;
                if (len > 4) {
                    len = 4;
                }
                Array.Copy(CurrentPlayers, roundNumber, buf, 4 - len, len);
                for (int i = 0; i < 4 - len; i++) {
                    buf[i] = CurrentPlayers[i];
                }
                return buf;
            }
        }

        public String ActivePlayerString
        {
            get {
                String activePlayers = "";
                foreach (var player in this.ActivePlayers)
                {
                    activePlayers += " " + player.Name;
                }
                return activePlayers.Substring(1);
            }
        }

        public List<Game> GameHistory { get; private set; }

        private int roundNumber;

        public Session(Player[] player) {
            GameHistory = new List<Game>();
            CurrentPlayers = player;

            roundNumber = 0;
        }

        private void addGame(Game game) {
            GameHistory.Add(game);
            roundNumber = (roundNumber + 1) % CurrentPlayers.Length;
        }

        public void AddStandard(int points, bool bock, params String[] winner)
        {
            addGame(Game.StandardGame(ActivePlayers, points, bock, winner));
        }
        public void AddSolo(int points, bool bock, params String[] winner)
        {
            addGame(Game.Solo(ActivePlayers, points, bock, winner));
        }

        public String GetLastGameString()
        {
            return this.getTable(true);
        }

        public String GetSessionString()
        {
            return this.getTable(false);
        }

        private String getTable(bool onlyLast)
        {
            StringBuilder builder = new StringBuilder();
            // Print Header
            builder.Append(" | ");
            foreach (var player in this.CurrentPlayers)
            {
                builder.AppendFormat("{0} | ", player.Name);
            }
            builder.AppendLine("| Spiel | Bock |");

            // Print points
            Points point = new Points(this.CurrentPlayers);
            Game game;
            // Loop through games
            for (int i = 0; i < this.GameHistory.Count; i++)
            {
                game = this.GameHistory[i];
                point.AddGame(game);
                if (!onlyLast || (i == this.GameHistory.Count - 1))
                {
                    builder.Append(" | ");
                    // Loop through players
                    foreach (var player in this.CurrentPlayers)
                    {
                        String format = String.Format("{{0,{0}}}", player.Name.Length);
                        // Print score only if player is active player
                        if (player.IsIn(game.Players))
                        {
                            var playerPoint = point.GamePoints[player];
                            // Print only last games score (including Bock, that's why we are still iterating)
                            builder.AppendFormat(format + " | ", onlyLast ? playerPoint.Item1 : playerPoint.Item2);
                        }
                        else
                        {
                            builder.AppendFormat(format + " | ", "-");
                        }
                    }
                    builder.AppendFormat("| {0,5} | {1,4} |\n", point.LastValue, point.BockCounter);
                }
            }
            return builder.ToString();
        }

    }
}
