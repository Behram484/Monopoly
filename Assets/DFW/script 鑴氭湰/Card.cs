using UnityEngine;

[System.Serializable]
public class Card
{
    public enum CardType { Chance, Community }
    
    public CardType type;        // 卡片类型：机会卡或命运卡
    public string title;         // 卡片标题
    public string description;   // 卡片描述
    public CardActionType actionType;  // 卡片动作类型
    public int value;            // 相关数值（例如金钱数量）
    
    // 卡片动作类型枚举
    public enum CardActionType
    {
        CollectMoney,       // 获得金钱
        PayMoney,           // 支付金钱
        MoveToLocation,     // 移动到指定位置
        MoveToPrison,       // 进监狱
        GetOutOfPrison,     // 出狱卡
        MoveBack,           // 后退几步
        PayPerHouse,        // 房屋税
        Birthday,           // 生日（其他玩家付钱给你）
        Repair,             // 修缮费
        MoveForward         // 添加这个值
    }
}
