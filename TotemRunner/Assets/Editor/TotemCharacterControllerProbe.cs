using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;

// In-memory native deserialization/Move probe. No scene is saved.
public static class TotemCharacterControllerProbe
{
    [Serializable] private sealed class State
    {
        public string label, path;
        public bool enabled, activeInHierarchy;
        public Vector3 position, boundsCenter, boundsSize;
        public float radius, height, skinWidth;
        public Vector3 center;
    }
    [Serializable] private sealed class Report
    {
        public string unityVersion;
        public bool nativeDefaultEnabled, loadedSceneEnabled, nativeMoveEffective;
        public List<State> states = new List<State>();
        public string note = "Read-only on disk. Temporarily enables only the loaded CharacterController and moves it in native Editor physics. Does not save scene.";
    }
    public static void Run()
    {
        Capture("character-controller-native-probe.json");
    }
    public static void RunRestored()
    {
        Capture("character-controller-native-after-recovery.json");
    }
    private static void Capture(string reportName)
    {
        Report report = new Report { unityVersion = Application.unityVersion };
        EditorSceneManager.OpenScene("Assets/Levels/Block_00_(Tutorial)/Tutorial.unity", OpenSceneMode.Single);
        CharacterController[] controllers = UnityEngine.Object.FindObjectsOfType<CharacterController>(true);
        if (controllers.Length != 1) throw new Exception("Expected one original Tutorial CharacterController, got " + controllers.Length);
        CharacterController controller = controllers[0];
        GameObject nativeDefault = new GameObject("Temporary default CharacterController probe");
        nativeDefault.transform.position = new Vector3(-100000,-100000,-100000);
        CharacterController defaultController = nativeDefault.AddComponent<CharacterController>();
        report.nativeDefaultEnabled = defaultController.enabled;
        report.states.Add(Read("native AddComponent default", defaultController));
        UnityEngine.Object.DestroyImmediate(nativeDefault);
        Physics.SyncTransforms();
        report.loadedSceneEnabled = controller.enabled;
        report.states.Add(Read("deserialized source scene before correction",controller));
        Vector3 before = controller.transform.position;
        controller.enabled = true;
        Physics.SyncTransforms();
        report.states.Add(Read("in-memory enabled=true before Move",controller));
        controller.Move(new Vector3(10,0,0));
        Physics.SyncTransforms();
        report.states.Add(Read("after native CharacterController.Move(10,0,0)",controller));
        report.nativeMoveEffective = controller.transform.position.x > before.x + 9f;
        string output = Path.GetFullPath(Path.Combine(Application.dataPath,"../../Analysis/TotemRunner/" + reportName));
        File.WriteAllText(output,JsonUtility.ToJson(report,true));
        Debug.Log("TOTEM_CC_PROBE defaultEnabled=" + report.nativeDefaultEnabled + " loadedEnabled=" + report.loadedSceneEnabled + " moveEffective=" + report.nativeMoveEffective + " output=" + output);
        if (!report.nativeDefaultEnabled || !report.nativeMoveEffective) throw new Exception("Native CharacterController default/Move probe failed");
    }
    private static State Read(string label,CharacterController controller)
    {
        return new State { label = label, path = controller.name, enabled = controller.enabled,
            activeInHierarchy = controller.gameObject.activeInHierarchy, position = controller.transform.position,
            boundsCenter = controller.bounds.center, boundsSize = controller.bounds.size,
            radius = controller.radius, height = controller.height, center = controller.center, skinWidth = controller.skinWidth };
    }
}
