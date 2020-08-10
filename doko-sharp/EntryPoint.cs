using System;
using System.Collections.Generic;
using System.Linq;
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
                }
            );

            parser.BindFunction(
                "Add", (str) => {
                    rawPlayer.Add(new Player(String.Join(' ', str)));
                }
            );

            parser.BindFunction(
                "Default", (str) => {
                    rawPlayer.Add(new Player("Joe"));
                    rawPlayer.Add(new Player("William"));
                    rawPlayer.Add(new Player("Jack"));
                    rawPlayer.Add(new Player("Averell"));
                    rawPlayer.Add(new Player("Luke"));
                }
            );

            parser.BindFunction(
                "Start", (str) => {
                    if (session is null)
                    {
                        if (rawPlayer.Count < 4) {
                            Console.WriteLine("Need more player!");
                            return;
                        }

                        session = new Session(rawPlayer.ToArray());
                    }
                    else {
                        Console.WriteLine("Game already started");
                    }
                }
            );

            parser.BindFunction(
                "Print", (str) => {
                    if (session is null)
                    {
                        Console.WriteLine("Game not started!");
                        return;
                    }
                    session.PrintSession();
                }
            );

            parser.BindFunction(
                "Solo", (str) => {
                    if (session is null) {
                        Console.WriteLine("Game not started!");
                        return;
                    }
                    
                    if (str.Length != 2 && str.Length != 3) {
                        Console.WriteLine("Wrong parameter!");
                        return;
                    }

                    String playerName = str[0];

                    if (!session.ActivePlayers.HasPlayer(playerName)) {
                        Console.WriteLine("Player does not play!");
                        return;
                    }

                    int points = Int32.Parse(str[1]);
                    bool bock = str.Length == 3 ? true : false;

                    session.AddSolo(points, bock, playerName);
                    session.PrintLastGame();
                }
            );

            parser.BindFunction(
                "LostSolo", (str) => {
                    if (session is null)
                    {
                        Console.WriteLine("Game not started!");
                        return;
                    }

                    if (str.Length != 2 && str.Length != 3)
                    {
                        Console.WriteLine("Wrong parameter!");
                        return;
                    }

                    String playerName = str[0];

                    if (!session.ActivePlayers.HasPlayer(playerName))
                    {
                        Console.WriteLine("Player does not play!");
                        return;
                    }

                    int points = Int32.Parse(str[1]);
                    bool bock = str.Length == 3 ? true : false;

                    session.AddSolo(points, bock, 
                        (from player in session.ActivePlayers.Invert(new Player[] { new Player(playerName) }) select player.Name).ToArray()
                    );
                    session.PrintLastGame();
                }
            );

            parser.BindDefault(
                (str) => {
                    if (session is null)
                    {
                        Console.WriteLine("Game not started!");
                        return;
                    }

                    if (str.Length != 3 && str.Length != 4)
                    {
                        Console.WriteLine("Wrong parameter!");
                        return;
                    }

                    String[] players = new String[] { str[0], str[1] };

                    foreach (var player in players)
                    {
                        if (!session.ActivePlayers.HasPlayer(player))
                        {
                            Console.WriteLine("Player " + player + " does not play!");
                            return;
                        }
                    }

                    int points = Int32.Parse(str[2]);
                    bool bock = str.Length == 4 ? true : false;

                    session.AddStandard(points, bock, players);
                    session.PrintLastGame();
                }
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
