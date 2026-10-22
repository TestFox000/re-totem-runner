#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

internal static class TotemRuntimeDiagnostics
{
    private static TotemRuntimeProbe probe;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        GameObject host = new GameObject("Restoration runtime diagnostics");
        Object.DontDestroyOnLoad(host);
        probe = host.AddComponent<TotemRuntimeProbe>();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Report(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Report(scene);
    }

    private static void Report(Scene scene)
    {
        Debug.Log("TOTEM_SCENE_READY name=" + scene.name + " index=" + scene.buildIndex
            + " roots=" + scene.rootCount + " activeCameras=" + Camera.allCamerasCount);
        if (probe != null) probe.StartCoroutine(probe.ReportAfterStart(scene));
    }
}

internal sealed class TotemRuntimeProbe : MonoBehaviour
{
    private float nextStateReport = 15f;
    private int touchReports;

    private void Update()
    {
        if (touchReports < 20)
            for (int i = 0; i < Input.touchCount; i++)
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    touchReports++;
                    Debug.Log("TOTEM_TOUCH scene=" + SceneManager.GetActiveScene().name
                        + " position=" + Input.GetTouch(i).position);
                }
        if (Time.realtimeSinceStartup < nextStateReport) return;
        nextStateReport = Time.realtimeSinceStartup + 20f;
        PlayerController player = Object.FindObjectOfType<PlayerController>();
        Debug.Log("TOTEM_RUNTIME_STATE scene=" + SceneManager.GetActiveScene().name
            + " player=" + (player != null ? player.transform.position.ToString() : "none")
            + " timeScale=" + Time.timeScale + " fps=" + (1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f)));
        ReportControls(player);
    }

    private static string Field(PlayerController player, string name)
    {
        System.Reflection.FieldInfo field = typeof(PlayerController).GetField(name,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        return field != null ? System.Convert.ToString(field.GetValue(player)) : "n/a";
    }

    private static void ReportControls(PlayerController player)
    {
        if (player == null) return;
        GuiGameHud hud = Object.FindObjectOfType<GuiGameHud>();
        GameInput input = Object.FindObjectOfType<GameInput>();
        PlayerAnimation animation = player.GetComponent<PlayerAnimation>();
        CharacterController controller = player.GetComponent<CharacterController>();
        AnimationState state = animation != null ? animation.GetCurentAnimationState() : null;
        Debug.Log("TOTEM_CONTROL_STATE hud=" + (hud != null ? hud.GetGuiState().ToString() : "none")
            + " inputStyle=" + GameInput.inputStyle + " controlEnabled=" + player.IsEnableControl()
            + " colliderEnabled=" + (controller != null && controller.enabled)
            + " firstSpawnTime=" + Field(player, "firstSpawnTime") + " firstSpawnState=" + Field(player, "firstSpawnState")
            + " disableController=" + Field(player, "disableController")
            + " animation=" + (state != null ? state.name + " time=" + state.time + " length=" + state.length
                + " speed=" + state.speed + " enabled=" + state.enabled + " wrap=" + state.wrapMode : "none")
            + " boarHidden=" + (input != null ? GameInput.IsHiddenButton(eGameInputButton.skinBoar).ToString() : "n/a")
            + " eagleHidden=" + (input != null ? GameInput.IsHiddenButton(eGameInputButton.skinEagle).ToString() : "n/a"));
    }

    public IEnumerator ReportAfterStart(Scene scene)
    {
        yield return new WaitForSecondsRealtime(2f);
        if (SceneManager.GetActiveScene() != scene) yield break;
        foreach (Camera camera in Camera.allCameras)
        {
            Matrix4x4 matrix = camera.projectionMatrix;
            Debug.Log("TOTEM_CAMERA name=" + camera.name + " pos=" + camera.transform.position
                + " rot=" + camera.transform.eulerAngles + " ortho=" + camera.orthographic
                + " size=" + camera.orthographicSize + " fov=" + camera.fieldOfView + " aspect=" + camera.aspect
                + " near=" + camera.nearClipPlane + " far=" + camera.farClipPlane + " depth=" + camera.depth
                + " clear=" + camera.clearFlags + " mask=" + camera.cullingMask + " rect=" + camera.rect
                + " physical=" + camera.usePhysicalProperties + " matrix=" + matrix.ToString("F5"));
        }
        Renderer[] renderers = Object.FindObjectsOfType<Renderer>();
        int enabledCount = 0;
        foreach (Renderer renderer in renderers) if (renderer.enabled) enabledCount++;
        PlayerController player = Object.FindObjectOfType<PlayerController>();
        Debug.Log("TOTEM_RENDERERS scene=" + scene.name + " active=" + renderers.Length + " enabled=" + enabledCount
            + " player=" + (player != null ? player.transform.position.ToString() : "none") + " timeScale=" + Time.timeScale);
        ReportControls(player);
        if (player != null)
            foreach (Camera camera in Camera.allCameras)
                Debug.Log("TOTEM_PLAYER_VIEW camera=" + camera.name + " viewport=" + camera.WorldToViewportPoint(player.transform.position));
    }
}
#endif
