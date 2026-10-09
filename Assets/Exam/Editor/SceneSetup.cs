using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using PanyawatHainak;
namespace PanyawatHainak.Editor {
public static class SceneSetup {
    public static readonly string[] Names={"MainMenu","StageSelection","Stage1","Stage2","Options","Credits","Win","GameOver"};
    public static void Build() {
        foreach(var name in Names) {
            var path="Assets/Exam/Scenes/"+name+".unity";
            if(File.Exists(path)) throw new Exception("Scene already exists; refusing to overwrite: "+path);
        }
        foreach(var name in Names) {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            new GameObject("Screen").AddComponent<ExamScreen>().screen=name;
            if(name!="Options") { var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)); camera.tag="MainCamera"; camera.GetComponent<Camera>().clearFlags=CameraClearFlags.SolidColor; camera.GetComponent<Camera>().backgroundColor=new Color(.035f,.055f,.085f); }
            EditorSceneManager.SaveScene(scene,"Assets/Exam/Scenes/"+name+".unity");
        }
        EditorBuildSettings.scenes=Names.Select(n=>new EditorBuildSettingsScene("Assets/Exam/Scenes/"+n+".unity",true)).ToArray();
        EditorSceneManager.OpenScene("Assets/Exam/Scenes/MainMenu.unity");
        AssetDatabase.SaveAssets(); Debug.Log("EXAM_SCENES_BUILD_OK");
    }
}
[InitializeOnLoad]
public static class NavigationTestLauncher {
    static NavigationTestLauncher() {
        EditorApplication.playModeStateChanged+=s=>{
            if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("ExamNavigationTest",false)) {
                SessionState.SetBool("ExamNavigationTest",false);
                var go=new GameObject("Navigation tests");UnityEngine.Object.DontDestroyOnLoad(go);
                go.AddComponent<NavigationChecks>();
            }
        };
    }
    public static void Run() {
        EditorSceneManager.OpenScene("Assets/Exam/Scenes/MainMenu.unity");
        SessionState.SetBool("ExamNavigationTest",true);EditorApplication.isPlaying=true;
    }
}
}