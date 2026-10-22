using System;
using System.Collections.Generic;
using UnityEngine;

// Unity 3.5 accepted non-convex trigger meshes. Modern PhysX rejects them.
// The original component and its triangles remain available for precise queries.
public sealed class LegacyConcaveTrigger : MonoBehaviour
{
    public MeshCollider originalCollider;

    private void OnEnable()
    {
        if (originalCollider == null) originalCollider = GetComponent<MeshCollider>();
        LegacyConcaveTriggerBridge.Register(this);
    }

    private void OnDisable() { LegacyConcaveTriggerBridge.Unregister(this); }
    private void OnDestroy() { LegacyConcaveTriggerBridge.Unregister(this); }
}

public static class LegacyConcaveTriggerBridge
{
    private struct Capsule
    {
        public Vector3 first, second, center;
        public float radius;
    }

    private sealed class PlayerState
    {
        public Capsule before;
        public Vector3 lastPosition;
        public bool hasPosition, prepared;
        public Collider[] overlaps = new Collider[32];
        public RaycastHit[] hits = new RaycastHit[32];
        public readonly HashSet<LegacyConcaveTrigger> contacts = new HashSet<LegacyConcaveTrigger>();
        public readonly HashSet<LegacyConcaveTrigger> start = new HashSet<LegacyConcaveTrigger>();
        public readonly HashSet<LegacyConcaveTrigger> finish = new HashSet<LegacyConcaveTrigger>();
        public readonly Dictionary<LegacyConcaveTrigger, float> encountered = new Dictionary<LegacyConcaveTrigger, float>();
        public readonly List<LegacyConcaveTrigger> ordered = new List<LegacyConcaveTrigger>();
        public int moves, enters, stays, exits, sweptOnly, bufferGrowths;
    }

    private static readonly Dictionary<Collider, LegacyConcaveTrigger> markers = new Dictionary<Collider, LegacyConcaveTrigger>();
    private static readonly Dictionary<CharacterController, PlayerState> players = new Dictionary<CharacterController, PlayerState>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { markers.Clear(); players.Clear(); }

    public static void Register(LegacyConcaveTrigger marker)
    {
        if (marker != null && marker.originalCollider != null) markers[marker.originalCollider] = marker;
    }

    public static void Unregister(LegacyConcaveTrigger marker)
    {
        if (ReferenceEquals(marker, null)) return;
        if (!ReferenceEquals(marker.originalCollider, null)) markers.Remove(marker.originalCollider);
        foreach (PlayerState state in players.Values)
        {
            // Native triggers do not promise an Exit when the other GO is disabled.
            state.contacts.Remove(marker);
        }
    }

    public static void ResetPlayer(CharacterController controller)
    {
        if (!ReferenceEquals(controller, null)) players.Remove(controller);
    }

    private static bool Usable(LegacyConcaveTrigger marker)
    {
        return marker != null && marker.isActiveAndEnabled && marker.originalCollider != null
            && marker.originalCollider.enabled && marker.originalCollider.gameObject.activeInHierarchy
            && marker.originalCollider.sharedMesh != null;
    }

    private static int LayerMask(CharacterController controller)
    {
        int mask = 0;
        for (int layer = 0; layer < 32; ++layer)
            if (!Physics.GetIgnoreLayerCollision(controller.gameObject.layer, layer)) mask |= 1 << layer;
        return mask;
    }

    private static Capsule Shape(CharacterController controller)
    {
        // Native CharacterController stays world-Y upright even when the player's
        // visual Transform tilts. Native bounds also incorporate scaled center,
        // nonuniform scale and the height>=diameter clamp without skin inflation.
        Bounds bounds = controller.bounds;
        float radius = Mathf.Max(bounds.extents.x, bounds.extents.z);
        float height = Mathf.Max(bounds.size.y, 2f * radius);
        Vector3 center = bounds.center;
        Vector3 offset = Vector3.up * (height * 0.5f - radius);
        return new Capsule { center = center, first = center + offset, second = center - offset, radius = radius };
    }

    public static void BeforePlayerMove(CharacterController controller)
    {
        if (controller == null || !controller.enabled || !controller.gameObject.activeInHierarchy)
        {
            ResetPlayer(controller);
            return;
        }
        if (controller.GetComponent<LegacyConcaveTriggerPlayerLifetime>() == null)
            controller.gameObject.AddComponent<LegacyConcaveTriggerPlayerLifetime>().hideFlags = HideFlags.HideInInspector;
        PlayerState state;
        if (!players.TryGetValue(controller, out state)) players.Add(controller, state = new PlayerState());
        // Spawn/teleport happens outside Move. Do not sweep from a previous life.
        if (state.hasPosition && (controller.transform.position - state.lastPosition).sqrMagnitude > 0.000001f)
            state.contacts.Clear();
        Physics.SyncTransforms();
        foreach (KeyValuePair<Collider, LegacyConcaveTrigger> pair in markers)
            if (Usable(pair.Value)) Physics.IgnoreCollision(controller, pair.Key, true);
        state.before = Shape(controller);
        state.start.Clear();
        Overlap(state, state.before, LayerMask(controller), state.start);
        state.prepared = true;
    }

    public static void AfterPlayerMove(CharacterController controller)
    {
        PlayerState state;
        if (controller == null || !players.TryGetValue(controller, out state) || !state.prepared) return;
        state.prepared = false;
        if (!controller.enabled || !controller.gameObject.activeInHierarchy) { ResetPlayer(controller); return; }
        Physics.SyncTransforms();
        Capsule finish = Shape(controller);
        int mask = LayerMask(controller);
        state.finish.Clear();
        Overlap(state, finish, mask, state.finish);
        state.encountered.Clear();
        foreach (LegacyConcaveTrigger marker in state.start) state.encountered[marker] = 0f;
        Vector3 displacement = finish.center - state.before.center;
        float distance = displacement.magnitude;
        if (distance > 0.000001f)
        {
            Sweep(state, displacement, distance, mask, false);
            // Native probe confirms shape casts miss back-facing single surfaces.
            // Query the same swept volume backwards too; no global backface flag.
            Sweep(state, displacement, distance, mask, true);
        }
        foreach (LegacyConcaveTrigger marker in state.finish)
            if (!state.encountered.ContainsKey(marker)) state.encountered[marker] = distance;
        state.ordered.Clear();
        state.ordered.AddRange(state.encountered.Keys);
        state.ordered.Sort((a, b) => {
            int compare = state.encountered[a].CompareTo(state.encountered[b]);
            return compare != 0 ? compare : a.GetInstanceID().CompareTo(b.GetInstanceID());
        });
        Vector3 movedPosition = controller.transform.position;
        foreach (LegacyConcaveTrigger marker in state.ordered)
        {
            if (!Usable(marker)) continue;
            if (state.contacts.Contains(marker)) { Deliver(marker, "OnTriggerStay", controller); ++state.stays; }
            else { Deliver(marker, "OnTriggerEnter", controller); ++state.enters; }
            if (!StillCurrent(controller, state, movedPosition)) { ResetPlayer(controller); return; }
            if (!state.finish.Contains(marker) && !state.contacts.Contains(marker))
            {
                if (Usable(marker)) Deliver(marker, "OnTriggerExit", controller);
                ++state.exits; ++state.sweptOnly;
                if (!StillCurrent(controller, state, movedPosition)) { ResetPlayer(controller); return; }
            }
        }
        state.ordered.Clear();
        state.ordered.AddRange(state.contacts);
        foreach (LegacyConcaveTrigger marker in state.ordered)
            if (!state.finish.Contains(marker))
            {
                if (Usable(marker)) Deliver(marker, "OnTriggerExit", controller);
                ++state.exits;
                if (!StillCurrent(controller, state, movedPosition)) { ResetPlayer(controller); return; }
            }
        state.contacts.Clear();
        foreach (LegacyConcaveTrigger marker in state.finish) if (Usable(marker)) state.contacts.Add(marker);
        state.lastPosition = controller.transform.position;
        state.hasPosition = true;
        ++state.moves;
        if (state.moves == 1 || state.moves % 300 == 0) LogSummary(state);
    }

    private static bool StillCurrent(CharacterController controller, PlayerState expected, Vector3 movedPosition)
    {
        PlayerState current;
        return controller != null && controller.enabled && controller.gameObject.activeInHierarchy
            && players.TryGetValue(controller, out current) && ReferenceEquals(current, expected)
            && (controller.transform.position - movedPosition).sqrMagnitude < 0.000001f;
    }

    private static void Sweep(PlayerState state, Vector3 displacement, float distance, int mask, bool reverse)
    {
        Vector3 offset = reverse ? displacement : Vector3.zero;
        Vector3 direction = (reverse ? -displacement : displacement) / distance;
        int count;
        while (true)
        {
            count = Physics.CapsuleCastNonAlloc(state.before.first + offset, state.before.second + offset,
                state.before.radius, direction, state.hits, distance, mask, QueryTriggerInteraction.Ignore);
            if (count < state.hits.Length) break;
            Array.Resize(ref state.hits, checked(state.hits.Length * 2)); ++state.bufferGrowths;
        }
        for (int i = 0; i < count; ++i)
        {
            LegacyConcaveTrigger marker;
            if (!markers.TryGetValue(state.hits[i].collider, out marker) || !Usable(marker)) continue;
            float along = reverse ? distance - state.hits[i].distance : state.hits[i].distance;
            float previous;
            if (!state.encountered.TryGetValue(marker, out previous) || along < previous) state.encountered[marker] = along;
        }
    }

    private static void Overlap(PlayerState state, Capsule capsule, int mask, HashSet<LegacyConcaveTrigger> output)
    {
        int count;
        while (true)
        {
            count = Physics.OverlapCapsuleNonAlloc(capsule.first, capsule.second, capsule.radius,
                state.overlaps, mask, QueryTriggerInteraction.Ignore);
            if (count < state.overlaps.Length) break;
            Array.Resize(ref state.overlaps, checked(state.overlaps.Length * 2)); ++state.bufferGrowths;
        }
        for (int i = 0; i < count; ++i)
        {
            LegacyConcaveTrigger marker;
            if (markers.TryGetValue(state.overlaps[i], out marker) && Usable(marker)) output.Add(marker);
        }
    }

    private static void Deliver(LegacyConcaveTrigger marker, string message, CharacterController controller)
    {
        marker.gameObject.SendMessage(message, controller, SendMessageOptions.DontRequireReceiver);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    private static void LogSummary(PlayerState state)
    {
        Debug.Log("TOTEM_CONCAVE_BRIDGE moves=" + state.moves + " activeMarkers=" + markers.Count
            + " contacts=" + state.contacts.Count + " enter=" + state.enters + " stay=" + state.stays
            + " exit=" + state.exits + " sweptOnly=" + state.sweptOnly + " bufferGrowths=" + state.bufferGrowths);
    }
}
