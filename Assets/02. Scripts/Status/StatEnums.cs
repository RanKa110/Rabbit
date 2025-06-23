using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    MaxHp,
    CurHp,

    MaxMp,
    CurMp,
    
    AttackPow,
    AttackSpd,
    AttackRange,

    MoveSpeed,
    RunMultiplier,
    
    Defense,
}

public enum StatModifierType
{
    Base,
    BasePercent,
    
    BuffFlat,
    BuffPercent,
    
    Equipment,
}