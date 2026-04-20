using UnityEngine;

namespace InfinityRunner
{
    [CreateAssetMenu(menuName = "Infinity Runner/Block Definition", fileName = "BlockDefinition")]
    public sealed class BlockDefinition : ScriptableObject
    {
        public GameObject prefab;
        [Min(5f)] public float length = 30f;
        public BlockKind kind = BlockKind.Safe;
        public DifficultyStageMask allowedStages = DifficultyStageMask.All;
        [Min(0)] public int weight = 1;
        public LaneMask safeLanes = LaneMask.All;
        public LaneMask occupiedLanes = LaneMask.None;
        public bool isSpecial;

        public bool IsAllowed(DifficultyStage stage)
        {
            DifficultyStageMask flag = DifficultyToMask(stage);
            return (allowedStages & flag) != 0;
        }

        public static DifficultyStageMask DifficultyToMask(DifficultyStage stage)
        {
            switch (stage)
            {
                case DifficultyStage.Start:
                    return DifficultyStageMask.Start;
                case DifficultyStage.Middle:
                    return DifficultyStageMask.Middle;
                case DifficultyStage.Late:
                    return DifficultyStageMask.Late;
                default:
                    return DifficultyStageMask.None;
            }
        }
    }
}
