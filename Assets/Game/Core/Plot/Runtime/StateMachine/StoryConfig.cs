using System.Collections.Generic;
using UnityEngine;

namespace RubbishPot.Core
{
    [CreateAssetMenu(fileName = "StoryConfig", menuName = "RubbishPot/Story Config")]
    public class StoryConfig : ScriptableObject
    {
        [Header("Все персонажи в игре")]
        public List<CharacterModel> Characters = new();

        [Header("Все варианты фонов (Основа общая)")]
        public List<string> BackgroundVariants = new() { "Day", "Night", "Destroyed" };
    }
}
