using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    // Assigned by the manager when it instantiates the card
    public System.Action<RoguelikeCardView> onClicked;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(HandleClick);
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
        transform.localScale = focused ? Vector3.one * 1.1f : Vector3.one;
    }

    private void HandleClick()
    {
        onClicked?.Invoke(this);
    }
}