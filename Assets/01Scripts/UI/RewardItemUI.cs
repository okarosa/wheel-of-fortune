using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class RewardItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;

    private CanvasGroup _canvasGroup;
    private string _rewardName;
    private int _totalAmount;

    public string RewardName => _rewardName;
    public int TotalAmount => _totalAmount;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private bool IsCaseItem()
    {
        return !string.IsNullOrEmpty(_rewardName) && _rewardName.StartsWith("case_", System.StringComparison.OrdinalIgnoreCase);
    }

    private string FormatAmount(int value)
    {
        string formatted = value.ToString("N0").Replace(",", ".");
        return IsCaseItem() ? "x" + formatted : formatted;
    }

    public void SetReward(WheelSliceData reward)
    {
        _rewardName = reward.rewardName;
        _totalAmount = reward.rewardAmount;
        if (iconImage != null)
            iconImage.sprite = reward.icon;
        if (amountText != null)
            amountText.text = FormatAmount(_totalAmount);
    }

    public void AddAmount(int amount)
    {
        _totalAmount += amount;
        if (amountText != null)
            amountText.text = FormatAmount(_totalAmount);
    }

    public bool HasName(string name)
    {
        return string.Equals(_rewardName, name, System.StringComparison.OrdinalIgnoreCase);
    }

    public void PlayAppearAnimation(float delay = 0f)
    {
        if (_canvasGroup == null) return;

        transform.localScale = Vector3.zero;
        _canvasGroup.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(delay);
        seq.Append(transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
        seq.Join(_canvasGroup.DOFade(1f, 0.2f));
    }

    public void PlayMoveToTopAnimation()
    {
        transform.DOKill();
        transform.SetAsFirstSibling();
        transform.DOScale(1.05f, 0.15f).SetEase(Ease.OutQuad).OnComplete(() =>
            transform.DOScale(1f, 0.15f).SetEase(Ease.InQuad));
    }

    public void SetIcon(Sprite sprite)
    {
        if (iconImage != null)
            iconImage.sprite = sprite;
    }

    public void SetAmountText(int value)
    {
        if (amountText != null)
            amountText.text = FormatAmount(value);
    }

    public void PlayCountUp(int fromValue, int toValue, System.Action onComplete = null)
    {
        if (amountText == null)
        {
            onComplete?.Invoke();
            return;
        }

        amountText.text = FormatAmount(fromValue);
        int diff = toValue - fromValue;
        if (diff <= 0)
        {
            onComplete?.Invoke();
            return;
        }

        float duration = Mathf.Min(0.5f + diff * 0.02f, 1.5f);
        int soundStep = Mathf.Max(1, diff / 15);
        int lastTick = -1;

        DOVirtual.Int(fromValue, toValue, duration, val =>
        {
            amountText.text = FormatAmount(val);
            int tick = (val - fromValue) / soundStep;
            if (tick > lastTick)
            {
                lastTick = tick;
                AudioManager.Instance.Play("sfx_coin_collect");
            }
        }).SetEase(Ease.OutQuad).OnComplete(() => onComplete?.Invoke());
    }
}
