using System;

namespace InfinityRunner
{
    public enum RunnerState
    {
        Menu,
        TransitionToRun,
        Running,
        Clash,
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

    [Flags]
    public enum LaneMask
    {
        None = 0,
        Left = 1 << 0,
        Center = 1 << 1,
        Right = 1 << 2,
        All = Left | Center | Right
    }

    public enum BlockKind
    {
        Safe,
        PropsAndPeople,
        Jump,
        LaneBlock,
        DynamicCart,
        FallingPillar,
        Catapult,
        PowerUpSetup,
        FallingBlock,
        Clash
    }

    public enum RunnerInteractableType
    {
        Person,
        Destructible,
        Hazard,
        PowerUp,
        ClashTrigger,
        RampLanding
    }

    public enum PowerUpType
    {
        DestroyAll,
        DivineRamp
    }

    public enum DestructionMode
    {
        InstantVfx,
        PrebakedBreak
    }
}
