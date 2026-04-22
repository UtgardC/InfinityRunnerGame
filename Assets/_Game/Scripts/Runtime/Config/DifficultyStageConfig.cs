using UnityEngine;

namespace InfinityRunner
{
    [CreateAssetMenu(menuName = "Infinity Runner/Difficulty Stage", fileName = "DifficultyStageConfig")]
    public sealed class DifficultyStageConfig : ScriptableObject
    {
        public DifficultyStage stage = DifficultyStage.Start;
        [Min(1f)] public float speedMetersPerSecond = 10f;
        [Min(0)] public int startAfterBlockCount;
    }
}
