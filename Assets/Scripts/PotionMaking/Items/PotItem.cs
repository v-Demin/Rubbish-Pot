using UnityEngine;

public class PotItem : MonoBehaviour
{
    [SerializeField] private ReactionComponent _reactionComponent;

    private void Start()
    {
        _reactionComponent.Init();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.TryGetComponent<PotItem>(out var item))
        {
            _reactionComponent.Collide(item._reactionComponent);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
    }
}
