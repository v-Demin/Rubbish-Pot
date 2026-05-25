using System;
using System.Collections.Generic;
using UnityEngine;

namespace RubbishPot.Core
{
    [Serializable]
    public class NodeSaveData
    {
        public string ID;
        public string NodeType;
        public Vector2 Position;
        // Общие данные
        public string TextData; 
        public string Key;
        // Для ноды выбора
        public List<string> Choices = new();
    }
}
