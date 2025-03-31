using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    [Header("基本信息")]
    public string cardTitle;
    public string description;
    public Card.CardType cardType;
    public Card.CardActionType actionType;
    public int value;
    [Tooltip("可选的卡片图片")]
    public Sprite cardImage;
}
