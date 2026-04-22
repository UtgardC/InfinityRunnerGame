using UnityEngine;

namespace InfinityRunner
{
    [CreateAssetMenu(menuName = "Infinity Runner/Power Up Definition", fileName = "PowerUpDefinition")]
    public sealed class PowerUpDefinition : ScriptableObject
    {
        public PowerUpType type = PowerUpType.InvincibleRock;
        [Min(0.1f)] public float durationSeconds = 6f;
        [Min(1)] public int spawnWeight = 1;
        public PowerUpPickup pickupPrefab;
        [Min(2)] public int scoreMultiplierValue = 2;
    }
}
