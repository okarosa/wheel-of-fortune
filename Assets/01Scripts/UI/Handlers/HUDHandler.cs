using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class HUDHandler
{
    private UIManager _ui;

    private Dictionary<WheelType, string> _wheelTypeLabels;

    public HUDHandler(UIManager ui)
    {
        _ui = ui;
        _wheelTypeLabels = new Dictionary<WheelType, string>
        {
            { WheelType.Bronze, "BRONZE SPIN" },
            { WheelType.Silver, "SILVER SPIN" },
            { WheelType.Golden, "GOLDEN SPIN" },
        };
    }

    public void UpdateAllUI()
    {
        if (_ui.currentZoneText != null)
            _ui.currentZoneText.text = $"{GameManager.Instance.CurrentZone.ToString()}";

        if (_ui.wheelTypeText != null && _wheelTypeLabels.TryGetValue(GameManager.Instance.CurrentWheelType, out var label))
            _ui.wheelTypeText.text = $"{label}";

        var config = GameManager.Instance.GetCurrentWheelConfig();
        if (config != null && config.backgroundMusic != null)
            GameServices.Instance.AudioManager?.PlayMusic(config.backgroundMusic);

        if (_ui.superZoneIndicator != null)
            _ui.superZoneIndicator.SetActive(true);

        if (_ui.safeZoneValueText != null)
        {
            int next = GameManager.Instance.GetNextSilverZone();
            _ui.safeZoneValueText.text = next > 0 ? next.ToString() : (GameManager.Instance.CurrentZone >= 55 ? "60" : "--");
        }
        if (_ui.superZoneValueText != null)
        {
            int next = GameManager.Instance.GetNextSuperZone();
            _ui.superZoneValueText.text = next > 0 ? next.ToString() : (GameManager.Instance.CurrentZone >= 60 ? "60" : "--");
        }

        AnimateSafeZoneIndicator();
        UpdateMoneyDisplay();
    }

    public void UpdateZoneProgress()
    {
        if (_ui.zoneProgressBar != null)
            _ui.zoneProgressBar.ResetToZone(GameManager.Instance.CurrentZone);
    }

    public void AnimateZoneProgress(int fromZone, int toZone)
    {
        toZone = Mathf.Min(toZone, GameManager.Instance.maxZone);
        if (_ui.zoneProgressBar != null)
            _ui.zoneProgressBar.AnimateToZone(fromZone, toZone);
    }

    public void UpdateMoneyDisplay()
    {
        if (_ui.moneyText != null) _ui.moneyText.text = FormatNumber(PlayerPrefs.GetInt("TotalMoney", 1000));
        if (_ui.coinsText != null) _ui.coinsText.text = FormatNumber(PlayerPrefs.GetInt("TotalCoins", 100));
    }

    private string FormatNumber(int value)
    {
        if (value >= 1000000)
            return (value / 1000000f).ToString("0.0") + "m";
        if (value >= 100000)
            return (value / 1000f).ToString("0.0") + "k";
        return value.ToString("N0").Replace(",", ".");
    }

    public void AnimateExitButton(GameState state)
    {
        if (_ui.exitButton == null) return;
        _ui.exitButton.transform.DOKill();
        _ui.exitButton.image.DOKill();

        if (state == GameState.Spinning)
        {
            _ui.exitButton.interactable = false;
            _ui.exitButton.transform.DOScale(0f, 0.25f).SetEase(Ease.InBack);
            _ui.exitButton.image.DOFade(0f, 0.2f);
        }
        else
        {
            _ui.exitButton.gameObject.SetActive(true);
            _ui.exitButton.image.DOFade(1f, 0.25f);
            _ui.exitButton.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack)
                .OnComplete(() => _ui.exitButton.interactable = true);
        }
    }

    private void AnimateSafeZoneIndicator()
    {
        if (_ui.safeZoneIndicator == null) return;
        _ui.safeZoneIndicator.transform.DOKill();
        bool show = !GameManager.Instance.IsSilverZone();
        if (show)
            _ui.safeZoneIndicator.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        else
            _ui.safeZoneIndicator.transform.DOScale(0f, 0.25f).SetEase(Ease.InBack);
    }

    public void StartWheelTypePulse()
    {
        if (_ui.wheelTypeText == null) return;
        _ui.wheelTypeText.transform.localScale = Vector3.one;
        _ui.wheelTypeText.transform.DOScale(1.1f, 0.9f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    public void StartCharacterIdleAnimation()
    {
        if (_ui.characterEffectImage == null) return;
        _ui.characterEffectImage.transform.localScale = Vector3.one;
        DOTween.Sequence()
            .Append(_ui.characterEffectImage.transform.DOScale(1.03f, 2f).SetEase(Ease.InOutSine))
            .Append(_ui.characterEffectImage.transform.DOScale(1f, 2f).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void UpdateAutoSpinButton(bool active)
    {
        if (_ui.autoSpinButton == null) return;
        var img = _ui.autoSpinButton.GetComponent<Image>();
        if (img == null) return;

        string hex = active ? "#FF5A40" : "#00DFFF";
        string text = active ? "PAUSE" : "AUTO SPIN";

        if (ColorUtility.TryParseHtmlString(hex, out Color c)) img.color = c;
        if (_ui.autoSpinText != null) _ui.autoSpinText.text = $"{text}";
    }
}
