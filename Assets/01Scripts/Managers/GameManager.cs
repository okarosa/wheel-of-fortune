using UnityEngine;

public enum GameState
{
    Idle,
    Spinning,
    Result,
    Bomb,
    RewardComplete
}

public enum WheelType
{
    Bronze,
    Silver,
    Golden
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Settings")]
    public int maxZone = 60;
    [SerializeField] private int silverZoneInterval = 5;
    [SerializeField] private int superZoneInterval = 30;

    [Header("Wheel Configs")]
    [SerializeField] private WheelConfigData bronzeConfig;
    [SerializeField] private WheelConfigData silverConfig;
    [SerializeField] private WheelConfigData goldenConfig;
    public WheelSliceData bombSlice;

    public GameState CurrentState { get; private set; }
    public int CurrentZone { get; private set; } = 1;
    public WheelType CurrentWheelType { get; private set; } = WheelType.Bronze;

    private System.Action _onStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        GameServices.Instance?.Register(this);
    }

    private void Start()
    {
        SetState(GameState.Idle);
        UpdateWheelType();
    }

    public void SetState(GameState state)
    {
        CurrentState = state;
        _onStateChanged?.Invoke();
    }

    public void RegisterStateChanged(System.Action callback) => _onStateChanged += callback;
    public void UnregisterStateChanged(System.Action callback) => _onStateChanged -= callback;

    public void NextZone()
    {
        CurrentZone++;
        UpdateWheelType();
        SetState(GameState.Idle);
    }

    public void RestartGame()
    {
        CurrentZone = 1;
        UpdateWheelType();
        SetState(GameState.Idle);
    }

    public bool IsSilverZone() => CurrentZone % silverZoneInterval == 0 && !IsSuperZone();
    public bool IsSuperZone() => CurrentZone % superZoneInterval == 0;
    public bool IsRewardComplete() => CurrentZone >= maxZone;

    public bool CanLeave() => CurrentState == GameState.Idle && (IsSilverZone() || IsSuperZone()) && !IsRewardComplete();

    public int GetNextSilverZone()
    {
        for (int z = CurrentZone + 1; z <= maxZone; z++)
        {
            if (z % silverZoneInterval == 0 && z % superZoneInterval != 0)
                return z;
        }
        return -1;
    }

    public int GetNextSuperZone()
    {
        if (CurrentZone < 30) return 30;
        if (CurrentZone < 60) return 60;
        return -1;
    }

    private void UpdateWheelType()
    {
        if (IsSuperZone())
            CurrentWheelType = WheelType.Golden;
        else if (IsSilverZone())
            CurrentWheelType = WheelType.Silver;
        else
            CurrentWheelType = WheelType.Bronze;
    }

    public WheelConfigData GetCurrentWheelConfig()
    {
        switch (CurrentWheelType)
        {
            case WheelType.Bronze: return bronzeConfig;
            case WheelType.Silver: return silverConfig;
            case WheelType.Golden: return goldenConfig;
            default: return bronzeConfig;
        }
    }
}
