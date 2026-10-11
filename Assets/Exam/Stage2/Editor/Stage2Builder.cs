using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.ProBuilder;
using UnityEngine.InputSystem;
using Cinemachine;
using StarterAssets;
namespace PanyawatHainak.Editor {
public static class Stage2Builder {
    const string Root="Assets/Exam/Stage2";
    static Transform level;
    static Material stone,grass,wall,accent;
    public static void Build(){
        if(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/Ch02Player.prefab"))throw new Exception("Stage2 already built; refusing overwrite");
        string modelPath=Root+"/Models/Ch02_nonPBR.fbx";
        var importer=(ModelImporter)AssetImporter.GetAtPath(modelPath);
        importer.animationType=ModelImporterAnimationType.Human;
        importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
        importer.SaveAndReimport();
        Directory.CreateDirectory(Root+"/Models/Textures");
        importer.ExtractTextures(Root+"/Models/Textures");
        AssetDatabase.Refresh();
        importer.SaveAndReimport();
        var avatar=AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Avatar>().FirstOrDefault();
        if(!avatar||!avatar.isValid||!avatar.isHuman)throw new Exception("Ch02 Avatar is not valid Humanoid");
        stone=Mat("Ivory paving",new Color(.70f,.77f,.75f));grass=Mat("Garden green",new Color(.21f,.48f,.36f));
        wall=Mat("Sandstone",new Color(.66f,.54f,.36f));accent=Mat("Blue edging",new Color(.16f,.50f,.65f));
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        level=new GameObject("ProBuilder Garden 28 x 28").transform;
        Block("Ground 784 square metres",new Vector3(0,-.5f,0),new Vector3(28,1,28),grass);
        foreach(float z in new[]{-8f,0,8f})Block("Paved collection lane",new Vector3(0,.01f,z),new Vector3(26,.02f,3.5f),stone);
        foreach(float x in new[]{-11f,11f})Block("Connecting path",new Vector3(x,.02f,0),new Vector3(3.5f,.02f,24),stone);
        foreach(float z in new[]{-4f,4f}){
            Block("Garden wall",new Vector3(0,1,z),new Vector3(12,2,.65f),wall);
            Block("Wall cap",new Vector3(0,2.04f,z),new Vector3(12.2f,.12f,.85f),stone);
        }
        foreach(float x in new[]{-13.7f,13.7f})foreach(float z in new[]{-13.7f,13.7f}){
            Block("Corner pillar",new Vector3(x,1,z),new Vector3(.6f,2,.6f),accent);
        }
        var items=new GameObject("Interaction Item Prefabs").transform;
        var coin=Item("ScorePlus500",ItemEffect.ScorePlus);
        var minus=Item("ScoreMinus500",ItemEffect.ScoreMinus);
        var time=Item("TimePlus3",ItemEffect.TimePlus);
        var drain=Item("TimeMinus5",ItemEffect.TimeMinus);
        foreach(float z in new[]{-8f,0,8f})for(int i=0;i<12;i++)Place(coin,new Vector3(-11+2*i,.9f,z),items);
        foreach(var p in new[]{new Vector3(11,.9f,-4),new Vector3(-11,.9f,4),new Vector3(11,.9f,11)})Place(time,p,items);
        foreach(var p in new[]{new Vector3(-7,.9f,-11),new Vector3(7,.9f,-11),new Vector3(-7,.9f,11),new Vector3(7,.9f,11)})Place(minus,p,items);
        foreach(var p in new[]{new Vector3(-8,.9f,-4),new Vector3(8,.9f,-4),new Vector3(-8,.9f,4),new Vector3(8,.9f,4)})Place(drain,p,items);

        var player=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));player.name="Ch02Player";
        player.transform.position=new Vector3(-11,.12f,-11);player.tag="Player";
        foreach(var t in player.GetComponentsInChildren<Transform>())t.gameObject.layer=2;
        var animator=player.GetComponent<Animator>()??player.AddComponent<Animator>();
        animator.avatar=avatar;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        animator.runtimeAnimatorController=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/StarterAssets/ThirdPersonController/Character/Animations/StarterAssetsThirdPerson.controller");
        if(!animator.runtimeAnimatorController)throw new Exception("Starter Assets animation controller missing");
        foreach(var renderer in player.GetComponentsInChildren<Renderer>()){
            var mats=renderer.sharedMaterials;
            for(int i=0;i<mats.Length;i++){
                if(!mats[i])continue;
                var copy=new Material(mats[i]);copy.shader=Shader.Find("Standard");copy.SetFloat("_Glossiness",.15f);
                string path=AssetDatabase.GenerateUniqueAssetPath(Root+"/Materials/Ch02-"+i+".mat");
                AssetDatabase.CreateAsset(copy,path);mats[i]=copy;
            }renderer.sharedMaterials=mats;
        }
        var cc=player.AddComponent<CharacterController>();cc.height=1.8f;cc.radius=.28f;cc.center=new Vector3(0,.9f,0);cc.stepOffset=.3f;cc.skinWidth=.03f;
        var input=player.AddComponent<StarterAssetsInputs>();input.cursorLocked=false;input.cursorInputForLook=false;
        var pi=player.AddComponent<PlayerInput>();pi.actions=AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/StarterAssets/InputSystem/StarterAssets.inputactions");pi.defaultActionMap="Player";pi.defaultControlScheme="KeyboardMouse";pi.notificationBehavior=PlayerNotifications.SendMessages;
        var target=new GameObject("CameraTarget");target.transform.SetParent(player.transform,false);target.transform.localPosition=new Vector3(0,1.45f,0);
        var controller=player.AddComponent<ThirdPersonController>();controller.CinemachineCameraTarget=target;controller.GroundLayers=1;controller.MoveSpeed=3;controller.SprintSpeed=6.5f;controller.FootstepAudioClips=Array.Empty<AudioClip>();
        player.AddComponent<Stage2Interactor>();
        var prefab=PrefabUtility.SaveAsPrefabAsset(player,Root+"/Prefabs/Ch02Player.prefab");UnityEngine.Object.DestroyImmediate(player);
        player=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
        var game=new GameObject("Stage2 Game").AddComponent<Stage2Game>();game.player=player.GetComponent<ThirdPersonController>();
        var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener),typeof(CinemachineBrain));camera.tag="MainCamera";
        camera.GetComponent<Camera>().backgroundColor=new Color(.65f,.83f,.93f);camera.GetComponent<Camera>().clearFlags=CameraClearFlags.SolidColor;
        var brain=camera.GetComponent<CinemachineBrain>();brain.m_DefaultBlend=new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut,0);
        var vcam=new GameObject("Third Person Follow").AddComponent<CinemachineVirtualCamera>();
        vcam.Follow=player.transform.Find("CameraTarget");vcam.LookAt=vcam.Follow;vcam.m_Lens.FieldOfView=65;
        var follow=vcam.AddCinemachineComponent<CinemachineTransposer>();follow.m_BindingMode=CinemachineTransposer.BindingMode.LockToTargetWithWorldUp;
        follow.m_FollowOffset=new Vector3(0,3.5f,-6);follow.m_XDamping=follow.m_YDamping=follow.m_ZDamping=.15f;
        vcam.AddCinemachineComponent<CinemachineComposer>();
        var light=new GameObject("Sun",typeof(Light)).GetComponent<Light>();light.type=LightType.Directional;light.intensity=1.1f;light.shadows=LightShadows.Soft;light.transform.rotation=Quaternion.Euler(50,-30,0);
        RenderSettings.ambientLight=new Color(.65f,.68f,.7f);RenderSettings.fog=false;
        EditorSceneManager.SaveScene(scene,"Assets/Exam/Scenes/Stage2.unity");
        AssetDatabase.SaveAssets();
        File.WriteAllText("Evidence/Stage2/build.txt","Stage2: 28x28m = 784m2; "+level.GetComponentsInChildren<ProBuilderMesh>().Length+" ProBuilder meshes; 47 interaction items; Ch02 valid Humanoid; original StarterAssets.ThirdPersonController + Cinemachine 2.10.7 + Input System; 30s strict >10000 win at expiry.");
        Debug.Log("STAGE2_BUILD_OK");
    }
    public static void EnsureAnimation(){
        var path=Root+"/Prefabs/Ch02Player.prefab";
        var go=PrefabUtility.LoadPrefabContents(path);
        go.GetComponent<Animator>().cullingMode=AnimatorCullingMode.AlwaysAnimate;
        PrefabUtility.SaveAsPrefabAsset(go,path);PrefabUtility.UnloadPrefabContents(go);AssetDatabase.SaveAssets();
    }
    static Material Mat(string name,Color color){var m=new Material(Shader.Find("Standard")){name=name,color=color};AssetDatabase.CreateAsset(m,Root+"/Materials/"+name+".mat");return m;}
    static void Block(string name,Vector3 pos,Vector3 size,Material mat){
        var pb=ShapeGenerator.CreateShape(ShapeType.Cube);pb.name=name;pb.transform.SetParent(level);pb.transform.position=pos;pb.transform.localScale=size;pb.GetComponent<Renderer>().sharedMaterial=mat;if(!pb.GetComponent<Collider>())pb.gameObject.AddComponent<BoxCollider>();
    }
    static GameObject Item(string name,ItemEffect effect){
        var go=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Exam/Stage1/Prefabs/"+name+".prefab"));
        go.name="Interact-"+name;var old=go.GetComponent<Stage1Item>();var visual=old.visual;UnityEngine.Object.DestroyImmediate(old);
        var item=go.AddComponent<Stage2Item>();item.effect=effect;item.visual=visual;
        var saved=PrefabUtility.SaveAsPrefabAsset(go,Root+"/Prefabs/Interact-"+name+".prefab");UnityEngine.Object.DestroyImmediate(go);return saved;
    }
    static void Place(GameObject prefab,Vector3 pos,Transform parent){var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);go.transform.SetParent(parent);go.transform.position=pos;}
}
[InitializeOnLoad]
public static class Stage2TestLauncher {
    static Stage2TestLauncher(){EditorApplication.playModeStateChanged+=s=>{
        if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("Stage2Test",false)){
            SessionState.SetBool("Stage2Test",false);var go=new GameObject("Stage2 checks");UnityEngine.Object.DontDestroyOnLoad(go);go.AddComponent<Stage2Checks>();
        }
    };}
    public static void Run(){Stage2Builder.EnsureAnimation();EditorSceneManager.OpenScene("Assets/Exam/Scenes/Stage2.unity");SessionState.SetBool("Stage2Test",true);EditorApplication.isPlaying=true;}
}
}