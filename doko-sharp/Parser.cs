using System;
using System.Collections.Generic;
using System.Text;

namespace doko
{
    public class CommandParser {

        private Dictionary<String, Action<String[]>> commandList;
        private Action<String[]> defaultCmd = null;
        
        public CommandParser() {
            commandList = new Dictionary<string, Action<string[]>>();
        }

        public void BindFunction(String name, Action<String[]> command) {
            commandList.Add(name, command);
        }

        public void BindDefault(Action<String[]> command) {
            defaultCmd = command;
        }

        public void Parse(String cmdLine) {
            // TODO check
            // Preop


            if (cmdLine.Equals("?")) {
                foreach (string key in commandList.Keys) {
                    Console.Write(key + " ");
                }
                Console.WriteLine();
                return;
            }

            String[] values = cmdLine.Split(' ');
            String[] args = new string[values.Length - 1];
            Array.Copy(values, 1, args, 0, values.Length - 1);
            String cmd = values[0];
            if (commandList.ContainsKey(cmd))
            {
                commandList[cmd](args);
            }
            else {
                if (defaultCmd is null)
                {
                    Console.WriteLine("Command \"" + cmd + "\" not found");
                }
                else {
                    defaultCmd(values);
                }
            }
            
        }

    }
}

