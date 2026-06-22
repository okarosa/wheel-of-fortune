using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class RewardFeedHandler
{
    private UIManager _ui;
    private Canvas _canvas;

    private Dictionary<string, Sprite> _weaponIcons;
    private static readonly HashSet<string> _weaponNames = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        { "rifle", "armor", "knife", "pistol", "submachine", "shotgun", "sniper" };

    public RewardFeedHandler(UIManager ui, Canvas canvas)
    {
        _ui = ui;
        _canvas = canvas;

        _weaponIcons = new Dictionary<string, Sprite>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "armor",      _ui.armorIcon      },
            { "knife",      _ui.knifeIcon      },
            { "rifle",      _ui.rifleIcon      },
            { "pistol",     _ui.pistolIcon     },
            { "submachine", _ui.submachineIcon },
            { "shotgun",    _ui.shotgunIcon    },
            { "sniper",     _ui.sniperIcon     },
        };
    }

    public void AddRewardItem(WheelSliceData reward)
    {
        GameServices.Instance.AudioManager?.Play("sfx_item_reward");
        if (_ui.rewardItemPrefab == null || _ui.rewardContentParent == null) return;
        GameServices.Instance.AudioManager?.Play("sfx_item_collect");

        RewardItemUI targetItem = FindExistingRewardItem(reward.rewardName);

        if (targetItem != null)
        {
            UpdateExistingRewardItem(targetItem, reward);
        }
        else
        {
            CreateNewRewardItem(reward);
        }

        GameServices.Instance.HUD?.UpdateMoneyDisplay();
    }

    private RewardItemUI FindExistingRewardItem(string rewardName)
    {
        for (int i = 0; i < _ui.rewardContentParent.childCount; i++)
        {
            var ui = _ui.rewardContentParent.GetChild(i).GetComponent<RewardItemUI>();
            if (ui != null && ui.HasName(rewardName)) return ui;
        }
        return null;
    }

    private void UpdateExistingRewardItem(RewardItemUI targetItem, WheelSliceData reward)
    {
        int oldValue = targetItem.TotalAmount;
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

        GameObject itemGO = Object.Instantiate(_ui.rewardItemPrefab, _ui.rewardContentParent);
        itemGO.transform.SetAsFirstSibling();

        var targetItem = itemGO.GetComponent<RewardItemUI>();
        if (targetItem == null) return;

        WheelSliceData displayData = ScriptableObject.CreateInstance<WheelSliceData>();
        displayData.rewardName = reward.rewardName;
        displayData.rewardAmount = addAmount;
        displayData.icon = reward.icon;

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
        if (_ui.rewardScrollRect == null) return;
        Canvas.ForceUpdateCanvases();
        DOTween.To(
            () => _ui.rewardScrollRect.verticalNormalizedPosition,
            x => _ui.rewardScrollRect.verticalNormalizedPosition = x,
            1f, 0.35f).SetEase(Ease.OutCubic);
    }

    public void SpawnCoinFlyEffect(System.Action onComplete = null)
    {
        if (_canvas == null || _ui.reviveCoinButton == null) { onComplete?.Invoke(); return; }

        Transform target = _ui.coinsImage != null ? _ui.coinsImage.transform
                         : _ui.coinsText != null ? _ui.coinsText.transform : null;
        if (target == null) { onComplete?.Invoke(); return; }

        Vector3 targetPos = target.position;
        if (_ui.coinsImage != null)
        {
            Vector3[] corners = new Vector3[4];
            _ui.coinsImage.rectTransform.GetWorldCorners(corners);
            targetPos = (corners[0] + corners[2]) * 0.5f;
        }

        Vector3 startPos = _ui.reviveCoinButton.transform.position;
        int remaining = 5;

        for (int i = 0; i < 5; i++)
        {
            GameObject coin = new GameObject("fly_coin", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            coin.transform.SetParent(_canvas.transform, false);

            var rt = coin.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(40, 40);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.position = startPos + new Vector3(Random.Range(-30f, 30f), Random.Range(-20f, 20f), 0f);
            rt.localScale = Vector3.zero;

            var img = coin.GetComponent<Image>();
            if (_ui.coinFlySprite != null) img.sprite = _ui.coinFlySprite;
            img.color = Color.yellow;
            img.raycastTarget = false;

            float delay = i * 0.05f;
            rt.DOScale(1f, 0.2f).SetDelay(delay).SetEase(Ease.OutBack);
            rt.DOMove(targetPos, 0.5f).SetDelay(delay + 0.15f).SetEase(Ease.InBack);

            Sequence flySeq = DOTween.Sequence();
            flySeq.Insert(delay + 0.15f, rt.DOScale(0.2f, 0.5f).SetEase(Ease.InBack));
            flySeq.OnComplete(() =>
            {
                Object.Destroy(coin);
                remaining--;
                if (remaining <= 0)
                {
                    if (_ui.coinsImage != null)
                    {
                        _ui.coinsImage.transform.DOKill();
                        _ui.coinsImage.transform.DOScale(1.3f, 0.15f).SetEase(Ease.OutBack)
                            .OnComplete(() => _ui.coinsImage.transform.DOScale(1f, 0.2f).SetEase(Ease.InBack));
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
        iconRT.sizeDelta = new Vector2(80, 80);
        iconRT.anchorMin = iconRT.anchorMax = new Vector2(0.5f, 0.5f);
        iconRT.anchoredPosition = Vector2.zero;
        iconRT.localScale = Vector3.zero;

        var iconImage = iconGO.GetComponent<Image>();
        iconImage.sprite = iconSprite;
        iconImage.raycastTarget = false;

        iconRT.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack);
        iconGO.transform.DOMove(targetPosition, 0.7f).SetDelay(0.3f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                iconRT.DOScale(0f, 0.15f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    Object.Destroy(iconGO);
                    onComplete?.Invoke();
                });
            });
        iconGO.transform.DORotate(new Vector3(0, 0, 360), 0.7f, RotateMode.LocalAxisAdd).SetDelay(0.3f).SetEase(Ease.Linear);
    }

    private int GetDisplayAmount(WheelSliceData reward)
    {
        return reward.rewardAmount;
    }

    private Sprite GetWeaponIcon(string name)
    {
        return _weaponIcons.TryGetValue(name, out var sprite) ? sprite : null;
    }

    public void ClearRewardItems()
    {
        if (_ui.rewardContentParent == null) return;
        for (int i = _ui.rewardContentParent.childCount - 1; i >= 0; i--)
            Object.Destroy(_ui.rewardContentParent.GetChild(i).gameObject);
    }
}
