using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Read-only on disk: this probe never saves the opened scene.
public static class TotemCameraProjectionProbe
{
    [Serializable]
    private sealed class CameraState
    {
        public string label;
        public string name;
        public int projectionMatrixMode;
        public bool usePhysicalProperties;
        public Vector2 sensorSize;
        public float focalLength;
        public bool orthographic;
        public float orthographicSize;
        public float fieldOfView;
        public float aspect;
        public Vector3 position;
        public Vector3 forward;
        public float nearClip;
        public float farClip;
        public float[] projectionMatrix;
        public Vector3 viewportCenterAt800;
    }

    [Serializable]
    private sealed class Probe
    {
        public string unityVersion;
        public string enumDefinition;
        public List<CameraState> states = new List<CameraState>();
        public string note = "No scene was saved. ResetProjectionMatrix calls affect only this temporary Editor session.";
    }

    public static void Capture()
    {
        Probe probe = new Probe();
        probe.unityVersion = Application.unityVersion;
        Type mode = typeof(Camera).GetNestedType("ProjectionMatrixMode", BindingFlags.NonPublic);
        if (mode != null)
        {
            List<string> values = new List<string>();
            foreach (object value in Enum.GetValues(mode)) values.Add(value + "=" + Convert.ToInt32(value));
            probe.enumDefinition = string.Join(", ", values);
        }
        GameObject nativeDefault = new GameObject("Native Default Camera Probe");
        nativeDefault.hideFlags = HideFlags.HideAndDontSave;
        Camera camera = nativeDefault.AddComponent<Camera>();
        probe.states.Add(Read("native AddComponent default", camera));
        UnityEngine.Object.DestroyImmediate(nativeDefault);

        EditorSceneManager.OpenScene("Assets/Levels/Block_00_(Tutorial)/Tutorial.unity", OpenSceneMode.Single);
        Camera[] cameras = UnityEngine.Object.FindObjectsOfType<Camera>(true);
        Array.Sort(cameras, (a, b) => a.depth.CompareTo(b.depth));
        foreach (Camera item in cameras)
        {
            probe.states.Add(Read("loaded scene before reset", item));
            item.ResetProjectionMatrix();
            probe.states.Add(Read("in-memory ResetProjectionMatrix", item));
        }
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string output = Path.Combine(Directory.GetParent(projectRoot).FullName, "Analysis/TotemRunner/camera-projection-probe.json");
        File.WriteAllText(output, JsonUtility.ToJson(probe, true));
        Debug.Log("TOTEM_CAMERA_PROJECTION_PROBE " + output + " enum=" + probe.enumDefinition);
    }

    private static CameraState Read(string label, Camera camera)
    {
        CameraState state = new CameraState();
        state.label = label;
        state.name = camera.name;
        SerializedProperty mode = new SerializedObject(camera).FindProperty("m_projectionMatrixMode");
        state.projectionMatrixMode = mode == null ? -1 : mode.intValue;
        state.usePhysicalProperties = camera.usePhysicalProperties;
        state.sensorSize = camera.sensorSize;
        state.focalLength = camera.focalLength;
        state.orthographic = camera.orthographic;
        state.orthographicSize = camera.orthographicSize;
        state.fieldOfView = camera.fieldOfView;
        state.aspect = camera.aspect;
        state.position = camera.transform.position;
        state.forward = camera.transform.forward;
        state.nearClip = camera.nearClipPlane;
        state.farClip = camera.farClipPlane;
        state.projectionMatrix = new float[16];
        Matrix4x4 matrix = camera.projectionMatrix;
        for (int row = 0; row < 4; row++)
            for (int column = 0; column < 4; column++)
                state.projectionMatrix[row * 4 + column] = matrix[row, column];
        state.viewportCenterAt800 = camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 800f));
        Debug.Log("TOTEM_CAMERA_PROJECTION " + label + " " + state.name + " mode=" + state.projectionMatrixMode
            + " physical=" + state.usePhysicalProperties + " fov=" + state.fieldOfView
            + " ortho=" + state.orthographic + " orthoSize=" + state.orthographicSize
            + " matrix00=" + matrix[0, 0] + " matrix11=" + matrix[1, 1]
            + " matrix22=" + matrix[2, 2] + " position=" + state.position);
        return state;
    }
}
