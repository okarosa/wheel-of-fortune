using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class ZoneProgressBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform contentParent;
    [SerializeField] private Image currentZoneFillImage;
    [SerializeField] private TextMeshProUGUI currentZoneText;
    [SerializeField] private GameObject zoneIndicatorPrefab;

    [Header("Settings")]
    [SerializeField] private Color bronzeColor = new Color(0.58f, 0.337f, 0.11f);
    [SerializeField] private Color silverColor = Color.green;
    [SerializeField] private Color goldColor = new Color(1f, 0.76f, 0.055f);
    [SerializeField] private float fillDuration = 0.5f;
    [SerializeField] private float scrollDuration = 0.4f;
    [SerializeField] private float scrollOffset = 100f;
    [SerializeField] private int maxIndicators = 12;
    [SerializeField] private float initialContentX = 309.3f;

    private List<TextMeshProUGUI> _zoneTexts = new List<TextMeshProUGUI>();
    private bool _isInitialized;

    private void Awake()
    {
        GameServices.Instance?.Register(this);
        FindReferences();
    }

    private void FindReferences()
    {
        var topbar = GameObject.Find("ui_topbar");
        if (topbar == null) return;

        if (contentParent == null)
        {
            var content = GameObject.Find("ui_topbar_content");
            if (content != null) contentParent = content.GetComponent<RectTransform>();
        }

        if (currentZoneFillImage == null && topbar != null)
        {
            var currentZone = topbar.transform.Find("ui_image_indicator_currentzone");
            if (currentZone != null)
            {
                var fill = currentZone.transform.Find("Image");
                if (fill != null) currentZoneFillImage = fill.GetComponent<Image>();
                currentZoneText = currentZone.GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        if (currentZoneFillImage != null)
        {
            currentZoneFillImage.type = Image.Type.Filled;
            currentZoneFillImage.fillMethod = Image.FillMethod.Horizontal;
            currentZoneFillImage.fillAmount = 1f;
        }

        if (contentParent != null)
        {
            for (int i = 0; i < contentParent.childCount; i++)
            {
                var child = contentParent.GetChild(i);
                if (child.name == "ui_rect_indicator_zone")
                {
                    zoneIndicatorPrefab = child.gameObject;
                    break;
                }
            }
        }
    }

    public void Initialize()
    {
        if (_isInitialized) return;
        if (contentParent == null) return;

        if (!zoneIndicatorPrefab)
        {
            for (int i = 0; i < contentParent.childCount; i++)
            {
                var child = contentParent.GetChild(i);
                if (child.name == "ui_rect_indicator_zone")
                {
                    zoneIndicatorPrefab = child.gameObject;
                    break;
                }
            }
        }

        if (!zoneIndicatorPrefab) return;

        _zoneTexts.Clear();

        var template = Instantiate(zoneIndicatorPrefab, contentParent);
        template.name = "ui_rect_indicator_zone";

        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            var child = contentParent.GetChild(i);
            if (child.gameObject != template && child.name == "ui_rect_indicator_zone")
                DestroyImmediate(child.gameObject);
        }

        var text = template.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null) _zoneTexts.Add(text);

        for (int i = 1; i < maxIndicators; i++)
        {
            var go = Instantiate(template, contentParent);
            go.name = "ui_rect_indicator_zone";
            var t = go.GetComponentInChildren<TextMeshProUGUI>();
            if (t != null) _zoneTexts.Add(t);
        }

        zoneIndicatorPrefab = template;
        _isInitialized = true;

        var pos = contentParent.anchoredPosition;
        pos.x = initialContentX;
        contentParent.anchoredPosition = pos;

        int startZone = GameManager.Instance != null ? GameManager.Instance.CurrentZone : 1;
        SetupForZone(startZone);
    }

    public void SetupForZone(int zone)
    {
        for (int i = 0; i < _zoneTexts.Count; i++)
        {
            int zoneNum = zone + i;
            _zoneTexts[i].text = zoneNum <= 60 ? zoneNum.ToString() : "";
            _zoneTexts[i].color = GetZoneColor(zoneNum);
        }

        if (currentZoneText != null)
        {
            currentZoneText.text = zone.ToString();
            currentZoneText.color = GetZoneColor(zone);
        }

        if (currentZoneFillImage != null)
        {
            currentZoneFillImage.fillAmount = 1f;
            currentZoneFillImage.color = GetZoneFillColor(zone);
        }
    }

    public void AnimateToZone(int fromZone, int toZone)
    {
        if (currentZoneFillImage != null)
        {
            currentZoneFillImage.color = GetZoneFillColor(toZone);
            currentZoneFillImage.fillAmount = 0f;
        }

        contentParent.DOKill();

        Sequence seq = DOTween.Sequence();

        if (currentZoneFillImage != null)
            seq.Append(currentZoneFillImage.DOFillAmount(1f, fillDuration).SetEase(Ease.Linear));

        if (contentParent != null)
        {
            float targetX = contentParent.anchoredPosition.x - scrollOffset;
            seq.Join(contentParent.DOAnchorPosX(targetX, scrollDuration).SetEase(Ease.OutCubic));
        }

        if (currentZoneText != null)
        {
            seq.Join(currentZoneText.transform.DOScale(1.2f, fillDuration * 0.5f).SetEase(Ease.OutQuad));
            seq.Join(currentZoneText.DOFade(0f, fillDuration * 0.5f));
            seq.AppendCallback(() => {
                currentZoneText.text = toZone.ToString();
                currentZoneText.transform.localScale = Vector3.one;
                currentZoneText.color = GetZoneColor(toZone);
                currentZoneText.DOFade(1f, 0.2f);
            });
        }

    }


    public void ResetToZone(int zone)
    {
        if (!_isInitialized)
        {
            Initialize();
            return;
        }

        contentParent.DOKill();

        var pos = contentParent.anchoredPosition;
        pos.x = initialContentX;
        contentParent.anchoredPosition = pos;

        SetupForZone(zone);
    }

    private Color GetZoneColor(int zone)
    {
        if (zone == 30 || zone == 60)
            return goldColor;
        if (zone % 5 == 0)
            return silverColor;
        return bronzeColor;
    }

    private Color GetZoneFillColor(int zone)
    {
        if (zone == 30 || zone == 60)
            return new Color(1f, 0.76f, 0.055f);
        if (zone % 5 == 0)
            return Color.green;
        return Color.white;
    }
}
