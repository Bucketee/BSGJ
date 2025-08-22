using UnityEngine;
using UnityEngine.XR;

public enum GAME_STATE
{
    SIsReady,
    SNotReady,
}

public class GameManager : MonoBehaviour, IListener
{
    public static GameManager Instance { get { return _instance; } }
    private static GameManager _instance = null;
    
    public GAME_STATE CurrentState { get { return _currentState; } }
    private GAME_STATE _currentState = GAME_STATE.SIsReady;
    
    public GAME_STATE LastState { get { return _lastState; } }
    private GAME_STATE _lastState = GAME_STATE.SIsReady;

    void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        DestroyImmediate(gameObject);
    }

    void Start()
    {
        EventManager.Instance.AddListener(EVENT_TYPE.EUseMove, this);
        EventManager.Instance.AddListener(EVENT_TYPE.EUseSkill, this);
        EventManager.Instance.AddListener(EVENT_TYPE.EAnimDone, this);
    }

    private void ChangeState(GAME_STATE nextState)
    {
        Debug.Log($"Change State: {nextState}");
        _lastState = _currentState;
        _currentState = nextState;
    }

    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.EUseMove:
            case EVENT_TYPE.EUseSkill:
                ChangeState(GAME_STATE.SNotReady);
                break;
            case EVENT_TYPE.EAnimDone:
                ChangeState(GAME_STATE.SIsReady);
                break;
        }
    }
}
