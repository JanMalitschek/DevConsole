using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

namespace DevConsole.Commands.Time{
    public class DeltaTime : ICommand{
        public string Key{
            get{
                return "Time.deltaTime";
            }
        }
        public string Syntax{
            get{
                return "Time.deltaTime";
            }
        }
        public string Description{
            get{
                return "Logs the current deltaTime.";
            }
        }
        public void Execute(Console console, params string[] args){
            console.Log(UnityEngine.Time.deltaTime.ToString());
        }
    }
    public class UnscaledDeltaTime : ICommand{
        public string Key{
            get{
                return "Time.unscaledDeltaTime";
            }
        }
        public string Syntax{
            get{
                return "Time.unscaledDeltaTime";
            }
        }
        public string Description{
            get{
                return "Logs the current uncaledDeltaTime.";
            }
        }
        public void Execute(Console console, params string[] args){
            console.Log(UnityEngine.Time.unscaledDeltaTime.ToString());
        }
    }
    public class FixedDeltaTime : ICommand{
        public string Key{
            get{
                return "Time.fixedDeltaTime";
            }
        }
        public string Syntax{
            get{
                return "Time.fixedDeltaTime";
            }
        }
        public string Description{
            get{
                return "Logs the current fixedDeltaTime.";
            }
        }
        public void Execute(Console console, params string[] args){
            console.Log(UnityEngine.Time.fixedDeltaTime.ToString());
        }
    } 
    public class FixedUnscaledDeltaTime : ICommand{
        public string Key{
            get{
                return "Time.fixedUnscaledDeltaTime";
            }
        }
        public string Syntax{
            get{
                return "Time.fixedUnscaledDeltaTime";
            }
        }
        public string Description{
            get{
                return "Logs the current fixedUnscaledDeltaTime.";
            }
        }
        public void Execute(Console console, params string[] args){
            console.Log(UnityEngine.Time.fixedUnscaledDeltaTime.ToString());
        }
    }  
    public class Time : ICommand{
        public string Key{
            get{
                return "Time.time";
            }
        }
        public string Syntax{
            get{
                return "Time.time";
            }
        }
        public string Description{
            get{
                return "Logs the current time.";
            }
        }
        public void Execute(Console console, params string[] args){
            console.Log(UnityEngine.Time.time.ToString());
        }
    }  
    public class UnscaledTime : ICommand{
        public string Key{
            get{
                return "Time.unscaledTime";
            }
        }
        public string Syntax{
            get{
                return "Time.unscaledTime";
            }
        }
        public string Description{
            get{
                return "Logs the current unscaledTime.";
            }
        }
        public void Execute(Console console, params string[] args){
            console.Log(UnityEngine.Time.unscaledTime.ToString());
        }
    } 
    public class FixedTime : ICommand{
        public string Key{
            get{
                return "Time.fixedTime";
            }
        }
        public string Syntax{
            get{
                return "Time.fixedTime";
            }
        }
        public string Description{
            get{
                return "Logs the current fixedTime.";
            }
        }
        public void Execute(Console console, params string[] args){
            console.Log(UnityEngine.Time.fixedTime.ToString());
        }
    }  
    public class FixedUnscaledTime : ICommand{
        public string Key{
            get{
                return "Time.fixedUnscaledTime";
            }
        }
        public string Syntax{
            get{
                return "Time.fixedUnscaledTime";
            }
        }
        public string Description{
            get{
                return "Logs the current fixedUnscaledTime.";
            }
        }
        public void Execute(Console console, params string[] args){
            console.Log(UnityEngine.Time.fixedUnscaledTime.ToString());
        }
    }
    public class TimeScale : ICommand{
        public string Key{
            get{
                return "Time.timeScale";
            }
        }
        public string Syntax{
            get{
                return "Time.timeScale";
            }
        }
        public string Description{
            get{
                return "Logs the current timeScale.";
            }
        }
        public void Execute(Console console, params string[] args){
            console.Log(UnityEngine.Time.timeScale.ToString());
        }
    } 
    public class SetTimeScale : ICommand{
        public string Key{
            get{
                return "Time.SetTimeScale";
            }
        }
        public string Syntax{
            get{
                return "Time.SetTimeScale <timeScale>";
            }
        }
        public string Description{
            get{
                return "Sets the current timeScale.";
            }
        }
        public void Execute(Console console, params string[] args){
            if(args.Length == 0)
                UnityEngine.Time.timeScale = 1.0f;
            else
                UnityEngine.Time.timeScale = System.Convert.ToSingle(args[0], CultureInfo.InvariantCulture);
        }
    } 
    public class TimeSinceLevelLoad : ICommand{
        public string Key{
            get{
                return "Time.timeSinceLevelLoad";
            }
        }
        public string Syntax{
            get{
                return "Time.timeSinceLevelLoad";
            }
        }
        public string Description{
            get{
                return "Logs the current timeSinceLevelLoad.";
            }
        }
        public void Execute(Console console, params string[] args){
            console.Log(UnityEngine.Time.timeSinceLevelLoad.ToString());
        }
    } 
    public class Framerate : ICommand{
        public string Key{
            get{
                return "Time.frameRate";
            }
        }
        public string Syntax{
            get{
                return "Time.frameRate";
            }
        }
        public string Description{
            get{
                return "Logs the current frame rate.";
            }
        }
        public void Execute(Console console, params string[] args){
            console.Log((1.0f / UnityEngine.Time.unscaledDeltaTime).ToString());
        }
    } 
}