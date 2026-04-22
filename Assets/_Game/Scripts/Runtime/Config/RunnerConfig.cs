using UnityEngine;

namespace InfinityRunner
{
    [CreateAssetMenu(menuName = "Infinity Runner/Runner Config", fileName = "RunnerConfig")]
    public sealed class RunnerConfig : ScriptableObject
    {
        [Header("Lanes")]
        [Min(1f)] public float laneSpacing = 3f;
        [Min(0.01f)] public float laneChangeDuration = 0.18f;

        [Header("Jump")]
        [Min(0.1f)] public float jumpVelocity = 12.5f;
        [Min(0.1f)] public float gravity = 28f;
        [Min(0.1f)] public float fastFallVelocity = 24f;
        [Min(0f)] public float groundHeight = 0.75f;
        [Min(0f)] public float visualRollSpeed = 75f;

        [Header("World Generation")]
        [Min(30f)] public float spawnAheadDistance = 130f;
        [Min(5f)] public float despawnBehindDistance = 45f;
        [Min(1)] public int maxBlocksActive = 7;

        [Header("Input")]
        [Min(8f)] public float touchSwipeThresholdPixels = 80f;
        [Min(0.01f)] public float touchTapMaxDuration = 0.25f;

        [Header("Score")]
        [Min(0f)] public float distanceScorePerMeter = 1f;
        public int personScore = 100;
        public int destructibleScore = 25;

        public float LaneToX(Lane lane)
        {
            return (int)lane * laneSpacing;
        }
    }
}
