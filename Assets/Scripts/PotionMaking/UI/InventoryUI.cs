using DG.Tweening;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private RectTransform _moveRoot;
    [SerializeField] private RectTransform _showWp;
    [SerializeField] private RectTransform _hideWp;

    [SerializeField] private bool _baseStateOnStart;
    private bool _currentShowState;

    private void Start()
    {
        if (_baseStateOnStart)
        {
            ShowImmediate();
        }
        else
        {
            HideImmediate();
        }

        _currentShowState = _baseStateOnStart;
    }

    public void Show()
    {
        _moveRoot.DOMove(_showWp.position, 0.75f)
            .SetEase(Ease.InOutQuart);

        _currentShowState = true;
    }

    public void ShowImmediate()
    {
        _moveRoot.position = _showWp.position;
        
        _currentShowState = true;
    }

    public void Hide()
    {
        _moveRoot.DOMove(_hideWp.position, 0.75f)
            .SetEase(Ease.InOutQuart);
        
        _currentShowState = false;
    }

    public void HideImmediate()
    {
        _moveRoot.position = _hideWp.position;
        
        _currentShowState = false;
    }

    public void Switch()
    {
        if (_currentShowState)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }
    
    public void SwitchImmediate()
    {
        if (_currentShowState)
        {
            HideImmediate();
        }
        else
        {
            ShowImmediate();
        }
    }
}
