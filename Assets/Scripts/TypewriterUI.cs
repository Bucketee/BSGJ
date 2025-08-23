using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TypewriterUI : MonoBehaviour
{
    [Header("Target")]
    public TMP_Text textLabel;

    [Header("Typing Settings")]
    public float charDelay = 0.03f;     // 글자 간 지연

    [Header("Submit Settings")]
    public bool submitOnEnter = true;

    private readonly Queue<string> _queue = new Queue<string>();
    private Coroutine _typing;
    private bool _isTyping;

    private string _currentTyping = null;
    private int _currentIndex = 0;

    void Update()
    {
        if (!submitOnEnter) return;

        // ✅ 새 Input System 전역 폴링
        var kb = Keyboard.current;
        if (kb == null) return; // (에디터/PC가 아니라면 null일 수 있음)

        if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
        {
            SubmitAndClear(); // 현재까지 클릭·타이핑된 모든 내용 즉시 로그 + 비우기
        }
    }

    public void ClearAll()
    {
        if (textLabel != null) textLabel.text = "";
    }

    public void TypeAppend(string s)
    {
        if (string.IsNullOrEmpty(s)) return;
        _queue.Enqueue(s);
        if (_typing == null) _typing = StartCoroutine(TypingLoop());
    }

    IEnumerator TypingLoop()
    {
        _isTyping = true;
        while (_queue.Count > 0)
        {
            _currentTyping = _queue.Dequeue();
            _currentIndex = 0;

            for (; _currentIndex < _currentTyping.Length; _currentIndex++)
            {
                AppendChar(_currentTyping[_currentIndex]);
                yield return new WaitForSeconds(charDelay);
            }

            _currentTyping = null; // 한 문자열 완료
        }
        _isTyping = false;
        _typing = null;
    }

    void AppendChar(char c)
    {
        if (textLabel != null) textLabel.text += c;
    }
    

    public void SubmitAndClear()
    {
        if (textLabel == null) return;

        // 1) 타이핑 중단
        if (_typing != null) { StopCoroutine(_typing); _typing = null; }
        _isTyping = false;

        // 2) 현재 타이핑 중이던 남은 글자 붙이기
        if (!string.IsNullOrEmpty(_currentTyping) && _currentIndex < _currentTyping.Length)
        {
            textLabel.text += _currentTyping.Substring(_currentIndex);
            _currentTyping = null;
            _currentIndex = 0;
        }

        // 3) 큐에 남은 문자열 모두 붙이기
        while (_queue.Count > 0)
            textLabel.text += _queue.Dequeue();

        // 4) 로그 출력
        string all = textLabel.text;
        if (!string.IsNullOrEmpty(all))
            Debug.Log($"[TypewriterUI Submit] {all}");

        // 5) 비우기
        ClearAll();
    }
}
