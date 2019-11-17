using System;
using System.Collections.Generic;

namespace doko
{
    public class EntryPoint
    {
        static void Main() {
            Players p = new Players(new String[] { "Joe", "William", "Jack", "Averell" });
            Player[] players = p.Names;
            Session s = new Session(p);

            s.AddGame(new Game(getPlayer(players, 1, 2), 3, false));
            s.AddGame(new Game(getPlayer(players, 0, 2), 1, false));
            s.AddGame(new Game(getPlayer(players, 0, 1), 2, false));
            s.AddGame(new Game(getPlayer(players, 1, 3), 1, true));
            s.AddGame(new Game(getPlayer(players, 0, 2,3), 3, false));
            s.AddGame(new Game(getPlayer(players, 2), 4, false));
            s.AddGame(new Game(getPlayer(players, 1, 3), 1, true));
            s.AddGame(new Game(getPlayer(players, 0, 3), 2, false));
            s.PrintSession();
            Console.WriteLine("");
            s.PrintLastGame();
        }

        static Player[] getPlayer(Player[] p, params int[] index) {
            List<Player> player = new List<Player>();
            foreach (int i in index) {
                player.Add(p[i]);
            }
            return player.ToArray();
        }
    }
}
