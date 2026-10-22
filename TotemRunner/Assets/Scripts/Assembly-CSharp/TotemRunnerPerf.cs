using UnityEngine;

public class TotemRunnerPerf : MonoBehaviour
{
    private float acc;
    private float window;
    private int frames;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        GameObject obj = new GameObject("TotemRunnerPerf");
        Object.DontDestroyOnLoad(obj);
        obj.AddComponent<TotemRunnerPerf>();
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;
        acc += dt;
        window += dt;
        frames++;
        if (window >= 10f)
        {
            Debug.Log("[FPS] avg=" + (frames / acc).ToString("0.0") + " frames=" + frames + " window=" + window.ToString("0.0"));
            acc = 0f;
            frames = 0;
            window = 0f;
        }
    }
}
