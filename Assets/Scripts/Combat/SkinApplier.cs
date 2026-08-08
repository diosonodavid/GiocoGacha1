using GachaGame.Data;
using UnityEngine;

namespace GachaGame.Combat
{
    // Spawns a character's equipped skin's battle model/VFX at the given point instead of the
    // default model. Standalone by design: no battle-scene spawning system exists yet in this
    // codebase for it to hook into automatically, so callers invoke ApplySkin explicitly once a
    // SkinData is resolved for a combatant.
    public class SkinApplier : MonoBehaviour
    {
        public GameObject ApplySkin(Transform spawnPoint, SkinData skin)
        {
            if (skin == null || skin.battleModelPrefab == null || spawnPoint == null) return null;
            return Instantiate(skin.battleModelPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
        }
    }
}
