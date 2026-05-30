namespace RubbishPot.Core
{
    [System.Serializable]
    public class LoadedCharacterData
    {
        public string CharacterID;
        public string SelectedVariant;

        public string FormId() => $"{CharacterID}_{SelectedVariant}";
    }
}