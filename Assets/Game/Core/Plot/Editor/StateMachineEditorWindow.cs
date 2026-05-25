using System.Collections.Generic;

namespace RubbishPot.Core
{
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;

public class StateMachineEditorWindow : EditorWindow
{
    private StateMachineGraphView _graphView;
    private PlotAsset _currentAsset;
    private Label _assetLabel;

    [MenuItem("Tools/State Machine Editor")]
    public static void Open() => GetWindow<StateMachineEditorWindow>("State Machine");

    private void OnEnable()
    {
        _graphView = new StateMachineGraphView(this)
        {
            style = { flexGrow = 1 }
        };

        rootVisualElement.Add(_graphView);
        _graphView.GenerateDefaultNodes();
        GenerateToolbar();
    }

    private void GenerateToolbar()
    {
        var toolbar = new UnityEditor.UIElements.Toolbar();

        _assetLabel = new Label(_currentAsset != null
            ? $"Active Asset: {_currentAsset.name}"
            : "No Asset Loaded (Double-click any PlotAsset)");
        _assetLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        _assetLabel.style.paddingLeft = 5;
        _assetLabel.style.paddingRight = 10;
        toolbar.Add(_assetLabel);

        toolbar.Add(new Button(() => SaveData()) { text = "Save Asset" });

        rootVisualElement.Add(toolbar);
    }

    private void SaveData()
    {
        if (_currentAsset == null)
        {
            Debug.LogError("Невозможно сохранить: ни один PlotAsset не открыт в редакторе!");
            return;
        }

        _currentAsset.Data.Nodes.Clear();
        _currentAsset.Data.Edges.Clear();

        var uiNodes = _graphView.nodes.ToList().OfType<BaseGraphViewNode>();
        foreach (var uiNode in uiNodes)
        {
            uiNode.UpdatePosition();
            _currentAsset.Data.Nodes.Add(uiNode.GetRuntimeNode());
        }

        var edges = _graphView.edges.ToList().OfType<Edge>();
        foreach (var edge in edges)
        {
            if (edge.output?.node is BaseGraphViewNode outNode && edge.input?.node is BaseGraphViewNode inNode)
            {
                // ФИКС: Ищем индекс только среди ПОРТОВ, игнорируя вложенность в строки/контейнеры
                var allOutputPorts = outNode.outputContainer.Query<Port>().ToList();
                int portIndex = allOutputPorts.IndexOf(edge.output);

                _currentAsset.Data.Edges.Add(new EdgeSaveData
                {
                    OutputNodeID = outNode.GetRuntimeNode().ID,
                    InputNodeID = inNode.GetRuntimeNode().ID,
                    OutputPortIndex = portIndex,
                    PortName = edge.output.portName
                });
            }
        }

        EditorUtility.SetDirty(_currentAsset);
        AssetDatabase.SaveAssets();

        Debug.Log($"<color=green>[SUCCESS]</color> Изменения успешно сохранены в ассет: {_currentAsset.name}");
    }

    public void LoadAsset(PlotAsset plotAsset)
    {
        if (plotAsset == null || plotAsset.Data == null) return;

        _currentAsset = plotAsset;

        if (_assetLabel != null)
        {
            _assetLabel.text = $"Active Asset: {_currentAsset.name}";
        }

        ClearAndPopulateGraph(_currentAsset.Data.Nodes, _currentAsset.Data.Edges);
        Debug.Log($"Successfully loaded asset via AssetOpener: {plotAsset.name}");
    }

    private void ClearAndPopulateGraph(List<RuntimeNode> nodes, List<EdgeSaveData> edges)
    {
        // 1. Полная очистка холста
        var currentUiNodes = _graphView.nodes.ToList().OfType<BaseGraphViewNode>().ToList();
        foreach (var node in currentUiNodes) _graphView.RemoveElement(node);

        var currentEdges = _graphView.edges.ToList().OfType<Edge>().ToList();
        foreach (var edge in currentEdges) _graphView.RemoveElement(edge);

        // Авто-генерация обязательных нод для новых ассетов
        if (!nodes.Any(n => n is RuntimeEntryNode))
            nodes.Add(new RuntimeEntryNode { Position = new Vector2(100, 200) });
        if (!nodes.Any(n => n is RuntimeExitNode)) nodes.Add(new RuntimeExitNode { Position = new Vector2(600, 200) });

        // 2. Восстановление UI-нод
        var spawnedNodesMap = new Dictionary<string, BaseGraphViewNode>();
        foreach (var runtimeNode in nodes)
        {
            BaseGraphViewNode uiNode = runtimeNode switch
            {
                RuntimeEntryNode n => new EntryUiNode(n),
                RuntimeExitNode n => new ExitUiNode(n),
                RuntimePhraseNode n => new PhraseUiNode(n),
                RuntimeAnimationNode n => new AnimationUiNode(n),
                RuntimeGroupNode n => new GroupUiNode(n),
                RuntimeParallelNode n => new ParallelUiNode(n),
                RuntimeChoiceNode n => new ChoiceUiNode(n),
                RuntimeMakeOrderNode n => new MakeOrderUiNode(n),
                RuntimeStorageNode n => new StorageUiNode(n),
                RuntimeHandOverItemNode n => new HandOverItemUiNode(n),
                RuntimeCancelOrderNode n => new CancelOrderUiNode(n),
                RuntimeAddKnowledgeNode n => new AddKnowledgeUiNode(n),
                _ => null
            };

            if (uiNode != null)
            {
                _graphView.AddElement(uiNode);
                spawnedNodesMap.Add(uiNode.NodeID, uiNode);
            }
        }

        // 3. Восстановление связей между портами
        foreach (var edgeData in edges)
        {
            if (spawnedNodesMap.TryGetValue(edgeData.OutputNodeID, out var outUiNode) &&
                spawnedNodesMap.TryGetValue(edgeData.InputNodeID, out var inUiNode))
            {
                // ФИКС: Собираем именно список портов вывода
                var allOutputPorts = outUiNode.outputContainer.Query<Port>().ToList();
                Port outputPort = null;

                // 1. Пробуем взять строго по сохраненному индексу порта
                if (edgeData.OutputPortIndex >= 0 && edgeData.OutputPortIndex < allOutputPorts.Count)
                {
                    outputPort = allOutputPorts[edgeData.OutputPortIndex];
                }

                // 2. Фаллбэк: если индекс сломался, ищем по совпадению имени
                if (outputPort == null && !string.IsNullOrEmpty(edgeData.PortName))
                {
                    outputPort = allOutputPorts.FirstOrDefault(p => p.portName == edgeData.PortName);
                }

                // 3. Последний рубеж: берем самый первый порт
                if (outputPort == null)
                {
                    outputPort = allOutputPorts.FirstOrDefault();
                }

                Port inputPort = inUiNode.inputContainer.Query<Port>().First();

                if (outputPort != null && inputPort != null)
                {
                    var newEdge = outputPort.ConnectTo(inputPort);
                    _graphView.AddElement(newEdge);
                }
            }
        }
    }
}

public class StateMachineGraphView : GraphView
{
    public EditorWindow Window { get; private set; }

    public StateMachineGraphView(EditorWindow window)
    {
        Window = window;

        Insert(0, new GridBackground());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        
        nodeCreationRequest = context => 
        {
            var provider = ScriptableObject.CreateInstance<StateMachineSearchWindowProvider>();
            provider.Init(this);
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), provider);
        };
        
        graphViewChanged = OnGraphViewChanged;
    }
    
    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if (graphViewChange.edgesToCreate != null)
        {
            foreach (var edge in graphViewChange.edgesToCreate)
            {
                if (edge.output?.node is ParallelUiNode parallelNode)
                {
                    schedule.Execute(() => parallelNode.CleanUpPorts()).ExecuteLater(10);
                }
                else if (edge.output?.node is GroupUiNode groupNode)
                {
                    schedule.Execute(() => groupNode.CleanUpPorts()).ExecuteLater(10);
                }
            }
        }

        if (graphViewChange.elementsToRemove != null)
        {
            var removedEdges = graphViewChange.elementsToRemove.OfType<Edge>().ToList();
        
            foreach (var edge in removedEdges)
            {
                if (edge.output?.node is ParallelUiNode parallelNode)
                {
                    schedule.Execute(() => parallelNode.CleanUpPorts()).ExecuteLater(50);
                }
                else if (edge.output?.node is GroupUiNode groupNode)
                {
                    schedule.Execute(() => groupNode.CleanUpPorts()).ExecuteLater(50);
                }
            }
        }

        return graphViewChange;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endPort => endPort.direction != startPort.direction && endPort.node != startPort.node).ToList();
    }
    
    public void GenerateDefaultNodes()
    {
        var existingNodes = nodes.ToList().OfType<BaseGraphViewNode>().ToList();
        if (existingNodes.Any(n => n.GetRuntimeNode() is RuntimeEntryNode)) return;

        var entryRuntime = new RuntimeEntryNode();
        var entryUI = new EntryUiNode(entryRuntime);
        entryUI.SetPosition(new Rect(100, 200, 100, 150));
        AddElement(entryUI);

        var exitRuntime = new RuntimeExitNode();
        var exitUI = new ExitUiNode(exitRuntime);
        exitUI.SetPosition(new Rect(600, 200, 100, 150));
        AddElement(exitUI);
    }
}
}