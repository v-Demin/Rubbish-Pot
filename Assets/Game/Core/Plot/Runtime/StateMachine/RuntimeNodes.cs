using System;
using System.Collections.Generic;
using UnityEngine;

namespace RubbishPot.Core
{
    public interface ISubCommand 
    { 
        void Execute(); 
    }

    [Serializable]
    public abstract class RuntimeNode 
    {
        public string ID = Guid.NewGuid().ToString();
        public Vector2 Position;
        
        [SerializeReference] 
        public List<ISubCommand> SubCommands = new();
    }
    
    [Serializable]
    public class GiveItemCommand : ISubCommand 
    {
        public string ItemID;
        public int Count;
        public void Execute() => Debug.Log($"Игроку выдано: {ItemID} x{Count}");
    }

    [Serializable]
    public class ChangeReputationCommand : ISubCommand 
    {
        public string FactionID;
        public int Value;
        public void Execute() => Debug.Log($"Репутация {FactionID} изменена на {Value}");
    }

    [Serializable] public class RuntimeEntryNode : RuntimeNode { }
    [Serializable] public class RuntimeExitNode : RuntimeNode { }
    [Serializable] public class RuntimePhraseNode : RuntimeNode { public string Text; }
    [Serializable] public class RuntimeChoiceNode : RuntimeNode { public List<string> Choices = new(); }
    [Serializable] public class RuntimeMakeOrderNode : RuntimeNode { public string OrderID; }
    [Serializable] public class RuntimeCancelOrderNode : RuntimeNode { }
}
