using UnityEngine;

public class ThrowingObject : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidbody;
    
    public void Grab()
    {
        _rigidbody.gravityScale = 0f;
    }

    public void Drop()
    {
        _rigidbody.gravityScale = 1f;
    }
}
