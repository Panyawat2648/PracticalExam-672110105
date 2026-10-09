#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEditor;
namespace PanyawatHainak {
public class NavigationChecks:MonoBehaviour {
    readonly List<string> results=new();
    readonly List<string> errors=new();
    void Awake(){Application.logMessageReceived+=Log;}
    void Log(string msg,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception)errors.Add(msg);}
    void Check(bool ok,string msg){if(!ok)throw new Exception(msg);results.Add("PASS: "+msg);}
    void Click(string scene,string name){
        var button=UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.gameObject.scene.name==scene&&b.name==name);
        Check(button.interactable,scene+" / "+name+" enabled");button.onClick.Invoke();
    }
    IEnumerator WaitScene(string name) {
        float timeout=Time.realtimeSinceStartup+8;
        while(SceneManager.GetActiveScene().name!=name) {if(Time.realtimeSinceStartup>timeout)throw new Exception("Timeout "+name);yield return null;}
        yield return null;yield return null;
        Check(UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length==1,"Exactly one EventSystem in "+name);
    }
    IEnumerator Start() {
        var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0){
            bool more=false;object next=null;
            try{more=stack.Peek().MoveNext();if(more)next=stack.Peek().Current;}
            catch(Exception e){results.Add("FAIL: "+e);Finish(1);yield break;}
            if(!more){stack.Pop();continue;}
            if(next is IEnumerator nested)stack.Push(nested);else yield return next;
        }
        results.AddRange(errors.Select(e=>"ERROR: "+e));Finish(errors.Count==0?0:1);
    }
    IEnumerator Run(){
        yield return WaitScene("MainMenu");
        Check(EditorBuildSettings.scenes.Count(s=>s.enabled)==8,"Eight enabled exam scenes");
        Click("MainMenu","Options");yield return new WaitForSeconds(.6f);
        Check(SceneManager.sceneCount==2&&SceneManager.GetSceneByName("Options").isLoaded,"Options loads additively");
        Check(SceneManager.GetActiveScene().name=="MainMenu","Options preserves Main Menu");
        Check(UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length==1,"Options shares one EventSystem");
        Click("MainMenu","Options");yield return null;Check(SceneManager.sceneCount==2,"Repeated Options does not duplicate scene");
        Click("Options","OK");yield return new WaitForSeconds(.4f);
        Check(!SceneManager.GetSceneByName("Options").isLoaded,"OK unloads Options");
        Click("MainMenu","Credits");yield return WaitScene("Credits");
        Check(UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None).Any(t=>t.text.Contains("672110105")),"Credits displays student code");
        Click("Credits","Back");yield return WaitScene("MainMenu");
        Click("MainMenu","Start");yield return WaitScene("StageSelection");
        Click("StageSelection","Back");yield return WaitScene("MainMenu");
        foreach(var stage in new[]{"Stage1","Stage2"}) {
            Click("MainMenu","Start");yield return WaitScene("StageSelection");
            string label=stage=="Stage1"?"Stage 1 - Capsule Player":"Stage 2 - Humanoid Player";
            Click("StageSelection",label);yield return WaitScene(stage);
            Click(stage,"Back");yield return WaitScene("StageSelection");
            Click("StageSelection",label);yield return WaitScene(stage);
            Click(stage,"Lose (test)");yield return WaitScene("GameOver");
            Click("GameOver","Try Again");yield return WaitScene(stage);
            Click(stage,"Win (test)");yield return WaitScene("Win");
            Click("Win","Back To Main Menu");yield return WaitScene("MainMenu");
            Click("MainMenu","Start");yield return WaitScene("StageSelection");
            Click("StageSelection",label);yield return WaitScene(stage);
            Click(stage,"Lose (test)");yield return WaitScene("GameOver");
            Click("GameOver","Back To Main Menu");yield return WaitScene("MainMenu");
        }
        var canvas=UnityEngine.Object.FindAnyObjectByType<Canvas>();
        var camera=Camera.main;
        var rt=new RenderTexture(1280,720,24);
        canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
        camera.targetTexture=rt;Canvas.ForceUpdateCanvases();camera.Render();
        var previous=RenderTexture.active;RenderTexture.active=rt;
        var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
        image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
        File.WriteAllBytes("Evidence/MainMenu.png",image.EncodeToPNG());
        RenderTexture.active=previous;camera.targetTexture=null;
        canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;
        rt.Release();Destroy(rt);Destroy(image);
        yield return new WaitForSeconds(.5f);
        Check(UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Any(b=>b.name=="Exit"),"Exit button available (quits Play Mode in Editor / application in build)");
    }
    void Finish(int code){
        Application.logMessageReceived-=Log;
        File.WriteAllLines("Evidence/navigation-checks.txt",results);
        Debug.Log(code==0?"EXAM_NAVIGATION_PASS":"EXAM_NAVIGATION_FAIL");
        if(Application.isBatchMode)EditorApplication.Exit(code);else EditorApplication.isPlaying=false;
    }
}
}
#endif