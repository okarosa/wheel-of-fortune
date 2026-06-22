using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Zone Info")]
    [SerializeField] private TextMeshProUGUI currentZoneText;
    [SerializeField] private TextMeshProUGUI wheelTypeText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("Zone Indicators")]
    [SerializeField] private GameObject safeZoneIndicator;
    [SerializeField] private TextMeshProUGUI safeZoneValueText;
    [SerializeField] private GameObject superZoneIndicator;
    [SerializeField] private TextMeshProUGUI superZoneValueText;

    [Header("Panels")]
    [SerializeField] private GameObject bombPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject rewardCompletePanel;
    [SerializeField] private GameObject buyCoinPanel;

    [Header("Buttons")]
    [SerializeField] private Button exitButton;
    [SerializeField] private Button giveUpButton;
    [SerializeField] private Button reviveCoinButton;
    [SerializeField] private Button reviveAdsButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button autoSpinButton;
    [SerializeField] private TextMeshProUGUI autoSpinText;

    [Header("Reward List")]
    [SerializeField] private ScrollRect rewardScrollRect;
    [SerializeField] private Transform rewardContentParent;
    [SerializeField] private GameObject rewardItemPrefab;

    [Header("Weapon Icons")]
    [SerializeField] private Sprite armorIcon;
    [SerializeField] private Sprite knifeIcon;
    [SerializeField] private Sprite rifleIcon;
    [SerializeField] private Sprite pistolIcon;
    [SerializeField] private Sprite submachineIcon;
    [SerializeField] private Sprite shotgunIcon;
    [SerializeField] private Sprite sniperIcon;

    [Header("Zone Progress")]
    [SerializeField] private ZoneProgressBar zoneProgressBar;

    [Header("Character Effect")]
    [SerializeField] private Image characterEffectImage;

    [Header("Revive")]
    [SerializeField] private TextMeshProUGUI reviveCoinCostText;
    [SerializeField] private Image coinsImage;
    [SerializeField] private Sprite coinFlySprite;
    private int _reviveCost = 25;

    [Header("Buy Coins")]
    [SerializeField] private BuyCoinConfig buyCoinConfig;
    [SerializeField] private TextMeshProUGUI coinAmountText;
    [SerializeField] private Button noThanksButton;
    [SerializeField] private Button[] buyCoinButtons;

    private Canvas _canvas;
    private WheelController _wheelController;

    private Dictionary<WheelType, string> _wheelTypeLabels;
    private Dictionary<string, Sprite> _weaponIcons;
    private static readonly HashSet<string> _weaponNames = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        { "rifle", "armor", "knife", "pistol", "submachine", "shotgun", "sniper" };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        InitPlayerPrefs();
        InitLookups();
        InitButtons();
        InitPanels();

        GameManager.Instance.RegisterStateChanged(OnGameStateChanged);
        UpdateAllUI();
        AnimateExitButton(GameManager.Instance.CurrentState);
        UpdateAutoSpinButton(false);
        StartWheelTypePulse();
        StartCharacterIdleAnimation();
        UpdateZoneProgress();
    }

    private void InitPlayerPrefs()
    {
        if (!PlayerPrefs.HasKey("TotalCoins"))  PlayerPrefs.SetInt("TotalCoins", 100);
        if (!PlayerPrefs.HasKey("TotalMoney"))  PlayerPrefs.SetInt("TotalMoney", 1000);
        PlayerPrefs.Save();
    }

    private void InitLookups()
    {
        _wheelController = FindObjectOfType<WheelController>();

        _wheelTypeLabels = new Dictionary<WheelType, string>
        {
            { WheelType.Bronze, "BRONZE SPIN" },
            { WheelType.Silver, "SILVER SPIN" },
            { WheelType.Golden, "GOLDEN SPIN" },
        };

        _weaponIcons = new Dictionary<string, Sprite>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "armor",      armorIcon      },
            { "knife",      knifeIcon      },
            { "rifle",      rifleIcon      },
            { "pistol",     pistolIcon     },
            { "submachine", submachineIcon },
            { "shotgun",    shotgunIcon    },
            { "sniper",     sniperIcon     },
        };
    }

    private void InitButtons()
    {
        if (exitButton != null)       exitButton.onClick.AddListener(OnExitClicked);
        if (giveUpButton != null)     giveUpButton.onClick.AddListener(OnGiveUpClicked);
        if (reviveCoinButton != null) reviveCoinButton.onClick.AddListener(OnReviveCoinClicked);
        if (reviveAdsButton != null)  reviveAdsButton.onClick.AddListener(OnReviveAdsClicked);
        if (noThanksButton != null)   noThanksButton.onClick.AddListener(OnNoThanksClicked);
        if (restartButton != null)    restartButton.onClick.AddListener(OnRestartClicked);
        if (autoSpinButton != null)   autoSpinButton.onClick.AddListener(OnAutoSpinClicked);

        if (buyCoinButtons == null) return;
        for (int i = 0; i < buyCoinButtons.Length; i++)
        {
            int index = i;
            if (buyCoinButtons[i] != null)
                buyCoinButtons[i].onClick.AddListener(() => OnBuyCoinClicked(index));
        }
    }

    private void InitPanels()
    {
        if (bombPanel != null)          bombPanel.SetActive(false);
        if (gameOverPanel != null)      gameOverPanel.SetActive(false);
        if (rewardCompletePanel != null) rewardCompletePanel.SetActive(false);
        if (buyCoinPanel != null)       buyCoinPanel.SetActive(false);
    }

    private void OnGameStateChanged()
    {
        AnimateExitButton(GameManager.Instance.CurrentState);
        UpdateAllUI();
    }

    public void UpdateZoneProgress()
    {
        if (zoneProgressBar != null)
            zoneProgressBar.ResetToZone(GameManager.Instance.CurrentZone);
    }

    public void AnimateZoneProgress(int fromZone, int toZone)
    {
        toZone = Mathf.Min(toZone, GameManager.Instance.maxZone);
        if (zoneProgressBar != null)
            zoneProgressBar.AnimateToZone(fromZone, toZone);
    }

    public void UpdateAllUI()
    {
        if (currentZoneText != null)
            currentZoneText.text = $"{GameManager.Instance.CurrentZone.ToString()}";

        if (wheelTypeText != null && _wheelTypeLabels.TryGetValue(GameManager.Instance.CurrentWheelType, out var label))
            wheelTypeText.text = $"{label}";

        var config = GameManager.Instance.GetCurrentWheelConfig();
        if (config != null && config.backgroundMusic != null)
            AudioManager.Instance.PlayMusic(config.backgroundMusic);

        if (superZoneIndicator != null)
            superZoneIndicator.SetActive(true);

        if (safeZoneValueText != null)
        {
            int next = GameManager.Instance.GetNextSilverZone();
            safeZoneValueText.text = next > 0 ? next.ToString() : (GameManager.Instance.CurrentZone >= 55 ? "60" : "--");
        }
        if (superZoneValueText != null)
        {
            int next = GameManager.Instance.GetNextSuperZone();
            superZoneValueText.text = next > 0 ? next.ToString() : (GameManager.Instance.CurrentZone >= 60 ? "60" : "--");
        }

        AnimateSafeZoneIndicator();
        UpdateMoneyDisplay();
    }

    private void AnimateExitButton(GameState state)
    {
        if (exitButton == null) return;
        exitButton.transform.DOKill();
        exitButton.image.DOKill();

        if (state == GameState.Spinning)
        {
            exitButton.interactable = false;
            exitButton.transform.DOScale(0f, 0.25f).SetEase(Ease.InBack);
            exitButton.image.DOFade(0f, 0.2f);
        }
        else
        {
            exitButton.gameObject.SetActive(true);
            exitButton.image.DOFade(1f, 0.25f);
            exitButton.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack)
                .OnComplete(() => exitButton.interactable = true);
        }
    }

    private void AnimateSafeZoneIndicator()
    {
        if (safeZoneIndicator == null) return;
        safeZoneIndicator.transform.DOKill();
        bool show = !GameManager.Instance.IsSilverZone();
        if (show)
            safeZoneIndicator.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        else
            safeZoneIndicator.transform.DOScale(0f, 0.25f).SetEase(Ease.InBack);
    }

    private string FormatNumber(int value)
    {
        if (value >= 1000000)
            return (value / 1000000f).ToString("0.0") + "m";
        if (value >= 100000)
            return (value / 1000f).ToString("0.0") + "k";
        return value.ToString("N0").Replace(",", ".");
    }

    private void UpdateMoneyDisplay()
    {
        if (moneyText != null)  moneyText.text  = FormatNumber(PlayerPrefs.GetInt("TotalMoney", 1000));
        if (coinsText != null)  coinsText.text  = FormatNumber(PlayerPrefs.GetInt("TotalCoins", 100));
    }

    private void UpdateReviveButtonText()
    {
        if (reviveCoinCostText != null)
            reviveCoinCostText.text = $"{_reviveCost.ToString()}";
    }

    private int GetDisplayAmount(WheelSliceData reward)
    {
        return reward.rewardAmount;
    }

    private Sprite GetWeaponIcon(string name)
    {
        return _weaponIcons.TryGetValue(name, out var sprite) ? sprite : null;
    }

    public void AddRewardItem(WheelSliceData reward)
    {
        AudioManager.Instance.Play("sfx_item_reward");
        if (rewardItemPrefab == null || rewardContentParent == null) return;
        AudioManager.Instance.Play("sfx_item_collect");

        RewardItemUI targetItem = FindExistingRewardItem(reward.rewardName);

        if (targetItem != null)
        {
            UpdateExistingRewardItem(targetItem, reward);
        }
        else
        {
            CreateNewRewardItem(reward);
        }

        UpdateMoneyDisplay();
    }

    private RewardItemUI FindExistingRewardItem(string rewardName)
    {
        for (int i = 0; i < rewardContentParent.childCount; i++)
        {
            var ui = rewardContentParent.GetChild(i).GetComponent<RewardItemUI>();
            if (ui != null && ui.HasName(rewardName)) return ui;
        }
        return null;
    }

    private void UpdateExistingRewardItem(RewardItemUI targetItem, WheelSliceData reward)
    {
        int oldValue  = targetItem.TotalAmount;
        int addAmount = GetDisplayAmount(reward);

        targetItem.transform.SetAsFirstSibling();
        targetItem.PlayMoveToTopAnimation();
        Canvas.ForceUpdateCanvases();

        SpawnCollectIcon(reward.icon, targetItem.transform.position, () =>
        {
            targetItem.PlayCountUp(oldValue, oldValue + addAmount, () => targetItem.AddAmount(addAmount));
            SmoothScrollToTop();
        });
    }

    private void CreateNewRewardItem(WheelSliceData reward)
    {
        int addAmount = GetDisplayAmount(reward);

        GameObject itemGO = Instantiate(rewardItemPrefab, rewardContentParent);
        itemGO.transform.SetAsFirstSibling();

        var targetItem = itemGO.GetComponent<RewardItemUI>();
        if (targetItem == null) return;

        WheelSliceData displayData = ScriptableObject.CreateInstance<WheelSliceData>();
        displayData.rewardName   = reward.rewardName;
        displayData.rewardAmount = addAmount;
        displayData.icon         = reward.icon;

        targetItem.SetReward(displayData);
        targetItem.SetAmountText(0);

        Sprite weaponSprite = GetWeaponIcon(reward.rewardName);
        if (weaponSprite != null) targetItem.SetIcon(weaponSprite);

        targetItem.transform.localScale = Vector3.zero;
        var cg = targetItem.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 0f;

        Canvas.ForceUpdateCanvases();

        SpawnCollectIcon(reward.icon, targetItem.transform.position, () =>
        {
            targetItem.PlayAppearAnimation(0f);
            targetItem.PlayCountUp(0, addAmount);
            SmoothScrollToTop();
        });
    }

    private void SmoothScrollToTop()
    {
        if (rewardScrollRect == null) return;
        Canvas.ForceUpdateCanvases();
        DOTween.To(
            () => rewardScrollRect.verticalNormalizedPosition,
            x  => rewardScrollRect.verticalNormalizedPosition = x,
            1f, 0.35f).SetEase(Ease.OutCubic);
    }

    private void SpawnCoinFlyEffect(System.Action onComplete = null)
    {
        if (_canvas == null || reviveCoinButton == null) { onComplete?.Invoke(); return; }

        Transform target = coinsImage != null ? coinsImage.transform
                         : coinsText  != null ? coinsText.transform : null;
        if (target == null) { onComplete?.Invoke(); return; }

        Vector3 targetPos = target.position;
        if (coinsImage != null)
        {
            Vector3[] corners = new Vector3[4];
            coinsImage.rectTransform.GetWorldCorners(corners);
            targetPos = (corners[0] + corners[2]) * 0.5f;
        }

        Vector3 startPos  = reviveCoinButton.transform.position;
        int     remaining = 5;

        for (int i = 0; i < 5; i++)
        {
            GameObject coin = new GameObject("fly_coin", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            coin.transform.SetParent(_canvas.transform, false);

            var rt = coin.GetComponent<RectTransform>();
            rt.sizeDelta   = new Vector2(40, 40);
            rt.anchorMin   = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.position    = startPos + new Vector3(Random.Range(-30f, 30f), Random.Range(-20f, 20f), 0f);
            rt.localScale  = Vector3.zero;

            var img = coin.GetComponent<Image>();
            if (coinFlySprite != null) img.sprite = coinFlySprite;
            img.color          = Color.yellow;
            img.raycastTarget  = false;

            float delay = i * 0.05f;
            rt.DOScale(1f, 0.2f).SetDelay(delay).SetEase(Ease.OutBack);
            rt.DOMove(targetPos, 0.5f).SetDelay(delay + 0.15f).SetEase(Ease.InBack);

            Sequence flySeq = DOTween.Sequence();
            flySeq.Insert(delay + 0.15f, rt.DOScale(0.2f, 0.5f).SetEase(Ease.InBack));
            flySeq.OnComplete(() =>
            {
                Destroy(coin);
                remaining--;
                if (remaining <= 0)
                {
                    if (coinsImage != null)
                    {
                        coinsImage.transform.DOKill();
                        coinsImage.transform.DOScale(1.3f, 0.15f).SetEase(Ease.OutBack)
                            .OnComplete(() => coinsImage.transform.DOScale(1f, 0.2f).SetEase(Ease.InBack));
                    }
                    onComplete?.Invoke();
                }
            });
            rt.DORotate(new Vector3(0, 0, Random.Range(-360f, 360f)), 0.6f).SetDelay(delay).SetEase(Ease.Linear);
        }
    }

    private void SpawnCollectIcon(Sprite iconSprite, Vector3 targetPosition, System.Action onComplete)
    {
        if (_canvas == null || iconSprite == null) { onComplete?.Invoke(); return; }

        var iconGO = new GameObject("collect_icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconGO.transform.SetParent(_canvas.transform, false);

        var iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.sizeDelta       = new Vector2(80, 80);
        iconRT.anchorMin       = iconRT.anchorMax = new Vector2(0.5f, 0.5f);
        iconRT.anchoredPosition = Vector2.zero;
        iconRT.localScale      = Vector3.zero;

        var iconImage = iconGO.GetComponent<Image>();
        iconImage.sprite        = iconSprite;
        iconImage.raycastTarget = false;

        iconRT.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack);
        iconGO.transform.DOMove(targetPosition, 0.7f).SetDelay(0.3f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                iconRT.DOScale(0f, 0.15f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    Destroy(iconGO);
                    onComplete?.Invoke();
                });
            });
        iconGO.transform.DORotate(new Vector3(0, 0, 360), 0.7f, RotateMode.LocalAxisAdd).SetDelay(0.3f).SetEase(Ease.Linear);
    }

    private void StartWheelTypePulse()
    {
        if (wheelTypeText == null) return;
        wheelTypeText.transform.localScale = Vector3.one;
        wheelTypeText.transform.DOScale(1.1f, 0.9f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    private void StartCharacterIdleAnimation()
    {
        if (characterEffectImage == null) return;
        characterEffectImage.transform.localScale = Vector3.one;
        DOTween.Sequence()
            .Append(characterEffectImage.transform.DOScale(1.03f, 2f).SetEase(Ease.InOutSine))
            .Append(characterEffectImage.transform.DOScale(1f,    2f).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void UpdateAutoSpinButton(bool active)
    {
        if (autoSpinButton == null) return;
        var img = autoSpinButton.GetComponent<Image>();
        if (img == null) return;

        string hex  = active ? "#FF5A40" : "#00DFFF";
        string text = active ? "PAUSE"   : "AUTO SPIN";

        if (ColorUtility.TryParseHtmlString(hex, out Color c)) img.color = c;
        if (autoSpinText != null) autoSpinText.text = $"{text}";
    }

    private void OnAutoSpinClicked()
    {
        if (_wheelController == null) return;

        bool enable = !_wheelController.AutoSpinEnabled;
        _wheelController.AutoSpinEnabled = enable;

        if (autoSpinButton != null)
        {
            autoSpinButton.transform.DOKill();
            autoSpinButton.transform.localRotation = Quaternion.identity;
            autoSpinButton.transform.DORotate(new Vector3(0, 360, 0), 0.4f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.OutCubic);
        }

        UpdateAutoSpinButton(enable);
        if (enable) _wheelController.SpinWheel();
    }

    public void ShowBombPanel()
    {
        AudioManager.Instance.Play("sfx_game_over");
        UpdateMoneyDisplay();
        UpdateReviveButtonText();

        GameObject target = bombPanel != null ? bombPanel : gameOverPanel;
        if (target != null)
        {
            target.SetActive(true);
            target.transform.localScale = Vector3.zero;
            target.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
        }
    }

    public void ShowBuyCoinPanel()
    {
        UpdateMoneyDisplay();
        if (coinAmountText != null)
            coinAmountText.text = FormatNumber(PlayerPrefs.GetInt("TotalCoins", 100));

        if (buyCoinPanel == null) return;

        if (gameOverPanel != null)
        {
            gameOverPanel.transform.DOKill();
            gameOverPanel.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack)
                .OnComplete(() => gameOverPanel.SetActive(false));
        }

        buyCoinPanel.SetActive(true);
        buyCoinPanel.transform.localScale = Vector3.zero;
        buyCoinPanel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
    }

    public void HideBuyCoinPanel()
    {
        if (buyCoinPanel == null) return;
        buyCoinPanel.transform.DOKill();
        buyCoinPanel.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack)
            .OnComplete(() => buyCoinPanel.SetActive(false));
    }

    public void HideBombPanel()
    {
        if (bombPanel != null)     bombPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void ShowRewardComplete()
    {
        if (rewardCompletePanel == null) return;
        rewardCompletePanel.SetActive(true);
        rewardCompletePanel.transform.localScale = Vector3.zero;
        rewardCompletePanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
        var rect = rewardCompletePanel.GetComponent<RectTransform>();
        if (rect != null)
            rect.DOAnchorPos(Vector2.zero, 0.4f).From(new Vector2(0f, 200f)).SetEase(Ease.OutCubic);
    }

    private void ClearRewardItems()
    {
        if (rewardContentParent == null) return;
        for (int i = rewardContentParent.childCount - 1; i >= 0; i--)
            Destroy(rewardContentParent.GetChild(i).gameObject);
    }

    private void OnExitClicked()
    {
        RewardCollector.Instance.CashOut();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnGiveUpClicked()
    {
        RewardCollector.Instance.ResetAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnReviveCoinClicked()
    {
        if (reviveCoinButton != null) reviveCoinButton.interactable = false;

        int savedCoins = PlayerPrefs.GetInt("TotalCoins", 100);
        if (savedCoins >= _reviveCost)
        {
            AudioManager.Instance.Play("sfx_coin_collect");
            int targetCoins = savedCoins - _reviveCost;

            System.Action finish = () =>
            {
                PlayerPrefs.SetInt("TotalCoins", targetCoins);
                PlayerPrefs.Save();
                _reviveCost *= 2;
                UpdateReviveButtonText();
                HideBombPanel();
                GameManager.Instance.SetState(GameState.Idle);
                ResetWheel();
                if (reviveCoinButton != null) reviveCoinButton.interactable = true;
            };

            if (coinsText != null)
            {
                coinsText.text = FormatNumber(savedCoins);
                DOTween.To(() => (float)savedCoins, x =>
                {
                    coinsText.text = FormatNumber(Mathf.RoundToInt(x));
                }, (float)targetCoins, 0.5f).SetEase(Ease.OutQuad);
            }

            SpawnCoinFlyEffect(finish);
        }
        else
        {
            ShowBuyCoinPanel();
            if (reviveCoinButton != null) reviveCoinButton.interactable = true;
        }
    }

    private void OnReviveAdsClicked()
    {
        HideBombPanel();
        GameManager.Instance.SetState(GameState.Idle);
        ResetWheel();
    }

    private void OnRestartClicked()
    {
        RewardCollector.Instance.CashOut();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnBuyCoinClicked(int index)
    {
        int coinReward = 0;
        if (buyCoinConfig != null && buyCoinConfig.options != null && index < buyCoinConfig.options.Length)
            coinReward = buyCoinConfig.options[index].coinReward;
        else
        {
            int[] defaults = { 200, 1000, 2500 };
            if (index < defaults.Length) coinReward = defaults[index];
            else return;
        }

        int savedCoins = PlayerPrefs.GetInt("TotalCoins", 100);
        int newCoins   = savedCoins + coinReward;
        PlayerPrefs.SetInt("TotalCoins", newCoins);
        PlayerPrefs.Save();

        AudioManager.Instance.Play("sfx_coin_collect");

        if (coinsText != null)
        {
            DOTween.To(() => savedCoins, x =>
            {
                int val = Mathf.RoundToInt(x);
                coinsText.text = FormatNumber(val);
                if (coinAmountText != null) coinAmountText.text = FormatNumber(val);
            }, newCoins, 0.5f).SetEase(Ease.OutQuad);
        }
        else if (coinAmountText != null)
        {
            DOTween.To(() => savedCoins, x =>
            {
                coinAmountText.text = FormatNumber(Mathf.RoundToInt(x));
            }, newCoins, 0.5f).SetEase(Ease.OutQuad);
        }
    }

    private void OnNoThanksClicked()
    {
        if (buyCoinPanel == null) return;
        buyCoinPanel.transform.DOKill();
        buyCoinPanel.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                buyCoinPanel.SetActive(false);
                if (gameOverPanel != null)
                {
                    gameOverPanel.SetActive(true);
                    gameOverPanel.transform.localScale = Vector3.zero;
                    gameOverPanel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
                }
            });
    }

    private void ResetWheel()
    {
        if (_wheelController != null) _wheelController.SetupWheel();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.UnregisterStateChanged(OnGameStateChanged);
        if (exitButton != null)       exitButton.onClick.RemoveListener(OnExitClicked);
        if (giveUpButton != null)     giveUpButton.onClick.RemoveListener(OnGiveUpClicked);
        if (reviveCoinButton != null) reviveCoinButton.onClick.RemoveListener(OnReviveCoinClicked);
        if (reviveAdsButton != null)  reviveAdsButton.onClick.RemoveListener(OnReviveAdsClicked);
        if (noThanksButton != null)   noThanksButton.onClick.RemoveListener(OnNoThanksClicked);
        if (restartButton != null)    restartButton.onClick.RemoveListener(OnRestartClicked);
        if (autoSpinButton != null)   autoSpinButton.onClick.RemoveListener(OnAutoSpinClicked);
    }
}