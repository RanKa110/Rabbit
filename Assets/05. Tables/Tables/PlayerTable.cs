using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerTable", menuName = "Tables/PlayerTable", order = 0)]
public class PlayerTable : BaseTable<int, PlayerSO>
{
    protected override string[] DataPath => new[] { "Assets/05. Tables/Datas/Player" };

    public override void CreateTable()
    {
        Type = GetType();
        foreach (PlayerSO item in dataList)
        {
            DataDic[item.ID] = item;
        }
    }
}