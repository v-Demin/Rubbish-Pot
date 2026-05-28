using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Submodules.Common.Utils;

namespace RubbishPot.Screen.Counter
{
    public class ChooseWing : MonoBehaviour
    {
        [SerializeField] private List<OptionInfo> _options;
        
        public bool Validate(ChooseUIController.ChooseOption chooseOption)
        {
            return Validate(chooseOption.Index);
        }
        
        public bool Validate(int index)
        {
            return _options.Any(o => o.Index.Equals(index));
        }
        
        public void Show(IEnumerable<ChooseUIController.ChooseOption> validOptions, Action onComplete)
        { 
            var filteredOptions = _options
                .Join(validOptions, info => info.Index, option => option.Index, (info, option) => (info, option));
            
            filteredOptions.WaitCompletion((f, done) => f.info.UI.Show(f.option, done), onComplete);
        }
        
        public void Hide(int index, Action onComplete)
        {
            _options.FirstOrDefault(o => o.Index.Equals(index)).UI.HideAsUnselected(onComplete);
        }
        
        public void SetInputState(bool state)
        {
            _options.ForEach(o => o.UI.SetInputState(state));
        }
        
        [System.Serializable]
        private class OptionInfo
        {
            public int Index;
            public DisplayableUIOption UI;
        }
    }
}
