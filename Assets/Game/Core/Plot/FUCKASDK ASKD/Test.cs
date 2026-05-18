namespace RubbishPot.Core
{
using UnityEngine;

public class Test : MonoBehaviour
{
    [Header("Настройки Графа")]
    [SerializeField] private TextAsset _graphJsonFile;
    [SerializeField] private GraphRuntimeInterpreter _interpreter;

    // Состояния для интерактивного теста с клавиатуры
    private bool _waitingForChoice = false;
    private bool _canCancelOrder = false;
    private ShowPlayerChoicesCommand _currentChoiceCommand;
    private string _currentCancelNodeId;

    private void Start()
    {
        if (_graphJsonFile == null || _interpreter == null)
        {
            Debug.LogError("Забыли накинуть JSON файл или ссылку на Интерпретатор в инспекторе!");
            return;
        }

        SubscribeToAllEvents();

        Debug.Log("<color=green>=== ЗАПУСК ТЕСТОВОГО ГРАФА ===</color>");
        Debug.Log("<color=gray>[ИНФО] Управление в тесте:\nЦифры 1, 2, 3, 4 — Выбор ответа\nКлавиша ESC — Отмена заказа</color>");
        
        _interpreter.StartGraph(_graphJsonFile.text);
    }

    private void Update()
    {
        if (_waitingForChoice && _currentChoiceCommand != null)
        {
            int selectedIndex = -1;
    
            if (Input.GetKeyDown(KeyCode.Alpha1)) selectedIndex = 0;
            else if (Input.GetKeyDown(KeyCode.Alpha2)) selectedIndex = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha3)) selectedIndex = 2;
            else if (Input.GetKeyDown(KeyCode.Alpha4)) selectedIndex = 3;

            if (selectedIndex >= 0 && selectedIndex < _currentChoiceCommand.Choices.Count)
            {
                _waitingForChoice = false;
                string chosenText = _currentChoiceCommand.Choices[selectedIndex];
                string choiceNodeId = _currentChoiceCommand.NodeId;
        
                Debug.Log($"<color=yellow>[КЛАВИАТУРА]</color> Выбран вариант [{selectedIndex + 1}]: \"{chosenText}\"");
        
                // ФИКС ТУТ: Пинаем интерпретатор по специальному методу для выбора!
                _interpreter.ProceedChoice(choiceNodeId, selectedIndex);
        
                _currentChoiceCommand = null;
            }
        }

        // 2. Обработка отмены заказа (Клавиша ESC)
        if (_canCancelOrder && Input.GetKeyDown(KeyCode.Escape))
        {
            _canCancelOrder = false;
            Debug.Log("<color=red>[КЛАВИАТУРА]</color> Нажат ESC! Активирован принудительный отказ от заказа.");
            
            // Толкаем граф по ветке отмены
            ContinueGraph(_currentCancelNodeId);
        }
    }

    private void SubscribeToAllEvents()
    {
        // Перехватываем фразы персонажа
        EventBus.Subscribe<PlayPhraseCommand>(cmd => {
            Debug.Log($"<color=cyan>[PLAY PHRASE]</color> Node ID: {cmd.NodeId} | Текст: \"{cmd.Text}\"");
            
            // Авто-прокрутка фраз через 2 секунды для удобства теста
            Invoke(() => ContinueGraph(cmd.NodeId), 2.0f);
        });

        // Перехватываем анимации
        EventBus.Subscribe<PlayAnimationCommand>(cmd => {
            Debug.Log($"<color=orange>[ANIMATION]</color> Node ID: {cmd.NodeId} | Запуск анимации: {cmd.AnimationName}");
        });

        // МЕНЯЕМ ТУТ: Перехватываем выбор игрока и включаем режим ожидания клавиатуры
        EventBus.Subscribe<ShowPlayerChoicesCommand>(cmd => {
            Debug.Log($"<color=yellow>[PLAYER CHOICES]</color> Node ID: {cmd.NodeId} | Ожидание выбора (Нажми 1-{cmd.Choices.Count} на клавиатуре):");
            for (int i = 0; i < cmd.Choices.Count; i++)
            {
                Debug.Log($" [{i + 1}] -> {cmd.Choices[i]}");
            }
            
            _currentChoiceCommand = cmd;
            _waitingForChoice = true;
        });

        // Перехватываем создание заказа
        EventBus.Subscribe<MakeOrderCommand>(cmd => {
            Debug.Log($"<color=magenta>[MAKE ORDER]</color> Node ID: {cmd.NodeId} | Геймплей запущен для заказа: {cmd.OrderId}");
        });

        // Перехватываем выставление данных в хранилище
        EventBus.Subscribe<SetStorageDataCommand<int>>(cmd => {
            Debug.Log($"<color=white>[STORAGE INJECT]</color> Node ID: {cmd.NodeId} | Ключ: {cmd.Key} | Значение: {cmd.Value}");
        });

        // МЕНЯЕМ ТУТ: Перехватываем триггер кнопки отказа от заказа
        EventBus.Subscribe<ToggleCancelOrderButtonCommand>(cmd => {
            _canCancelOrder = cmd.IsVisible;
            _currentCancelNodeId = cmd.NodeId;

            if (cmd.IsVisible)
            {
                Debug.Log($"<color=red>[UI CONTROL]</color> Кнопка отмены заказа активна! <color=white>(Можно нажать ESC)</color>");
            }
            else
            {
                Debug.Log($"<color=red>[UI CONTROL]</color> Кнопка отмены заказа скрыта.");
            }
        });

        // Финал
        EventBus.Subscribe<NodeCompletedCommand>(cmd => {
            Debug.Log($"<color=green>=== [GRAPH FINISHED] ===</color> Работа завершена на Exit-ноде: {cmd.NodeId}");
        });
    }

    private void ContinueGraph(string nodeId)
    {
        Debug.Log($"<color=gray>[SYSTEM]</color> Нода {nodeId} завершена, передаю управление в интерпретатор...");
        _interpreter.Proceed(nodeId);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<PlayPhraseCommand>(null);
        EventBus.Unsubscribe<PlayAnimationCommand>(null);
        EventBus.Unsubscribe<ShowPlayerChoicesCommand>(null);
        EventBus.Unsubscribe<MakeOrderCommand>(null);
        EventBus.Unsubscribe<SetStorageDataCommand<int>>(null);
        EventBus.Unsubscribe<ToggleCancelOrderButtonCommand>(null);
        EventBus.Unsubscribe<NodeCompletedCommand>(null);
    }

    private void Invoke(System.Action action, float delay)
    {
        StartCoroutine(InvokeRoutine(action, delay));
    }

    private System.Collections.IEnumerator InvokeRoutine(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
}
}
