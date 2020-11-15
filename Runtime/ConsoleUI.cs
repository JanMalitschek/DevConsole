using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DevConsole{
    public class ConsoleUI : MonoBehaviour
    {
        private GUISkin skin;
        private Console console;
        private string currentCommand = string.Empty;
        private Vector2 scrollPosition = new Vector2(0.0f, 800.0f);

        private bool uiVisible = false;
        private bool toggled = false;

        private void Awake() {
            skin = Resources.Load("ConsoleSkin") as GUISkin;
            console = new Console();
        }

        private void OnGUI() {
            Event e = Event.current;  
            if(e.isKey){
                if(!toggled && (e.keyCode == KeyCode.Backslash || e.keyCode == KeyCode.Tilde)){
                    toggled = true;
                    uiVisible = !uiVisible;
                    if(uiVisible)
                        GUI.FocusControl("CommandLine");
                }
                if(toggled && e.keyCode == KeyCode.None)
                    toggled = false;
            }
            if(uiVisible){
                GUI.skin = skin;
                scrollPosition = GUI.BeginScrollView(new Rect(0, 0, Screen.width, Screen.height / 3.0f), scrollPosition, new Rect(0, 0, Screen.width - 20, 800));
                GUI.TextArea(new Rect(0, 0, Screen.width, 800), console.History);
                GUI.EndScrollView();
                GUI.SetNextControlName("CommandLine");
                currentCommand = GUI.TextArea(new Rect(0, Screen.height / 3.0f, Screen.width, 20.0f), currentCommand);
                if(currentCommand.Length >= 2 && currentCommand.Last() == '\n'){
                    currentCommand = currentCommand.Remove(currentCommand.Length - 1);
                    console.SubmitCommand(currentCommand, this);
                    currentCommand = string.Empty;
                }
                if(toggled)
                    GUI.FocusControl("CommandLine");
            }
        }   

        public object[] FindBindingInstances(System.Type t){
            return FindObjectsOfType(t);
        }
    }
}