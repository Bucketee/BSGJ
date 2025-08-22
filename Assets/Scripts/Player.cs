using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;

public class Player : MonoBehaviour
{
    private GameManager _gameManager;
    private Transform transform;

    private void Awake()
    {
        transform = this.GetComponent<Transform>();
    }

    private void Start()
    {
        _gameManager = GameManager.Instance.GetComponent<GameManager>();
    }
    
    private void Update()
    {
        
    }

    void OnMove(InputValue value)
    {
        if (_gameManager.CurrentState != GAME_STATE.SIsReady)
        {
            Debug.LogError(("Can't move!"));
            return;
        }
        Vector2 input = value.Get<Vector2>();
        if (input == Vector2.zero) return;
        //Debug.Log(input.ToString());
        //transform.position += (Vector3)input;
        EventManager.Instance.PostNotification(EVENT_TYPE.EUseMove, this, input.ToString());
        MovingAnimation(input);
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
        OnMoveEnd();
    }

    void OnMoveEnd()
    {
        EventManager.Instance.PostNotification(EVENT_TYPE.EAnimDone, this);
    }
}
