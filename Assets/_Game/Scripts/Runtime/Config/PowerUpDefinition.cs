using UnityEngine;

namespace InfinityRunner
{
    [CreateAssetMenu(menuName = "Infinity Runner/Power Up Definition", fileName = "PowerUpDefinition")]
    public sealed class PowerUpDefinition : ScriptableObject
    {
        public PowerUpType type = PowerUpType.DestroyAll;
        public GameObject pickupPrefab;
        [Min(0.1f)] public float duration = 5f;
        public int bonusScore;
        public bool requiresFallingBlockAfterPickup;
    }
}
