using System;
using System.Collections.Generic;
using UnityEngine;

namespace RubbishPot.Core
{
    [Serializable]
    public class PlotData
    {
        // Верхнее окно: Глобальные ноды (Вход, Выход, Заказ, Торговец и т.д.)
        [SerializeReference]
        public List<RuntimeGlobalNode> GlobalNodes = new();
        
        // Связи между глобальными нодами
        public List<EdgeSaveData> GlobalEdges = new();
    }
}