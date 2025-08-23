using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour, IListener
{
    [SerializeField]
    private Dictionary<Enemy, Vector2> EnemyPos
        = new Dictionary<Enemy, Vector2>();
    
    private Vector2 playerLastPos = Vector2.zero;
    
    void Start()
    {
        Player player = GameManager.Instance.player;
        playerLastPos = player.transform.position;
        
        EventManager.Instance.AddListener(EVENT_TYPE.EUseMove, this);
        EventManager.Instance.AddListener(EVENT_TYPE.EUseSkill, this);

        foreach (Enemy enemy in GetComponentsInChildren<Enemy>())
        {
            EnemyPos.Add(enemy, enemy.transform.position);
        }
    }
    
    private bool CheckPosHasMob(Vector2 pos)
    {
        foreach (Vector2 p in EnemyPos.Values)
        {
            if (p == pos) return true;
        }

        return false;
    }
    
    private bool CheckPosHasMob(Vector2 pos, Dictionary<Enemy, Vector2> dic)
    {
        foreach (Vector2 p in dic.Values)
        {
            if (p == pos) return true;
        }

        return false;
    }
    
    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.EUseMove:
                EnemyMoves((Vector2) param);
                break;
            case EVENT_TYPE.EUseSkill:
                //Check Damage
                EnemyMoves((param != null) ? (Vector2) param : playerLastPos);
                break;
        }
    }

    private void EnemyMoves(Vector2 playerPos)
    {
        List<Vector2> poses = new List<Vector2>();
        Dictionary<Enemy, Vector2> newEnemyPos = new Dictionary<Enemy, Vector2>();
        foreach (KeyValuePair<Enemy, Vector2> pair in EnemyPos)
        {
            poses = EnemyMovableDirection(playerPos - pair.Value);
            bool flag = true;
            foreach (Vector2 p in poses)
            {
                if (!CheckPosHasMob(p + pair.Value) && !CheckPosHasMob(p + pair.Value, newEnemyPos) && p + pair.Value != playerPos)
                {
                    pair.Key.MoveToPos(p + pair.Value);
                    newEnemyPos.Add(pair.Key, p + pair.Value);
                    flag = false;
                    break;
                }
            }

            if (flag)
            {
                newEnemyPos.Add(pair.Key, pair.Value);
            }
        }

        EnemyPos = newEnemyPos;
        Debug.Log(EnemyPos);
    }

    private List<Vector2> EnemyMovableDirection(Vector2 direction)
    {
        if (direction.x == 0 || direction.y == 0)
        {
            var list = new List<Vector2>();
            list.Add(direction.normalized);
            return list;
        }
        else
        {
            var list = new List<Vector2>();
            list.Add(direction.x > 0 ? Vector2.right : Vector2.left);
            list.Add(direction.y > 0 ? Vector2.up : Vector2.down);
            if (Random.Range(0, 2) == 0)
            {
                list.Reverse();
            }
            return list;
        }
    }
}
