using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using DG.Tweening;

public class RadialDiagram : MonoBehaviour
{
    [SerializeField] private Image _prefab;
    [SerializeField] private Transform _parent;

    private Dictionary<object, Slice> _sliceDictionary = new ();

    public void RefreshDiagram()
    {
        if (_sliceDictionary.Count == 0) return;

        foreach (var slice in _sliceDictionary)
        {
            slice.Value.Refresh();
        }
    }

    public void AddSlice(object key, Color color, Func<float> valueGetter)
    {
        if (_sliceDictionary.ContainsKey(key)) return;
        
        _sliceDictionary.Add(key, new Slice(_prefab, _parent, color, valueGetter));
        RefreshDiagram();
    }

    public bool IsContains(object key)
    {
        return _sliceDictionary.ContainsKey(key);
    }

    public void RemoveSlice(object key)
    {
        if (!_sliceDictionary.ContainsKey(key)) return;
        
        _sliceDictionary[key].DestroySlice();
        _sliceDictionary.Remove(key);
        RefreshDiagram();
    }
    
    private class Slice
    {
        private Image _circleImage;
        public Func<float> ValueGetter;

        public Slice(Image prefab, Transform parent, Color color, Func<float> valueGetter)
        {
            _circleImage = Instantiate(prefab, parent);
            _circleImage.color = color;
            _circleImage.gameObject.SetActive(true);
            _circleImage.transform.SetSiblingIndex(0);
            
            ValueGetter = valueGetter;
        }

        public void Refresh()
        {
            DOVirtual.Float(_circleImage.fillAmount, ValueGetter.Invoke(), 0.3f,
                value => _circleImage.fillAmount = value)
                .SetEase(Ease.InOutQuad);
        }

        public void DestroySlice()
        {
            Destroy(_circleImage.gameObject);
        }
    }
}
