using System;
using UnityEngine;

namespace RubbishPot.Core.UI
{
    public class DisplayableBubble : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        
        public void Show(Action onComplete)
        {
            _root.SetActive(true);
            onComplete?.Invoke();
        }

        public void Hide(Action onComplete)
        {
            _root.SetActive(false);
            onComplete?.Invoke();
        }
    }
}
