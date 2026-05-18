using UnityEngine;

namespace RubbishPot.Core
{
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraphRuntimeInterpreter : MonoBehaviour
{
    private Dictionary<string, RuntimeNode> _nodesMap = new();
    private List<EdgeSaveData> _edges = new();
    private HashSet<string> _activeNodes = new();

    public void StartGraph(string jsonContent)
    {
        var data = JsonUtility.FromJson<GraphSaveData>(jsonContent);
        _edges = data.Edges;
        _nodesMap = data.Nodes.ToDictionary(n => n.ID);

        var entryNode = data.Nodes.FirstOrDefault(n => n is RuntimeEntryNode);
        if (entryNode != null) ExecuteNode(entryNode.ID);
    }

    public void ExecuteNode(string nodeId)
    {
        if (!_nodesMap.TryGetValue(nodeId, out var node)) return;

        _activeNodes.Add(nodeId);
        node.Execute(this); 
    }

    public void Proceed(string currentNodeId)
    {
        if (!_activeNodes.Contains(currentNodeId)) return;
        _activeNodes.Remove(currentNodeId);

        var nextEdges = _edges.Where(e => e.OutputNodeID == currentNodeId);
        foreach (var edge in nextEdges)
        {
            ExecuteNode(edge.InputNodeID);
        }
    }

// ФИКС: Специальный метод перехода конкретно для ВЫБОРА ИГРОКА
    public void ProceedChoice(string currentNodeId, int chosenIndex)
    {
        if (!_activeNodes.Contains(currentNodeId)) return;
        _activeNodes.Remove(currentNodeId);

        // Ищем связь, у которой OutputNodeID совпадает с нашей нодой выбора,
        // И OutputPortIndex строго равен индексу нажатой кнопки!
        var chosenEdge = _edges.FirstOrDefault(e => e.OutputNodeID == currentNodeId && e.OutputPortIndex == chosenIndex);

        if (chosenEdge != null)
        {
            ExecuteNode(chosenEdge.InputNodeID);
        }
        else
        {
            Debug.LogError($"[Interpreter] Не найдена связь для ноды выбора {currentNodeId} с индексом порта {chosenIndex}!");
        }
    }

    // ФИКС ТУТ: Метод, который вызывается параллельными и групповыми нодами
    public void ExecuteAllOutputs(string currentNodeId)
    {
        // Находим ВСЕ связи, которые выходят из этой ноды
        var nextEdges = _edges.Where(e => e.OutputNodeID == currentNodeId).ToList();
        
        foreach (var edge in nextEdges)
        {
            // Запускаем каждую ветку независимо и одновременно
            ExecuteNode(edge.InputNodeID);
        }
    }
}
}
