using System.Collections.Generic;

namespace RubbishPot.Core
{
    [System.Serializable]
    public class CharacterModel
    {
        public string CharacterID;
        public string DisplayName;
        public List<string> Variants = new() { "Healthy", "Wounded" };
    }
}