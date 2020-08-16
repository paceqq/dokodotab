using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using doko.Debug;

namespace doko
{
    public class EntryPoint
    {

        static void Main() {

            bool run = true;
            List<Player> rawPlayer = new List<Player>();
            Session session = null;

            CommandParser parser = new CommandParser();
            parser.BindFunction(
                "Exit", (str) => { 
                    run = false;
                    return null;
                },
                State.CreateParty, State.RunSession
            );

            parser.BindFunction(
                "Add", (str) => {
                    rawPlayer.Add(new Player(String.Join(' ', str)));
                    return null;
                },
                State.CreateParty
            );

            parser.BindFunction(
                "Default", (str) => {
                    rawPlayer.Add(new Player("Joe"));
                    rawPlayer.Add(new Player("William"));
                    rawPlayer.Add(new Player("Jack"));
                    rawPlayer.Add(new Player("Averell"));
                    rawPlayer.Add(new Player("Luke"));
                    return null;
                },
                State.CreateParty
            );

            parser.BindFunction(
                "List", (str) => {
                    foreach (var player in rawPlayer) {
                        Console.WriteLine(player.Name);
                    }
                    return null;
                },
                State.CreateParty
            );

            parser.BindFunction(
                "Start", (str) => {
                    if (rawPlayer.Count < 4) {
                        Console.WriteLine("Need more player!");
                        return null;
                    }

                    session = new Session(rawPlayer.ToArray());
                    
                    return State.RunSession;
                },
                State.CreateParty
            );

            parser.BindFunction(
                "Print", (str) => {
                    session.PrintSession();
                    return null;
                },
                State.RunSession
            );

            parser.BindFunction(
                "Active", (str) => {
                    Console.WriteLine("Es spielen: " + session.ActivePlayerString);
                    return null;
                },
                State.RunSession
            );

            parser.BindFunction(
                "Solo", (str) => {

                    // Defaultvalues
                    bool lost = false;
                    bool bock = false;

                    // Needs to be checked if set
                    String playerName = null;
                    int points = -1;

                    foreach (var arg in str) {
                        if (arg.Equals("Lost"))
                        {
                            lost = true;
                        }
                        else if (arg.Equals("Bock"))
                        {
                            bock = true;
                        }
                        else if (session.ActivePlayers.HasPlayer(arg) && playerName is null)
                        {
                            playerName = arg;
                        }
                        else {
                            try
                            {
                                points = Int32.Parse(arg);
                            }
                            catch
                            {
                                Console.WriteLine("Parametereingabe ist falsch!" + " (" + arg + ")");
                                Console.WriteLine("Parameter muss eine Punktezahl, den Solo-Spieler und optional \"Bock\" oder \"Lost\" angeben.");

                                Console.WriteLine("Es spielen: " + session.ActivePlayerString);
                                return null;
                            }
                        }
                    }

                    if (points == -1) {
                        Console.WriteLine("Spielwert muss angegeben werden!");
                        return null;
                    }

                    if (playerName is null)
                    {
                        Console.WriteLine("Solo Spieler muss angegeben werden!");
                        return null;
                    }

                    if (lost)
                    {
                        String[] winningPlayersName = (from player in session.ActivePlayers.Invert(new Player[] { new Player(playerName) }) select player.Name).ToArray();
                        session.AddSolo(points, bock, winningPlayersName);
                    } else {
                        session.AddSolo(points, bock, playerName);
                    }
                    session.PrintLastGame();
                    return null;
                },
                State.RunSession
            );

            parser.BindDefault(
                (str) => {
                    // default value
                    bool bock = false;

                    // needs to be checked if set
                    List<String> players = new List<String>();
                    int points = -1;

                    foreach (var arg in str)
                    {
                        if (arg.Equals("Bock"))
                        {
                            bock = true;
                        }
                        else if (session.ActivePlayers.HasPlayer(arg) && players.Count< 2 && (players.Count != 1 || !players[0].Equals(arg))) // Last Expression is an =>
                        {
                            players.Add(arg);
                        }
                        else
                        {
                            try
                            {
                                points = Int32.Parse(arg);
                            }
                            catch
                            {
                                Console.WriteLine("Parametereingabe ist falsch!" + " (" + arg + ")");
                                Console.WriteLine("Parameter muss eine Punktezahl, zwei aktive Spieler und optional \"Bock\" angeben.");
                                Console.WriteLine("Es spielen: " + session.ActivePlayerString);
                                return null;
                            }
                        }
                    }

                    if (points == -1)
                    {
                        Console.WriteLine("Spielwert muss angegeben werden!");
                        return null;
                    }

                    if (players.Count !=2)
                    {
                        Console.WriteLine("Es müssen genau 2 Gewinner angegeben werden!");
                        return null;
                    }

                    session.AddStandard(points, bock, players.ToArray());
                    session.PrintLastGame();
                    return null;
                },
                State.RunSession
            );

            while (run) {
                Console.Write("> ");
                parser.Parse(Console.ReadLine());
            }

            Console.WriteLine("ByeBye");
            
            return;
        }

    }

}
