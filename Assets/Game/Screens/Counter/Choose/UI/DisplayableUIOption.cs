using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

namespace RubbishPot.Screen.Counter
{
    public class DisplayableUIOption : MonoBehaviour
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Button _button;
        
        public void Show(ChooseUIController.ChooseOption option, Action onComplete)
        {
            _text.text = option.Text;
            
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(option.Select);
            
            _root.localScale = new Vector3(0f, 1f, 1f);
            _root.gameObject.SetActive(true);
            _root.DOScaleX(1f, Random.Range(0.5f, 0.6f))
                .SetEase(Ease.OutQuad)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void HideAsUnselected(Action onComplete)
        {
            _root.DOScaleX(0f, 0.2f)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    _root.gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
        }

        public void HideAsSelected(Action onComplete)
        {
            _root.DOScaleX(0f, 0.2f)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    _root.gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
        }

        public void SetInputState(bool state)
        {
            _image.raycastTarget = state;
        }
    }
}
