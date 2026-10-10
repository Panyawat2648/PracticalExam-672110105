#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;
namespace PanyawatHainak {
public class Stage1Checks:MonoBehaviour {
    readonly List<string> results=new(); readonly List<string> errors=new();
    InputSettings original;Keyboard keyboard;
    Stage1Game Game=>FindAnyObjectByType<Stage1Game>();
    CapsuleMotor Player=>Game.player;
    Rigidbody Body=>Player.GetComponent<Rigidbody>();
    void Awake(){
        original=InputSystem.settings;InputSystem.settings=Instantiate(original);
        InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
        Application.runInBackground=true;keyboard=Keyboard.current??InputSystem.AddDevice<Keyboard>();Application.logMessageReceived+=Log;
    }
    void Log(string m,string s,LogType t){if(t==LogType.Error||t==LogType.Exception)errors.Add(m);}
    void Check(bool ok,string msg){if(!ok)throw new Exception(msg);results.Add("PASS: "+msg);File.WriteAllLines("Evidence/Stage1/verification.txt",results);}
    void Keys(params Key[] keys){InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));}
    IEnumerator Start(){
        var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0){
            object next=null;bool more;
            try{more=stack.Peek().MoveNext();if(more)next=stack.Peek().Current;}catch(Exception e){results.Add("FAIL: "+e);Finish(1);yield break;}
            if(!more){stack.Pop();continue;}if(next is IEnumerator routine)stack.Push(routine);else yield return next;
        }
        results.AddRange(errors.Select(e=>"ERROR: "+e));Finish(errors.Count>0?1:0);
    }
    void Finish(int code){Keys();Application.logMessageReceived-=Log;if(original!=null)InputSystem.settings=original;File.WriteAllLines("Evidence/Stage1/verification.txt",results);Debug.Log(code==0?"STAGE1_PLAY_OK":"STAGE1_PLAY_FAIL");if(Application.isBatchMode)EditorApplication.Exit(code);else EditorApplication.isPlaying=false;}
    IEnumerator Reload(){Keys();SceneManager.LoadScene("Stage1");yield return new WaitForSeconds(.4f);}
    IEnumerator Collect(ItemEffect effect){
        var item=FindObjectsByType<Stage1Item>(FindObjectsSortMode.None).First(i=>i.effect==effect);
        Body.position=item.transform.position+Vector3.up*.1f;Body.linearVelocity=Vector3.zero;Physics.SyncTransforms();
        yield return new WaitForSeconds(.15f);
        Check(!item.gameObject.activeSelf,effect+" collected by collision and disabled");
    }
    IEnumerator WalkTo(Vector3 target,float yaw){
        Body.rotation=Quaternion.Euler(0,yaw,0);Body.angularVelocity=Vector3.zero;
        yield return new WaitForFixedUpdate();Keys(Key.W,Key.LeftShift);
        float deadline=Time.time+6;
        while(Vector2.Distance(new Vector2(Body.position.x,Body.position.z),new Vector2(target.x,target.z))>.22f){
            if(Time.time>deadline)throw new Exception("Walking route failed at "+Body.position+" toward "+target);
            yield return new WaitForFixedUpdate();
        }
        Keys();yield return new WaitForSeconds(.06f);
        Check(Body.position.y>.8f,"Crossed bridge to "+target+" without falling");
    }
    IEnumerator Run(){
        yield return new WaitForSeconds(.4f);
        Check(Game.seconds==30&&Game.Remaining>28&&Game.Remaining<30,"Starts with 30-second countdown");
        Check(Player.GetComponent<CapsuleCollider>()&&Body.useGravity,"Capsule collider and dynamic Rigidbody gravity");
        Check(FindObjectsByType<Stage1Item>(FindObjectsSortMode.None).Count(i=>i.effect==ItemEffect.ScorePlus)==36,"36 +500 coins provide more than target score");
        Check(FindObjectsByType<UnityEngine.ProBuilder.ProBuilderMesh>(FindObjectsSortMode.None).Length>=20,"Level built with actual ProBuilder meshes");
        Check(FindObjectsByType<Button>(FindObjectsSortMode.None).All(b=>!b.name.Contains("(test)")),"No manual Win/Lose buttons in gameplay");
        Game.enabled=false;
        float y=Body.position.y;Keys(Key.Space);yield return new WaitForSeconds(.2f);Keys();
        Check(Body.position.y>y+.35f,"Space jumps");
        yield return new WaitForSeconds(1.3f);Check(Player.Grounded,"Capsule returns to ground at "+Body.position);
        Keys(Key.D);yield return new WaitForSeconds(.3f);Keys();yield return new WaitForSeconds(.1f);
        Check(Body.rotation.eulerAngles.y>25,"D turns Capsule");
        Body.rotation=Quaternion.identity;
        int points=Game.Points;yield return Collect(ItemEffect.ScorePlus);Check(Game.Points==points+500,"Score pickup adds exactly 500");
        yield return new WaitForSeconds(.2f);Check(Game.Points==points+500,"Collected item cannot apply twice");
        points=Game.Points;yield return Collect(ItemEffect.ScoreMinus);Check(Game.Points==points-500,"Penalty removes exactly 500");
        float time=Game.Remaining;yield return Collect(ItemEffect.TimePlus);Check(Mathf.Abs(Game.Remaining-time-3)<.01f,"Time bonus adds exactly 3 seconds");
        time=Game.Remaining;yield return Collect(ItemEffect.TimeMinus);Check(Mathf.Abs(Game.Remaining-time+5)<.01f,"Time penalty removes exactly 5 seconds");
        yield return Reload();
        Check(Game.Points==0&&FindObjectsByType<Stage1Item>(FindObjectsSortMode.None).Length==47,"Restart restores score and all 47 items");
        Capture("Stage1-start.png");
        yield return WalkTo(new Vector3(0,0,20),0);
        yield return WalkTo(new Vector3(20,0,20),90);
        yield return WalkTo(new Vector3(20,0,0),180);
        yield return WalkTo(Vector3.zero,270);
        Check(Game.Points>10000,"Playable full loop collects enough coins to win: "+Game.Points);
        Check(Game.Remaining>0&&SceneManager.GetActiveScene().name=="Stage1","Exceeding target does not end run before timeout");
        Capture("Stage1-route.png");
        int final=Game.Points;Game.Advance(Game.Remaining+1);yield return new WaitForSeconds(.3f);
        Check(SceneManager.GetActiveScene().name=="Win"&&ExamScreen.Score==final,"Timeout above 10000 goes to Win with final score");Capture("Stage1-win.png");
        yield return Reload();Game.Advance(31);yield return new WaitForSeconds(.3f);
        Check(SceneManager.GetActiveScene().name=="GameOver","Timeout below target loses");
        FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name=="Try Again").onClick.Invoke();yield return new WaitForSeconds(.3f);
        Check(Game&&Game.Points==0&&Game.Remaining>29,"Try Again starts fresh Stage1");
        Game.enabled=false;for(int i=0;i<20;i++)Game.Apply(ItemEffect.ScorePlus);
        Game.Advance(31);yield return new WaitForSeconds(.3f);
        Check(SceneManager.GetActiveScene().name=="GameOver"&&ExamScreen.Score==10000,"Exactly 10000 loses under documented strict greater-than rule");
        yield return Reload();Body.position=new Vector3(50,-9,50);yield return new WaitForSeconds(.3f);
        Check(SceneManager.GetActiveScene().name=="GameOver"&&ExamScreen.ResultReason.Contains("Fell"),"Falling below level loses immediately");
        yield return Reload();Game.Advance(Game.Remaining-4);Game.Apply(ItemEffect.TimeMinus);yield return new WaitForSeconds(.3f);
        Check(SceneManager.GetActiveScene().name=="GameOver","Time penalty reaching zero resolves game immediately");
        yield return Reload();FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name=="Back").onClick.Invoke();yield return new WaitForSeconds(.3f);
        Check(SceneManager.GetActiveScene().name=="StageSelection","Back returns to Stage Selection");
    }
    void Capture(string name){
        var camera=Camera.main;var canvas=FindAnyObjectByType<Canvas>();
        var rt=new RenderTexture(1280,720,24);var prior=RenderTexture.active;
        canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
        camera.targetTexture=rt;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;
        var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
        File.WriteAllBytes("Evidence/Stage1/"+name,image.EncodeToPNG());Destroy(image);camera.targetTexture=null;RenderTexture.active=prior;rt.Release();Destroy(rt);
        canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;
    }
}
}
#endif