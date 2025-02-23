using UnityEngine;

public class PotItem : MonoBehaviour
{
    [SerializeField] private ReactionComponent _reactionComponent;

    private void Start()
    {
        _reactionComponent.Init(this, Destroy);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.TryGetComponent<PotItem>(out var item))
        {
            _reactionComponent.Collide(item._reactionComponent);
            return;
        }
        
        if (other.gameObject.TryGetComponent<Spoon>(out var spoon))
        {
            _reactionComponent.Collide(spoon);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent<Water>(out var water))
        {
            _reactionComponent.DipIntoWater(water);
        }
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}
