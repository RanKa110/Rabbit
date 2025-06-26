using UnityEngine;

//  보스에게만 적용 (일반 공격 시 게이지 충전 → 게이지를 모을 때마다 패턴 공격 시행!
public interface IHasGauge
{
    void AddBasicGauge();
}
