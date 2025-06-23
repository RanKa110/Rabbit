using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyTable", menuName = "Tables/EnemyTable", order = 0)]
public class EnemyTable : BaseTable<int, EnemySO>
{
    protected override string[] DataPath => new[] { "Assets/05. Tables/Datas/Enemy" };

    public override void CreateTable()
    {
        Type = GetType();
        DataDic.Clear();
        //foreach (EnemySO item in dataList)
        //{
        //    DataDic[item.ID] = item;
        //}
    }
}