using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Finite native Play Mode probe; creates temporary objects and saves no scene.
[InitializeOnLoad]
public static class TotemParticleLifecycleProbe
{
    private const string PendingKey = "TotemParticleLifecycleProbe.Pending";
    private const string ExitCodeKey = "TotemParticleLifecycleProbe.ExitCode";
    [Serializable] private sealed class Check { public string name; public bool passed; public string evidence; }
    [Serializable] private sealed class Report
    {
        public string unityVersion;
        public bool passed = true;
        public string note = "Native Unity Play Mode. Synthetic source-like emitter/animator lifecycle; no original scene saved. Offscreen particle culling disabled only on probe objects.";
        public List<Check> checks = new List<Check>();
    }

    private static Report report;
    private static GameObject root;
    private static GameObject empty;
    private static GameObject live;
    private static ParticleEmitter emptyEmitter;
    private static ParticleEmitter liveEmitter;
    private static ParticleSystem liveSystem;
    private static int pausedCount;
    private static float pausedLifetime;
    private static double started;
    private static double stageStarted;
    private static int stage;
    private static bool startedProbe;

    static TotemParticleLifecycleProbe()
    {
        EditorApplication.playModeStateChanged += OnPlayModeState;
        if (SessionState.GetBool(PendingKey, false) && EditorApplication.isPlaying)
            EditorApplication.delayCall += BeginProbe;
    }

    public static void Run()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetBool(PendingKey, true);
        SessionState.SetInt(ExitCodeKey, 1);
        EditorApplication.isPlaying = true;
    }

    private static void OnPlayModeState(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(PendingKey, false)) return;
        if (state == PlayModeStateChange.EnteredPlayMode) BeginProbe();
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            SessionState.SetBool(PendingKey, false);
            EditorApplication.Exit(SessionState.GetInt(ExitCodeKey, 1));
        }
    }

    private static void BeginProbe()
    {
        if (startedProbe || !EditorApplication.isPlaying) return;
        startedProbe = true;
        report = new Report { unityVersion = Application.unityVersion };
        started = stageStarted = EditorApplication.timeSinceStartup;
        try
        {
            Time.timeScale = 1f;
            root = new GameObject("temporary particle lifecycle probe");
            root.SetActive(false);
            emptyEmitter = MakeEmitter(root.transform, "never emitted disabled model child", out empty);
            emptyEmitter.enabled = false;
            liveEmitter = MakeEmitter(root.transform, "reusable grass drop model child", out live);
            root.SetActive(true);
            // Same sequence as EnableGrassDropEffect(false), followed by model
            // SetActiveRecursively: emitter remains disabled while GO is active.
            empty.SetActive(false);
            empty.SetActive(true);
            liveEmitter.Emit(4);
            liveSystem = live.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = liveSystem.main;
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            liveEmitter.enabled = false;
            pausedCount = liveSystem.particleCount;
            pausedLifetime = FirstLifetime(liveSystem);
            EditorApplication.update += Tick;
        }
        catch (Exception error) { Fail(error); }
    }

    private static ParticleEmitter MakeEmitter(Transform parent, string name, out GameObject go)
    {
        go = new GameObject(name);
        go.transform.SetParent(parent, false);
        ParticleEmitter emitter = go.AddComponent<ParticleEmitter>();
        emitter.minEnergy = emitter.maxEnergy = .2f;
        emitter.minEmission = emitter.maxEmission = 80f;
        emitter.minSize = emitter.maxSize = .1f;
        emitter.m_Emit = true;
        ParticleAnimator animator = go.AddComponent<ParticleAnimator>();
        animator.autodestruct = true;
        animator.damping = 1f;
        return emitter;
    }

    private static void Tick()
    {
        try
        {
            double now = EditorApplication.timeSinceStartup;
            if (now - started > 15) throw new TimeoutException("Particle lifecycle probe exceeded 15 seconds.");
            if (stage == 0 && now - stageStarted > .4)
            {
                CheckResult("empty disabled emitter survives recursive model activation", empty != null && !emptyEmitter.enabled && emptyEmitter.particleCount == 0,
                    "exists=" + (empty != null) + " enabled=" + (emptyEmitter != null && emptyEmitter.enabled) + " count=" + (emptyEmitter != null ? emptyEmitter.particleCount : -1));
                int count = liveSystem != null ? liveSystem.particleCount : -1;
                float lifetime = liveSystem != null ? FirstLifetime(liveSystem) : -1;
                CheckResult("disabled emitter pauses native particles beyond their lifetime", live != null && count == pausedCount && pausedCount > 0 && Mathf.Abs(lifetime - pausedLifetime) < .001f,
                    "beforeCount=" + pausedCount + " afterCount=" + count + " beforeLifetime=" + pausedLifetime + " afterLifetime=" + lifetime + " elapsed=" + (now - stageStarted));
                if (live == null) throw new InvalidOperationException("Reusable effect was destroyed while disabled.");
                live.SetActive(false);
                live.SetActive(true);
                CheckResult("previously emitted disabled effect survives model reactivation", live != null && !liveEmitter.enabled,
                    "exists=" + (live != null) + " countAfterNativeHide=" + liveSystem.particleCount);
                liveEmitter.enabled = true;
                CheckResult("reenabling restores particle state cleared by native model hide", liveSystem.particleCount == pausedCount && Mathf.Abs(FirstLifetime(liveSystem) - pausedLifetime) < .001f,
                    "restoredCount=" + liveSystem.particleCount + " restoredLifetime=" + FirstLifetime(liveSystem));
                stage = 1; stageStarted = now;
            }
            else if (stage == 1 && now - stageStarted > .12)
            {
                CheckResult("reenabled emitter resumes native simulation and emission", live != null && liveEmitter.enabled && liveSystem.isPlaying && liveEmitter.particleCount > pausedCount && FirstLifetime(liveSystem) < pausedLifetime,
                    "playing=" + liveSystem.isPlaying + " count=" + liveEmitter.particleCount + " previousCount=" + pausedCount + " firstLifetime=" + FirstLifetime(liveSystem));
                liveEmitter.emit = false;
                stage = 2; stageStarted = now;
            }
            else if (stage == 2 && now - stageStarted > .5)
            {
                CheckResult("emission stop still auto destroys after real particles expire", live == null,
                    "destroyed=" + (live == null) + " elapsed=" + (now - stageStarted));
                CheckResult("other empty disabled effect remains reusable", empty != null && !emptyEmitter.enabled,
                    "exists=" + (empty != null));
                Finish();
            }
        }
        catch (Exception error) { Fail(error); }
    }

    private static float FirstLifetime(ParticleSystem particles)
    {
        ParticleSystem.Particle[] data = new ParticleSystem.Particle[particles.main.maxParticles];
        return particles.GetParticles(data) > 0 ? data[0].remainingLifetime : -1;
    }

    private static void CheckResult(string name, bool passed, string evidence)
    {
        report.checks.Add(new Check { name = name, passed = passed, evidence = evidence });
        report.passed &= passed;
        Debug.Log("TOTEM_PARTICLE_CHECK " + passed + " " + name + " " + evidence);
    }

    private static void Fail(Exception error)
    {
        if (report == null) report = new Report { unityVersion = Application.unityVersion };
        CheckResult("probe completes without exception", false, error.ToString());
        Finish();
    }

    private static void Finish()
    {
        EditorApplication.update -= Tick;
        string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Analysis/TotemRunner/particle-lifecycle-probe.json"));
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, JsonUtility.ToJson(report, true));
        if (root != null) UnityEngine.Object.DestroyImmediate(root);
        SessionState.SetInt(ExitCodeKey, report.passed ? 0 : 1);
        Debug.Log("TOTEM_PARTICLE_LIFECYCLE_PROBE " + (report.passed ? "PASS" : "FAIL") + " " + path);
        EditorApplication.isPlaying = false;
    }
}
