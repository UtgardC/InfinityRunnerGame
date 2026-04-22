using System;

namespace InfinityRunner
{
    public enum RunnerState
    {
        Menu,
        TransitionToRun,
        Running,
        GameOver,
        TransitionToMenu
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
        Destructible,
        PowerUp
    }

    public enum DestructionMode
    {
        InstantVfx,
        PrebakedBreak
    }

    public enum PowerUpType
    {
        InvincibleRock,
        ScoreMultiplier
    }

    [Flags]
    public enum PowerUpTypeMask
    {
        None = 0,
        InvincibleRock = 1 << 0,
        ScoreMultiplier = 1 << 1,
        All = InvincibleRock | ScoreMultiplier
    }
}
