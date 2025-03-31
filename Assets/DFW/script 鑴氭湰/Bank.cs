using UnityEngine;

public class Bank : MonoBehaviour
{
    [Header("设置")]
    public int initialPlayerMoney = 0; // 初始资金
    public int passStartBonus = 200;      // 经过起点奖励
    
    // 单例实例
    public static Bank Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    // 资金转移（从玩家到玩家）
    public bool TransferMoney(int fromPlayerIndex, int toPlayerIndex, int amount)
    {
        if (amount <= 0) return false;
        
        CharacterMovement fromPlayer = GameManager.Instance.GetPlayer(fromPlayerIndex);
        CharacterMovement toPlayer = GameManager.Instance.GetPlayer(toPlayerIndex);
        
        if (fromPlayer == null || toPlayer == null) return false;
        
        // 检查付款方是否有足够的钱
        if (fromPlayer.money < amount) return false;
        
        // 转移资金
        fromPlayer.money -= amount;
        toPlayer.money += amount;
        
        // 更新UI
        UIManager.Instance.UpdatePlayerMoney(fromPlayerIndex, fromPlayer.money);
        UIManager.Instance.UpdatePlayerMoney(toPlayerIndex, toPlayer.money);
        
        // 显示交易消息
        string message = $"玩家{fromPlayerIndex+1}支付{amount}给玩家{toPlayerIndex+1}";
        UIManager.Instance.ShowEventMessage(message);
        
        return true;
    }
    
    // 玩家付款给银行
    public bool PlayerPayBank(int playerIndex, int amount)
    {
        if (amount <= 0) return false;
        
        CharacterMovement player = GameManager.Instance.GetPlayer(playerIndex);
        if (player == null) return false;
        
        // 检查玩家是否有足够的钱
        if (player.money < amount)
        {
            // 资金不足，可能触发破产
            HandleBankruptcy(playerIndex);
            return false;
        }
        
        // 扣减资金
        player.money -= amount;
        
        // 更新UI
        UIManager.Instance.UpdatePlayerMoney(playerIndex, player.money);
        
        // 显示消息
        string message = $"玩家{playerIndex+1}支付{amount}给银行";
        UIManager.Instance.ShowEventMessage(message);
        
        return true;
    }
    
    // 银行付款给玩家
    public void BankPayPlayer(int playerIndex, int amount)
    {
        if (amount <= 0) return;
        
        CharacterMovement player = GameManager.Instance.GetPlayer(playerIndex);
        if (player == null) return;
        
        // 增加资金
        player.money += amount;
        
        // 更新UI
        UIManager.Instance.UpdatePlayerMoney(playerIndex, player.money);
        
        // 显示消息
        string message = $"银行支付{amount}给玩家{playerIndex+1}";
        UIManager.Instance.ShowEventMessage(message);
    }
    
    // 抵押地产
    public void MortgageLand(Land land, int playerIndex)
    {
        if (land == null || land.ownerIndex != playerIndex || land.isMortgaged) return;
        
        // 计算抵押价值（通常是购买价的一半）
        int mortgageValue = land.purchasePrice / 2;
        
        // 设置抵押状态
        land.isMortgaged = true;
        
        // 银行支付抵押金给玩家
        BankPayPlayer(playerIndex, mortgageValue);
        
        // 显示消息
        string message = $"玩家{playerIndex+1}抵押了{land.landName}，获得{mortgageValue}";
        UIManager.Instance.ShowEventMessage(message);
        
        // 更新地块外观
        land.UpdateAppearance();
    }
    
    // 解除抵押
    public bool UnmortgageLand(Land land, int playerIndex)
    {
        if (land == null || land.ownerIndex != playerIndex || !land.isMortgaged) return false;
        
        // 计算解除抵押价值（抵押价值 + 10%利息）
        int mortgageValue = land.purchasePrice / 2;
        int unmortgageCost = (int)(mortgageValue * 1.1f);
        
        // 检查玩家是否有足够的钱
        CharacterMovement player = GameManager.Instance.GetPlayer(playerIndex);
        if (player == null || player.money < unmortgageCost) return false;
        
        // 玩家支付费用给银行
        if (!PlayerPayBank(playerIndex, unmortgageCost)) return false;
        
        // 解除抵押状态
        land.isMortgaged = false;
        
        // 更新地块外观
        land.UpdateAppearance();
        
        // 显示消息
        string message = $"玩家{playerIndex+1}解除了{land.landName}的抵押，支付{unmortgageCost}";
        UIManager.Instance.ShowEventMessage(message);
        
        return true;
    }
    
    // 处理破产
    public void HandleBankruptcy(int playerIndex)
    {
        CharacterMovement player = GameManager.Instance.GetPlayer(playerIndex);
        if (player == null) return;
        
        // 显示破产消息
        string message = $"玩家{playerIndex+1}破产了!";
        UIManager.Instance.ShowEventMessage(message);
        
        // 1. 释放所有地产
        GameManager.Instance.ReleaseAllLands(playerIndex);
        
        // 2. 重置玩家资金
        player.money = 0;
        
        // 3. 更新UI
        UIManager.Instance.UpdatePlayerMoney(playerIndex, 0);
        
        // 4. 可选：将玩家标记为破产状态或移出游戏
        player.isBankrupt = true;
    }
    
    // 给玩家初始资金
    public void GiveInitialMoney(int playerIndex)
    {
        CharacterMovement player = GameManager.Instance.GetPlayer(playerIndex);
        if (player == null) return;
        
        player.money = initialPlayerMoney;
        UIManager.Instance.UpdatePlayerMoney(playerIndex, player.money);
    }
    
    // 玩家经过起点奖励
    public void GivePassStartBonus(int playerIndex)
    {
        BankPayPlayer(playerIndex, passStartBonus);
        
        string message = $"玩家{playerIndex+1}经过起点，获得{passStartBonus}";
        UIManager.Instance.ShowEventMessage(message);
    }
}
