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
        private List<ValueBinding> valueBindings = new List<ValueBinding>();
        private Queue<string> history = new Queue<string>();
        private Queue<string> commandHistory = new Queue<string>();
        private int commandHistoryPointer = 0;
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
                            if(mi.GetBaseDefinition() != mi)
                                continue;
                            BindingAttribute attribute = mi.GetCustomAttribute<BindingAttribute>();
                            if(attribute != null){
                                attribute.Command = $"{t.Name}.{attribute.Command}";
                                bool parametersPrimitive = true;
                                foreach(ParameterInfo pi in mi.GetParameters()){
                                    if(!pi.ParameterType.IsPrimitive && pi.ParameterType != typeof(string)){
                                        parametersPrimitive = false;
                                        break;
                                    }
                                }
                                if(!parametersPrimitive){
                                    Error($"Skipped binding '{attribute.Command}'! Method must not only have primitive or string parameters!");
                                    continue;
                                }
                                bindings.Add(new Binding{attribute = attribute, type = t, method = mi});
                            }
                        }
                        foreach(FieldInfo fi in t.GetFields()){
                            if(fi.DeclaringType != t)
                                continue;
                            ValueBindingAttribute attribute = fi.GetCustomAttribute<ValueBindingAttribute>();
                            if(attribute != null){
                                attribute.Key = $"{t.Name}.{attribute.Key}";
                                if(!fi.FieldType.IsPrimitive && fi.FieldType != typeof(string)){
                                    Error($"Skipped value binding '{attribute.Key}'! Field/Property type must be primitive or string!");
                                    continue;
                                }
                                valueBindings.Add(new ValueBinding{attribute = attribute, type = t, isProperty = false, field = fi, property = null});
                            }
                        }
                        foreach(PropertyInfo pi in t.GetProperties()){
                            if(pi.DeclaringType != t)
                                continue;
                            ValueBindingAttribute attribute = pi.GetCustomAttribute<ValueBindingAttribute>();
                            if(attribute != null){
                                attribute.Key = $"{t.Name}.{attribute.Key}";
                                if(!pi.PropertyType.IsPrimitive && pi.PropertyType != typeof(string)){
                                    Error($"Skipped value binding '{attribute.Key}'! Field/Property type must be primitive or string!");
                                    continue;
                                }
                                valueBindings.Add(new ValueBinding{attribute = attribute, type = t, isProperty = true, field = null, property = pi});
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
            commandHistory.Enqueue(command);
            if(commandHistory.Count > historyMax)
                commandHistory.Dequeue();
            commandHistoryPointer = commandHistory.Count - 1;
            string[] args = command.Split(' ');
            command = args[0];
            var tempArgs = new List<string>(args);
            tempArgs.RemoveAt(0);
            args = tempArgs.ToArray();
            //Builtin Commands
            try{
                switch(command){
                    case "help":
                        Log("Basic Builtin Commands");
                        Log("\tType 'commands' to get a list of all available commands!");
                        Log("\tType 'bindings' to get a list of all available bindings!");
                        Log("\tType 'desc' followed by a command to get a description of its functionality!");
                        Log("\tType 'syntax' followed by a command to view its syntax!");
                    break;
                    case "clear":
                        history.Clear();
                        FormatHistory();
                    break;
                    case "history":
                        Log("");
                        foreach(string s in commandHistory)
                            Log(s);
                    break;
                    case "commands":
                        Log("Builtin Commands");
                        LogCommand("\thelp");
                        LogCommand("\tclear");
                        LogCommand("\thistory");
                        LogCommand("\tcommands");
                        LogCommand("\tsyntax");
                        LogCommand("\tdesc");
                        LogCommand("\tinstances");
                        Log("Custom Commands");
                        foreach(ICommand c in commands)
                            LogCommand("\t" + c.Key);
                    break;
                    case "bindings":
                        Log("Method Bindings");
                        foreach(Binding b in bindings)
                            LogBinding("\t" + b.attribute.Command);
                        Log("Value Bindings");
                        foreach(ValueBinding b in valueBindings)
                            LogBinding("\t" + b.attribute.Key);
                    break;
                    case "syntax":
                        if(CommandUtil.DoesArgExist(0, args)){
                            foreach(ICommand c in commands)
                                if(c.Key == args[0]){
                                    Syntax(c.Syntax);
                                    return;
                                }
                            foreach(Binding b in bindings){
                                if(b.attribute.Command == args[0]){
                                    BindingSyntax(b);
                                    return;
                                }
                            }
                            foreach(ValueBinding b in valueBindings){
                                if(b.attribute.Key == args[0]){
                                    BindingSyntax(b);
                                    return;
                                }
                            }
                            switch(args[0]){
                                case "help": Syntax("help"); break;
                                case "clear": Syntax("clear"); break;
                                case "history": Syntax("history"); break;
                                case "commands": Syntax("commands"); break;
                                case "syntax": Syntax("syntax <command/binding>"); break;
                                case "desc": Syntax("desc <command>"); break;
                                case "instances": Syntax("instances <binding>"); break;
                            }
                            Error($"Unknown command or binding '{args[0]}'");
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
                                case "history": Log("Lists all your previously submitted commands."); break;
                                case "commands": Log("Lists all the available commands."); break;
                                case "syntax": Log("Shows the syntax for the specified command."); break;
                                case "desc": Log("Shows the description for the specified command."); break;
                                case "instances": Log("Lists all available instances for a non-static binding."); break;
                            }
                            Error($"No description available for '{args[0]}'");
                        }
                        else
                            Syntax("desc <command>");
                    break;
                    case "instances":
                        if(CommandUtil.DoesArgExist(0, args)){
                            foreach(Binding b in bindings)
                                if(b.attribute.Command == args[0]){
                                    if(b.method.IsStatic){
                                        Error($"Binding '{b.attribute.Command}' is static!");
                                        return;
                                    }
                                    else{
                                        Log($"Available Instances for binding '{b.attribute.Command}'");
                                        object[] instances = ui.FindBindingInstances(b.type);
                                        foreach(object o in instances)
                                            Log("\t" + (o as UnityEngine.Object).name);
                                    }
                                    return;
                                }
                            foreach(ValueBinding b in valueBindings)
                                if(b.attribute.Key == args[0]){
                                    if(b.isProperty){
                                        if(b.property.GetSetMethod().IsStatic){
                                            Error($"Binding '{b.attribute.Key}' is static!");
                                            return;
                                        }
                                        else{
                                            Log($"Available Instances for binding '{b.attribute.Key}'");
                                            object[] instances = ui.FindBindingInstances(b.type);
                                            foreach(object o in instances)
                                                Log("\t" + (o as UnityEngine.Object).name);
                                        }
                                    }
                                    else{
                                        if(b.field.IsStatic){
                                            Error($"Binding '{b.attribute.Key}' is static!");
                                            return;
                                        }
                                        else{
                                            Log($"Available Instances for binding '{b.attribute.Key}'");
                                            object[] instances = ui.FindBindingInstances(b.type);
                                            foreach(object o in instances)
                                                Log("\t" + (o as UnityEngine.Object).name);
                                        }
                                    }
                                    return;
                                }
                            Error($"Unknown command or binding '{args[0]}'");
                        }
                        else
                            Syntax("instances <binding>");
                    break;
                    default:
                        foreach(ICommand c in commands)
                            if(c.Key == command){
                                c.Execute(this, args);
                                return;
                            }
                        foreach(Binding b in bindings)
                            if(b.attribute.Command == command){
                                ParameterInfo[] parameterInfos = b.method.GetParameters();
                                if(args.Length < parameterInfos.Length){
                                    Error($"Invalid parameters!");
                                    BindingSyntax(b);
                                    return;
                                }
                                object[] parameters = new object[parameterInfos.Length];
                                for(int i = 0; i < parameterInfos.Length; i++)
                                    parameters[i] = Convert.ChangeType(args[i], parameterInfos[i].ParameterType);
                                if(b.method.IsStatic)
                                    b.method.Invoke(null, parameters);
                                else{
                                    object[] instances = ui.FindBindingInstances(b.type);

                                    List<string> argInstances = new List<string>();
                                    bool exclude = true;
                                    if(CommandUtil.IsOptionSet("-i", args)){
                                        for(int i = parameters.Length + 1; i < args.Length; i++)
                                            argInstances.Add(args[i]);
                                        exclude = false;
                                    }
                                    else if(CommandUtil.IsOptionSet("-e", args)){
                                        for(int i = parameters.Length + 1; i < args.Length; i++)
                                            argInstances.Add(args[i]);
                                    }
                                    else{
                                        for(int i = parameters.Length; i < args.Length; i++)
                                            argInstances.Add(args[i]);
                                    }

                                    int executions = 0;
                                    foreach(object o in instances){
                                        if((exclude && !argInstances.Contains((o as UnityEngine.Object).name)) || 
                                           (!exclude && argInstances.Contains((o as UnityEngine.Object).name))){
                                            b.method.Invoke(o, parameters);
                                            executions++;
                                           }
                                    }
                                    Log($"Executed binding '{b.attribute.Command}' on {executions} instances!");
                                }
                                return;
                            }
                        foreach(ValueBinding b in valueBindings){
                            if(b.attribute.Key == command){
                                object[] instances = ui.FindBindingInstances(b.type);
                                bool exclude = true;
                                if(CommandUtil.IsOptionSet("-i", args))
                                    exclude = false;
                                List<string> argInstances = new List<string>();
                                if(args.Length >= 2){
                                    int idx = args.Length - 1;
                                    while(!CommandUtil.IsArgOption(idx, args) && idx >= 1)
                                        argInstances.Add(args[idx--]);
                                }
                                //Get
                                if(args.Length == 0 || CommandUtil.IsArgOption(0, args)){
                                    if(b.isProperty){
                                        if(b.property.GetSetMethod().IsStatic){
                                            Log(b.property.GetValue(null).ToString());
                                            return;
                                        }
                                        else{
                                            foreach(object o in instances)
                                                if((exclude && !argInstances.Contains((o as UnityEngine.Object).name)) || 
                                                !exclude && argInstances.Contains((o as UnityEngine.Object).name))
                                                    Log($"{(o as UnityEngine.Object).name}\t{b.property.GetValue(o).ToString()}");
                                            return;
                                        }
                                    }
                                    else{
                                        if(b.field.IsStatic){
                                            Log(b.field.GetValue(null).ToString());
                                            return;
                                        }
                                        else{
                                            foreach(object o in instances)
                                                if((exclude && !argInstances.Contains((o as UnityEngine.Object).name)) || 
                                                !exclude && argInstances.Contains((o as UnityEngine.Object).name))
                                                    Log($"{(o as UnityEngine.Object).name}\t{b.field.GetValue(o).ToString()}");
                                            return;
                                        }
                                    }
                                }
                                else if(args.Length == 1 || CommandUtil.IsArgOption(1, args)){
                                    try{
                                    if(b.isProperty){
                                        if(b.property.GetSetMethod().IsStatic){
                                            b.property.SetValue(null, Convert.ChangeType(args[0], b.property.PropertyType));
                                            return;
                                        }
                                        else{
                                            int executions = 0;
                                            foreach(object o in instances){
                                                if((exclude && !argInstances.Contains((o as UnityEngine.Object).name)) || 
                                                !exclude && argInstances.Contains((o as UnityEngine.Object).name)){
                                                    b.property.SetValue(o, Convert.ChangeType(args[0], b.property.PropertyType));
                                                    executions++;
                                                }
                                            }
                                            Log($"Set value binding '{b.attribute.Key}' on {executions} instances!");
                                            return;
                                        }
                                    }
                                    else{
                                        if(b.field.IsStatic){
                                            b.field.SetValue(null, Convert.ChangeType(args[0], b.field.FieldType));
                                            return;
                                        }
                                        else{
                                            int executions = 0;
                                            foreach(object o in instances){
                                                if((exclude && !argInstances.Contains((o as UnityEngine.Object).name)) || 
                                                !exclude && argInstances.Contains((o as UnityEngine.Object).name)){
                                                    b.field.SetValue(o, Convert.ChangeType(args[0], b.field.FieldType));
                                                    executions++;
                                                }
                                            }
                                            Log($"Set value binding '{b.attribute.Key}' on {executions} instances!");
                                            return;
                                        }
                                    }
                                    }catch{
                                        Error($"Invalid arguments!");
                                        BindingSyntax(b);    
                                    }
                                }
                                else{
                                    Error($"Invalid syntax!");
                                    BindingSyntax(b);
                                }
                            }
                        }
                        Error($"Unknown command or binding '{command}'");
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
            string syntax = $"{b.attribute.Command}";
            foreach(ParameterInfo pi in b.method.GetParameters())
                syntax += $" <{pi.ParameterType.Name}>";
            syntax += " <option> <instance 0> <instance 1> ... <instance n>\n";
            syntax += "\t-i\tInclude only the following instances.\n";
            syntax += "\t-e\tExclude the following instances.\n";
            Syntax(syntax);
        }
        private void BindingSyntax(ValueBinding b){
            Log("To print the binding value");
            Syntax("\t" + b.attribute.Key + " <option> <instance 0> <instance 1> ... <instance n>");
            Log("To set the binding value");
            Syntax("\t" + b.attribute.Key + " <new value> <option> <instance 0> <instance 1> ... <instance n>\n\t-i\tInclude only the following instances.\n\t-e\tExclude the following instances.\n");
        }

        public string GetPreviousCommand(){
            if(commandHistory.Count == 0)
                return string.Empty;
            string result = commandHistory.ToArray()[commandHistoryPointer--];
            if(commandHistoryPointer < 0)
                commandHistoryPointer = 0; 
            return result;
        }
        public string GetNextCommand(){
            if(commandHistory.Count == 0)
                return string.Empty;
            string result = commandHistory.ToArray()[commandHistoryPointer++];
            if(commandHistoryPointer >= commandHistory.Count){
                commandHistoryPointer = commandHistory.Count - 1; 
                return string.Empty;
            }
            return result;            
        }
    }
}