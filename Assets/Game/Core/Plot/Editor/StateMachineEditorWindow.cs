using System.Collections.Generic;

namespace RubbishPot.Core
{
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System.IO;

public class StateMachineEditorWindow : EditorWindow
{
    private StateMachineGraphView _graphView;
    private string _fileName = "NewStateMachine.json";

    [MenuItem("Tools/State Machine Editor")]
    public static void Open() => GetWindow<StateMachineEditorWindow>("State Machine");

    private void OnEnable()
    {
        // 1. Создаем граф
        _graphView = new StateMachineGraphView(this) 
        {
            style = { flexGrow = 1 }
        };
    
        // 2. Сначала ДОБАВЛЯЕМ его на экран
        rootVisualElement.Add(_graphView);
    
        // 3. И только ТЕПЕРЬ генерируем дефолтные ноды
        _graphView.GenerateDefaultNodes();
    
        GenerateToolbar();
    }

    private void GenerateToolbar()
    {
        var toolbar = new UnityEditor.UIElements.Toolbar();
        
        var fileNameTextField = new TextField("File Name:");
        fileNameTextField.value = _fileName;
        fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
        toolbar.Add(fileNameTextField);

        toolbar.Add(new Button(() => SaveData()) { text = "Save Data" });
        toolbar.Add(new Button(() => LoadData()) { text = "Load Data" });
        
        rootVisualElement.Add(toolbar);
    }

    private void SaveData()
    {
        var saveData = new GraphSaveData();
        var uiNodes = _graphView.nodes.ToList().OfType<BaseGraphViewNode>();
    
        foreach (var uiNode in uiNodes)
        {
            uiNode.UpdatePosition(); 
        
            // ФИКС: забираем ноду через правильный метод, а не несуществующее поле
            saveData.Nodes.Add(uiNode.GetRuntimeNode());
        }

        var edges = _graphView.edges.ToList().OfType<Edge>();
        foreach (var edge in edges)
        {
            if (edge.output?.node is BaseGraphViewNode outNode && edge.input?.node is BaseGraphViewNode inNode)
            {
                // Находим порядковый номер порта в контейнере выходов ноды
                int portIndex = outNode.outputContainer.IndexOf(edge.output);

                saveData.Edges.Add(new EdgeSaveData
                {
                    OutputNodeID = outNode.GetRuntimeNode().ID,
                    InputNodeID = inNode.GetRuntimeNode().ID,
                    OutputPortIndex = portIndex // ФИКС: Передаем индекс в сохранение
                });
            }
        }

        string path = Path.Combine(Application.dataPath, _fileName);
        string json = UnityEditor.EditorJsonUtility.ToJson(saveData, true);
        File.WriteAllText(path, json);
        UnityEditor.AssetDatabase.Refresh();
        Debug.Log($"Saved to {path}");
    }

private void LoadData()
{
    string path = Path.Combine(Application.dataPath, _fileName);
    if (!File.Exists(path))
    {
        Debug.LogError($"File not found at {path}");
        return;
    }

    // 1. Полностью очищаем холст от старых элементов перед загрузкой,
    // чтобы дефолтные ноды из конструктора не дублировались
    var currentUiNodes = _graphView.nodes.ToList().OfType<BaseGraphViewNode>().ToList();
    foreach (var node in currentUiNodes)
    {
        _graphView.RemoveElement(node);
    }

    var currentEdges = _graphView.edges.ToList().OfType<Edge>().ToList();
    foreach (var edge in currentEdges)
    {
        _graphView.RemoveElement(edge);
    }

    // 2. Читаем полиморфный JSON
    string json = File.ReadAllText(path);
    var saveData = new GraphSaveData();
    
    // Передаем объект по ссылке, так как EditorJsonUtility работает через Overwrite
    EditorJsonUtility.FromJsonOverwrite(json, saveData);

    // 3. Восстанавливаем UI-ноды на основе их Рантайм-типов
    var spawnedNodesMap = new System.Collections.Generic.Dictionary<string, BaseGraphViewNode>();

    foreach (var runtimeNode in saveData.Nodes)
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

    // 4. Восстанавливаем связи (Edges) между портами
    foreach (var edgeData in saveData.Edges)
    {
        if (spawnedNodesMap.TryGetValue(edgeData.OutputNodeID, out var outUiNode) &&
            spawnedNodesMap.TryGetValue(edgeData.InputNodeID, out var inUiNode))
        {
            // Берем порты. Для обычных нод берем первый попавшийся, 
            // для ChoiceNode ищем по имени порта (edgeData.PortName)
            Port outputPort = string.IsNullOrEmpty(edgeData.PortName) 
                ? outUiNode.outputContainer.Query<Port>().First()
                : outUiNode.outputContainer.Query<Port>().Where(p => p.portName == edgeData.PortName).First();

            Port inputPort = inUiNode.inputContainer.Query<Port>().First();

            if (outputPort != null && inputPort != null)
            {
                var newEdge = outputPort.ConnectTo(inputPort);
                _graphView.AddElement(newEdge);
            }
        }
    }

    Debug.Log($"Successfully loaded {saveData.Nodes.Count} nodes from {path}");
}
}

public class StateMachineGraphView : GraphView
{
    public EditorWindow Window { get; private set; }

    // Принимаем окно в конструкторе
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
        // Сценарий 1: Геймдизайнер СОЗДАЛ новую связь (прицепил ребро)
        if (graphViewChange.edgesToCreate != null)
        {
            foreach (var edge in graphViewChange.edgesToCreate)
            {
                // Нам нужно пнуть ноду, ИЗ которой выходит эта новая связь
                if (edge.output?.node is ParallelUiNode parallelNode)
                {
                    // Вызываем очистку/пересчет портов чуть позже, когда Unity закроет транзакцию связи
                    schedule.Execute(() => parallelNode.CleanUpPorts()).ExecuteLater(10);
                }
                else if (edge.output?.node is GroupUiNode groupNode)
                {
                    schedule.Execute(() => groupNode.CleanUpPorts()).ExecuteLater(10);
                }
            }
        }

        // Сценарий 2: Геймдизайнер УДАЛИЛ связь (нажал Delete или скинул стрелку)
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
        // ФИКС: проверяем через метод GetRuntimeNode()
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
