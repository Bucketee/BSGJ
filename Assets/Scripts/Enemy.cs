using UnityEngine;

public class Enemy : MonoBehaviour, IListener
{
    void Start()
    {
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
            case EVENT_TYPE.EUseMove:
            case EVENT_TYPE.EUseSkill:
                //Debug.Log($"{sender} moved {param}");
                //do sth...
                break;
        }
    }
}
