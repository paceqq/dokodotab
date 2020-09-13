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

        static void Main(string[]args)
        {
            string domain = "localhost";
            string uri = @"G:\GIT\dokodotab\doko-sharp\Test.html";
            if (args.Length > 0) {
                uri = args[0];
            }
            if (args.Length > 1) {
                domain = args[1];
            }
            runWebsocket(domain, uri);
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
                try
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
                } catch
                {
                    Console.WriteLine("Unhandled Exception with WebSocket");
                    break;
                }
            }
        }

        static void runWebsocket(string domain, string uri) {            
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://"+ domain + "/");
            listener.Start();
            Console.WriteLine("Listening...");

            bool run = true;
            while (run)
            {
                try
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
                    string responseString = File.ReadAllText(uri).Replace("<!--DOMAIN-->",domain);
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                    // Get a response stream and write the response to it.
                    response.ContentLength64 = buffer.Length;
                    System.IO.Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    // You must close the output stream.
                    output.Close();

                    Console.WriteLine("Served client");
                }
                catch {
                    Console.WriteLine("Got weird exception while handling client");
                }
            }
            listener.Stop();
        }

        static CommandParser createParser(Session session, List<Player> player, StringBuilder builder) {
            CommandParser parser = new CommandParser();

            /*
            parser.BindFunction(
                "Add", (str) => {
                    if (str.Length != 1)
                    {
                        builder.AppendLine("Leerzeichen im Namen nicht erlaubt!");
                        return null;
                    }
                    else
                    {
                        var name = str[0].Trim();
                        if (String.IsNullOrEmpty(name)) {
                            builder.AppendLine("Name wird benötigt.");
                            return null;
                        }
                        player.Add(new Player(name));
                        builder.AppendLine("Spieler " + name + " wurde hinzugefügt.");
                    }
                    return null;
                },
                State.CreateParty
            );*/
            parser.BindDefault(
                (str) =>
                {
                    if (str.Length != 1)
                    {
                        builder.AppendLine("Leerzeichen im Namen nicht erlaubt!");
                        return null;
                    }
                    else
                    {
                        var name = str[0].Trim();
                        if (String.IsNullOrEmpty(name))
                        {
                            builder.AppendLine("Name wird benötigt.");
                            return null;
                        }
                        player.Add(new Player(name));
                        builder.AppendLine("Spieler " + name + " wurde hinzugefügt.");
                    }
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

            parser.BindDefault(
                (str) =>
                {
                    // default value
                    bool bock = false;
                    bool lost = false; // if player count eq 1

                    // needs to be checked if set
                    List<String> players = new List<String>();
                    int points = -1;

                    foreach (var arg in str)
                    {
                        if (arg.Equals("Bock"))
                        {
                            bock = true;
                        }
                        else if (arg.Equals("Lost"))
                        {
                            lost = true;
                        }
                        else if (session.ActivePlayers.HasPlayer(arg))
                        {
                            if (player.Count == 2) {
                                builder.AppendLine("Es können maximal nur 2 Spieler angegeben werden!\nFür verlorene Solo-Runde entsprechend den Solo Spieler mit \"Lost\" eingeben");
                                return null;
                            }
                            if (players.Count == 1 && players[0].Equals(arg)) 
                            {
                                builder.AppendLine("Ein Spieler kann nur einmal angegeben werden!");
                                return null;
                            }
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
                                builder.AppendLine("Parametereingabe kann nicht verwertet werden!" + " (" + arg + ")");
                                builder.AppendLine("\nEs spielen: " + session.ActivePlayerString);
                                return null;
                            }
                        }
                    }

                    if (points == -1)
                    {
                        builder.AppendLine("Spielwert muss angegeben werden!");
                        return null;
                    }

                    if (players.Count == 2)
                    {
                        if (lost) {
                            builder.AppendLine("\"Lost\" Parameter wird ignoriert!");
                        }
                        session.AddStandard(points, bock, players.ToArray());
                        builder.AppendLine("Standardspiel gewonnen von " + String.Join(" ", players.ToArray()));
                    }
                    else if (players.Count == 1)
                    {
                        String[] winningPlayers = players.ToArray();
                        if (lost) {
                            var soloPlayer = new Player[] { new Player(players.First()) };
                            winningPlayers = (from player in session.ActivePlayers.Invert(soloPlayer) select player.Name).ToArray();
                        }
                        session.AddSolo(points, bock, winningPlayers);
                        builder.AppendLine("Solospiel gewonnen von " + String.Join(" ", winningPlayers));
                    }
                    else 
                    {
                        builder.AppendLine("Genau ein Spieler eingeben für ein Solo.\nZwei Spieler für ein gewöhnliches Spiel.\nAlles andere ist falsch!");
                        return null;
                    }

                    builder.Append(session.GetLastGameString());
                    return null;
                },
                State.RunSession
                );
            return parser;
        }

    }

}
