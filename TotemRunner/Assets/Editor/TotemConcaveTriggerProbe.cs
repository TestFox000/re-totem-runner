using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Creates temporary in-memory objects only. Never saves scenes or materials.
public static class TotemConcaveTriggerProbe
{
    [Serializable] private sealed class Check { public string name; public bool passed; public string evidence; }
    [Serializable] private sealed class Report
    {
        public string unityVersion;
        public bool passed;
        public List<Check> checks = new List<Check>();
        public string note = "Native Editor physics, temporary objects only. No scene/mesh saved. Query capsule uses radius/height without additional skinWidth.";
    }
    private static Report report;

    public static void Run()
    {
        report = new Report { unityVersion = Application.unityVersion, passed = true };
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        List<GameObject> objects = new List<GameObject>();
        try
        {
            Mesh band = OpenBand();
            GameObject wall = MakeMesh("open concave U wall", band, objects);
            MeshCollider mesh = wall.GetComponent<MeshCollider>();
            GameObject player = new GameObject("native CC probe"); objects.Add(player);
            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1f; cc.radius = .2f; cc.center = Vector3.zero; cc.skinWidth = .01f; cc.minMoveDistance = 0f;
            player.transform.position = new Vector3(2f, 4f, 0f);
            Physics.SyncTransforms();
            bool before = Contains(Physics.OverlapCapsule(new Vector3(2, 4.3f, 0), new Vector3(2, 3.7f, 0), .2f), mesh);
            Physics.IgnoreCollision(cc, mesh, true);
            bool after = Contains(Physics.OverlapCapsule(new Vector3(2, 4.3f, 0), new Vector3(2, 3.7f, 0), .2f), mesh);
            CheckResult("IgnoreCollision does not filter OverlapCapsule", before && after, "before=" + before + " after=" + after + " ignored=" + Physics.GetIgnoreCollision(cc, mesh));
            bool notch = Contains(Physics.OverlapCapsule(new Vector3(3, 4.3f, 0), new Vector3(3, 3.7f, 0), .2f), mesh);
            CheckResult("open concave notch remains empty", !notch, "notch (3,4,0), within convex hull; overlap=" + notch);
            RaycastHit ray;
            bool rayHit = mesh.Raycast(new Ray(new Vector3(3, 4, 0), Vector3.left), out ray, 2f);
            CheckResult("original nonconvex Collider.Raycast remains valid", rayHit, "rayHit=" + rayHit + " distance=" + ray.distance);
            player.transform.position = new Vector3(1, 4, 0); Physics.SyncTransforms();
            cc.Move(new Vector3(2, 0, 0));
            CheckResult("ignored CC Move crosses exact concave wall", player.transform.position.x > 2.8f, "final=" + player.transform.position);
            player.transform.rotation = Quaternion.Euler(0, 0, 45); Physics.SyncTransforms();
            CheckResult("rotated native CC stays world-Y upright", (cc.bounds.extents - new Vector3(.2f,.5f,.2f)).sqrMagnitude < .000001f, "rotation45 bounds.center=" + cc.bounds.center + " extents=" + cc.bounds.extents);
            player.transform.localScale = new Vector3(2,3,4); cc.center = new Vector3(.5f,.25f,0); Physics.SyncTransforms();
            CheckResult("record nonuniform scaled native CC capsule", cc.bounds.extents.x > 0 && Mathf.Abs(cc.bounds.extents.x-cc.bounds.extents.z)<.0001f,
                "scale=(2,3,4) rotation45 center=(.5,.25,0) native center=" + cc.bounds.center + " extents=" + cc.bounds.extents + " transformedCenter=" + player.transform.TransformPoint(cc.center));
            player.transform.localScale = Vector3.one; cc.center = Vector3.zero;
            player.transform.rotation = Quaternion.identity;

            Mesh thin = PlaneMesh();
            GameObject plane = MakeMesh("thin surface fast crossing", thin, objects);
            plane.transform.position = new Vector3(10, 0, 0);
            MeshCollider thinCollider = plane.GetComponent<MeshCollider>();
            Physics.SyncTransforms();
            RaycastHit[] hits = Physics.CapsuleCastAll(new Vector3(8, .3f, 0), new Vector3(8, -.3f, 0), .2f, Vector3.right, 4f, ~0, QueryTriggerInteraction.Ignore);
            bool thinHit = false; foreach (RaycastHit hit in hits) if (hit.collider == thinCollider) thinHit = true;
            CheckResult("fast CapsuleCast catches zero thickness surface", thinHit, "4m crossing hits=" + hits.Length);
            RaycastHit[] reverseHits = Physics.CapsuleCastAll(new Vector3(12,.3f,0), new Vector3(12,-.3f,0), .2f, Vector3.left, 4f, ~0, QueryTriggerInteraction.Ignore);
            bool reverseHit = false; foreach (RaycastHit hit in reverseHits) if (hit.collider == thinCollider) reverseHit = true;
            CheckResult("record backface thin-surface cast", true, "reverseCrossingHit=" + reverseHit + " (bridge must cover both directions)");

            LegacyConcaveTrigger marker = plane.AddComponent<LegacyConcaveTrigger>(); marker.originalCollider = thinCollider; LegacyConcaveTriggerBridge.Register(marker);
            TotemConcaveProbeReceiver receiver = plane.AddComponent<TotemConcaveProbeReceiver>();
            player.transform.position = new Vector3(8, 0, 0);
            LegacyConcaveTriggerBridge.BeforePlayerMove(cc); cc.Move(new Vector3(4, 0, 0)); LegacyConcaveTriggerBridge.AfterPlayerMove(cc);
            CheckResult("bridge fast crossing delivers original CC Enter Exit", receiver.enters == 1 && receiver.exits == 1 && receiver.lastCollider == cc && player.transform.position.x > 11.8f,
                "enter=" + receiver.enters + " stay=" + receiver.stays + " exit=" + receiver.exits + " final=" + player.transform.position + " originalCC=" + (receiver.lastCollider == cc));
            LegacyConcaveTriggerBridge.BeforePlayerMove(cc); cc.Move(new Vector3(-4, 0, 0)); LegacyConcaveTriggerBridge.AfterPlayerMove(cc);
            CheckResult("bridge reverse fast crossing also delivers Enter Exit", receiver.enters == 2 && receiver.exits == 2 && player.transform.position.x < 8.2f,
                "enter=" + receiver.enters + " exit=" + receiver.exits + " final=" + player.transform.position);
            player.transform.position = new Vector3(10, 0, 0);
            LegacyConcaveTriggerBridge.BeforePlayerMove(cc); cc.Move(Vector3.zero); LegacyConcaveTriggerBridge.AfterPlayerMove(cc);
            LegacyConcaveTriggerBridge.BeforePlayerMove(cc); cc.Move(Vector3.zero); LegacyConcaveTriggerBridge.AfterPlayerMove(cc);
            CheckResult("bridge persistent surface overlap stays without duplicate Enter", receiver.enters == 3 && receiver.stays >= 1, "enter=" + receiver.enters + " stay=" + receiver.stays + " exit=" + receiver.exits);
            plane.SetActive(false); LegacyConcaveTriggerBridge.Unregister(marker);
            plane.SetActive(true); LegacyConcaveTriggerBridge.Register(marker);
            LegacyConcaveTriggerBridge.BeforePlayerMove(cc); cc.Move(Vector3.zero); LegacyConcaveTriggerBridge.AfterPlayerMove(cc);
            CheckResult("disable reenable clears prior contact", receiver.enters == 4, "enter=" + receiver.enters);
            cc.enabled = false; LegacyConcaveTriggerBridge.BeforePlayerMove(cc); cc.enabled = true;
            LegacyConcaveTriggerBridge.BeforePlayerMove(cc); cc.Move(Vector3.zero); LegacyConcaveTriggerBridge.AfterPlayerMove(cc);
            CheckResult("disabled player clears prior contact", receiver.enters == 5, "enter=" + receiver.enters);
            Physics.IgnoreLayerCollision(0, 12, true); plane.layer = 12;
            int previousEnters = receiver.enters;
            player.transform.position = new Vector3(8, 0, 0);
            LegacyConcaveTriggerBridge.BeforePlayerMove(cc); cc.Move(new Vector3(4, 0, 0)); LegacyConcaveTriggerBridge.AfterPlayerMove(cc);
            CheckResult("original collision layer filtering retained", receiver.enters == previousEnters, "enterBefore=" + previousEnters + " after=" + receiver.enters);
            Physics.IgnoreLayerCollision(0, 12, false); plane.layer = 0;

            CheckAssetSurface("Assets/Mesh/collider_22.asset", objects);
            CheckAssetSurface("Assets/Mesh/Spikes_01.asset", objects);
            List<TotemConcaveProbeReceiver> many = new List<TotemConcaveProbeReceiver>();
            for (int i = 0; i < 40; ++i)
            {
                GameObject item = MakeMesh("buffer growth surface " + i, thin, objects);
                item.transform.position = new Vector3(100 + i * .001f, 0, 0);
                LegacyConcaveTrigger itemMarker = item.AddComponent<LegacyConcaveTrigger>();
                itemMarker.originalCollider = item.GetComponent<MeshCollider>(); LegacyConcaveTriggerBridge.Register(itemMarker);
                many.Add(item.AddComponent<TotemConcaveProbeReceiver>());
            }
            player.transform.position = new Vector3(100,0,0);
            LegacyConcaveTriggerBridge.BeforePlayerMove(cc); cc.Move(Vector3.zero); LegacyConcaveTriggerBridge.AfterPlayerMove(cc);
            bool allEntered = true; foreach (TotemConcaveProbeReceiver item in many) allEntered &= item.enters == 1;
            CheckResult("saturated NonAlloc overlap buffer grows and retries", allEntered, "40 simultaneous surfaces all entered=" + allEntered);
            player.transform.position = new Vector3(98,0,0);
            LegacyConcaveTriggerBridge.BeforePlayerMove(cc); cc.Move(new Vector3(4,0,0)); LegacyConcaveTriggerBridge.AfterPlayerMove(cc);
            bool allSwept = true; foreach (TotemConcaveProbeReceiver item in many) allSwept &= item.enters == 2 && item.exits == 1;
            CheckResult("saturated NonAlloc cast buffer grows and retries", allSwept, "40 swept surfaces all enter/exit=" + allSwept);
        }
        catch (Exception exception) { CheckResult("probe exception", false, exception.ToString()); }
        finally
        {
            foreach (GameObject obj in objects) if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            string output = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Analysis/TotemRunner/concave-trigger-native-probe.json"));
            File.WriteAllText(output, JsonUtility.ToJson(report, true));
            Debug.Log("TOTEM_CONCAVE_PROBE " + (report.passed ? "PASSED " : "FAILED ") + output);
        }
        if (!report.passed) throw new Exception("Native concave trigger compatibility probe failed; inspect report.");
    }

    private static void CheckAssetSurface(string path, List<GameObject> objects)
    {
        Mesh source = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (source == null) { CheckResult("source mesh " + path, false, "asset missing"); return; }
        GameObject go = MakeMesh(path, source, objects);
        go.transform.position = new Vector3(10000, 10000, 10000);
        Physics.SyncTransforms();
        Vector3[] vertices = source.vertices; int[] triangles = source.triangles;
        bool overlap = false, sweep = false; int examined = 0;
        Vector3 bridgePoint = Vector3.zero, bridgeNormal = Vector3.zero; float bridgeRadius = .05f;
        for (int i = 0; i + 2 < triangles.Length && (!overlap || !sweep); i += 3)
        {
            Vector3 a = vertices[triangles[i]], b = vertices[triangles[i + 1]], c = vertices[triangles[i + 2]];
            Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
            if (normal.sqrMagnitude < .5f) continue;
            ++examined;
            Vector3 point = go.transform.TransformPoint((a + b + c) / 3f);
            float radius = Mathf.Min(.05f, Mathf.Max(.001f, source.bounds.size.magnitude * .0001f));
            bridgePoint = point; bridgeNormal = normal; bridgeRadius = Mathf.Max(.01f,radius);
            overlap |= Contains(Physics.OverlapCapsule(point, point, radius, ~0, QueryTriggerInteraction.Ignore), go.GetComponent<MeshCollider>());
            foreach (RaycastHit hit in Physics.CapsuleCastAll(point + normal * radius * 5, point + normal * radius * 5, radius, -normal, radius * 10, ~0, QueryTriggerInteraction.Ignore))
                if (hit.collider == go.GetComponent<MeshCollider>()) sweep = true;
        }
        CheckResult("exact original surface query " + path, overlap && sweep, "overlap=" + overlap + " sweep=" + sweep + " examinedTriangles=" + examined);
        LegacyConcaveTrigger marker = go.AddComponent<LegacyConcaveTrigger>(); marker.originalCollider = go.GetComponent<MeshCollider>(); LegacyConcaveTriggerBridge.Register(marker);
        TotemConcaveProbeReceiver receiver = go.AddComponent<TotemConcaveProbeReceiver>();
        GameObject player = new GameObject("source mesh crossing CC"); objects.Add(player);
        CharacterController cc = player.AddComponent<CharacterController>(); cc.radius = bridgeRadius; cc.height = 2f * bridgeRadius; cc.center = Vector3.zero; cc.skinWidth = .001f; cc.minMoveDistance = 0;
        Vector3 travel = bridgeNormal * (bridgeRadius * 20f);
        player.transform.position = bridgePoint + travel * .5f;
        LegacyConcaveTriggerBridge.BeforePlayerMove(cc); cc.Move(-travel); LegacyConcaveTriggerBridge.AfterPlayerMove(cc);
        bool forwardBridge = receiver.enters == 1 && receiver.exits == 1 && receiver.lastCollider == cc;
        LegacyConcaveTriggerBridge.BeforePlayerMove(cc); cc.Move(travel); LegacyConcaveTriggerBridge.AfterPlayerMove(cc);
        CheckResult("original mesh forward and reverse bridge " + path, forwardBridge && receiver.enters == 2 && receiver.exits == 2,
            "forwardPassed=" + forwardBridge + " enter=" + receiver.enters + " exit=" + receiver.exits + " travel=" + travel);
        player.SetActive(false);
        go.SetActive(false);
    }

    private static GameObject MakeMesh(string name, Mesh mesh, List<GameObject> objects)
    {
        GameObject go = new GameObject(name); objects.Add(go);
        go.AddComponent<MeshCollider>().sharedMesh = mesh;
        return go;
    }
    private static Mesh OpenBand()
    {
        Vector2[] outline = { new Vector2(0,0), new Vector2(6,0), new Vector2(6,6), new Vector2(4,6), new Vector2(4,2), new Vector2(2,2), new Vector2(2,6), new Vector2(0,6) };
        List<Vector3> vertices = new List<Vector3>(); List<int> triangles = new List<int>();
        for (int i = 0; i < outline.Length; ++i)
        {
            Vector2 a = outline[i], b = outline[(i + 1) % outline.Length]; int n = vertices.Count;
            vertices.Add(new Vector3(a.x,a.y,-1)); vertices.Add(new Vector3(b.x,b.y,-1)); vertices.Add(new Vector3(b.x,b.y,1)); vertices.Add(new Vector3(a.x,a.y,1));
            triangles.AddRange(new[] { n,n+1,n+2,n,n+2,n+3 });
        }
        Mesh mesh = new Mesh(); mesh.vertices = vertices.ToArray(); mesh.triangles = triangles.ToArray(); mesh.RecalculateBounds(); return mesh;
    }
    private static Mesh PlaneMesh()
    {
        Mesh mesh = new Mesh(); mesh.vertices = new[] { new Vector3(0,-5,-2), new Vector3(0,5,-2), new Vector3(0,5,2), new Vector3(0,-5,2) };
        mesh.triangles = new[] { 0,2,1,0,3,2 }; mesh.RecalculateBounds(); return mesh;
    }
    private static bool Contains(Collider[] colliders, Collider expected) { foreach (Collider collider in colliders) if (collider == expected) return true; return false; }
    private static void CheckResult(string name, bool passed, string evidence)
    {
        report.checks.Add(new Check { name = name, passed = passed, evidence = evidence }); report.passed &= passed;
        Debug.Log("TOTEM_CONCAVE_CHECK " + passed + " " + name + " " + evidence);
    }
}
