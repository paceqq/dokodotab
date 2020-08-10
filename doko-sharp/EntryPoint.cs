using System;
using System.Collections.Generic;

using doko.Debug;

namespace doko
{
    public class EntryPoint
    {
        static void Main() {
            Player[] players = Player.CreatePlayers(new String[] { "Joe", "William", "Jack", "Averell", "Luke"});
            Session s = new Session(players);

            s.AddStandard(3, false, "William", "Jack");
            s.AddStandard(1, false, "Averell", "Jack");
            s.AddStandard(2, false, "Joe", "Luke");
            s.AddStandard(1, true, "William", "Averell");
            s.AddSolo(3, false, "Joe", "Jack", "Luke");
            s.AddSolo(4, false, "Jack");
            s.AddStandard(1, true, "William", "Averell");
            s.AddStandard(2, false, "Joe", "Averell");

            s.PrintSession();
            Console.WriteLine("");
            s.PrintLastGame();
        }

    }

}
