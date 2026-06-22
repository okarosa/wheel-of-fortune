using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class WheelController : MonoBehaviour
{
    [Header("Wheel Visuals")]
    [SerializeField] private Image wheelBackgroundImage;
    [SerializeField] private Image wheelPointerImage;
    [SerializeField] private Image wheelValueImage;
    [SerializeField] private Transform wheelItemsContainer;
    [SerializeField] private Button spinButton;

    [Header("Spin Settings")]
    [SerializeField] private float minSpinDuration = 3f;
    [SerializeField] private float maxSpinDuration = 5f;
    [SerializeField] private float finalRotationOffset = 0f;

    [Header("Idle Rotation")]
    [SerializeField] private float idleRotationSpeed = 30f;

    private const int SliceCount = 8;
    private const float DegreesPerSlice = 360f / SliceCount;

    private bool _isSpinning;
    private int _targetSliceIndex;
    private WheelSliceData[] _currentSlices;
    private Tween _idleTween;

    public bool AutoSpinEnabled { get; set; }

    private void Start()
    {
        if (wheelValueImage == null)
        {
            var go = GameObject.Find("ui_image_wheel_value");
            if (go != null) wheelValueImage = go.GetComponent<Image>();
        }

        if (spinButton != null)
            spinButton.onClick.AddListener(SpinWheel);

        SetupWheel();
    }

    public void SetupWheel()
    {
        var config = GameManager.Instance.GetCurrentWheelConfig();
        if (config == null) return;

        if (wheelBackgroundImage != null)
        {
            if (config.wheelBackground != null)
                wheelBackgroundImage.sprite = config.wheelBackground;
            wheelBackgroundImage.color = config.wheelBgColor;
        }

        if (wheelPointerImage != null && config.wheelPointerSprite != null)
            wheelPointerImage.sprite = config.wheelPointerSprite;

        RandomizeSlices(config);
        ApplySlicesToUI();
        AnimateSlicesAppear();
        ResetWheelRotation();
        StartIdleRotation();
    }

    private void RandomizeSlices(WheelConfigData config)
    {
        var pool = new List<WheelSliceData>();
        if (config.rewards != null)
            pool.AddRange(config.rewards);

        pool.RemoveAll(s => s != null && s.isBomb);
        Shuffle(pool);

        _currentSlices = new WheelSliceData[SliceCount];

        if (config.hasBomb)
        {
            for (int i = 0; i < SliceCount - 1; i++)
                _currentSlices[i] = pool[i];
            _currentSlices[SliceCount - 1] = GameManager.Instance.bombSlice;
        }
        else
        {
            for (int i = 0; i < SliceCount; i++)
                _currentSlices[i] = pool[i];
        }

        Shuffle(_currentSlices);
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void ApplySlicesToUI()
    {
        if (wheelItemsContainer == null) return;

        for (int i = 0; i < wheelItemsContainer.childCount; i++)
        {
            var sliceUI = wheelItemsContainer.GetChild(i).GetComponent<WheelSliceUI>();
            if (sliceUI == null) continue;

            bool valid = i < _currentSlices.Length && _currentSlices[i] != null;
            sliceUI.gameObject.SetActive(valid);
            if (valid) sliceUI.SetSliceData(_currentSlices[i]);
        }
    }

    private void AnimateSlicesAppear()
    {
        if (wheelItemsContainer == null) return;
        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < wheelItemsContainer.childCount; i++)
        {
            var child = wheelItemsContainer.GetChild(i);
            if (!child.gameObject.activeSelf) continue;
            child.localScale = Vector3.zero;
            seq.Insert(i * 0.04f, child.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
        }
    }

    private void ResetWheelRotation()
    {
        if (wheelItemsContainer != null)
            wheelItemsContainer.localRotation = Quaternion.identity;
    }

    private void StartIdleRotation()
    {
        if (wheelItemsContainer == null) return;
        StopIdleRotation();
        _idleTween = wheelItemsContainer
            .DORotate(new Vector3(0, 0, -360), 360f / idleRotationSpeed, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    private void StopIdleRotation()
    {
        if (_idleTween != null && _idleTween.IsActive())
            _idleTween.Kill();
        _idleTween = null;
    }

    public void SpinWheel()
    {
        if (_isSpinning) return;
        if (GameManager.Instance.CurrentState != GameState.Idle) return;

        AudioManager.Instance.Play("sfx_revolver_spin");
        GameManager.Instance.SetState(GameState.Spinning);
        _isSpinning = true;

        if (spinButton != null)
        {
            spinButton.interactable = false;
            spinButton.transform.DOKill();
            spinButton.transform.DOScale(0f, 0.25f).SetEase(Ease.InBack);
        }

        StopIdleRotation();

        _targetSliceIndex = Random.Range(0, SliceCount);
        int   extraSpins     = Random.Range(5, 10);
        float totalRotation  = extraSpins * 360f + (360f - _targetSliceIndex * DegreesPerSlice) + finalRotationOffset;
        float duration       = Random.Range(minSpinDuration, maxSpinDuration);

        if (wheelItemsContainer != null)
        {
            wheelItemsContainer.DOKill();
            wheelItemsContainer
                .DORotate(new Vector3(0, 0, -totalRotation), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic)
                .OnComplete(OnSpinComplete);
        }
    }

    private void OnSpinComplete()
    {
        _isSpinning = false;

        var landed = _currentSlices[_targetSliceIndex];
        if (landed == null) return;

        AnimateSpinResult();

        if (landed.isBomb)
        {
            GameManager.Instance.SetState(GameState.Bomb);
            UIManager.Instance?.ShowBombPanel();
            BombEffect.Instance?.PlayBombEffect();
            DisableAutoSpin();
        }
        else
        {
            RewardCollector.Instance.AddReward(landed);
            UIManager.Instance?.AddRewardItem(landed);
            GameManager.Instance.SetState(GameState.Result);

            int fromZone = GameManager.Instance.CurrentZone;
            UIManager.Instance?.AnimateZoneProgress(fromZone, fromZone + 1);

            DOVirtual.DelayedCall(0.5f, () =>
            {
                if (!GameManager.Instance.IsRewardComplete())
                {
                    GameManager.Instance.NextZone();
                    SetupWheel();
                    if (AutoSpinEnabled) SpinWheel();
                }
                else
                {
                    UIManager.Instance?.ShowRewardComplete();
                    GameManager.Instance.SetState(GameState.RewardComplete);
                    DisableAutoSpin();
                }
            });
        }
    }

    private void DisableAutoSpin()
    {
        AutoSpinEnabled = false;
        UIManager.Instance?.UpdateAutoSpinButton(false);
    }

    private void AnimateSpinResult()
    {
        Sequence seq = DOTween.Sequence();

        if (wheelValueImage != null)
        {
            wheelValueImage.transform.localScale = Vector3.one;
            seq.Append(wheelValueImage.transform.DOScale(0f, 0.15f).SetEase(Ease.InBack));
            seq.Join(wheelValueImage.transform.DORotate(new Vector3(0, 0, 180), 0.15f, RotateMode.LocalAxisAdd).SetEase(Ease.InBack));
        }
        if (spinButton != null)
        {
            spinButton.transform.localScale = Vector3.one;
            seq.Join(spinButton.transform.DOScale(0f, 0.15f).SetEase(Ease.InBack));
        }

        seq.AppendInterval(0.3f);

        if (wheelValueImage != null)
        {
            seq.Append(wheelValueImage.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
            seq.Join(wheelValueImage.transform.DORotate(new Vector3(0, 0, -180), 0.25f, RotateMode.LocalAxisAdd).SetEase(Ease.OutBack));
        }
        if (spinButton != null)
        {
            spinButton.interactable = false;
            seq.Join(spinButton.transform.DOScale(1.1f, 0.25f).SetEase(Ease.OutBack));
            seq.AppendCallback(() =>
            {
                spinButton.transform.localScale = Vector3.one;
                spinButton.interactable = true;
            });
        }
    }

    private void OnDestroy()
    {
        StopIdleRotation();
        if (spinButton != null)
            spinButton.onClick.RemoveListener(SpinWheel);
    }
}