using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.ProBuilder;
namespace PanyawatHainak.Editor {
public static class Stage1Builder {
    const string Root="Assets/Exam/Stage1";
    static Material deck,trim,playerMat,gold,green,red,purple;
    static Transform level;
    public static void Build(){
        if(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/CapsulePlayer.prefab"))throw new Exception("Stage 1 already built; refusing overwrite");
        deck=Mat("Deck",new Color(.12f,.23f,.32f));trim=Mat("Teal",new Color(.12f,.65f,.66f));
        playerMat=Mat("Player",new Color(.91f,.96f,1));gold=Mat("Gold",new Color(1,.72f,.12f));
        green=Mat("TimePlus",new Color(.22f,1,.44f));red=Mat("ScoreMinus",new Color(1,.2f,.22f));purple=Mat("TimeMinus",new Color(.75f,.24f,1));
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        level=new GameObject("ProBuilder Sky Circuit").transform;
        foreach(var point in new[]{new Vector3(0,0,0),new Vector3(0,0,20),new Vector3(20,0,20),new Vector3(20,0,0)}){
            Block("Wide island 10 x 10",point+Vector3.down*.6f,new Vector3(10,1.2f,10),deck);
            Block("Island underside",point+Vector3.down*1.45f,new Vector3(8,.5f,8),trim);
            foreach(float x in new[]{-4.5f,4.5f})foreach(float z in new[]{-4.5f,4.5f})
                Block("Corner marker",point+new Vector3(x,.12f,z),new Vector3(.5f,.24f,.5f),trim);
        }
        Block("West narrow bridge",new Vector3(0,-.4f,10),new Vector3(3.2f,.8f,10),deck);
        Block("East narrow bridge",new Vector3(20,-.4f,10),new Vector3(3.2f,.8f,10),deck);
        Block("North wide bridge",new Vector3(10,-.4f,20),new Vector3(10,.8f,4.8f),deck);
        Block("South wide bridge",new Vector3(10,-.4f,0),new Vector3(10,.8f,4.8f),deck);
        // Edge stripes identify the unguarded drop without obstructing movement.
        foreach(float x in new[]{-1.55f,1.55f,18.45f,21.55f})
            Block("Narrow edge",new Vector3(x,.015f,10),new Vector3(.1f,.03f,10),trim);
        foreach(float z in new[]{-2.35f,2.35f,17.65f,22.35f})
            Block("Wide edge",new Vector3(10,.015f,z),new Vector3(10,.03f,.1f),trim);
        var coins=Item("ScorePlus500",ItemEffect.ScorePlus,gold,PrimitiveType.Cylinder);
        var plus=Item("TimePlus3",ItemEffect.TimePlus,green,PrimitiveType.Cube);
        var minus=Item("ScoreMinus500",ItemEffect.ScoreMinus,red,PrimitiveType.Cube);
        var timeMinus=Item("TimeMinus5",ItemEffect.TimeMinus,purple,PrimitiveType.Sphere);
        var items=new GameObject("Item Prefab Instances").transform;
        for(int n=1;n<=9;n++){
            Place(coins,new Vector3(0,.9f,n*2),items);
            Place(coins,new Vector3(n*2,.9f,20),items);
            Place(coins,new Vector3(20,.9f,20-n*2),items);
            Place(coins,new Vector3(20-n*2,.9f,0),items);
        }
        foreach(var p in new[]{new Vector3(0,.9f,20),new Vector3(20,.9f,20),new Vector3(20,.9f,0)})
            Place(plus,p,items);
        foreach(var p in new[]{new Vector3(-1,.9f,9),new Vector3(21,.9f,11),new Vector3(8,.9f,21.5f),new Vector3(12,.9f,-1.5f)})
            Place(minus,p,items);
        foreach(var p in new[]{new Vector3(1,.9f,13),new Vector3(19,.9f,7),new Vector3(12,.9f,18.5f),new Vector3(8,.9f,1.5f)})
            Place(timeMinus,p,items);
        var body=GameObject.CreatePrimitive(PrimitiveType.Capsule);body.name="CapsulePlayer";body.tag="Player";body.layer=2;
        body.GetComponent<Renderer>().sharedMaterial=playerMat;
        var physics=new PhysicsMaterial("Capsule frictionless"){dynamicFriction=0,staticFriction=0,bounciness=0,frictionCombine=PhysicsMaterialCombine.Minimum};
        AssetDatabase.CreateAsset(physics,Root+"/Materials/Capsule.physicsMaterial");
        body.GetComponent<CapsuleCollider>().sharedMaterial=physics;
        var rb=body.AddComponent<Rigidbody>();rb.mass=1;rb.constraints=RigidbodyConstraints.FreezeRotation;rb.interpolation=RigidbodyInterpolation.Interpolate;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
        body.AddComponent<CapsuleMotor>();
        var nose=GameObject.CreatePrimitive(PrimitiveType.Cube);nose.name="Forward marker";nose.transform.SetParent(body.transform,false);nose.transform.localPosition=new Vector3(0,.35f,.47f);nose.transform.localScale=new Vector3(.4f,.25f,.12f);UnityEngine.Object.DestroyImmediate(nose.GetComponent<Collider>());nose.GetComponent<Renderer>().sharedMaterial=trim;
        var prefab=PrefabUtility.SaveAsPrefabAsset(body,Root+"/Prefabs/CapsulePlayer.prefab");UnityEngine.Object.DestroyImmediate(body);
        var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab);instance.transform.position=new Vector3(0,1.1f,-2);
        var game=new GameObject("Stage1 Game").AddComponent<Stage1Game>();game.player=instance.GetComponent<CapsuleMotor>();
        var cameraObject=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener),typeof(CapsuleCamera));cameraObject.tag="MainCamera";
        var camera=cameraObject.GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.035f,.065f,.12f);camera.fieldOfView=60;camera.farClipPlane=150;
        cameraObject.GetComponent<CapsuleCamera>().target=instance.transform;camera.transform.position=instance.transform.position+new Vector3(0,10,-12);camera.transform.LookAt(instance.transform.position);
        var light=new GameObject("Sun",typeof(Light)).GetComponent<Light>();light.type=LightType.Directional;light.intensity=1.35f;light.shadows=LightShadows.Soft;light.transform.rotation=Quaternion.Euler(45,-35,0);
        RenderSettings.ambientLight=new Color(.5f,.6f,.7f);RenderSettings.fog=true;RenderSettings.fogColor=camera.backgroundColor;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=35;RenderSettings.fogEndDistance=100;
        EditorSceneManager.SaveScene(scene,"Assets/Exam/Scenes/Stage1.unity");
        AssetDatabase.SaveAssets();
        File.WriteAllText("Evidence/Stage1/build.txt","Stage1: "+level.GetComponentsInChildren<ProBuilderMesh>().Length+" ProBuilder meshes; 36 score coins, 3 time bonuses, 4 score penalties, 4 time penalties. Capsule Rigidbody, gravity, collider, input actions. Four item prefabs and one player prefab. Strict >10000 win at time expiry.");
        Debug.Log("STAGE1_BUILD_OK");
    }
    static Material Mat(string name,Color color){var m=new Material(Shader.Find("Standard")){name=name,color=color};m.SetFloat("_Glossiness",.3f);AssetDatabase.CreateAsset(m,Root+"/Materials/"+name+".mat");return m;}
    static void Block(string name,Vector3 pos,Vector3 size,Material material){
        var pb=ShapeGenerator.CreateShape(ShapeType.Cube);pb.name=name;pb.transform.SetParent(level);pb.transform.position=pos;pb.transform.localScale=size;pb.GetComponent<Renderer>().sharedMaterial=material;
        if(!pb.GetComponent<Collider>())pb.gameObject.AddComponent<BoxCollider>();
    }
    static GameObject Item(string name,ItemEffect effect,Material mat,PrimitiveType shape){
        var item=new GameObject(name);var trigger=item.AddComponent<SphereCollider>();trigger.isTrigger=true;trigger.radius=.62f;
        var script=item.AddComponent<Stage1Item>();script.effect=effect;
        var visual=GameObject.CreatePrimitive(shape);visual.name="Visual";visual.transform.SetParent(item.transform,false);UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
        visual.GetComponent<Renderer>().sharedMaterial=mat;
        if(shape==PrimitiveType.Cylinder){visual.transform.localScale=new Vector3(.65f,.10f,.65f);visual.transform.localRotation=Quaternion.Euler(90,0,0);}
        else {visual.transform.localScale=Vector3.one*.55f;if(effect==ItemEffect.ScoreMinus)visual.transform.localRotation=Quaternion.Euler(0,0,45);}
        script.visual=visual.transform;
        var saved=PrefabUtility.SaveAsPrefabAsset(item,Root+"/Prefabs/"+name+".prefab");UnityEngine.Object.DestroyImmediate(item);return saved;
    }
    static void Place(GameObject prefab,Vector3 pos,Transform parent){var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);go.transform.SetParent(parent);go.transform.position=pos;}
}
[InitializeOnLoad]
public static class Stage1TestLauncher {
    static Stage1TestLauncher(){EditorApplication.playModeStateChanged+=s=>{
        if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("Stage1Test",false)){
            SessionState.SetBool("Stage1Test",false);var go=new GameObject("Stage1 checks");UnityEngine.Object.DontDestroyOnLoad(go);go.AddComponent<Stage1Checks>();
        }
    };}
    public static void Run(){EditorSceneManager.OpenScene("Assets/Exam/Scenes/Stage1.unity");SessionState.SetBool("Stage1Test",true);EditorApplication.isPlaying=true;}
}
}