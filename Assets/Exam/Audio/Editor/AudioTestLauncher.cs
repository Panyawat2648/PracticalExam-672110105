using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace PanyawatHainak.Editor {
[InitializeOnLoad] public static class AudioTestLauncher {
    static AudioTestLauncher(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("ExamAudioTest",false)){SessionState.SetBool("ExamAudioTest",false);var go=new GameObject("Audio checks");Object.DontDestroyOnLoad(go);go.AddComponent<AudioChecks>();}};}
    public static void Run(){EditorSceneManager.OpenScene("Assets/Exam/Scenes/MainMenu.unity");SessionState.SetBool("ExamAudioTest",true);EditorApplication.isPlaying=true;}
}
}
