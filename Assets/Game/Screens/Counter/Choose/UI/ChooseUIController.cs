using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Submodules.Common.Utils;

namespace RubbishPot.Screen.Counter
{
    public class ChooseUIController : MonoBehaviour
    { 
        [SerializeField] private List<ChooseWing> _wings;

        private IEnumerable<ChooseOption> _options;
        
        public void Show(IEnumerable<ChooseOption> options, Action onComplete)
        {
            _options = options;
            
            new List<Action<Action>>()
            {
                a => ShowInner(_options, _wings.First(), a),
                a => ShowInner(_options, _wings.Last(), a)
            }.WaitCompletion(onComplete);
        }

        private void ShowInner(IEnumerable<ChooseOption> options, ChooseWing wing, Action onComplete)
        {
            var validOptions = options.Where(wing.Validate);
            wing.Show(validOptions, onComplete);
        }
        
        public void PlayHideAnimation(int index, Action onComplete)
        {
            HideAllExcept(index, () =>
                Hide(index, () =>
            {
                _options = null;
                onComplete?.Invoke();
            }));
        }
        
        private void HideAllExcept(int index, Action onComplete)
        {
            var selectedOptions = _options.Where(o => !o.Index.Equals(index));
            
            new List<Action<Action>>()
            {
                a => HideAllInner(selectedOptions, _wings.First(), a),
                a => HideAllInner(selectedOptions, _wings.Last(), a)
            }.WaitCompletion(onComplete);
        }

        private void HideAllInner(IEnumerable<ChooseOption> options, ChooseWing wing, Action onComplete)
        {
            var validOptions = options.Where(wing.Validate);
            
            if (!validOptions.Any())
            {
                onComplete?.Invoke();
                return;
            }
            
            foreach (var option in validOptions)
            {
                wing.Hide(option.Index, onComplete);
            }
        }
        
        private void Hide(int index, Action onComplete)
        {
            _wings.Where(w => w.Validate(index)).WaitCompletion((w, done) => w.Hide(index, done), onComplete);
        }
        
        public void EnableInput()
        {
            _wings.ForEach(w => w.SetInputState(true));
        }

        public void DisableInput()
        {
            _wings.ForEach(w => w.SetInputState(false));
        }
        
        public class ChooseOption
        {
            public int Index { get; }
            public string Text { get; }
            private Action _selectedAction;
            
            public ChooseOption(int index, string text, Action onSelected)
            {
                Index = index;
                Text = text;
                _selectedAction = onSelected;
            }

            public void Select()
            {
                _selectedAction?.Invoke();
            }
        }
    }
}
