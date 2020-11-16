using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace DevConsole{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class BindingAttribute : Attribute
    {
        public string Command {get; set;}
        public BindingAttribute(string command){
            Command = command;
        }
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class ValueBindingAttribute : Attribute
    {
        public string Key {get; set;}
        public ValueBindingAttribute(string key){
            Key = key;
        }
    }

    public class Binding{
        public BindingAttribute attribute;
        public Type type;
        public MethodInfo method;
    }

    public class ValueBinding{
        public ValueBindingAttribute attribute;
        public Type type;
        public bool isProperty;
        public FieldInfo field;
        public PropertyInfo property;
    }
}