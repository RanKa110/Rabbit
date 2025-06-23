using System;

public interface ITable
{
    public Type Type { get; }
    public void AutoAssignDatas();
    public void CreateTable();
}