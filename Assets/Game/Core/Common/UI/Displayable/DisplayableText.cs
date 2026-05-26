using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace RubbishPot.Core.UI
{
    public class DisplayableText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _textMesh;

        private Tween _textingTween;
        
        public void Show(string text, Action onComplete)
        {
            _textMesh.text = text;
            _textMesh.maxVisibleCharacters = 0;
            
            //[Todo]: замена анимации появления текста на чёт покрасивше обычной печати
            _textingTween = DOVirtual.Int(0, _textMesh.text.Length, 20f, v => _textMesh.maxVisibleCharacters = v)
                .SetSpeedBased()
                .SetEase(Ease.Linear)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void ForceComplete()
        {
            _textingTween.Kill(true);
        }

        public void Hide(Action onComplete)
        {
            _textMesh.maxVisibleCharacters = 0;
            onComplete?.Invoke();
        }
    }
}
