using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace DevConsole{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class BindingAttribute : Attribute
    {
        public string Command {get; private set;}
        public BindingAttribute(string command){
            Command = command;
        }
    }

    public class Binding{
        public BindingAttribute attribute;
        public Type type;
        public MethodInfo method;
    }
}