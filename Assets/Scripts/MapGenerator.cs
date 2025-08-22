using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour, IListener
{
    [SerializeField] private Vector2 currentTilePos = new Vector2(0, 0);
    private Transform player;
    private float threshold = 5f;

    [Header("Tilemap")]
    [SerializeField] private GameObject tilemapParent;
    [SerializeField] private List<GameObject> tilemap = new List<GameObject>();

    [Header("Grid Settings")]
    [SerializeField] private float cell = 11f; // 타일 간격 (변동가능)
    private float lineJump; // 한 라인을 반대편으로 보낼 때 이동량 = cell * 3 (변동가능)

    private void Start()
    {
        lineJump = cell * 3f;

        EventManager.Instance.AddListener(EVENT_TYPE.EUseMove, this);

        tilemap.Clear();
        foreach (Transform child in tilemapParent.transform)
        {
            tilemap.Add(child.gameObject);
        }

        Debug.Log($"tilemap에 {tilemap.Count}개의 자식 오브젝트가 등록되었습니다.");
    }

    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.EUseMove:
                if (player == null)
                {
                    var p = GameObject.FindGameObjectWithTag("Player");
                    if (p != null) player = p.transform;
                    else return;
                }

                float x = player.position.x;
                float y = player.position.y;

                // 오른쪽으로
                if (x - currentTilePos.x >= threshold)
                {
                    var leftCol = GetLeftColumn();
                    moveThreeMap(leftCol, isVertical: false, direction: +1);
                    currentTilePos += new Vector2(cell, 0);
                }
                // 왼쪽으로
                else if (x - currentTilePos.x <= -threshold)
                {
                    var rightCol = GetRightColumn();
                    moveThreeMap(rightCol, isVertical: false, direction: -1);
                    currentTilePos += new Vector2(-cell, 0);
                }
                // 위로
                else if (y - currentTilePos.y >= threshold)
                {
                    var bottomRow = GetBottomRow();
                    moveThreeMap(bottomRow, isVertical: true, direction: +1);
                    currentTilePos += new Vector2(0, cell);
                }
                // 아래로
                else if (y - currentTilePos.y <= -threshold)
                {
                    var topRow = GetTopRow();
                    moveThreeMap(topRow, isVertical: true, direction: -1);
                    currentTilePos += new Vector2(0, -cell);
                }
                break;
        }
    }

    /// <summary>
    /// 전달된 3개 타일을 X/Y 방향으로 한 라인만큼 이동
    /// </summary>
    private void moveThreeMap(int[] li, bool isVertical, int direction)
    {
        float delta = lineJump * direction;

        foreach (int i in li)
        {
            Vector3 pos = tilemap[i].transform.position;
            if (isVertical) pos.y += delta;
            else            pos.x += delta;
            tilemap[i].transform.position = pos;
        }
    }
    
    private int[] GetTopRow()
    {
        // 위쪽 3개
        return tilemap
            .Select((go, idx) => new { idx, y = go.transform.position.y })
            .OrderByDescending(t => t.y)
            .Take(3)
            .Select(t => t.idx)
            .ToArray();
    }

    private int[] GetBottomRow()
    {
        // 아래쪽 3개
        return tilemap
            .Select((go, idx) => new { idx, y = go.transform.position.y })
            .OrderBy(t => t.y)
            .Take(3)
            .Select(t => t.idx)
            .ToArray();
    }

    private int[] GetLeftColumn()
    {
        // 왼쪽 3개
        return tilemap
            .Select((go, idx) => new { idx, x = go.transform.position.x })
            .OrderBy(t => t.x)
            .Take(3)
            .Select(t => t.idx)
            .ToArray();
    }

    private int[] GetRightColumn()
    {
        // 오른쪽 3개
        return tilemap
            .Select((go, idx) => new { idx, x = go.transform.position.x })
            .OrderByDescending(t => t.x)
            .Take(3)
            .Select(t => t.idx)
            .ToArray();
    }
}
