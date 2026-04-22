using UnityEngine;

namespace InfinityRunner
{
    [CreateAssetMenu(menuName = "Infinity Runner/Block Definition", fileName = "BlockDefinition")]
    public sealed class BlockDefinition : ScriptableObject
    {
        public GameObject prefab;
        public DifficultyStageMask allowedStages = DifficultyStageMask.All;
        [Min(1)] public int weight = 1;

        public bool Allows(DifficultyStage stage)
        {
            return (allowedStages & ToMask(stage)) != 0;
        }

        public static DifficultyStageMask ToMask(DifficultyStage stage)
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
