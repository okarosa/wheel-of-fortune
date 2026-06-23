using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Zone Info")]
    public TextMeshProUGUI currentZoneText;
    public TextMeshProUGUI wheelTypeText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI coinsText;

    [Header("Zone Indicators")]
    public GameObject safeZoneIndicator;
    public TextMeshProUGUI safeZoneValueText;
    public GameObject superZoneIndicator;
    public TextMeshProUGUI superZoneValueText;

    [Header("Panels")]
    public GameObject bombPanel;
    public GameObject gameOverPanel;
    public GameObject rewardCompletePanel;
    public GameObject buyCoinPanel;

    [Header("Buttons")]
    public Button exitButton;
    public Button giveUpButton;
    public Button reviveCoinButton;
    public Button reviveAdsButton;
    public Button restartButton;
    public Button autoSpinButton;
    public TextMeshProUGUI autoSpinText;

    [Header("Reward List")]
    public ScrollRect rewardScrollRect;
    public Transform rewardContentParent;
    public GameObject rewardItemPrefab;

    [Header("Weapon Icons")]
    public Sprite armorIcon;
    public Sprite knifeIcon;
    public Sprite rifleIcon;
    public Sprite pistolIcon;
    public Sprite submachineIcon;
    public Sprite shotgunIcon;
    public Sprite sniperIcon;

    [Header("Zone Progress")]
    public ZoneProgressBar zoneProgressBar;

    [Header("Character Effect")]
    public Image characterEffectImage;

    [Header("Canvas Reference")]
    public Canvas targetCanvas;

    [Header("Revive")]
    public TextMeshProUGUI reviveCoinCostText;
    public Image coinsImage;
    public Sprite coinFlySprite;

    [Header("Buy Coins")]
    public BuyCoinConfig buyCoinConfig;
    public TextMeshProUGUI coinAmountText;
    public Button noThanksButton;
    public Button[] buyCoinButtons;

    public HUDHandler HUD { get; private set; }
    public PanelController Panels { get; private set; }
    public RewardFeedHandler RewardFeed { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        HUD = new HUDHandler(this);
        Panels = new PanelController(this);
        var canvas = targetCanvas != null ? targetCanvas : GetComponent<Canvas>();
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        RewardFeed = new RewardFeedHandler(this, canvas);

        var services = GameServices.Instance;
        if (services != null)
        {
            services.Register(this);
            services.Register(HUD);
            services.Register(Panels);
            services.Register(RewardFeed);
        }

        InitPlayerPrefs();
        InitButtons();
        InitPanels();

        GameManager.Instance.RegisterStateChanged(OnGameStateChanged);
        HUD.UpdateAllUI();
        HUD.AnimateExitButton(GameManager.Instance.CurrentState);
        HUD.UpdateAutoSpinButton(false);
        HUD.StartWheelTypePulse();
        HUD.StartCharacterIdleAnimation();
        HUD.UpdateZoneProgress();
    }

    private void InitPlayerPrefs()
    {
        if (!PlayerPrefs.HasKey("TotalCoins")) PlayerPrefs.SetInt("TotalCoins", 100);
        if (!PlayerPrefs.HasKey("TotalMoney")) PlayerPrefs.SetInt("TotalMoney", 1000);
        PlayerPrefs.Save();
    }

    private void InitButtons()
    {
        if (exitButton != null) exitButton.onClick.AddListener(() => Panels?.OnExitClicked());
        if (giveUpButton != null) giveUpButton.onClick.AddListener(() => Panels?.OnGiveUpClicked());
        if (reviveCoinButton != null) reviveCoinButton.onClick.AddListener(() => Panels?.OnReviveCoinClicked());
        if (reviveAdsButton != null) reviveAdsButton.onClick.AddListener(() => Panels?.OnReviveAdsClicked());
        if (noThanksButton != null) noThanksButton.onClick.AddListener(() => Panels?.OnNoThanksClicked());
        if (restartButton != null) restartButton.onClick.AddListener(() => Panels?.OnRestartClicked());
        if (autoSpinButton != null) autoSpinButton.onClick.AddListener(OnAutoSpinClicked);

        if (buyCoinButtons == null) return;
        for (int i = 0; i < buyCoinButtons.Length; i++)
        {
            int index = i;
            if (buyCoinButtons[i] != null)
                buyCoinButtons[i].onClick.AddListener(() => Panels?.OnBuyCoinClicked(index));
        }
    }

    private void InitPanels()
    {
        if (bombPanel != null) bombPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (rewardCompletePanel != null) rewardCompletePanel.SetActive(false);
        if (buyCoinPanel != null) buyCoinPanel.SetActive(false);
    }

    private void OnAutoSpinClicked()
    {
        var wc = GameServices.Instance.WheelController;
        if (wc == null) return;

        bool enable = !wc.AutoSpinEnabled;
        wc.AutoSpinEnabled = enable;

        if (autoSpinButton != null)
        {
            autoSpinButton.transform.DOKill();
            autoSpinButton.transform.localRotation = Quaternion.identity;
            autoSpinButton.transform.DORotate(new Vector3(0, 360, 0), 0.4f, DG.Tweening.RotateMode.LocalAxisAdd)
                .SetEase(DG.Tweening.Ease.OutCubic);
        }

        HUD?.UpdateAutoSpinButton(enable);
        if (enable) wc.SpinWheel();
    }

    private void OnGameStateChanged()
    {
        HUD?.AnimateExitButton(GameManager.Instance.CurrentState);
        HUD?.UpdateAllUI();
    }

    public void UpdateAllUI() => HUD?.UpdateAllUI();
    public void UpdateZoneProgress() => HUD?.UpdateZoneProgress();
    public void AnimateZoneProgress(int fromZone, int toZone) => HUD?.AnimateZoneProgress(fromZone, toZone);
    public void AddRewardItem(WheelSliceData reward) => RewardFeed?.AddRewardItem(reward);
    public void ShowBombPanel() => Panels?.ShowBombPanel();
    public void ShowBuyCoinPanel() => Panels?.ShowBuyCoinPanel();
    public void HideBuyCoinPanel() => Panels?.HideBuyCoinPanel();
    public void HideBombPanel() => Panels?.HideBombPanel();
    public void ShowRewardComplete() => Panels?.ShowRewardComplete();
    public void UpdateAutoSpinButton(bool active) => HUD?.UpdateAutoSpinButton(active);

    private void OnDestroy()
    {
        DOTween.Kill(this);
        if (GameManager.Instance != null)
            GameManager.Instance.UnregisterStateChanged(OnGameStateChanged);
        if (exitButton != null) exitButton.onClick.RemoveAllListeners();
        if (giveUpButton != null) giveUpButton.onClick.RemoveAllListeners();
        if (reviveCoinButton != null) reviveCoinButton.onClick.RemoveAllListeners();
        if (reviveAdsButton != null) reviveAdsButton.onClick.RemoveAllListeners();
        if (noThanksButton != null) noThanksButton.onClick.RemoveAllListeners();
        if (restartButton != null) restartButton.onClick.RemoveAllListeners();
        if (autoSpinButton != null) autoSpinButton.onClick.RemoveAllListeners();
    }
}
