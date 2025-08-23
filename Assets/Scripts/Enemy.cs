using UnityEngine;
using System.Collections;
using Unity.Mathematics.Geometry;

public class Enemy : MonoBehaviour, IListener
{
    private int _health;
    
    private Vector2 lastPos = Vector2.zero;
    
    void Start()
    {
        Player player = GameManager.Instance.player;
        lastPos = player.transform.position;
        
        EventManager.Instance.AddListener(EVENT_TYPE.EUseMove, this);
        EventManager.Instance.AddListener(EVENT_TYPE.EUseSkill, this);
    }

    public void OnDeath()
    {
        EventManager.Instance.RemoveListener(EVENT_TYPE.EUseMove, this);
        EventManager.Instance.RemoveListener(EVENT_TYPE.EUseSkill, this);
        Destroy(gameObject);
    }

    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.EUseSkill:
                break;
        }
    }

    public void MoveToPos(Vector2 pos)
    {
        MovingAnimation(pos - (Vector2)transform.position);
        Debug.Log($"{transform.name} moving to {pos}");
    }
    
    void MovingAnimation(Vector2 dir)
    {
        StartCoroutine(MoveCoroutine(dir, 2f));
    }

    IEnumerator MoveCoroutine(Vector2 dir, float duration)
    {
        Vector2 dest = (Vector2)transform.position + dir;
        float time = 0f;
        while (time < 1.0f)
        {
            time += Time.deltaTime / duration;
            transform.position += (Vector3)dir * Time.deltaTime / duration;
            yield return null;
        }
        transform.position = dest;
    }

    public void Damaged(int amount)
    {
        _health -= amount;
        if (_health <= 0) OnDeath();
        
    }
}