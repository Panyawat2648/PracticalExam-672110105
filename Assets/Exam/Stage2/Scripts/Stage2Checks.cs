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
using StarterAssets;
namespace PanyawatHainak {
public class Stage2Checks:MonoBehaviour {
    readonly List<string> results=new();readonly List<string> errors=new();
    InputSettings original;Keyboard keyboard;Mouse mouse;
    Stage2Game Game=>FindAnyObjectByType<Stage2Game>();
    Stage2Interactor Actor=>Game.player.GetComponent<Stage2Interactor>();
    Transform Player=>Game.player.transform;
    void Awake(){original=InputSystem.settings;InputSystem.settings=Instantiate(original);InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;Application.runInBackground=true;Time.captureDeltaTime=1f/60f;keyboard=InputSystem.AddDevice<Keyboard>("Stage2TestKeyboard");mouse=InputSystem.AddDevice<Mouse>("Stage2TestMouse");Application.logMessageReceived+=Log;}
    void Log(string m,string s,LogType t){if(t==LogType.Error||t==LogType.Exception)errors.Add(m);}
    void Check(bool ok,string msg){if(!ok)throw new Exception(msg);results.Add("PASS: "+msg);File.WriteAllLines("Evidence/Stage2/verification.txt",results);}
    void Keys(params Key[] keys){InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));}
    IEnumerator Start(){
        var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0){
            object next=null;bool more;
            try{more=stack.Peek().MoveNext();if(more)next=stack.Peek().Current;}catch(Exception e){results.Add("FAIL: "+e);Finish(1);yield break;}
            if(!more){stack.Pop();continue;}if(next is IEnumerator r)stack.Push(r);else yield return next;
        }
        results.AddRange(errors.Select(e=>"ERROR: "+e));Finish(errors.Count>0?1:0);
    }
    void Finish(int code){Time.captureDeltaTime=0;Keys();Application.logMessageReceived-=Log;if(original)InputSystem.settings=original;File.WriteAllLines("Evidence/Stage2/verification.txt",results);Debug.Log(code==0?"STAGE2_PLAY_OK":"STAGE2_PLAY_FAIL");if(Application.isBatchMode)EditorApplication.Exit(code);else EditorApplication.isPlaying=false;}
    void PairKeyboard(){var pi=Player.GetComponent<PlayerInput>();pi.neverAutoSwitchControlSchemes=true;pi.SwitchCurrentControlScheme("KeyboardMouse",keyboard,mouse);keyboard.MakeCurrent();mouse.MakeCurrent();}
    IEnumerator Reload(){Keys();SceneManager.LoadScene("Stage2");yield return new WaitForSeconds(.6f);PairKeyboard();}
    void Teleport(Vector3 p){var cc=Player.GetComponent<CharacterController>();cc.enabled=false;Player.position=p;cc.enabled=true;Physics.SyncTransforms();}
    IEnumerator PressE(){Keys(Key.E);yield return new WaitForSeconds(.08f);Keys();yield return new WaitForSeconds(.12f);}
    IEnumerator Effect(ItemEffect effect){
        var item=FindObjectsByType<Stage2Item>(FindObjectsSortMode.None).First(i=>i.effect==effect);
        Teleport(new Vector3(item.transform.position.x,.08f,item.transform.position.z));yield return new WaitForSeconds(.15f);
        int score=Game.Points;float time=Game.Remaining;Check(!item.Collected,"Walking into "+effect+" does not auto-collect");
        Check(Actor.Target==item,"Nearest item selected for "+effect);
        yield return PressE();Check(item.Collected&&!item.gameObject.activeSelf,"E collects "+effect+" once");
        float expected=effect==ItemEffect.TimePlus?3:effect==ItemEffect.TimeMinus?-5:0;
        int points=effect==ItemEffect.ScorePlus?500:effect==ItemEffect.ScoreMinus?-500:0;
        Check(Game.Points==score+points&&Mathf.Abs(Game.Remaining-time-expected)<.01f,"Exact score/time effect for "+effect);
        Check(!item.Collect(Actor),"Cannot collect "+effect+" twice");
    }
    IEnumerator WalkAxis(Key direction,float destination,bool x,bool collect){
        float deadline=Time.time+8;int pulse=0;
        while(true){
            float value=x?Player.position.x:Player.position.z;
            bool reached=direction==Key.A||direction==Key.S?value<=destination:value>=destination;
            if(reached)break;
            if(Time.time>deadline){Capture("Stage2-route-debug.png");throw new Exception("Route timeout "+direction+" toward "+destination+" at "+Player.position+" input="+Player.GetComponent<StarterAssetsInputs>().move+" action="+Player.GetComponent<PlayerInput>().actions["Move"].ReadValue<Vector2>()+" speed="+Player.GetComponent<Animator>().GetFloat("Speed")+" yaw="+Camera.main.transform.eulerAngles+" time="+Game.Remaining);}
            if(collect&&pulse%2==0)Keys(direction,Key.LeftShift,Key.E);else Keys(direction,Key.LeftShift);
            pulse++;yield return new WaitForSeconds(.10f);
        }
        Keys();yield return new WaitForSeconds(.15f);
    }
    IEnumerator Run(){
        yield return new WaitForSeconds(.7f);PairKeyboard();
        Check(Game.seconds==30&&Game.Remaining>28,"30-second countdown starts");
        var animator=Player.GetComponent<Animator>();
        Check(animator.isHuman&&animator.avatar.isValid,"Ch02 uses valid Humanoid Avatar");
        Check(Player.GetComponent<ThirdPersonController>()&&Player.GetComponent<PlayerInput>()&&Player.GetComponent<CharacterController>(),"Original Starter Assets controller with PlayerInput and CharacterController");
        Check(Camera.main.GetComponent<Cinemachine.CinemachineBrain>(),"Cinemachine controls third-person camera");
        Check(FindObjectsByType<UnityEngine.ProBuilder.ProBuilderMesh>(FindObjectsSortMode.None).Length>=10,"ProBuilder garden meshes present");
        Check(GameObject.Find("Ground 784 square metres").GetComponent<Renderer>().bounds.size.x>=28,"Floor at least 784 square metres");
        Check(FindObjectsByType<Stage2Item>(FindObjectsSortMode.None).Length==47,"All 47 interaction item instances");
        Game.enabled=false;
        var start=Player.position;var leg=animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).localRotation;
        Keys(Key.W);yield return new WaitForSeconds(.6f);
        Check(Player.position.z>start.z+.7f,"W moves Humanoid through Starter Assets: "+Player.position+" input="+Player.GetComponent<StarterAssetsInputs>().move+" key="+keyboard.wKey.isPressed+" scheme="+Player.GetComponent<PlayerInput>().currentControlScheme);
        Check(animator.GetFloat("Speed")>1f&&Quaternion.Angle(leg,animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).localRotation)>1f,"Locomotion blend and leg animation: speed="+animator.GetFloat("Speed")+" leg angle="+Quaternion.Angle(leg,animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).localRotation));
        Keys();yield return new WaitForSeconds(.2f);
start=Player.position;Keys(Key.Space);yield return new WaitForSeconds(.3f);Keys();Check(Player.position.y>start.y+.25f,"Space jumps with Humanoid animation");
        yield return new WaitForSeconds(1.3f);Check(Game.player.Grounded,"Humanoid lands");
        Teleport(new Vector3(-11,.08f,-11));yield return new WaitForSeconds(.2f);
        var first=FindObjectsByType<Stage2Item>(FindObjectsSortMode.None).First();
        first.transform.position=new Vector3(0,.9f,4.5f);
        Teleport(new Vector3(0,.08f,3.2f));yield return new WaitForSeconds(.1f);
        Check(!Actor.CanReach(first),"Wall blocks interaction line of sight");
        yield return Reload();Game.enabled=false;
        Teleport(new Vector3(0,.08f,-12));yield return new WaitForSeconds(.1f);
        int before=Game.Points;yield return PressE();Check(Game.Points==before,"E out of range has no effect");
        yield return Effect(ItemEffect.ScorePlus);yield return Effect(ItemEffect.ScoreMinus);yield return Effect(ItemEffect.TimePlus);yield return Effect(ItemEffect.TimeMinus);
        yield return Reload();Capture("Stage2-start.png");
        yield return WalkAxis(Key.W,-8,false,true);
        yield return PressE();
        yield return WalkAxis(Key.D,11,true,true);
        yield return PressE();
        yield return WalkAxis(Key.W,0,false,true);
        yield return PressE();
        yield return WalkAxis(Key.A,-11,true,true);
        yield return PressE();
        yield return WalkAxis(Key.W,8,false,true);
        yield return PressE();
        yield return WalkAxis(Key.D,11,true,true);
        yield return PressE();
        Check(Game.Points>10000&&Game.Remaining>0,"Real input route collects enough points before timeout: "+Game.Points);
        Check(SceneManager.GetActiveScene().name=="Stage2","Reaching target does not auto-win early");
        Capture("Stage2-route.png");int final=Game.Points;Game.Advance(Game.Remaining+1);yield return new WaitForSeconds(.3f);
        Check(SceneManager.GetActiveScene().name=="Win"&&ExamScreen.Score==final,"Timeout above target shows Win and score");Capture("Stage2-win.png");
        yield return Reload();Game.Advance(31);yield return new WaitForSeconds(.3f);
        Check(SceneManager.GetActiveScene().name=="GameOver","Low-score timeout loses");
        FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name=="Try Again").onClick.Invoke();yield return new WaitForSeconds(.5f);
        Check(Game&&Game.Points==0&&Game.Remaining>29&&FindObjectsByType<Stage2Item>(FindObjectsSortMode.None).Length==47,"Retry returns to Stage2 and restores all items/time/score");
        Game.enabled=false;for(int i=0;i<20;i++)Game.Apply(ItemEffect.ScorePlus);Game.Advance(31);yield return new WaitForSeconds(.3f);
        Check(SceneManager.GetActiveScene().name=="GameOver","Exactly 10000 loses under strict greater-than rule");
        yield return Reload();Teleport(new Vector3(40,-9,40));yield return new WaitForSeconds(.2f);
        Check(SceneManager.GetActiveScene().name=="GameOver","Falling outside garden loses");
        yield return Reload();Game.Advance(Game.Remaining-4);Game.Apply(ItemEffect.TimeMinus);yield return new WaitForSeconds(.2f);
        Check(SceneManager.GetActiveScene().name=="GameOver","Time penalty reaching zero immediately resolves game");
        yield return Reload();FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name=="Back").onClick.Invoke();yield return new WaitForSeconds(.2f);
        Check(SceneManager.GetActiveScene().name=="StageSelection","Back returns to stage selection");
    }
    void Capture(string name){
        var camera=Camera.main;var canvas=FindAnyObjectByType<Canvas>();var rt=new RenderTexture(1280,720,24);var prior=RenderTexture.active;
        canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
        camera.targetTexture=rt;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;
        var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
        File.WriteAllBytes("Evidence/Stage2/"+name,image.EncodeToPNG());Destroy(image);camera.targetTexture=null;RenderTexture.active=prior;rt.Release();Destroy(rt);canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;
    }
}
}
#endif