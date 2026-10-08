using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace PanyawatHainak.Editor
{
    public static class ProjectSetup
    {
        public static void Configure()
        {
            PlayerSettings.companyName = "Panyawat Hainak";
            PlayerSettings.productName = "PracticalExam-672110105";
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var input = settings.FindProperty("activeInputHandler");
            if (input != null) { input.intValue = 0; settings.ApplyModifiedPropertiesWithoutUndo(); }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var probe = ShapeGenerator.CreateShape(ShapeType.Cube);
            if(probe == null || probe.GetComponent<MeshFilter>().sharedMesh == null) throw new Exception("ProBuilder mesh generation failed");
            UnityEngine.Object.DestroyImmediate(probe.gameObject);
            var report = new System.Collections.Generic.List<string>();
            report.Add("Unity " + Application.unityVersion);
            report.Add("PASS: ProBuilder creates a cube mesh.");
            foreach(var file in new[]{"Assets/Standard Assets/Characters/FirstPersonCharacter/Prefabs/FPSController.prefab",
                                      "Assets/Standard Assets/Characters/ThirdPersonCharacter/Prefabs/ThirdPersonController.prefab"})
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(file);
                if(prefab==null) throw new Exception("Missing prefab: "+file);
                int missing = prefab.GetComponentsInChildren<Transform>(true).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
                if(missing!=0) throw new Exception("Missing scripts in "+file+": "+missing);
                report.Add("PASS: Prefab loads with no missing scripts: "+file);
            }
            EditorSceneManager.SaveScene(scene,"Assets/Scenes/Empty.unity");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Empty.unity",true)};
            AssetDatabase.SaveAssets();
            report.Add("PASS: Empty scene saved with camera and directional light only.");
            report.Add("Standard Assets 2018 copied from local imported package with existing Unity 6 compatibility fixes.");
            report.Add("Built-in rendering; legacy Input Manager enabled for Standard Assets.");
            report.Add("Starter Assets Third Person required for exam Stage 2 is NOT installed.");
            report.Add("Prefab movement has not been Play Mode tested in this empty scene.");
            File.WriteAllLines("setup-verification.txt",report);
            Debug.Log("PRACTICAL_SETUP_OK");
        }
    }
}