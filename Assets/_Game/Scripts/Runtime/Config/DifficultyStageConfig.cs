using UnityEngine;

namespace InfinityRunner
{
    [CreateAssetMenu(menuName = "Infinity Runner/Difficulty Stage", fileName = "DifficultyStageConfig")]
    public sealed class DifficultyStageConfig : ScriptableObject
    {
        public DifficultyStage stage = DifficultyStage.Start;
        [Min(1f)] public float speedMetersPerSecond = 10f;
        [Min(1)] public int blocksBeforeForcedClash = 8;
        [Range(0f, 1f)] public float lateClashChance = 0.08f;
        [Min(1)] public int minimumBlocksBetweenLateClashes = 8;
    }
}
