using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace PanyawatHainak
{
    public class ExamScreen : MonoBehaviour
    {
        public string screen;
        public static string LastStage = "Stage1";
        public static int Score;
        public static string ResultReason = "";
        static bool optionsLoading;
        Font font;
        Transform panel;
        void Start() { Build(); }
        void Build()
        {
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bool modal=screen=="Options";
            if(!modal) {
                optionsLoading=false;
                var events=new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
                SceneManager.MoveGameObjectToScene(events,gameObject.scene);
            }
            var go=new GameObject(screen+" UI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            SceneManager.MoveGameObjectToScene(go,gameObject.scene);
            var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=modal?100:0;
            var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
            var bg=Rect("Background",go.transform,Vector2.zero,new Vector2(1280,720));
            bg.anchorMin=Vector2.zero;bg.anchorMax=Vector2.one;bg.sizeDelta=Vector2.zero;
            bg.gameObject.AddComponent<Image>().color=modal?new Color(.90f,.95f,.99f,.98f):new Color(.95f,.97f,1f);
            panel=Rect("Panel",bg,Vector2.zero,new Vector2(700,600));
            Label("PRACTICAL EXAM / SCENE MANAGEMENT",255,18,new Color(.12f,.38f,.55f));
            Label(Title(),195,36,new Color(.10f,.18f,.28f));
            Label("672110105  panyawat hainak",-265,18,new Color(.32f,.40f,.49f));
            switch(screen) {
                case "MainMenu":
                    Button("Start",95,()=>Go("StageSelection"));
                    Button("Options",35,OpenOptions);
                    Button("Credits",-25,()=>Go("Credits"));
                    Button("Exit",-85,Quit);
                    break;
                case "StageSelection":
                    Button("Stage 1 - Capsule Player",75,()=>Stage("Stage1"));
                    Button("Stage 2 - Humanoid Player",10,()=>Stage("Stage2"));
                    Button("Back",-115,()=>Go("MainMenu")); break;
                case "Stage1": case "Stage2":
                    LastStage=screen;
                    Label("Scene navigation preview",120,20,new Color(.10f,.18f,.28f));
                    Label("Gameplay will be added in the next tasks.",85,18,new Color(.32f,.40f,.49f));
                    Button("Win (test)",15,()=>Go("Win"));
                    Button("Lose (test)",-45,()=>Go("GameOver"));
                    Button("Back",-125,()=>Go("StageSelection"));break;
                case "Win": case "GameOver":
                    Label("Score: "+Score,95,26,new Color(.10f,.18f,.28f));
                    Label(ResultReason,55,17,new Color(.10f,.18f,.28f));
                    if(screen=="GameOver") Button("Try Again",15,()=>Stage(LastStage));
                    Button("Back To Main Menu",-100,()=>Go("MainMenu"));break;
                case "Credits":
                    Label("Created by",70,22,new Color(.10f,.18f,.28f));
                    Label("panyawat hainak",25,30,new Color(.10f,.18f,.28f));
                    Label("Student Code: 672110105",-20,22,new Color(.10f,.18f,.28f));
                    Button("Back",-125,()=>Go("MainMenu"));break;
                case "Options":
                    Label("BGM and SFX controls will be added in the audio task.",65,18,new Color(.10f,.18f,.28f));
                    Label("This page currently demonstrates additive scene loading.",15,18,new Color(.32f,.40f,.49f));
                    Button("OK",-115,CloseOptions);break;
            }
        }
        string Title() {
            switch(screen) {
                case "MainMenu": return "Main Menu";
                case "StageSelection": return "Stage Selection";
                case "Stage1": return "Stage 1 - Capsule Player";
                case "Stage2": return "Stage 2 - Humanoid Player";
                case "GameOver": return "Game Over";
                default:return screen;
            }
        }
        public static void Go(string target) { SceneManager.LoadScene(target); }
        static void Stage(string target) { LastStage=target;Score=0;ResultReason="";Go(target); }
        static void OpenOptions() {
            if(optionsLoading||SceneManager.GetSceneByName("Options").isLoaded)return;
            optionsLoading=true;
            var op=SceneManager.LoadSceneAsync("Options",LoadSceneMode.Additive);
            op.completed+=_=>optionsLoading=false;
        }
        static void CloseOptions() { if(SceneManager.GetSceneByName("Options").isLoaded) SceneManager.UnloadSceneAsync("Options"); }
        static void Quit() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
        RectTransform Rect(string name,Transform parent,Vector2 pos,Vector2 size) {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
            r.SetParent(parent,false);r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;return r;
        }
        void Label(string text,float y,int size,Color color) {
            var t=Rect(text,panel,new Vector2(0,y),new Vector2(680,55)).gameObject.AddComponent<Text>();
            t.font=font;t.text=text;t.fontSize=size;t.color=color;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;
        }
        void Button(string text,float y,UnityEngine.Events.UnityAction action) {
            var r=Rect(text,panel,new Vector2(0,y),new Vector2(430,48));
            var im=r.gameObject.AddComponent<Image>();im.color=new Color(.70f,.86f,.97f);
            var b=r.gameObject.AddComponent<Button>();b.targetGraphic=im;b.onClick.AddListener(action);
            var t=Rect("Label",r,Vector2.zero,new Vector2(410,45)).gameObject.AddComponent<Text>();
            t.font=font;t.text=text;t.fontSize=22;t.alignment=TextAnchor.MiddleCenter;t.color=new Color(.10f,.18f,.28f);t.raycastTarget=false;
        }
    }
}