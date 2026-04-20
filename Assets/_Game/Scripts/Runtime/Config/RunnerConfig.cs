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
        [Min(0f)] public float groundHeight = 0.75f;
        [Min(0.1f)] public float jumpClearHeight = 1.25f;

        [Header("World Generation")]
        [Min(30f)] public float spawnAheadDistance = 130f;
        [Min(5f)] public float despawnBehindDistance = 45f;
        [Min(1)] public int initialBlocks = 4;
        [Min(1)] public int maxBlocksActive = 7;

        [Header("Input")]
        [Min(8f)] public float touchSwipeThresholdPixels = 80f;
        [Min(0.01f)] public float touchTapMaxDuration = 0.25f;

        [Header("Score")]
        [Min(0f)] public float distanceScorePerMeter = 1f;
        public int personScore = 100;
        public int destructibleScore = 25;
        public int poweredHazardScore = 75;
        public int clashScore = 500;
        public int rampLandingScore = 200;

        [Header("Power Ups")]
        [Range(0f, 1f)] public float powerUpChancePerEligibleBlock = 0.22f;
        [Min(0)] public int powerUpCooldownBlocks = 3;

        [Header("Clash")]
        [Min(1f)] public float clashRequiredPower = 8f;
        [Min(0.1f)] public float clashTapPower = 1f;
        [Min(0f)] public float clashPowerDecayPerSecond = 0.45f;

        [Header("Divine Ramp")]
        [Min(0.25f)] public float rampFlightDuration = 1.55f;
        [Min(0.5f)] public float rampFlightHeight = 7f;
        [Min(0.1f)] public float rampInvulnerabilityPadding = 0.35f;

        public float LaneToX(Lane lane)
        {
            return (int)lane * laneSpacing;
        }
    }
}
