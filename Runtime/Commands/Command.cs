using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Reflection;

namespace DevConsole.Commands{
    public class CommandUtil{
        public static bool IsOptionSet(string option, string[] args){
            return new List<string>(args).Contains(option);
        }
        public static int GetNumSetOptions(string[] args){
            return new List<string>(args).Count(x => x[0] == '-');
        }
        public static bool DoesArgExist(int argIdx, string[] args){
            return args.Length > argIdx;
        }
        public static bool IsArgOption(int argIdx, string[] args){
            return args.Length > argIdx && args[argIdx][0] == '-';
        }
    }

    public interface ICommand
    {
        string Key {get;}
        string Syntax {get;}
        string Description {get;}
        void Execute(Console console, params string[] args);
    }

    public class Echo : ICommand {
        public string Key{
            get{
                return "echo";
            }
        }
        public string Syntax{
            get{
                return "echo <message>";
            }
        }
        public string Description{
            get{
                return "Prints out the message to the console.";
            }
        }
        public void Execute(Console console, params string[] args){
            if(args.Length == 0){
                console.Syntax(Syntax);
                return;
            }
            string message = string.Empty;
            foreach(string s in args)
                message += " " + s;
            console.Log(message);
        }
    }
}