using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple UI component for a single roguelike card.
/// Displays BuffData and notifies the manager when clicked.
/// </summary>
public class RoguelikeCardView : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI descriptionText;
    public Button button;

    [HideInInspector]
    public BuffData boundBuff;

    public System.Action<RoguelikeCardView> onClicked;

    private void Awake()
    {
        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }
    }

    public void Bind(BuffData buff)
    {
        boundBuff = buff;

        if (buff == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (iconImage != null) iconImage.sprite = buff.icon;
        if (nameText != null) nameText.text = buff.displayName;
        if (rarityText != null) rarityText.text = buff.rarity.ToString();
        if (descriptionText != null) descriptionText.text = buff.description;
    }

    public void SetFocused(bool focused)
    {
        // Very simple MVP: scale up when focused.
        transform.localScale = focused ? Vector3.one * 1.1f : Vector3.one;
    }

    private void HandleClick()
    {
        onClicked?.Invoke(this);
    }
}