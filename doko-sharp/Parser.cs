using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace doko
{
    public enum State
    {
        CreateParty,
        RunSession
    }

    public class CommandParser {
        
        private Dictionary<State, ParserState> statefulCommands;
        private State currentState;

        public StringBuilder Builder { get; set; }

        private static Regex rgx = new Regex(@"^[a-zA-Z0-9][a-zA-Z0-9 ]*[a-zA-Z0-9]$");

        public CommandParser() {
            statefulCommands = new Dictionary<State, ParserState>();
            foreach (State state in Enum.GetValues(typeof(State))) {
                statefulCommands.Add(state, new ParserState());
            }
            currentState = State.CreateParty;
        }

        public void BindFunction(String name, Func<String[], State?> command, params State[] states) {
            foreach (var state in states) {
                statefulCommands[state].BindFunction(name, command);
            }
        }

        public void BindDefault(Func<String[], State?> command, params State[] states) {
            foreach (var state in states) {
                statefulCommands[state].BindDefault(command);
            }
        }

        public void ChangeState(State newState) {
            currentState = newState;
        }

        public bool Parse(String cmdLine) {
            if (cmdLine.Equals("?")) {
                foreach (string key in statefulCommands[currentState].commandList.Keys) {
                    Builder.Append(key + " ");
                }
                if (!(statefulCommands[currentState].defaultCmd is null)) {
                    Builder.Append("[default]");
                }
                Console.WriteLine();
                return true;
            }

            if(!rgx.IsMatch(cmdLine))
            {
                Builder.AppendLine("Kommando enthält nicht legitime Zeichen.");
                return true;
            }

            String[] values = cmdLine.Split(' ').Where((s) => { return !string.IsNullOrEmpty(s); }).ToArray();
            String[] args = new string[values.Length - 1];
            Array.Copy(values, 1, args, 0, values.Length - 1);
            String cmd = values[0];
            State? returnState = null;
            if (statefulCommands[currentState].commandList.ContainsKey(cmd))
            {
                returnState = statefulCommands[currentState].commandList[cmd](args);
            }
            else {
                if (statefulCommands[currentState].defaultCmd is null)
                {
                    Builder.AppendLine("Command \"" + cmd + "\" not found");
                    return true;
                }
                else {
                    returnState = statefulCommands[currentState].defaultCmd(values);
                }
            }

            if (returnState.HasValue) {
                ChangeState(returnState.Value);
            }
            return false;
        }

    }

    public class ParserState {
        public Dictionary<String, Func<String[], State?>> commandList { get; private set; }
        public Func<String[], State?> defaultCmd { get; private set; } = null;

        public ParserState()
        {
            commandList = new Dictionary<String, Func<String[], State?>>();
        }

        public void BindFunction(String name, Func<String[], State?> command)
        {
            commandList.Add(name, command);
        }

        public void BindDefault(Func<String[], State?> command)
        {
            defaultCmd = command;
        }
    }
}

