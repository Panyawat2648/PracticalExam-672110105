using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
namespace PanyawatHainak {
public class Stage2Game:MonoBehaviour {
    public StarterAssets.ThirdPersonController player;
    public float seconds=30;
    public float fallHeight=-8;
    public float Remaining {get;private set;}
    public int Points {get;private set;}
    public bool Ended {get;private set;}
    Text timeText,scoreText,notice,prompt;float noticeUntil;
    Font font;Transform canvas;
    Stage2Interactor interactor;Stage2Item shownItem;float nextHudUpdate;int shownTenths=-1,shownPoints=int.MinValue;
    void Awake(){Remaining=seconds;Points=0;ExamScreen.Score=0;ExamScreen.LastStage="Stage2";Time.timeScale=1;}
    void Start(){
        if(!player.GetComponent<ObstacleAudio>())player.gameObject.AddComponent<ObstacleAudio>();
        interactor=player.GetComponent<Stage2Interactor>();
        QualitySettings.vSyncCount=0;Application.targetFrameRate=60;
        QualitySettings.shadows=ShadowQuality.HardOnly;
        QualitySettings.shadowResolution=ShadowResolution.Low;
        QualitySettings.shadowCascades=0;QualitySettings.shadowDistance=30;
        QualitySettings.antiAliasing=0;QualitySettings.pixelLightCount=1;
        QualitySettings.globalTextureMipmapLimit=1;
        var brain=Camera.main.GetComponent<Cinemachine.CinemachineBrain>();
        if(brain)brain.m_UpdateMethod=Cinemachine.CinemachineBrain.UpdateMethod.LateUpdate;
        BuildHUD();Refresh();
        prompt.text="Approach an item to interact";
    }
    void Update(){
        if(Ended)return;
        if(player.transform.position.y<fallHeight){Finish(false,"Fell from the platforms");return;}
        Advance(Time.deltaTime);
        if(Time.unscaledTime>=nextHudUpdate){nextHudUpdate=Time.unscaledTime+.1f;Refresh();}
        if(notice&&Time.time>noticeUntil)notice.text="";
        if(prompt&&interactor.Target!=shownItem) { shownItem=interactor.Target; prompt.text=shownItem ? "Press E  |  "+shownItem.Description : "Approach an item to interact"; }
    }
    public void Advance(float elapsed){
        if(Ended)return;
        Remaining=Mathf.Max(0,Remaining-Mathf.Max(0,elapsed));
        if(Remaining<=0)Finish(Points>10000,Points>10000?"Time up - target exceeded":"Time up - target not exceeded");
    }
    public void Apply(ItemEffect effect){
        if(Ended)return;
        ExamAudio.Instance?.Item(effect);
        string message="";
        switch(effect){
            case ItemEffect.ScorePlus:Points+=500;message="+500 POINTS";break;
            case ItemEffect.ScoreMinus:Points-=500;message="-500 POINTS";break;
            case ItemEffect.TimePlus:Remaining+=3;message="+3 SECONDS";break;
            case ItemEffect.TimeMinus:Remaining=Mathf.Max(0,Remaining-5);message="-5 SECONDS";break;
        }
        if(notice){notice.text=message;noticeUntil=Time.time+1.2f;}
        Refresh();
        if(Remaining<=0)Advance(0);
    }
    void Finish(bool won,string reason){
        if(Ended)return;Ended=true;player.enabled=false; player.GetComponent<Stage2Interactor>().enabled=false;
        ExamScreen.Score=Points;ExamScreen.LastStage="Stage2";ExamScreen.ResultReason=reason;
        SceneManager.LoadScene(won?"Win":"GameOver");
    }
    void Refresh(){
        int tenths=Mathf.RoundToInt(Remaining*10);
        if(timeText&&tenths!=shownTenths){shownTenths=tenths;timeText.text="TIME  "+Remaining.ToString("0.0")+" s";}
        if(scoreText&&Points!=shownPoints){shownPoints=Points;scoreText.text="SCORE  "+Points+"   /   > 10,000";}
        if(timeText)timeText.color=Remaining<=10?new Color(.72f,.12f,.16f):new Color(.10f,.18f,.28f);
    }
    void BuildHUD(){
        font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
        var go=new GameObject("Stage2 HUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        canvas=go.transform;go.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
        var top=Rect("Top",canvas,new Vector2(.5f,1),new Vector2(0,-58),new Vector2(1280,116));
        top.gameObject.AddComponent<Image>().color=new Color(.95f,.97f,1f,.98f);
        timeText=Label(top,"",new Vector2(-490,12),new Vector2(260,45),30);
        scoreText=Label(top,"",new Vector2(-60,12),new Vector2(560,45),26);
        Label(top,"STAGE 2 / GARDEN RUN",new Vector2(-420,-32),new Vector2(400,28),18);
        var back=Rect("Back",top,new Vector2(.5f,.5f),new Vector2(520,5),new Vector2(180,42));
        back.gameObject.AddComponent<Image>().color=new Color(.70f,.86f,.97f);
        back.gameObject.AddComponent<Button>().onClick.AddListener(()=>ExamScreen.Go("StageSelection"));
        Label(back,"Back",Vector2.zero,new Vector2(170,36),22);
        var bottom=Rect("Help",canvas,new Vector2(.5f,0),new Vector2(0,51),new Vector2(1280,102));
        bottom.gameObject.AddComponent<Image>().color=new Color(.95f,.97f,1f,.98f);
        Label(bottom,"WASD move   Shift sprint   Space jump   RMB drag camera   E collect",new Vector2(0,25),new Vector2(1200,28),19);
        Label(bottom,"GOLD +500     GREEN +3s     RED -500     PURPLE -5s",Vector2.zero,new Vector2(1200,28),19);
        Label(bottom,"Win when time ends with score > 10,000. Falling ends the run.",new Vector2(0,-28),new Vector2(1200,28),17);
        notice=Label(canvas,"",new Vector2(0,180),new Vector2(800,45),30);
        prompt=Label(canvas,"",new Vector2(0,-210),new Vector2(800,40),24); prompt.color=Color.white; prompt.gameObject.AddComponent<Shadow>().effectColor=Color.black;
        notice.color=Color.white; notice.gameObject.AddComponent<Shadow>().effectColor=new Color(0,0,0,.8f);
    }
    RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 pos,Vector2 size){
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=pos;r.sizeDelta=size;return r;
    }
    Text Label(Transform parent,string value,Vector2 position,Vector2 size,int fontSize){
        var text=Rect("Label",parent,new Vector2(.5f,.5f),position,size).gameObject.AddComponent<Text>();
        text.font=font;text.text=value;text.fontSize=fontSize;text.alignment=TextAnchor.MiddleCenter;text.color=new Color(.10f,.18f,.28f);text.raycastTarget=false;return text;
    }
}
}