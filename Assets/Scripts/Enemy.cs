using UnityEngine;

public class Enemy : MonoBehaviour, IListener
{
    void Awake()
    {
        
    }

    void Start()
    {
        EventManager.Instance.AddListener(EVENT_TYPE.EUseMove, this);
    }

    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.EUseMove:
            case EVENT_TYPE.EUseSkill:
                Debug.Log($"{sender} moved {param}");
                break;
        }
    }
}
