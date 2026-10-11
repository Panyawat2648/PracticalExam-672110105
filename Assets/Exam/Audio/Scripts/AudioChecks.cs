#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
namespace PanyawatHainak {
public class AudioChecks:MonoBehaviour {
    readonly List<string> results=new();readonly List<string> errors=new();
    float oldMusic,oldEffects;bool musicKey,effectsKey;
    ExamAudio A=>ExamAudio.Instance;
    void Awake(){Directory.CreateDirectory("Evidence/Audio");musicKey=PlayerPrefs.HasKey(ExamAudio.MusicKey);effectsKey=PlayerPrefs.HasKey(ExamAudio.EffectsKey);oldMusic=PlayerPrefs.GetFloat(ExamAudio.MusicKey,.35f);oldEffects=PlayerPrefs.GetFloat(ExamAudio.EffectsKey,.7f);Application.logMessageReceived+=Log;}
    void Log(string m,string stack,LogType t){if(t==LogType.Error||t==LogType.Exception)errors.Add(m);}
    void Check(bool ok,string msg){if(!ok)throw new Exception(msg);results.Add("PASS: "+msg);File.WriteAllLines("Evidence/Audio/verification.txt",results);}
    IEnumerator Start(){var stack=new Stack<IEnumerator>();stack.Push(Run());while(stack.Count>0){object next=null;bool more;try{more=stack.Peek().MoveNext();if(more)next=stack.Peek().Current;}catch(Exception e){results.Add("FAIL: "+e);Finish(1);yield break;}if(!more){stack.Pop();continue;}if(next is IEnumerator child)stack.Push(child);else yield return next;}results.AddRange(errors.Select(e=>"ERROR: "+e));Finish(errors.Count>0?1:0);}
    void Finish(int code){Application.logMessageReceived-=Log;if(A){A.SetMusic(oldMusic);A.SetEffects(oldEffects);}if(!musicKey)PlayerPrefs.DeleteKey(ExamAudio.MusicKey);if(!effectsKey)PlayerPrefs.DeleteKey(ExamAudio.EffectsKey);PlayerPrefs.Save();File.WriteAllLines("Evidence/Audio/verification.txt",results);Debug.Log(code==0?"EXAM_AUDIO_PASS":"EXAM_AUDIO_FAIL");if(Application.isBatchMode)EditorApplication.Exit(code);else EditorApplication.isPlaying=false;}
    void Click(string name){FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name==name).onClick.Invoke();}
    IEnumerator Load(string name){SceneManager.LoadScene(name);yield return new WaitForSeconds(.5f);}
    void Sources(string name){Check(FindObjectsByType<ExamAudio>(FindObjectsSortMode.None).Length==1,"One persistent audio manager in "+name);Check(FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Count(s=>s.loop&&s.isPlaying)==1,"Exactly one playing BGM in "+name);Check(FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(l=>l.enabled)==1,"One AudioListener in "+name);}
    IEnumerator Run(){
        yield return new WaitForSeconds(1);
        Check(A&&A.Music.clip&&A.Music.isPlaying,"Music starts on first scene");
        A.SetMusic(.35f);A.SetEffects(.7f);
        foreach(string clip in new[]{"Menu","Stage1","Stage2","Coin","TimeUp","ScoreDown","TimeDown","Impact","Win","Lose"}){var c=Resources.Load<AudioClip>("ExamAudio/"+clip);Check(c&&c.length>0,"Imported nonempty clip "+clip);}
        var manager=A;var song=A.Music.clip;int sample=A.Music.timeSamples;
        Click("Options");yield return new WaitForSeconds(.4f);Sources("additive Options");
        Check(A==manager&&A.Music.clip==song&&A.Music.timeSamples>sample,"Options preserves music without restarting");
        var sliders=FindObjectsByType<Slider>(FindObjectsSortMode.None);var music=sliders.Single(s=>s.name=="BGM Volume");var effects=sliders.Single(s=>s.name=="SFX Volume");
        music.value=0;Check(A.Music.volume==0&&Mathf.Abs(A.Effects.volume-.7f)<.001f,"BGM slider mutes only music");
        music.value=.4f;effects.value=0;Check(Mathf.Abs(A.Music.volume-.4f)<.001f&&A.Effects.volume==0,"SFX slider mutes only effects");
        effects.value=.6f;Click("Test SFX");yield return new WaitForSeconds(.05f);Check(A.LastEffect=="Preview"&&A.Effects.isPlaying,"SFX preview plays");
        Capture();Click("OK");yield return new WaitForSeconds(.3f);
        Check(Mathf.Abs(PlayerPrefs.GetFloat(ExamAudio.MusicKey)-.4f)<.001f&&Mathf.Abs(PlayerPrefs.GetFloat(ExamAudio.EffectsKey)-.6f)<.001f,"Volume choices saved independently");
        Click("Options");yield return new WaitForSeconds(.3f);
        Check(Mathf.Abs(FindObjectsByType<Slider>(FindObjectsSortMode.None).Single(s=>s.name=="BGM Volume").value-.4f)<.001f,"Reopening Options restores saved setting");Click("OK");yield return new WaitForSeconds(.2f);
        foreach(string scene in new[]{"Credits","StageSelection","Stage1","Stage2","Win","GameOver","MainMenu"}){
            yield return Load(scene);Sources(scene);
            string expected=scene=="Stage1"?"Stage1":scene=="Stage2"?"Stage2":"Menu";
            Check(A.Music.clip.name==expected,"Correct music in "+scene);
            if(scene=="Win"||scene=="GameOver")Check(A.LastEffect==(scene=="Win"?"Win":"Lose"),"Result sound in "+scene);
            Check(Mathf.Abs(A.Music.volume-.4f)<.001f&&Mathf.Abs(A.Effects.volume-.6f)<.001f,"Volume persists into "+scene);
        }
        yield return Load("Stage1");var g1=FindAnyObjectByType<Stage1Game>();g1.enabled=false;
        foreach(ItemEffect effect in Enum.GetValues(typeof(ItemEffect))){var item=FindObjectsByType<Stage1Item>(FindObjectsSortMode.None).First(i=>i.effect==effect);int before=A.EffectCount;var body=g1.player.GetComponent<Rigidbody>();body.position=item.transform.position+Vector3.up*.1f;body.linearVelocity=Vector3.zero;Physics.SyncTransforms();yield return new WaitForSeconds(.2f);Check(!item.gameObject.activeSelf&&A.EffectCount==before+1,"Stage1 trigger plays one effect for "+effect);}
        yield return Load("Stage2");var g2=FindAnyObjectByType<Stage2Game>();g2.enabled=false;var actor=g2.player.GetComponent<Stage2Interactor>();var cc=actor.GetComponent<CharacterController>();
        foreach(ItemEffect effect in Enum.GetValues(typeof(ItemEffect))){
            var item=FindObjectsByType<Stage2Item>(FindObjectsSortMode.None).First(i=>i.effect==effect);int before=A.EffectCount;
            cc.enabled=false;actor.transform.position=new Vector3(item.transform.position.x,.08f,item.transform.position.z);cc.enabled=true;Physics.SyncTransforms();yield return new WaitForSeconds(.15f);
            Check(!item.Collected&&A.EffectCount==before,"Stage2 proximity alone makes no pickup sound for "+effect);
            Check(item.Collect(actor)&&A.EffectCount==before+1,"Stage2 successful interaction plays once for "+effect);
            Check(!item.Collect(actor)&&A.EffectCount==before+1,"Repeated Stage2 collection is silent for "+effect);
        }
        cc.enabled=false;actor.transform.position=new Vector3(0,.08f,2.5f);cc.enabled=true;Physics.SyncTransforms();yield return new WaitForSeconds(.7f);
        int impacts=A.EffectCount;cc.Move(Vector3.forward*2);yield return null;
        Check(A.LastEffect=="Impact"&&A.EffectCount==impacts+1,"CharacterController wall collision plays impact sound");
        cc.Move(Vector3.forward);yield return null;Check(A.EffectCount==impacts+1,"Wall sound cooldown prevents per-frame spam");
    }
    void Capture(){var canvas=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Single(c=>c.gameObject.scene.name=="Options");var camera=Camera.main;var rt=new RenderTexture(1280,720,24);var prior=RenderTexture.active;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;camera.targetTexture=rt;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes("Evidence/Audio/Options.png",image.EncodeToPNG());Destroy(image);camera.targetTexture=null;RenderTexture.active=prior;rt.Release();Destroy(rt);canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;}
}
}
#endif
