using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Suimono.Core;

public static class SetupBoatScene
{
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/main.unity");

        var waterPath = AssetDatabase.GUIDToAssetPath("decb25ac7e8e6d9439bfb5397e23740b");
        var boatPath = AssetDatabase.GUIDToAssetPath("4e9797e1857919b4394def683c142f8e");
        Debug.Log("SetupBoatScene: waterPath=" + waterPath + " boatPath=" + boatPath);

        AssetDatabase.ImportAsset(waterPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.ImportAsset(boatPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

        var waterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(waterPath);
        var boatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(boatPath);

        if (waterPrefab == null || boatPrefab == null)
        {
            Debug.LogError("SetupBoatScene: missing prefab references");
            return;
        }

        if (GameObject.Find("SUIMONO_Module") == null)
        {
            var water = (GameObject)PrefabUtility.InstantiatePrefab(waterPrefab);
            water.transform.position = Vector3.zero;
        }

        // Existing water surface in this scene sits around (235.79, 11.589, 209.59);
        // place the boat in front of the camera (at x=235.79, z=109.59) so it's visible on the water.
        var boat = (GameObject)PrefabUtility.InstantiatePrefab(boatPrefab);
        boat.name = "SailBoat";
        boat.transform.position = new Vector3(235.79f, 11.6f, 160f);
        boat.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);

        var buoy = boat.AddComponent<fx_buoyancy>();
        buoy.keepAtSurface = true;
        buoy.engageBuoyancy = true;
        buoy.buoyancyOffset = -0.3f;

        boat.AddComponent<BoatSailing>();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("SetupBoatScene: done");
    }
}
