using System;
using System.Collections.Generic;
using UnityEngine;

namespace RubbishPot.Core
{
    public class CharacterPool : MonoBehaviour
    {
        [SerializeField] private Transform _characterRoot;
        
        private Dictionary<string, (Character character, bool state)> _loadedCharacters = new Dictionary<string, (Character, bool)>();

        public void Load(Func<Transform, Character> loadCharacter)
        {
            var character = loadCharacter?.Invoke(_characterRoot);
            _loadedCharacters.Add(character.Id, (character, false));
        }

        public Character GetCharacter(string characterId)
        {
            return _loadedCharacters[characterId].character;
        }

        public void SetCharacterState(string id, bool state)
        {
            var info = _loadedCharacters[id];
            _loadedCharacters[id] = (info.character, state);
        }
    }
}
