using System;

namespace InfinityRunner
{
    public enum RunnerState
    {
        Running,
        GameOver
    }

    public enum Lane
    {
        Left = -1,
        Center = 0,
        Right = 1
    }

    public enum DifficultyStage
    {
        Start,
        Middle,
        Late
    }

    [Flags]
    public enum DifficultyStageMask
    {
        None = 0,
        Start = 1 << 0,
        Middle = 1 << 1,
        Late = 1 << 2,
        All = Start | Middle | Late
    }

    public enum RunnerInteractableType
    {
        Death,
        Person,
        Destructible
    }

    public enum DestructionMode
    {
        InstantVfx,
        PrebakedBreak
    }
}
