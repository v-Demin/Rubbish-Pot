using UnityEngine;

public class PotItem : MonoBehaviour
{
    [SerializeField] private ReactionComponent _reactionComponent;
    [SerializeField] private float _initialVolume;
    private Vector3 _initialScale;
    private float _currentVolume;

    private bool _initialized = false;
    
    public IReactionPart ReactionPart => _reactionComponent;
    
    public float Volume
    {
        get => _currentVolume;
        set
        {
            transform.localScale = new Vector3(_initialScale.x * value, _initialScale.y * value, 1f);
            _currentVolume = value;
        }
    }

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        if (_initialized) return;

        _initialized = true;
        
        _initialScale = transform.localScale * _initialVolume;
        _currentVolume = _initialVolume;
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
