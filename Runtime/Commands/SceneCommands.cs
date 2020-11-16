using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevConsole.Commands.Scene{
    public class ListScenes : ICommand {
        public string Key{
            get{
                return "Scene.list";
            }
        }
        public string Syntax{
            get{
                return "Scene.list <option>\n"
                        + "\t-b\tAll scenes in the build settings.\n"
                        + "\t-l default\tAll scenes currently loaded.";
            }
        }
        public string Description{
            get{
                return "List all scenes specified in the build settings.";
            }
        }
        public void Execute(Console console, params string[] args){
            if(CommandUtil.IsOptionSet("-b", args))
                for(int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                    console.Log($"{i} - {System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i))}");
            else if(CommandUtil.IsOptionSet("-l", args) || CommandUtil.GetNumSetOptions(args) == 0)
                for(int i = 0; i < SceneManager.sceneCount; i++)
                    console.Log($"{i} - {SceneManager.GetSceneAt(i).name}");
        }
    }  

    public class LoadScene : ICommand{
        public string Key{
            get{
                return "Scene.load";
            }
        }
        public string Syntax{
            get{
                return "Scene.load <scene> <option>\n"
                        + "\t-s default\t Load a single scene.\n"
                        + "\t-a\t Load a scene additively.";
            }
        }
        public string Description{
            get{
                return "Loads the specified scene.";
            }
        }
        public void Execute(Console console, params string[] args){
            if(args.Length == 0){
                console.Syntax(Syntax);
                return;
            }
            if(CommandUtil.IsOptionSet("-s", args) || CommandUtil.GetNumSetOptions(args) == 0)
                SceneManager.LoadScene(args[0], LoadSceneMode.Single);
            else if(CommandUtil.IsOptionSet("-a", args))
                SceneManager.LoadScene(args[0], LoadSceneMode.Additive);
        }
    }  

    public class ReloadScene : ICommand{
        public string Key{
            get{
                return "Scene.reload";
            }
        }
        public string Syntax{
            get{
                return "Scene.reload";
            }
        }
        public string Description{
            get{
                return "Reloads the active scene.";
            }
        }
        public void Execute(Console console, params string[] args){
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }  

    public class MakeSceneActive : ICommand{
        public string Key{
            get{
                return "Scene.setActive";
            }
        }
        public string Syntax{
            get{
                return "Scene.setActive <scene>";
            }
        }
        public string Description{
            get{
                return "Sets the specified scene to be the active one.";
            }
        }
        public void Execute(Console console, params string[] args){
            if(args.Length == 0){
                console.Syntax(Syntax);
                return;
            }
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(args[0]));
        }
    }  
}