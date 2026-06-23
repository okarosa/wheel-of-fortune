using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;


public class PanelController
{
    private UIManager _ui;
    private int _reviveCost = 25;

    public PanelController(UIManager ui)
    {
        _ui = ui;
    }

    public void ShowBombPanel()
    {
        GameServices.Instance.AudioManager?.Play("sfx_game_over");
        GameServices.Instance.HUD?.UpdateMoneyDisplay();
        UpdateReviveButtonText();

        GameObject target = _ui.bombPanel != null ? _ui.bombPanel : _ui.gameOverPanel;
        if (target != null)
        {
            target.SetActive(true);
            target.transform.localScale = Vector3.zero;
            target.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
        }
    }

    public void ShowBuyCoinPanel()
    {
        GameServices.Instance.HUD?.UpdateMoneyDisplay();
        if (_ui.coinAmountText != null)
            _ui.coinAmountText.text = FormatNumber(PlayerPrefs.GetInt("TotalCoins", 100));

        if (_ui.buyCoinPanel == null) return;

        if (_ui.gameOverPanel != null)
        {
            _ui.gameOverPanel.transform.DOKill();
            _ui.gameOverPanel.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack)
                .OnComplete(() => _ui.gameOverPanel.SetActive(false));
        }

        _ui.buyCoinPanel.SetActive(true);
        _ui.buyCoinPanel.transform.localScale = Vector3.zero;
        _ui.buyCoinPanel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
    }

    public void HideBuyCoinPanel()
    {
        if (_ui.buyCoinPanel == null) return;
        _ui.buyCoinPanel.transform.DOKill();
        _ui.buyCoinPanel.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack)
            .OnComplete(() => _ui.buyCoinPanel.SetActive(false));
    }

    public void HideBombPanel()
    {
        if (_ui.bombPanel != null) _ui.bombPanel.SetActive(false);
        if (_ui.gameOverPanel != null) _ui.gameOverPanel.SetActive(false);
    }

    public void ShowRewardComplete()
    {
        if (_ui.rewardCompletePanel == null) return;
        _ui.rewardCompletePanel.SetActive(true);
        _ui.rewardCompletePanel.transform.localScale = Vector3.zero;
        _ui.rewardCompletePanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
        var rect = _ui.rewardCompletePanel.GetComponent<RectTransform>();
        if (rect != null)
            rect.DOAnchorPos(Vector2.zero, 0.4f).From(new Vector2(0f, 200f)).SetEase(Ease.OutCubic);
    }

    private string FormatNumber(int value)
    {
        if (value >= 1000000)
            return (value / 1000000f).ToString("0.0") + "m";
        if (value >= 100000)
            return (value / 1000f).ToString("0.0") + "k";
        return value.ToString("N0").Replace(",", ".");
    }

    private void UpdateReviveButtonText()
    {
        if (_ui.reviveCoinCostText != null)
            _ui.reviveCoinCostText.text = $"{_reviveCost.ToString()}";
    }

    private void ResetWheel()
    {
        var wc = GameServices.Instance.WheelController;
        if (wc != null) wc.SetupWheel();
    }

    private void ClearRewardItems()
    {
        GameServices.Instance.RewardFeed?.ClearRewardItems();
    }

    public void OnExitClicked()
    {
        GameServices.Instance.RewardCollector?.CashOut();
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnGiveUpClicked()
    {
        GameServices.Instance.RewardCollector?.ResetAll();
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnReviveCoinClicked()
    {
        if (_ui.reviveCoinButton != null) _ui.reviveCoinButton.interactable = false;

        int savedCoins = PlayerPrefs.GetInt("TotalCoins", 100);
        if (savedCoins >= _reviveCost)
        {
            GameServices.Instance.AudioManager?.Play("sfx_coin_collect");
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
                if (_ui.reviveCoinButton != null) _ui.reviveCoinButton.interactable = true;
            };

            if (_ui.coinsText != null)
            {
                _ui.coinsText.text = FormatNumber(savedCoins);
                DOTween.To(() => (float)savedCoins, x =>
                {
                    _ui.coinsText.text = FormatNumber(Mathf.RoundToInt(x));
                }, (float)targetCoins, 0.5f).SetEase(Ease.OutQuad);
            }

            GameServices.Instance.RewardFeed?.SpawnCoinFlyEffect(finish);
        }
        else
        {
            ShowBuyCoinPanel();
            if (_ui.reviveCoinButton != null) _ui.reviveCoinButton.interactable = true;
        }
    }

    public void OnReviveAdsClicked()
    {
        HideBombPanel();
        GameManager.Instance.SetState(GameState.Idle);
        ResetWheel();
    }

    public void OnRestartClicked()
    {
        GameServices.Instance.RewardCollector?.CashOut();
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnBuyCoinClicked(int index)
    {
        int coinReward = 0;
        if (_ui.buyCoinConfig != null && _ui.buyCoinConfig.options != null && index < _ui.buyCoinConfig.options.Length)
            coinReward = _ui.buyCoinConfig.options[index].coinReward;
        else
        {
            int[] defaults = { 200, 1000, 2500 };
            if (index < defaults.Length) coinReward = defaults[index];
            else return;
        }

        int savedCoins = PlayerPrefs.GetInt("TotalCoins", 100);
        int newCoins = savedCoins + coinReward;
        PlayerPrefs.SetInt("TotalCoins", newCoins);
        PlayerPrefs.Save();

        GameServices.Instance.AudioManager?.Play("sfx_coin_collect");

        if (_ui.coinsText != null)
        {
            DOTween.To(() => savedCoins, x =>
            {
                int val = Mathf.RoundToInt(x);
                _ui.coinsText.text = FormatNumber(val);
                if (_ui.coinAmountText != null) _ui.coinAmountText.text = FormatNumber(val);
            }, newCoins, 0.5f).SetEase(Ease.OutQuad);
        }
        else if (_ui.coinAmountText != null)
        {
            DOTween.To(() => savedCoins, x =>
            {
                _ui.coinAmountText.text = FormatNumber(Mathf.RoundToInt(x));
            }, newCoins, 0.5f).SetEase(Ease.OutQuad);
        }
    }

    public void OnNoThanksClicked()
    {
        if (_ui.buyCoinPanel == null) return;
        _ui.buyCoinPanel.transform.DOKill();
        _ui.buyCoinPanel.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                _ui.buyCoinPanel.SetActive(false);
                if (_ui.gameOverPanel != null)
                {
                    _ui.gameOverPanel.SetActive(true);
                    _ui.gameOverPanel.transform.localScale = Vector3.zero;
                    _ui.gameOverPanel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
                }
            });
    }
}
