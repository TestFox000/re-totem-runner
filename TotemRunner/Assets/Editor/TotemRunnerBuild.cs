using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

public static class TotemRunnerBuild
{
    public static void BuildAndroid()
    {
        TotemRunnerValidation.ValidateMeshData();
        const BuildTargetGroup group = BuildTargetGroup.Android;
        string sdk = @"C:\Users\TestFox\AppData\Local\Android\Sdk";
        string ndk = Path.Combine(sdk, "ndk", "21.3.6528147");
        string jdk = @"D:\SoftWare\jdk-11.0.32.1+1";
        foreach (string path in new[] { sdk, ndk, jdk })
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);

        EditorPrefs.SetString("AndroidSdkRoot", sdk);
        EditorPrefs.SetString("AndroidNdkRootR21D", ndk);
        EditorPrefs.SetString("Jdk11Path", jdk);
        EditorPrefs.SetBool("NdkUseEmbedded", false);
        EditorPrefs.SetBool("JdkUseEmbedded", false);
        Environment.SetEnvironmentVariable("ANDROID_NDK_ROOT", ndk);
        Environment.SetEnvironmentVariable("ANDROID_NDK_HOME", ndk);
        Environment.SetEnvironmentVariable("ANDROID_SDK_ROOT", sdk);
        Environment.SetEnvironmentVariable("JAVA_HOME", jdk);

        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(group, BuildTarget.Android))
            throw new InvalidOperationException("Cannot select Android build target.");

        PlayerSettings.companyName = "Local Restoration";
        PlayerSettings.productName = "Totem Runner Restored";
        PlayerSettings.bundleVersion = "0.1.0";
        PlayerSettings.SetApplicationIdentifier(group, "local.totemrunner.restored");
        PlayerSettings.SetScriptingBackend(group, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetManagedStrippingLevel(group, ManagedStrippingLevel.Low);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.stripEngineCode = false;
        PlayerSettings.runInBackground = false;
        PlayerSettings.Android.useCustomKeystore = false;

        string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (scenes.Length != 21 || !scenes[3].EndsWith("/Tutorial.unity", StringComparison.Ordinal))
            throw new InvalidOperationException("The original 21-scene order, with Tutorial at index 3, must be preserved.");
        foreach (string scene in scenes)
            if (!File.Exists(scene)) throw new FileNotFoundException("Recovered scene missing", scene);

        string output = Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds/TotemRunner-ARM64.apk"));
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            target = BuildTarget.Android,
            locationPathName = output,
            options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException("TOTEM_BUILD_FAILED: " + report.summary.result + ", errors=" + report.summary.totalErrors);
        Debug.Log("TOTEM_BUILD_SUCCEEDED: " + output + " bytes=" + report.summary.totalSize);
    }
}
