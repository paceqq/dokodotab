using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace doko.Debug
{
    static class DebugSessionExtensions
    {
        public static void PrintLastGame(this Session session)
        {
            session.Print(true);
        }

        public static void PrintSession(this Session session)
        {
            session.Print(false);
        }

        private static void Print(this Session session, bool onlyLast)
        {
            // Print Header
            Console.Write(" | ");
            foreach (var player in session.CurrentPlayers)
            {
                Console.Write("{0} | ", player.Name);
            }
            Console.WriteLine("| Spiel | Bock |");

            // Print points
            Points point = new Points(session.CurrentPlayers);
            Game game;
            // Loop through games
            for (int i = 0; i < session.GameHistory.Count; i++)
            {
                game = session.GameHistory[i];
                point.AddGame(game);
                if (!onlyLast || (i == session.GameHistory.Count - 1))
                {
                    Console.Write(" | ");
                    // Loop through players
                    foreach (var player in session.CurrentPlayers)
                    {
                        String format = String.Format("{{0,{0}}}", player.Name.Length);
                        // Print score only if player is active player
                        if (player.IsIn(game.Players))
                        {
                            var playerPoint = point.GamePoints[player];
                            // Print only last games score (including Bock, that's why we are still iterating)
                            Console.Write(format + " | ", onlyLast? playerPoint.Item1 : playerPoint.Item2);
                        }
                        else
                        {
                            Console.Write(format + " | ", "-");
                        }
                    }
                    Console.WriteLine("| {0,5} | {1,4} |", point.LastValue, point.BockCounter);
                }
            }
        }

    }
}
