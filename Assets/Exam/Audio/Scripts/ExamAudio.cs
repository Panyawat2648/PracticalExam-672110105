using UnityEngine;
using UnityEngine.SceneManagement;
namespace PanyawatHainak {
public sealed class ExamAudio : MonoBehaviour {
    public const string MusicKey="Exam.MusicVolume", EffectsKey="Exam.EffectsVolume";
    public static ExamAudio Instance { get; private set; }
    public AudioSource Music { get; private set; }
    public AudioSource Effects { get; private set; }
    public string LastEffect { get; private set; }
    public int EffectCount { get; private set; }
    AudioClip menu,stage1,stage2,coin,timeUp,scoreDown,timeDown,impact,win,lose;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics(){Instance=null;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap(){if(!Instance)new GameObject("Exam Audio").AddComponent<ExamAudio>();}
    void Awake(){
        if(Instance&&Instance!=this){Destroy(gameObject);return;}
        Instance=this;DontDestroyOnLoad(gameObject);
        Music=gameObject.AddComponent<AudioSource>();Music.playOnAwake=false;Music.loop=true;Music.spatialBlend=0;
        Effects=gameObject.AddComponent<AudioSource>();Effects.playOnAwake=false;Effects.spatialBlend=0;
        Music.volume=Mathf.Clamp01(PlayerPrefs.GetFloat(MusicKey,.35f));
        Effects.volume=Mathf.Clamp01(PlayerPrefs.GetFloat(EffectsKey,.7f));
        menu=Load("Menu");stage1=Load("Stage1");stage2=Load("Stage2");coin=Load("Coin");timeUp=Load("TimeUp");
        scoreDown=Load("ScoreDown");timeDown=Load("TimeDown");impact=Load("Impact");win=Load("Win");lose=Load("Lose");
        SceneManager.sceneLoaded+=OnSceneLoaded;
    }
    AudioClip Load(string name){var clip=Resources.Load<AudioClip>("ExamAudio/"+name);if(!clip)Debug.LogError("Missing exam audio: "+name);return clip;}
    void OnSceneLoaded(Scene scene,LoadSceneMode mode){
        if(scene.name=="Options"&&mode==LoadSceneMode.Additive)return;
        var clip=scene.name=="Stage1"?stage1:scene.name=="Stage2"?stage2:menu;
        if(Music.clip!=clip){Music.clip=clip;Music.Play();}else if(!Music.isPlaying)Music.Play();
        if(scene.name=="Win")Play("Win",win);else if(scene.name=="GameOver")Play("Lose",lose);
    }
    public void SetMusic(float value){Music.volume=Mathf.Clamp01(value);PlayerPrefs.SetFloat(MusicKey,Music.volume);}
    public void SetEffects(float value){Effects.volume=Mathf.Clamp01(value);PlayerPrefs.SetFloat(EffectsKey,Effects.volume);}
    public void Save(){PlayerPrefs.Save();}
    public void Preview(){Play("Preview",coin);}
    public void Impact(){Play("Impact",impact);}
    public void Item(ItemEffect effect){
        switch(effect){
            case ItemEffect.ScorePlus:Play("Coin",coin);break;
            case ItemEffect.TimePlus:Play("TimeUp",timeUp);break;
            case ItemEffect.ScoreMinus:Play("ScoreDown",scoreDown);break;
            case ItemEffect.TimeMinus:Play("TimeDown",timeDown);break;
        }
    }
    void Play(string label,AudioClip clip){if(!clip)return;LastEffect=label;EffectCount++;Effects.PlayOneShot(clip);}
    void OnApplicationQuit(){Save();}
    void OnDestroy(){SceneManager.sceneLoaded-=OnSceneLoaded;if(Instance==this)Instance=null;}
}
}
