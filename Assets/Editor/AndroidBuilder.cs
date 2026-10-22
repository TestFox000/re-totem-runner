using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class AndroidBuilder
{
    public static void BuildAPK()
    {
        var target = BuildTarget.Android;
        var group = BuildTargetGroup.Android;

        // Unity 2021.3.45f2c1 的 Android 外部工具键（已从
        // Editor/Data/PlaybackEngines/AndroidPlayer/UnityEditor.Android.Extensions.dll
        // 的 UTF-16 字符串表核实）：没有 _h<hash> 后缀，就是这几个名字。
        // 之前写死的 JdkPath_h4127442381 / AndroidNdkRoot_h773313625 等是抄来的，
        // 对本版本无效 —— 日志 451-458 行证明 SDK 读对了而 NDK 仍 not found。
        string sdkPath = "C:\\Users\\TestFox\\AppData\\Local\\Android\\Sdk";
        string ndkPath = sdkPath + "\\ndk\\21.3.6528147";   // r21d，Unity 2021.3 LTS 要求
        string jdkPath = "D:\\SoftWare\\jdk-11.0.32.1+1";  // Unity 2021.3 要求 JDK 11

        EditorPrefs.SetString("AndroidSdkRoot", sdkPath);
        EditorPrefs.SetString("AndroidNdkRootR21D", ndkPath);
        EditorPrefs.SetString("Jdk11Path", jdkPath);

        // 关掉"用 Unity 自带的那份"开关，否则路径键被忽略
        EditorPrefs.SetBool("NdkUseEmbedded", false);
        EditorPrefs.SetBool("JdkUseEmbedded", false);

        // 兜底：Unity 解析顺序是 AndroidNdkRootR21D 键 -> 环境变量 ANDROID_NDK_ROOT
        // -> 内置 NDK。键名如果因版本差异失效，环境变量还能接住。
        // 同时设 ANDROID_NDK_HOME（老名字）和 ANDROID_SDK_ROOT。
        System.Environment.SetEnvironmentVariable("ANDROID_NDK_ROOT", ndkPath);
        System.Environment.SetEnvironmentVariable("ANDROID_NDK_HOME", ndkPath);
        System.Environment.SetEnvironmentVariable("ANDROID_SDK_ROOT", sdkPath);
        System.Environment.SetEnvironmentVariable("JAVA_HOME", jdkPath);

        Debug.Log("SDK : " + sdkPath + " -> exists: " + System.IO.Directory.Exists(sdkPath));
        Debug.Log("NDK : " + ndkPath + " -> exists: " + System.IO.Directory.Exists(ndkPath));
        Debug.Log("JDK : " + jdkPath + " -> exists: " + System.IO.Directory.Exists(jdkPath));

        // 切 Android 平台
        var result = EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
        Debug.Log("Switch target: " + result);

        // IL2CPP + ARM64
        PlayerSettings.SetScriptingBackend(group, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        // 关键：本机 SDK 里只有 android-33 是完整的（platforms\android-37.0 是
        // sdkmanager 下载失败留下的空壳，0 个文件，没有 android.jar）。
        // Unity 2021.3.45 默认 target API 会去要 platforms;android-37，Gradle 直接
        // 报 "Failed to find Platform SDK with path: platforms;android-37"。
        // 显式钉到 33（Android 13，Unity 2021.3 LTS 官方支持档）。
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
        Debug.Log("minSdk=" + PlayerSettings.Android.minSdkVersion + " targetSdk=" + PlayerSettings.Android.targetSdkVersion);

        string apkPath = Application.dataPath + "/../AndroidTest.apk";

        BuildPlayerOptions opts = new BuildPlayerOptions();
        // scenes 必须给 EditorBuildSettings 里已启用的场景；空数组会被判 "no scenes"
        opts.scenes = new string[] { "Assets/Scenes/Main.unity" };
        opts.locationPathName = apkPath;
        opts.target = target;
        opts.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(opts);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
            Debug.Log("BUILD OK: " + apkPath + " (" + summary.totalSize + " bytes)");
        else
            Debug.LogError("BUILD FAILED: " + summary.totalErrors + " errors");
    }
}
