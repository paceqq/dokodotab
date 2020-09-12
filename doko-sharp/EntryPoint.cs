using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
//using doko.Debug;

using System.IO;

using System.Net.WebSockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace doko
{
    public class EntryPoint
    {

        static void Main()
        {
            runWebsocket();
        }

        static async void startWSHandler(object context_raw) {

            // create a Cancellation Token
            var TokenSource = new CancellationTokenSource();
            var Token = TokenSource.Token;

            // create the websocket
            HttpListenerContext context = context_raw as HttpListenerContext;
            var ws = await context.AcceptWebSocketAsync(null);
            
            // some content converter
            byte[] buffy = new byte[4096];
            var text_decoder = new System.Text.UTF8Encoding();


            List<Player> player = new List<Player>();
            StringBuilder stringBuilder = new StringBuilder();
            Session session = null;
            CommandParser parser = createParser(session, player, stringBuilder);
            parser.Builder = stringBuilder;
            // actual processing
            while (true)
            {
                var wsresult = await ws.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffy), Token);
                
                if (wsresult.MessageType != WebSocketMessageType.Close)
                {
                    string cmd = text_decoder.GetString(buffy, 0, wsresult.Count);
                    System.Console.Write("> ");
                    System.Console.WriteLine(cmd);

                    stringBuilder.Clear();
                    parser.Parse(cmd);
                    
                    if (stringBuilder.Length != 0)
                    {
                        System.Console.WriteLine(stringBuilder.ToString());
                        await ws.WebSocket.SendAsync(text_decoder.GetBytes(stringBuilder.ToString()), WebSocketMessageType.Text, true, Token);
                    }
                }
                else //else close
                {
                    ws.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", Token).Wait();
                    Console.WriteLine("Closed WebSocket");
                    return;
                }
            }
            
            
        }

        static void runWebsocket() {            
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost/");
            listener.Start();
            Console.WriteLine("Listening...");

            bool run = true;
            while (run)
            {
                // Wait for Request
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;

                if (request.RawUrl.Equals("/Exit"))
                {
                    run = false;
                }

                if (request.IsWebSocketRequest)
                {
                    var thread = new Thread(startWSHandler);
                    thread.Start(context);
                    continue;
                }

                HttpListenerResponse response = context.Response;

                // Construct a response.
                string responseString = File.ReadAllText(@"G:\GIT\dokodotab\doko-sharp\Test.html");
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();

                Console.WriteLine("Served client");
            }
            listener.Stop();
        }

        static CommandParser createParser(Session session, List<Player> player, StringBuilder builder) {
            CommandParser parser = new CommandParser();
            
            parser.BindFunction(
                "Add", (str) => {
                    player.Add(new Player(String.Join(' ', str)));
                    return null;
                },
                State.CreateParty
            );

            parser.BindFunction(
                "List", (str) => {
                    foreach (var player in player)
                    {
                        builder.AppendLine(player.Name);
                    }
                    return null;
                },
                State.CreateParty
            );

            parser.BindFunction(
                "Start", (str) => {
                    if (player.Count < 4)
                    {
                        builder.AppendLine("Need more player!");
                        return null;
                    }

                    session = new Session(player.ToArray());

                    return State.RunSession;
                },
                State.CreateParty
            );

            parser.BindFunction(
                "Print", (str) => {
                    builder.Append(session.GetSessionString());
                    return null;
                },
                State.RunSession
            );

            parser.BindFunction(
                "Active", (str) => {
                    builder.AppendLine("Es spielen: " + session.ActivePlayerString);
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

                    foreach (var arg in str)
                    {
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
                        else
                        {
                            try
                            {
                                points = Int32.Parse(arg);
                            }
                            catch
                            {
                                builder.AppendLine("Parametereingabe ist falsch!" + " (" + arg + ")");
                                builder.AppendLine("Parameter muss eine Punktezahl, den Solo-Spieler und optional \"Bock\" oder \"Lost\" angeben.");

                                builder.AppendLine("Es spielen: " + session.ActivePlayerString);
                                return null;
                            }
                        }
                    }

                    if (points == -1)
                    {
                        builder.AppendLine("Spielwert muss angegeben werden!");
                        return null;
                    }

                    if (playerName is null)
                    {
                        builder.AppendLine("Solo Spieler muss angegeben werden!");
                        return null;
                    }

                    if (lost)
                    {
                        String[] winningPlayersName = (from player in session.ActivePlayers.Invert(new Player[] { new Player(playerName) }) select player.Name).ToArray();
                        session.AddSolo(points, bock, winningPlayersName);
                    }
                    else
                    {
                        session.AddSolo(points, bock, playerName);
                    }
                    builder.Append(session.GetLastGameString());
                    return null;
                },
                State.RunSession
            );

            parser.BindDefault(
                (str) =>
                {
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
                        else if (session.ActivePlayers.HasPlayer(arg) && players.Count < 2 && (players.Count != 1 || !players[0].Equals(arg))) // Last Expression is an =>
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
                                builder.AppendLine("Parametereingabe ist falsch!" + " (" + arg + ")");
                                builder.AppendLine("Parameter muss eine Punktezahl, zwei aktive Spieler und optional \"Bock\" angeben.");
                                builder.AppendLine("Es spielen: " + session.ActivePlayerString);
                                return null;
                            }
                        }
                    }

                    if (points == -1)
                    {
                        builder.AppendLine("Spielwert muss angegeben werden!");
                        return null;
                    }

                    if (players.Count != 2)
                    {
                        builder.AppendLine("Es müssen genau 2 Gewinner angegeben werden!");
                        return null;
                    }

                    session.AddStandard(points, bock, players.ToArray());
                    builder.Append(session.GetLastGameString());
                    return null;
                },
                State.RunSession
                );
            return parser;
        }

    }

}
