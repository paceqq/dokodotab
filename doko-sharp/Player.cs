using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace doko
{
    public class Player { 
        public String Name { get; private set; }

        public static Player[] CreatePlayers(String[] names) {
            return (from name in names select new Player(name)).ToArray();
        }

        public Player(String name) {
            Name = name;
        }

        public bool IsIn(Session game)
        {
            return IsIn(game.CurrentPlayers);
        }
        public bool IsIn(Player[] playerSet)
        {
            return Array.Exists(playerSet, (player) => { return player.Equals(this); });
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

    public static class PlayerHelper
    {
        public static Player[] Invert(this Player[] players, Player[] a)
        {
            return (from player in players where !player.IsIn(a) select player).ToArray();
        }

        public static Player[] GetPlayers(this Player[] players, params String[] names) {
            /*
             * Maybe I should stop with that, but LINQ is to nice to not to play arount with it...
             * 
             * SELECT all player, where its name exist
             */
            Player[] ret = (from player in players where Array.Exists(names, (name) => { return player.Equals(name); }) select player).ToArray();
            
            if (ret.Length != names.Length) {
                throw new ArgumentException("Names of not participating players were given!");
            }

            return ret;
        }

        public static bool HasPlayer(this Player[] players, String name)
        {
            return players.Any((player) => { return player.Equals(name); });
        }
    }
}