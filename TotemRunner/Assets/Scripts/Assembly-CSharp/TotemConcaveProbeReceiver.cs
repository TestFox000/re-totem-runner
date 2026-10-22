#if UNITY_EDITOR
using UnityEngine;

public sealed class TotemConcaveProbeReceiver : MonoBehaviour
{
    public int enters, stays, exits;
    public Collider lastCollider;
    private void OnTriggerEnter(Collider other) { ++enters; lastCollider = other; }
    private void OnTriggerStay(Collider other) { ++stays; lastCollider = other; }
    private void OnTriggerExit(Collider other) { ++exits; lastCollider = other; }
}
#endif
