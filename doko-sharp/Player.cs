using System;
using System.Collections.Generic;
using System.Text;

namespace doko
{
    public class Players
    {
        public Player[] Names { get; private set; }
        
        public Players(String[] names) {
            if (names.Length != 4) {
                throw new ArgumentException("Wrong size");
            }

            Names = new Player[4];
            for (int i = 0; i < 4; i++) {
                Names[i] = new Player(names[i]);
            }
        }

        public Players(Player[] names) {
            if (names.Length != 4) {
                throw new ArgumentException("Wrong size");
            }

            Names = names;
        }

        public Player GetCurrentPlayer(int turn) {
            return Names[turn % 4];
        }

        public int GetPlayerIndex(String name) {
            for (int i = 0; i < 4; i++) {
                if (Names[i].Equals(name)) {
                    return i;
                }
            }
            throw new ArgumentOutOfRangeException("Name is not in the player list");
        }

        public bool Contains(String name) {
            try
            {
                GetPlayerIndex(name);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        public bool Contains(Player player) {
            return Contains(player.Name);
        }

        // Also checks for different names!
        public bool Contains(String name1, String name2) {
            return (Contains(name1) && Contains(name2) && !(name1.Equals(name2)));
        }

        public bool Contains(Player player1, Player player2) {
            return Contains(player1.Name, player2.Name);
        }

        public bool Contains(String[] players) {
            foreach (String player in players)
            {
                if (!Contains(player)) return false;
            }
            return true;
        }

        public bool Contains(Player[] players) {
            foreach (Player player in players)
            {
                if (!Contains(player)) return false;
            }
            return true;
        }

        public Player[] Invert(Player[] party) {
            if (!Contains(party)) throw new ArgumentException("Need a subset of player to create the inversion");

            List<Player> inversion = new List<Player>();

            foreach (Player invPlayer in Names) {
                bool append = true;
                foreach (Player setPlayer in party) {
                    if (invPlayer.Equals(setPlayer)) {
                        append = false;
                        break;
                    }
                }
                if(append) inversion.Add(invPlayer);
            }

            return inversion.ToArray();
        }

    }

    public class Player { 
        public String Name { get; private set; }

        public Player(String name) {
            Name = name;
        }

        public bool IsIn(Players players) {
            return players.Contains(this);
        }
        
        public bool IsIn(Session game) {
            return game.Players.Contains(this);
        }

        public override String ToString() {
            return "Player(" + Name + ")";
        }

        public bool Equals(String name)
        {
            return Name.Equals(name);
        }

        public override bool Equals(object obj)
        {
            Player rhs = obj as Player;
            if (rhs == null) return false;
            return rhs.Name == this.Name;
        }
    }
}