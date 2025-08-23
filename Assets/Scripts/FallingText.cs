using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class FallingText : MonoBehaviour, IPointerClickHandler
{
    // 물리 파라미터
    public float gravity = 2000f;           // 중력 가속도(px/s^2) - UI 기준
    public float terminalSpeed = 1500f;     // 최대 낙하 속도(px/s)
    public float sideClampPadding = 8f;     // 좌우 벽 패딩

    // 지지대 판정 여유
    public float supportEpsilon = 2f;  
    
    private RectTransform _rt;
    private RectTransform _container;
    private FallingTextSpawner _spawner;

    private Vector2 _vel;                   // 현재 속도
    private bool _settled = false;
    
    private TypewriterUI _typewriter;

    // 외부에서 초기화
    public void Init(FallingTextSpawner spawner, RectTransform container, TypewriterUI typewriter = null)
    {
        _spawner = spawner;
        _container = container;
        _rt = GetComponent<RectTransform>();
        _vel = Vector2.zero;
        _settled = false;
        _typewriter = typewriter;
    }

    void Update()
    {
        if (_rt == null || _container == null) return;
        
        if (_settled)
        {
            if (!HasValidSupport())
            {
                // 더이상 지지대가 없다 → 다시 낙하 시작
                _settled = false;
                _vel = Vector2.zero;
                _spawner?.NotifyUnsettled(_rt);
            }
            else
            {
                // 여전히 지지대가 있다면 그대로 유지
                return;
            }
        }

        float dt = Time.deltaTime;

        // 중력 적용
        _vel.y -= gravity * dt;
        if (_vel.y < -terminalSpeed) _vel.y = -terminalSpeed;

        // 위치 업데이트
        Vector2 pos = _rt.anchoredPosition;
        pos += _vel * dt;

        // 좌우 벽 클램프
        var cont = _container.rect;
        float halfW = cont.width * 0.5f;
        float halfH = cont.height * 0.5f;

        float myHalfW = _rt.rect.width * 0.5f;
        float myHalfH = _rt.rect.height * 0.5f;

        float left = -halfW + sideClampPadding + myHalfW;
        float right = halfW - sideClampPadding - myHalfW;

        pos.x = Mathf.Clamp(pos.x, left, right);

        // 바닥 충돌 체크 (컨테이너의 아래쪽)
        float bottomLimit = -halfH + myHalfH;
        if (pos.y <= bottomLimit)
        {
            pos.y = bottomLimit;
            Settle(pos);
            return;
        }

        // 이미 정착된 텍스트들과 충돌 체크 (간단 AABB)
        if (_spawner != null)
        {
            List<RectTransform> settled = _spawner.settledRects;
            for (int i = 0; i < settled.Count; i++)
            {
                var other = settled[i];
                if (other == null) continue;

                if (IsOverlapping(pos, _rt.rect, other.anchoredPosition, other.rect))
                {
                    // 위에서 내려오며 닿았다고 가정: 내 bottom을 other의 top에 맞춤
                    float otherTop = other.anchoredPosition.y + other.rect.height * 0.5f;
                    pos.y = otherTop + myHalfH;

                    // 좌우 간단 정렬(선택): 수평으로 너무 많이 겹치면 약간 밀어내기
                    float dx = pos.x - other.anchoredPosition.x;
                    if (Mathf.Abs(dx) < (myHalfW + other.rect.width * 0.5f) * 0.6f)
                    {
                        pos.x += Mathf.Sign(dx == 0 ? Random.Range(-1f, 1f) : dx) * 2f;
                        pos.x = Mathf.Clamp(pos.x, left, right);
                    }

                    Settle(pos);
                    return;
                }
            }
        }

        _rt.anchoredPosition = pos;
    }

    // 간단 AABB 충돌(anchoredPosition, pivot=0.5 가정)
    bool IsOverlapping(Vector2 posA, Rect rectA, Vector2 posB, Rect rectB)
    {
        float halfWA = rectA.width * 0.5f;
        float halfHA = rectA.height * 0.5f;
        float halfWB = rectB.width * 0.5f;
        float halfHB = rectB.height * 0.5f;

        bool overlapX = Mathf.Abs(posA.x - posB.x) < (halfWA + halfWB);
        bool overlapY = Mathf.Abs(posA.y - posB.y) < (halfHA + halfHB);
        return overlapX && overlapY;
    }

    void Settle(Vector2 finalPos)
    {
        _rt.anchoredPosition = finalPos;
        _vel = Vector2.zero;
        _settled = true;
        _spawner?.NotifySettled(_rt);
    }
    
    bool HasValidSupport()
    {
        if (_container == null) return false;

        var cont = _container.rect;
        float halfH = cont.height * 0.5f;
        float myHalfH = _rt.rect.height * 0.5f;

        float bottomY = _rt.anchoredPosition.y - myHalfH;

        // 1) 바닥이 지지대인 경우
        float bottomLimit = -halfH;
        if (bottomY <= bottomLimit + supportEpsilon) return true;

        // 2) 다른 정착 텍스트가 지지대인지 검사
        if (_spawner == null) return false;
        var settled = _spawner.settledRects;
        for (int i = 0; i < settled.Count; i++)
        {
            var other = settled[i];
            if (other == null || other == _rt) continue;

            // 가로로 겹치고, other의 top이 내 bottom 바로 아래/근처면 지지대로 판단
            float otherTop = other.anchoredPosition.y + other.rect.height * 0.5f;

            bool overlapX = Mathf.Abs(_rt.anchoredPosition.x - other.anchoredPosition.x)
                            < (_rt.rect.width + other.rect.width) * 0.5f;

            if (overlapX && bottomY - otherTop <= supportEpsilon && otherTop <= bottomY + supportEpsilon)
                return true;
        }
        return false;
    }
    
    public void ForceUnsettle()
    {
        if (_settled)
        {
            _settled = false;
            _vel = Vector2.zero;
            _spawner?.NotifyUnsettled(_rt);
        }
    }


    // 클릭: 문자열 로그 후 제거
    public void OnPointerClick(PointerEventData eventData)
    {
        var tmp = GetComponent<TMP_Text>();
        string content = tmp != null ? tmp.text : gameObject.name;
        Debug.Log($"Clicked Text: {content}");
        _typewriter?.TypeAppend(content);
        // 제거 알림(여기서 이 위에 있는 애들 nudge됨)
        _spawner?.NotifyRemoved(_rt);
        Destroy(gameObject);
    }
}
