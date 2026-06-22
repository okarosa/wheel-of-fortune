using UnityEngine;
using DG.Tweening;

public class BombEffect : MonoBehaviour
{
    public static BombEffect Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RectTransform bombIcon;
    [SerializeField] private CanvasGroup bombCanvasGroup;
    [SerializeField] private RectTransform shakeTarget;

    [Header("Settings")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeStrength = 20f;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float scaleDuration = 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        GameServices.Instance?.Register(this);

        if (bombCanvasGroup == null)
            bombCanvasGroup = GetComponent<CanvasGroup>();
    }

    public void PlayBombEffect()
    {
        if (bombCanvasGroup != null)
        {
            bombCanvasGroup.alpha = 0f;
            bombCanvasGroup.DOFade(1f, fadeInDuration);
        }

        if (bombIcon != null)
        {
            bombIcon.localScale = Vector3.zero;
            bombIcon.DOScale(1f, scaleDuration).SetEase(Ease.OutBack);
        }

        if (shakeTarget != null)
        {
            shakeTarget.DOShakePosition(shakeDuration, shakeStrength, 20, 90f, false, true);
        }

        if (bombCanvasGroup != null)
        {
            bombCanvasGroup.transform.DOShakePosition(shakeDuration * 0.5f, new Vector3(5f, 5f, 0f), 10, 0f, false, true)
                .SetDelay(shakeDuration * 0.5f);
        }
    }
}
