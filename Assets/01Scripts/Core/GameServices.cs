using UnityEngine;
using System;
using System.Collections.Generic;

public class GameServices : MonoBehaviour
{
    private static GameServices _instance;
    private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public static GameServices Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<GameServices>();
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        name = "_Managers";
    }

    private void Start()
    {
        DiscoverMissingServices();
#if UNITY_EDITOR
        Debug.Log($"[GameServices] {_services.Count} services registered. " +
            $"Managers: {Get<GameManager>() != null}, Controllers: {Get<WheelController>() != null}, " +
            $"UI: {Get<UIManager>() != null}, Audio: {Get<AudioManager>() != null}");
#endif
    }

    public string DebugSummary()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== GameServices Registry ===");
        foreach (var kvp in _services)
            sb.AppendLine($"  {kvp.Key.Name}: {kvp.Value.GetType().Name}");
        return sb.ToString();
    }

    private void DiscoverMissingServices()
    {
        RegisterIfMissing<GameManager>();
        RegisterIfMissing<WheelController>();
        RegisterIfMissing<AudioManager>();
        RegisterIfMissing<UIManager>();
        RegisterIfMissing<RewardCollector>();
        RegisterIfMissing<BombEffect>();
        RegisterIfMissing<DemoSettings>();
        RegisterIfMissing<ZoneProgressBar>();
    }

    private void RegisterIfMissing<T>() where T : MonoBehaviour
    {
        if (_services.ContainsKey(typeof(T))) return;
        var service = FindObjectOfType<T>();
        if (service != null) Register(service);
    }

    public void Register<T>(T service) where T : class
    {
        var type = typeof(T);
        if (!_services.ContainsKey(type))
            _services[type] = service;
    }

    public void Unregister<T>() where T : class
    {
        _services.Remove(typeof(T));
    }

    public T Get<T>() where T : class
    {
        _services.TryGetValue(typeof(T), out var service);
        return service as T;
    }

    public bool TryGet<T>(out T service) where T : class
    {
        if (_services.TryGetValue(typeof(T), out var obj) && obj is T typed)
        {
            service = typed;
            return true;
        }
        service = null;
        return false;
    }

    public GameManager GameManager => Get<GameManager>();
    public WheelController WheelController => Get<WheelController>();
    public AudioManager AudioManager => Get<AudioManager>();
    public RewardCollector RewardCollector => Get<RewardCollector>();
    public BombEffect BombEffect => Get<BombEffect>();
    public DemoSettings DemoSettings => Get<DemoSettings>();
    public ZoneProgressBar ZoneProgressBar => Get<ZoneProgressBar>();
    public HUDHandler HUD => Get<HUDHandler>();
    public PanelController Panels => Get<PanelController>();
    public RewardFeedHandler RewardFeed => Get<RewardFeedHandler>();
}
