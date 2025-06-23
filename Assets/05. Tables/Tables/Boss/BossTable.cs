using UnityEngine;

[CreateAssetMenu(fileName = "BossTable", menuName = "Scriptable Objects/BossTable")]
public class BossTable : BaseTable<int, BossSO>
{
    protected override string[] DataPath => new[] { "Assets/05. Tables/Datas/Boss" };

    public override void CreateTable()
    {
        Type = GetType();
        DataDic.Clear();

        //foreach(BossSO item in dataList)
        //{
        //    DataDic[item.ID] = item;
        //}
    }
}
