using System;
using RubbishPot.Core.UI;
using UnityEngine;

namespace RubbishPot.Screen.Counter
{
    public class PhraseUIBubbleController : MonoBehaviour
    {
        [SerializeField] private DisplayableBubble _bubble;
        [SerializeField] private DisplayableText _text;
        [SerializeField] private DisplayableIcon _icon;
        
        private State _state = State.Hidden;
        
        public void DisplayText(string commandText, Action onBubbleShown, Action onComplete)
        {
            ShowBubble(() =>
            {
                onBubbleShown?.Invoke();
                ShowText(commandText, () => ShowIcon(onComplete));
            });
        }

        public void ForceEndDisplayingText(Action onComplete)
        {
            _text.ForceComplete();
            ShowIcon(onComplete);
        }
        
        private void ShowBubble(Action onComplete)
        {
            if (_state != State.Hidden)
            {
                onComplete?.Invoke();
                return;
            }
            
            SetState(State.Showing);
            _bubble.Show(() =>
            {
                SetState(State.Showed);
                onComplete?.Invoke();
            });
        }

        private void ShowText(string commandText, Action onComplete)
        {
            _text.Show(commandText, onComplete);
        }

        private void ShowIcon(Action onComplete)
        {
            _icon.Show(onComplete);
        }
        
        public void Hide(Action onComplete)
        {
            SetState(State.Hiding);
            _text.Hide(() =>
            {
                SetState(State.Hidden);
                _bubble.Hide(onComplete);
            });
        }

        private void SetState(State state)
        {
            _state = state;
        }
        
        private enum State
        {
            Hidden = 0,
            Showing = 1,
            Showed = 2,
            Hiding = 3,
        }
    }
}
