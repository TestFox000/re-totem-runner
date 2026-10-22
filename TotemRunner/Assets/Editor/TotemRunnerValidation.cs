using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class TotemRunnerValidation
{
    public static void ValidateMeshData()
    {
        int count = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Mesh", new[] { "Assets/Mesh" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null || mesh.vertexCount == 0 || !mesh.HasVertexAttribute(VertexAttribute.Position))
                throw new InvalidDataException("Recovered mesh has no vertex positions: " + path);
            count++;
        }
        if (count != 598)
            throw new InvalidDataException("Expected 598 recovered meshes, found " + count);
        var obsoleteComponent = new Regex(@"^--- !u!(12|15|26|92|132) ", RegexOptions.Multiline);
        int cameraCount = 0;
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            string source = File.ReadAllText(scene.path);
            if (obsoleteComponent.IsMatch(source))
                throw new InvalidDataException("Removed legacy component remains in " + scene.path);
            if (Regex.IsMatch(source, @"^  m_projectionMatrixMode: 0\r?$", RegexOptions.Multiline))
                throw new InvalidDataException("Invalid explicit camera projection remains in " + scene.path);
            cameraCount += Regex.Matches(source, @"^  m_projectionMatrixMode: 1\r?$", RegexOptions.Multiline).Count;
        }
        if (cameraCount != 59) throw new InvalidDataException("Expected 59 original implicit-projection cameras, found " + cameraCount);
        Debug.Log("TOTEM_DATA_VALIDATION_SUCCEEDED: meshes=" + count + " cameras=" + cameraCount);
    }
}
