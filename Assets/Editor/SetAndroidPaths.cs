using UnityEditor;
using UnityEngine;
using System.IO;

public static class SetAndroidPaths
{
    public static void SetPaths()
    {
        string jdk = "D:\\SoftWare\\Eclipse Adoptium\\jdk-17.0.20.101-hotspot";
        string sdk = "C:\\Users\\TestFox\\AppData\\Local\\Android\\Sdk";
        string ndk = Path.Combine(sdk, "ndk", "23.1.7779620");

        // 检查文件存在
        Debug.Log($"javac: {File.Exists(Path.Combine(jdk, "bin", "javac.exe"))}");
        Debug.Log($"adb:   {Directory.Exists(sdk)}");
        Debug.Log($"ndk:   {Directory.Exists(ndk)}");

        // Unity 2021 用 EditorPrefs 存
        EditorPrefs.SetString("JdkPath", jdk);
        EditorPrefs.SetString("AndroidSdkRoot", sdk);
        EditorPrefs.SetString("AndroidNdkRoot", ndk);

        Debug.Log("Paths set. Verifying...");
        Debug.Log($"JdkPath  = {EditorPrefs.GetString("JdkPath", "(empty)")}");
        Debug.Log($"SdkRoot  = {EditorPrefs.GetString("AndroidSdkRoot", "(empty)")}");
        Debug.Log($"NdkRoot  = {EditorPrefs.GetString("AndroidNdkRoot", "(empty)")}");
    }
}
