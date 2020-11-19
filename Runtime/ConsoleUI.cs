using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace DevConsole{
    public class ConsoleUI : MonoBehaviour
    {
        private GUISkin skin;
        private Console console;
        private string currentCommand = string.Empty;
        private Vector2 scrollPosition = new Vector2(0.0f, 800.0f);
        private string[] autoCompleteResults = new string[0];
        private int autoCompleteIdx = -1;

        private bool uiVisible = false;
        private bool toggled = false;
        private bool autoCompleted = false;

        private void Awake() {
            skin = Resources.Load("ConsoleSkin") as GUISkin;
            console = new Console();
        }

        private void OnGUI() {
            Event e = Event.current;  
            if(e.isKey && e.type == EventType.KeyDown){
                if((e.keyCode == KeyCode.Backslash || e.keyCode == KeyCode.Tilde)){
                    uiVisible = !uiVisible;
                    toggled = true;
                    if(uiVisible)
                        GUI.FocusControl("CommandLine");
                }
            }
            if(uiVisible){
                GUI.skin = skin;
                scrollPosition = GUI.BeginScrollView(new Rect(0, 0, Screen.width, Screen.height / 3.0f), scrollPosition, new Rect(0, 0, Screen.width - 20, 800));
                GUI.TextArea(new Rect(0, 0, Screen.width, 800), console.History);
                GUI.EndScrollView();

                if(e.isKey && e.type == EventType.KeyDown){
                    if(autoCompleteResults.Length == 0){
                        if(e.keyCode == KeyCode.UpArrow)
                            currentCommand = console.GetPreviousCommand();
                        else if(e.keyCode == KeyCode.DownArrow)
                            currentCommand = console.GetNextCommand();
                        else if(e.keyCode == KeyCode.Return && currentCommand.Length >= 1){
                            SubmitCommand();
                            e.Use();
                            return;
                        }
                    }
                    else{
                        if(e.keyCode == KeyCode.UpArrow)
                            autoCompleteIdx = Mathf.Max(autoCompleteIdx - 1, -1);
                        else if(e.keyCode == KeyCode.DownArrow)
                            autoCompleteIdx = Mathf.Min(autoCompleteIdx + 1, autoCompleteResults.Length - 1);
                        else if(e.keyCode == KeyCode.Return && autoCompleteIdx >= 0){
                            currentCommand = autoCompleteResults[autoCompleteIdx];
                            currentCommand = Commands.CommandUtil.Clean(currentCommand);
                            autoCompleted = true;
                            autoCompleteIdx = -1;
                            e.Use();
                            return;
                        }
                        else if(e.keyCode == KeyCode.Return && autoCompleteIdx == -1){
                            SubmitCommand();
                            e.Use();
                            return;
                        }
                    }
                }

                GUI.SetNextControlName("CommandLine");
                currentCommand = GUI.TextArea(new Rect(0, Screen.height / 3.0f, Screen.width, 20.0f), currentCommand);
                for(int i = 0; i < autoCompleteResults.Length; i++){
                    if(autoCompleteIdx == i)
                        GUI.backgroundColor = Color.red;
                    GUI.Box(new Rect(0, Screen.height / 3.0f + 20.0f + 20.0f * i, Screen.width, 20.0f), autoCompleteResults[i]);
                    GUI.backgroundColor = Color.white;
                }
                if(autoCompleted){
                    TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                    te.MoveTextEnd();
                    autoCompleted = false;
                }

                if(GUI.changed){
                    currentCommand = Commands.CommandUtil.Clean(currentCommand);
                    if(currentCommand.Trim(' ').Length > 0){
                        autoCompleteResults = console.CompleteInput(currentCommand);
                    }
                    else if(currentCommand.Trim(' ').Length == 0){
                        autoCompleteResults = new string[0];
                    }
                }

                if(toggled){
                    GUI.FocusControl("CommandLine");
                    toggled = false;
                }
            }
        }   

        public void SubmitCommand(){
            currentCommand = Commands.CommandUtil.Clean(currentCommand);
            console.SubmitCommand(currentCommand, this);
            currentCommand = string.Empty;
        }

        public object[] FindBindingInstances(System.Type t){
            return FindObjectsOfType(t);
        }
    }
}