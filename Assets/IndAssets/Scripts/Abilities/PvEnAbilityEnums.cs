using System;

namespace IndAssets.Scripts.Abilities
{
    public enum PvEnDamageType
    {
        None = 0,
        Punch = 1,
        Pierce = 2,
        Flame = 3,
        Water = 4,
        NatureA = 5,
        NatureB = 6,
        NatureC = 7,
        Dark = 8,
        Light = 9
    }

    [Flags]
    public enum PvEnDamageForm : uint
    {
        None = 0,
        Aggressive = 1 << 0,
        Support = 1 << 1,
        Physical = 1 << 2,
        Magical = 1 << 3,
        Melee = 1 << 4,
        Ranged = 1 << 5,
        FollowUp = 1 << 6,
        Counter = 1 << 7,
        Additional = 1 << 8
    }
}