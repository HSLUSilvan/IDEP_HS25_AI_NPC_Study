using System.Collections;
using UnityEngine;

public class RiddleGameController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private ChatUiView view;
    [SerializeField] private ChatScrollController scrollController;

    [Header("Chat UI Activator (optional)")]
    [SerializeField] private ChatSystemActivator chatActivator;

    [Header("Backend Config (attach AiBackendConfig on same GameObject)")]
    [SerializeField] private AiBackendConfig backendConfig;

    [Header("Riddle Setup")]
    [SerializeField] private bool generateRiddleOnStart = true;
    [TextArea(2, 4)] public string riddleThemeExtra = "Make it a classic but not too common. Keep it short.";
    [SerializeField] private RiddleDefinition manualRiddle;

    [Header("Attempts")]
    [SerializeField] private int maxAttempts = 10;
    [SerializeField] private TMPro.TMP_Text attemptsText;

    [Header("Judge UI")]
    [SerializeField] private JudgeDisplayMode judgeDisplayMode = JudgeDisplayMode.AccuracyOnly;

    [Header("Win Threshold")]
    [Range(0, 100)]
    [SerializeField] private int winAccuracyThreshold = 100;

    [Header("Win / Lose Overlay")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TMPro.TMP_Text gameOverLabel;
    [SerializeField] private float endScreenSeconds = 3f;

    [Header("Return to Menu Targets")]
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject roomRoot;
    [SerializeField] private GameObject chatSystemRoot;

    [Header("Player Reset")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Scroll / Typing")]
    [SerializeField] private int scrollNotifyEveryNChars = 30;

    public GamePhase Phase { get; private set; } = GamePhase.Boot;
    public RiddleDefinition CurrentRiddle { get; private set; }
    public int RemainingAttempts { get; private set; }
    public bool IsEnded { get; private set; }

    public bool IsBusy =>
        IsEnded ||
        Phase == GamePhase.DoorResponding ||
        Phase == GamePhase.Judging;

    private IChatBackend _backend;
    private ContentPolicy _policy;
    private DoorNpcConversation _door;
    private RiddleJudge _judge;
    private RiddleGenerator _generator;
    private RiddleSessionLogger _sessionLog;

    private bool _conversationStarted;
    private Vector3 _initialPlayerPos;
    private Rigidbody2D _playerRb2D;

    private void Awake()
    {
        if (view == null)
        {
            Debug.LogError("[AI] RiddleGameController: view reference missing.");
            return;
        }

        if (backendConfig == null)
        {
            backendConfig = GetComponent<AiBackendConfig>();
            if (backendConfig == null)
            {
                Debug.LogError("[AI] Missing AiBackendConfig on ChatSystem.");
                return;
            }
        }

        _backend = CreateBackend(backendConfig);

        _policy = new ContentPolicy();
        _door = new DoorNpcConversation(_backend, _policy);
        _judge = new RiddleJudge(_backend);
        _generator = new RiddleGenerator(_backend);

        _sessionLog = new RiddleSessionLogger();

        RemainingAttempts = Mathf.Max(1, maxAttempts);
        UpdateAttemptsUI();

        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);

        if (playerTransform != null)
        {
            _initialPlayerPos = playerTransform.position;
            _playerRb2D = playerTransform.GetComponent<Rigidbody2D>();
        }

        ResetChatToGreeting();
        view.SetStatus("Approach the door and press E...");
    }

    private IChatBackend CreateBackend(AiBackendConfig cfg)
    {
        if (cfg.UseOpenAi)
        {
            AiDebugLog.Info("Backend: OpenAI");
            return new OpenAiChatBackend(cfg.OpenAiApiKey, cfg.OpenAiModel, cfg.Temperature);
        }

        AiDebugLog.Info("Backend: vLLM/Apertus");
        return new VllmChatBackend(new VllmChatClient());
    }

    public void BeginConversation()
    {
        if (IsEnded) return;
        if (_conversationStarted) return;

        _conversationStarted = true;
        StartCoroutine(BootFlow());
    }

    private IEnumerator BootFlow()
    {
        Phase = GamePhase.Boot;

        if (generateRiddleOnStart)
        {
            view.SetStatus("Summoning a riddle...");

            RiddleDefinition r = null;
            string err = null;

            yield return _generator.Generate(riddleThemeExtra, x => r = x, e => err = e);

            if (!string.IsNullOrEmpty(err))
            {
                _sessionLog.LogError(err);
                view.AppendLine($"\n[Error: {err}]");
                SafeNotifyScroll(true);
                view.SetStatus("Error.");
                yield break;
            }

            CurrentRiddle = r;
        }
        else
        {
            CurrentRiddle = manualRiddle;
        }

        if (CurrentRiddle == null || string.IsNullOrWhiteSpace(CurrentRiddle.question))
        {
            view.AppendLine("\n[No riddle defined.]");
            SafeNotifyScroll(true);
            view.SetStatus("Error.");
            yield break;
        }

        _sessionLog.LogRiddle(CurrentRiddle);

        Phase = GamePhase.PresentRiddle;
        view.AppendLine($"\nDoor: Very well. Solve this riddle:\n\"{CurrentRiddle.question}\"");
        SafeNotifyScroll(true);

        view.SetStatus("Answer the riddle.");
        Phase = GamePhase.AwaitPlayerAnswer;
    }

    public void SubmitPlayerMessage(string playerText)
    {
        if (IsEnded)
        {
            view.SetStatus("Finished.");
            return;
        }

        if (!_conversationStarted)
        {
            view.AppendLine("\n[Talk to the door first (press E).]");
            SafeNotifyScroll(true);
            return;
        }

        if (IsBusy)
        {
            view.SetStatus("Wait...");
            return;
        }

        if (!_policy.IsUserInputAllowed(playerText, out var reason))
        {
            view.AppendLine($"\n[Blocked: {reason}]");
            SafeNotifyScroll(true);
            view.SetStatus("Blocked.");
            return;
        }

        _sessionLog.LogPlayer(playerText);

        view.AppendLine($"\nYou: {playerText}\nDoor: ");
        SafeNotifyScroll(true);

        StartCoroutine(HandleTurn(playerText));
    }

    private IEnumerator HandleTurn(string playerText)
    {
        if (CurrentRiddle == null)
        {
            view.AppendLine("\n[The door hasn't posed a riddle yet. Press E again.]");
            SafeNotifyScroll(true);
            view.SetStatus("No riddle.");
            yield break;
        }

        Phase = GamePhase.DoorResponding;
        view.SetStatus("Door is speaking...");

        string doorErr = null;
        string fullDoor = "";

        int charCounter = 0;
        int n = Mathf.Max(5, scrollNotifyEveryNChars);

        scrollController?.BeginAutoFollow();

        int attemptsAtThisTurnStart = RemainingAttempts;

        yield return _door.SendPlayerMessageFakeStream(
            CurrentRiddle,
            attemptsAtThisTurnStart,
            maxAttempts,
            playerText,
            delta =>
            {
                fullDoor += delta;
                view.AppendToChat(delta);

                charCounter++;
                if (charCounter % n == 0)
                    SafeNotifyScroll(false);
            },
            _ => { },
            e => doorErr = e,
            80f
        );

        SafeNotifyScroll(true);
        scrollController?.EndAutoFollow();

        if (!string.IsNullOrEmpty(doorErr))
        {
            _sessionLog.LogError(doorErr);
            view.AppendLine("\n\n[Error: see Console]");
            SafeNotifyScroll(true);
            view.SetStatus("Error.");
            yield break;
        }

        _sessionLog.LogDoor(fullDoor);

        Phase = GamePhase.Judging;
        view.SetStatus("Judging your answer...");

        JudgeResponse jr = null;
        string judgeErr = null;

        yield return _judge.Evaluate(CurrentRiddle, playerText, r => jr = r, e => judgeErr = e);

        if (!string.IsNullOrEmpty(judgeErr))
        {
            _sessionLog.LogError(judgeErr);
            view.AppendLine($"\n\n[Judge Error: {judgeErr}]");
            SafeNotifyScroll(true);
            view.SetStatus("Error.");
            yield break;
        }

        _sessionLog.LogJudge(jr);

        int accuracyPct = 0;
        if (jr != null)
        {
            float decisionConf = (jr.confidence > 0f && jr.confidence <= 1f)
                ? jr.confidence
                : 0.75f;

            decisionConf = Mathf.Clamp01(decisionConf);

            float correctnessProb = jr.solved ? decisionConf : (1f - decisionConf);
            accuracyPct = Mathf.Clamp(Mathf.RoundToInt(correctnessProb * 100f), 0, 100);
        }

        if (judgeDisplayMode != JudgeDisplayMode.None && jr != null)
        {
            if (judgeDisplayMode == JudgeDisplayMode.AccuracyOnly)
                view.AppendLine($"\n\nRiddle Accuracy: {accuracyPct}%");
            else
                view.AppendLine($"\n\nRiddle Accuracy: {accuracyPct}%\n{jr.reason}");

            SafeNotifyScroll(true);
        }

        if (accuracyPct >= winAccuracyThreshold)
        {
            yield return EndRun(won: true);
            yield break;
        }

        bool solvedEnough = jr != null && jr.solved;

        if (!solvedEnough)
        {
            ConsumeAttempt();

            if (RemainingAttempts <= 0)
            {
                yield return EndRun(won: false);
                yield break;
            }

            view.SetStatus($"Try again. Attempts left: {RemainingAttempts}");
        }
        else
        {
            view.SetStatus("Close... refine your answer.");
        }

        Phase = GamePhase.AwaitPlayerAnswer;
    }

    private void ConsumeAttempt()
    {
        if (RemainingAttempts <= 0) return;
        RemainingAttempts--;
        UpdateAttemptsUI();
    }

    private void UpdateAttemptsUI()
    {
        if (attemptsText != null)
            attemptsText.text = $"Attempts: {RemainingAttempts}";
    }

    private IEnumerator EndRun(bool won)
    {
        if (IsEnded) yield break;
        IsEnded = true;

        if (gameOverScreen != null)
            gameOverScreen.SetActive(true);

        if (gameOverLabel != null)
        {
            gameOverLabel.text = won ? "You won!" : "Game Over";
            gameOverLabel.color = won ? Color.green : Color.red;
        }

        view.SetStatus(won ? "Solved!" : "Game Over");

        yield return new WaitForSeconds(endScreenSeconds);

        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);

        ResetForNewRun();

        if (mainMenuRoot != null)
            mainMenuRoot.SetActive(true);

        if (roomRoot != null)
            roomRoot.SetActive(false);

        if (chatSystemRoot != null)
            chatSystemRoot.SetActive(false);
    }

    private void ResetForNewRun()
    {
        IsEnded = false;
        Phase = GamePhase.Boot;

        _conversationStarted = false;
        CurrentRiddle = null;

        RemainingAttempts = Mathf.Max(1, maxAttempts);
        UpdateAttemptsUI();

        _door.ClearHistory();

        ResetChatToGreeting();

        if (chatActivator != null)
            chatActivator.HideChat();

        ResetPlayerTransform();
    }

    private void ResetChatToGreeting()
    {
        view.SetChatText($"Door: {_door.Greeting}\n");
        SafeNotifyScroll(true);
        view.SetStatus("Approach the door and press E...");
    }

    private void ResetPlayerTransform()
    {
        if (playerTransform == null) return;

        Vector3 target = playerSpawnPoint != null ? playerSpawnPoint.position : _initialPlayerPos;
        playerTransform.position = target;

        if (_playerRb2D != null)
        {
            _playerRb2D.linearVelocity = Vector2.zero;
            _playerRb2D.angularVelocity = 0f;
        }
    }

    private void SafeNotifyScroll(bool force)
    {
        if (scrollController != null && scrollController.gameObject.activeInHierarchy)
            scrollController.NotifyContentChanged(forceToBottom: force);
    }
}
