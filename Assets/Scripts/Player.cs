using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Transform transform;

    private void Awake()
    {
        transform = this.GetComponent<Transform>();
    }
    
    private void Update()
    {
        
    }

    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        Debug.Log(input.ToString());
        transform.position += (Vector3)input;
        
    }
}
