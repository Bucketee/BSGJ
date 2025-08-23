using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FallingTextSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public List<string> stringPool;           // 문자열 모음
    public TMP_Text textPrefab;               // text ui 프리팹 (RectTransform 포함)
    public float spawnInterval = 1f;          // 생성주기
    public Vector2 spawnPadding = new Vector2(10f, 10f); // 좌우/상단 패딩

    [Header("Refs")]
    public RectTransform container;
    
    [Header("Typewriter")]
    public TypewriterUI typewriter;
    
    // 낙하가 끝난 텍스트들의 RectTransform을 보관
    [HideInInspector] public List<RectTransform> settledRects = new List<RectTransform>();

    Coroutine _loop;

    void Awake()
    {
        if (container == null)
            container = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        _loop = StartCoroutine(SpawnLoop());
    }

    void OnDisable()
    {
        if (_loop != null) StopCoroutine(_loop);
    }

    IEnumerator SpawnLoop()
    {
        var wait = new WaitForSeconds(spawnInterval);
        while (true)
        {
            SpawnOne();
            yield return wait;
        }
    }

    void SpawnOne()
    {
        if (textPrefab == null || stringPool == null || stringPool.Count == 0) return;
        
        string s = stringPool[Random.Range(0, stringPool.Count)];
        TMP_Text t = Instantiate(textPrefab, container);
        t.text = s;
        RectTransform rt = t.rectTransform;

        // 폰트 사이즈
        // t.fontSize = Random.Range(24, 36);

        // 컨테이너 내부 상단에서 랜덤 x 위치로 생성
        var contRect = container.rect;
        float halfW = contRect.width * 0.5f;
        float halfH = contRect.height * 0.5f;

        float left = -halfW + spawnPadding.x;
        float right = halfW - spawnPadding.x;

        float spawnX = Random.Range(left, right);
        float spawnY = halfH - spawnPadding.y;

        rt.anchoredPosition = new Vector2(spawnX, spawnY);
        
        // 낙하 컴포넌트 설정
        var falling = t.gameObject.GetComponent<FallingText>();
        if (falling == null) falling = t.gameObject.AddComponent<FallingText>();

        falling.Init(this, container, typewriter);
    }

    // FallingText 정착 시 호출
    public void NotifySettled(RectTransform rt)
    {
        if (!settledRects.Contains(rt))
            settledRects.Add(rt);
    }
    
    public void NotifyUnsettled(RectTransform rt)
    {
        settledRects.Remove(rt);
    }

    // 클릭으로 제거
    public void NotifyRemoved(RectTransform removedRt)
    {
        settledRects.Remove(removedRt);
        NudgeAbove(removedRt);
    }
    void NudgeAbove(RectTransform removed)
    {
        if (removed == null) return;

        // 복사본을 사용(루프 중 목록 변경 방지)
        var snapshot = new List<RectTransform>(settledRects);

        float removedTop = removed.anchoredPosition.y + removed.rect.height * 0.5f;

        foreach (var rt in snapshot)
        {
            if (rt == null) continue;

            // 위에 있고 가로로 겹치면 대상
            bool isAbove = rt.anchoredPosition.y - removedTop > -0.5f; // 약간의 오차 허용
            bool overlapX = Mathf.Abs(rt.anchoredPosition.x - removed.anchoredPosition.x)
                            < (rt.rect.width + removed.rect.width) * 0.5f;

            if (isAbove && overlapX)
            {
                var ft = rt.GetComponent<FallingText>();
                if (ft != null) ft.ForceUnsettle();
            }
        }
    }
    
}
