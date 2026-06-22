using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WheelSliceUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;

    private void Awake()
    {
        if (iconImage == null)
            iconImage = transform.Find("ui_image_item_value")?.GetComponent<Image>();
        if (amountText == null)
            amountText = transform.Find("ui_text_item_value")?.GetComponent<TextMeshProUGUI>();
    }

    public void SetSliceData(WheelSliceData data)
    {
        if (iconImage != null)
        {
            if (data.icon != null)
            {
                iconImage.sprite = data.icon;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        if (amountText != null)
        {
            if (data.isBomb)
            {
                amountText.text = "";
                amountText.gameObject.SetActive(false);
            }
            else
            {
                amountText.text = "x" + data.rewardAmount.ToString();
                amountText.gameObject.SetActive(true);
            }
        }
    }
}
