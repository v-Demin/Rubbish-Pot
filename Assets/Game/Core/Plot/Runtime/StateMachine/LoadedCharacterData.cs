using UnityEngine;

namespace RubbishPot.Core
{
    [System.Serializable]
    public class LoadedCharacterData
    {
        [SerializeField] private string _characterID;
        [SerializeField] private string _selectedVariant;

        public string FormId() => $"{_characterID}_{_selectedVariant}";
    }
}