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

    }
}
