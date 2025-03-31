using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }
    
    [Header("卡片资源")]
    public CardData[] chanceCardDataAssets;
    public CardData[] communityCardDataAssets;
    
    // 运行时使用的卡片列表
    [HideInInspector]
    public List<Card> chanceCards = new List<Card>();
    [HideInInspector]
    public List<Card> communityCards = new List<Card>();
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
            
        // 从资源加载卡片
        LoadCardsFromAssets();
    }
    
    private void LoadCardsFromAssets()
    {
        // 清空列表
        chanceCards.Clear();
        communityCards.Clear();
        
        // 加载机会卡
        if (chanceCardDataAssets != null)
        {
            foreach (CardData data in chanceCardDataAssets)
            {
                Card card = new Card
                {
                    type = data.cardType,
                    title = data.cardTitle,
                    description = data.description,
                    actionType = data.actionType,
                    value = data.value
                };
                chanceCards.Add(card);
            }
        }
        
        // 加载命运卡
        if (communityCardDataAssets != null)
        {
            foreach (CardData data in communityCardDataAssets)
            {
                Card card = new Card
                {
                    type = data.cardType,
                    title = data.cardTitle,
                    description = data.description,
                    actionType = data.actionType,
                    value = data.value
                };
                communityCards.Add(card);
            }
        }
        
        // 打乱卡片顺序
        ShuffleCards();
        
        Debug.Log($"加载了 {chanceCards.Count} 张机会卡和 {communityCards.Count} 张命运卡");
    }
    
    // 洗牌
    public void ShuffleCards()
    {
        ShuffleList(chanceCards);
        ShuffleList(communityCards);
    }
    
    // 打乱列表
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            int r = i + Random.Range(0, n - i);
            T temp = list[i];
            list[i] = list[r];
            list[r] = temp;
        }
    }
    
    // 抽取机会卡
    public Card DrawChanceCard()
    {
        if (chanceCards.Count == 0) return null;
        
        Card card = chanceCards[0];
        chanceCards.RemoveAt(0);
        chanceCards.Add(card);  // 放回底部
        return card;
    }
    
    // 抽取命运卡
    public Card DrawCommunityCard()
    {
        if (communityCards.Count == 0) return null;
        
        Card card = communityCards[0];
        communityCards.RemoveAt(0);
        communityCards.Add(card);  // 放回底部
        return card;
    }
    
    // 执行卡片效果
    public void ExecuteCard(Card card, int playerIndex)
    {
        if (card == null) return;
        
        CharacterMovement player = GameManager.Instance.GetPlayer(playerIndex);
        if (player == null) return;
        
        // 先显示卡片
        UIManager.Instance.ShowCardUI(card);
        
        // 执行卡片效果
        switch (card.actionType)
        {
            case Card.CardActionType.CollectMoney:
                Bank.Instance.BankPayPlayer(playerIndex, card.value);
                break;
                
            case Card.CardActionType.PayMoney:
                Bank.Instance.PlayerPayBank(playerIndex, card.value);
                break;
                
            case Card.CardActionType.MoveToPrison:
                // 移动到监狱的逻辑
                TurnManager.Instance.MovePlayerToPrison(playerIndex);
                break;
                
            case Card.CardActionType.Birthday:
                // 所有其他玩家给当前玩家钱
                int playerCount = TurnManager.Instance.players.Count;
                for (int i = 0; i < playerCount; i++)
                {
                    if (i != playerIndex)
                    {
                        Bank.Instance.TransferMoney(i, playerIndex, card.value);
                    }
                }
                break;
                
            // 
        }
    }

    public int GetPlayerCount()
    {
        // 使用TurnManager中的players列表长度
        return TurnManager.Instance.players.Count;
    }

    // 运行时使用卡片数据创建卡片实例
    private void Start()
    {
        // 如果需要在Start中再次加载卡片，使用已有方法
        LoadCardsFromAssets();
    }
}
