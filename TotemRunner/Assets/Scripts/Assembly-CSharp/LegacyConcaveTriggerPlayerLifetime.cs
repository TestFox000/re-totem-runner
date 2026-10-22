using UnityEngine;

// Added in memory on first player Move; no additional serialized player data.
public sealed class LegacyConcaveTriggerPlayerLifetime : MonoBehaviour
{
    private void OnDisable() { LegacyConcaveTriggerBridge.ResetPlayer(GetComponent<CharacterController>()); }
    private void OnDestroy() { LegacyConcaveTriggerBridge.ResetPlayer(GetComponent<CharacterController>()); }
}
