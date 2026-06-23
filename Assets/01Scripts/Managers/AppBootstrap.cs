using UnityEngine;

public class AppBootstrap : MonoBehaviour
{
    [SerializeField] private int targetFrameRate = 60;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrameRate;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        EnsureGameServices();
    }

    private void EnsureGameServices()
    {
        if (GameServices.Instance != null) return;
        var go = new GameObject("_Managers");
        go.AddComponent<GameServices>();
    }
}
