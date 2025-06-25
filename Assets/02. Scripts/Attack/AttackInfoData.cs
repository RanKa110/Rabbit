using System;
using UnityEngine;

[Serializable]
public class AttackInfoData
{
    [field:SerializeField] public int ComboStateIndex { get; private set; }
    [field:SerializeField] public string AttackName { get; private set; }
    [field:SerializeField] public int ExtraDamage { get; private set; }
    [field:SerializeField][field:Range(0f, 1f)] public float ComboTransitionTime { get; private set; }
    [field:SerializeField][field:Range(0f, 1f)] public float DealingStartTransitionTime { get; private set; }
    [field:SerializeField][field:Range(0f, 1f)] public float DealingEndTransitionTime { get; private set; }
}