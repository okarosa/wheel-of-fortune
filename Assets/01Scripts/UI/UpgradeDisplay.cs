using UnityEngine;
using TMPro;
using DG.Tweening;

public class UpgradeDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI valueText;
    private bool _hasBeenActivated;

    private void Awake()
    {
        if (valueText == null)
            valueText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void PlayCountUp(int fromValue, int toValue, System.Action onComplete = null, System.Action onTick = null)
    {
        if (valueText == null) return;

        if (!_hasBeenActivated)
        {
            _hasBeenActivated = true;
            gameObject.SetActive(true);
            transform.localScale = Vector3.zero;
            transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        valueText.text = fromValue.ToString("N0");
        int diff = toValue - fromValue;
        if (diff <= 0)
        {
            onComplete?.Invoke();
            return;
        }

        float duration = Mathf.Min(0.5f + diff * 0.02f, 1.5f);
        float interval = duration / diff;

        int soundStep = Mathf.Max(1, diff / 30);
        int tickCount = 0;

        Sequence seq = DOTween.Sequence();
        for (int i = 1; i <= diff; i++)
        {
            int captured = fromValue + i;
            seq.AppendCallback(() => {
                valueText.text = captured.ToString("N0");
                valueText.transform.localScale = Vector3.one;
                valueText.transform.DOScale(1.3f, 0.08f).SetEase(Ease.OutQuad)
                    .OnComplete(() => valueText.transform.DOScale(1f, 0.08f).SetEase(Ease.InQuad));
                tickCount++;
                if (tickCount % soundStep == 0)
                    onTick?.Invoke();
            });
            seq.AppendInterval(interval);
        }
        seq.OnComplete(() => onComplete?.Invoke());
    }

    public void SetValueImmediate(int value)
    {
        if (valueText != null)
            valueText.text = value.ToString("N0");
    }
}
