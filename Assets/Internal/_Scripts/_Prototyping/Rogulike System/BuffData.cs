using UnityEngine;

public enum BuffRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

/// <summary>
/// ScriptableObject definition for a single buff.
/// Stored in a global list on RoguelikeCardManager for now.
/// </summary>
[CreateAssetMenu(menuName = "Roguelike/Buff Data", fileName = "BuffData")]
public class BuffData : ScriptableObject
{
    [Header("Identity")]
    public string buffId;
    public string displayName;

    [TextArea]
    public string description;

    public Sprite icon;
    public BuffRarity rarity;

    [Header("Effect (MVP)")]
    [Tooltip("Name/identifier of the buff effect. For now it's just data, not implemented.")]
    public string buffEffectId;

    [Tooltip("Numeric value for the buff (e.g. +0.2 for +20% tickets).")]
    public float buffValue = 1f;
}