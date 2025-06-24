using UnityEngine;

public class GaugeManager
{
    private float _gauge;
    private readonly float _maxGauge;
    private readonly float _gaugePerAttack;

    public GaugeManager(float maxGauge, float perAttack)
    {
        _maxGauge = maxGauge;
        _gaugePerAttack = perAttack;
        _gauge = 0f;
    }

    public void Add()
    {
        _gauge = Mathf.Min(_gauge + _gaugePerAttack, _maxGauge);
        Debug.Log($"게이지 증가: {_gauge} / {_maxGauge}");
    }

    public bool IsFull() => _gauge >= _maxGauge;
    public void Reset() => _gauge = 0f;
}
