using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using DevConsole.Commands;

namespace DevConsole{
    public class Console
    {
        private List<ICommand> commands = new List<ICommand>();
        private List<Binding> bindings = new List<Binding>();
        private Queue<string> history = new Queue<string>();
        private string formattedHistory;
        public string History{
            get{
                return formattedHistory;
            }
        }
        private int historyMax = 32;
        public int HistoryMax{
            get{
                return historyMax;
            }
            set{
                historyMax = value;
                while(history.Count > historyMax)
                    history.Dequeue();
                FormatHistory();
            }
        }

        public Console(){
            //Find all available Commands
            foreach(Assembly a in AppDomain.CurrentDomain.GetAssemblies()){
                foreach(Type t in a.GetTypes()){
                    if(typeof(ICommand).IsAssignableFrom(t)){
                        if(t.Name == "ICommand")
                            continue;
                        commands.Add((ICommand)Activator.CreateInstance(t));
                    }
                    else{
                        foreach(MethodInfo mi in t.GetMethods()){
                            BindingAttribute attribute = mi.GetCustomAttribute<BindingAttribute>();
                            if(attribute != null){
                                bool parametersPrimitive = true;
                                foreach(ParameterInfo pi in mi.GetParameters()){
                                    if(!pi.ParameterType.IsPrimitive && pi.ParameterType != typeof(string)){
                                        parametersPrimitive = false;
                                        break;
                                    }
                                }
                                if(!parametersPrimitive){
                                    Error($"Skipped binding '{attribute.Command}'! Method must not only have primitive parameters!");
                                    continue;
                                }
                                bindings.Add(new Binding{attribute = attribute, type = t, method = mi});
                            }
                        }
                    }
                }
            }
            Log("Type 'help' to view the basic built in commands!");
        }

        private void FormatHistory(){
            formattedHistory = string.Empty;
            int idx = 0;
            foreach(string s in history){
                formattedHistory += s;
                if(idx++ < history.Count - 1)
                    formattedHistory += "\n";
            }
        }

        public void SubmitCommand(string command, ConsoleUI ui){
            string[] args = command.Split(' ');
            command = args[0];
            var tempArgs = new List<string>(args);
            tempArgs.RemoveAt(0);
            args = tempArgs.ToArray();
            //Builtin Commands
            try{
                switch(command){
                    case "help":
                        Log("Type 'commands' to get a list of all available commands!");
                        Log("Type 'bindings' to get a list of all available bindings!");
                        Log("Type 'desc' followed by a command to get a description of its functionality!");
                        Log("Type 'syntax' followed by a command to view its syntax!");
                    break;
                    case "clear":
                        history.Clear();
                        FormatHistory();
                    break;
                    case "commands":
                        LogCommand("help");
                        LogCommand("clear");
                        LogCommand("commands");
                        LogCommand("syntax");
                        LogCommand("desc");
                        LogCommand("exec");
                        foreach(ICommand c in commands)
                            LogCommand(c.Key);
                    break;
                    case "bindings":
                        foreach(Binding b in bindings)
                            LogBinding(b.attribute.Command);
                    break;
                    case "syntax":
                        if(CommandUtil.DoesArgExist(0, args)){
                            foreach(ICommand c in commands)
                                if(c.Key == args[0]){
                                    Syntax(c.Syntax);
                                    break;
                                }
                            foreach(Binding b in bindings){
                                if(b.attribute.Command == args[0]){
                                    BindingSyntax(b);
                                    break;
                                }
                            }
                            switch(args[0]){
                                case "help": Syntax("help"); break;
                                case "clear": Syntax("clear"); break;
                                case "commands": Syntax("commands"); break;
                                case "syntax": Syntax("syntax <command/binding>"); break;
                                case "desc": Syntax("desc <command>"); break;
                                case "exec": Syntax("exec <binding>"); break;
                            }
                        }
                        else
                            Syntax("syntax <command/binding>");
                    break;
                    case "desc":
                        if(CommandUtil.DoesArgExist(0, args)){
                            foreach(ICommand c in commands)
                                if(c.Key == args[0]){
                                    Log(c.Description);
                                    break;
                                }
                            switch(args[0]){
                                case "help": Log("Lists the basic built in commands."); break;
                                case "clear": Log("Clears the console."); break;
                                case "commands": Log("Lists all the available commands."); break;
                                case "syntax": Log("Shows the syntax for the specified command."); break;
                                case "desc": Log("Shows the description for the specified command."); break;
                                case "exec": Log("Executes all occurences of the specified binding."); break;
                            }
                        }
                        else
                            Syntax("desc <command>");
                    break;
                    case "exec":
                        if(CommandUtil.DoesArgExist(0, args)){
                            foreach(Binding b in bindings)
                                if(b.attribute.Command == args[0]){
                                    ParameterInfo[] parameterInfos = b.method.GetParameters();
                                    if(args.Length - 1 != parameterInfos.Length){
                                        Error($"Invalid parameters!");
                                        BindingSyntax(b);
                                        return;
                                    }
                                    object[] parameters = new object[parameterInfos.Length];
                                    for(int i = 0; i < parameterInfos.Length; i++){
                                        parameters[i] = Convert.ChangeType(args[i + 1], parameterInfos[i].ParameterType);
                                    }
                                    if(b.method.IsStatic){
                                        b.method.Invoke(null, parameters);
                                    }
                                    else{
                                        object[] instances = ui.FindBindingInstances(b.type);
                                        foreach(object o in instances)
                                            b.method.Invoke(o, parameters);
                                        Log($"Executed binding '{b.attribute.Command}' on {instances.Length} instances!");
                                    }
                                    return;
                                }
                            Error($"Unknown binding '{args[0]}'");
                        }
                        else
                            Syntax("exec <binding>");
                    break;
                    default:
                        foreach(ICommand c in commands)
                            if(c.Key == command){
                                c.Execute(this, args);
                                return;
                            }
                        Error($"Unknown command '{command}'");
                    break;
                }
            }
            catch{
                Error("Invalid Syntax!");
            }
        }
        public void Log(string log){
            history.Enqueue($"<color=#888888>{log}</color>");
            if(history.Count > historyMax)
                history.Dequeue();
            FormatHistory();
        }
        private void LogCommand(string command){
            history.Enqueue($"<color=#9fd0e3>{command}</color>");
            if(history.Count > historyMax)
                history.Dequeue();
            FormatHistory();
        }
        private void LogBinding(string binding){
            history.Enqueue($"<color=#bb9fe3>{binding}</color>");
            if(history.Count > historyMax)
                history.Dequeue();
            FormatHistory();
        }
        public void Warning(string warning){
            history.Enqueue($"<color=#e6b737>{warning}</color>");
            if(history.Count > historyMax)
                history.Dequeue();
            FormatHistory();
        }
        public void Error(string error){
            history.Enqueue($"<color=#e34949>{error}</color>");
            if(history.Count > historyMax)
                history.Dequeue();
            FormatHistory();
        }
        public void Syntax(string syntax){
            history.Enqueue($"<color=#94e637>{syntax}</color>");
            if(history.Count > historyMax)
                history.Dequeue();
            FormatHistory();
        }
        private void BindingSyntax(Binding b){
            string syntax = $"exec {b.attribute.Command}";
            foreach(ParameterInfo pi in b.method.GetParameters())
                syntax += $" <{pi.ParameterType.Name}>";
            Syntax(syntax);
        }
    }
}