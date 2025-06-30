using UnityEngine;

public interface IParryable
{
    bool TryParrying();     //  패링 가능 시간 내에 패링 시도 시 호출하는 메서드    → true 반환 시, 패링 성공, false는 실패

    void OnParrySuccess();  //  패링이 성공했을 때 호출 → 추가 이펙트가 있을 경우 여기에 넣어둘 것!
}
