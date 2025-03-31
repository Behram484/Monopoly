using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // 单例模式
    public static GameManager Instance { get; private set; }
    
    // 玩家列表 - 将Player改为CharacterMovement
    [SerializeField] private List<CharacterMovement> playerList = new List<CharacterMovement>();
    
    // 存储玩家拥有的地块
    private Dictionary<int, List<Land>> playerOwnedLands = new Dictionary<int, List<Land>>();

    private UIManager uiManager;

    private void Awake()
    {
        // 单例设置
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("GameManager启动!");  
        
        uiManager = FindAnyObjectByType<UIManager>();
        
        // 确保玩家列表已初始化
        InitializePlayers();

        if (uiManager != null)
        {
            uiManager.InitializePlayerMoneyUI(playerList.Count);
            
            // 初始化每个玩家的金钱显示
            for (int i = 0; i < playerList.Count; i++)
            {
                CharacterMovement player = GetPlayer(i);
                if (player != null)
                {
                    uiManager.UpdatePlayerMoney(i, player.money);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // 初始化玩家列表
    private void InitializePlayers()
    {
        // 如果场景中已经有CharacterMovement对象但没有被添加到列表中
        if (playerList.Count == 0)
        {
            // 查找所有CharacterMovement对象
            CharacterMovement[] playersInScene = FindObjectsByType<CharacterMovement>(FindObjectsSortMode.None);
            
            if (playersInScene.Length > 0)
            {
                playerList.AddRange(playersInScene);
                
                // 初始化玩家地块字典
                for (int i = 0; i < playersInScene.Length; i++)
                {
                    playerOwnedLands[i] = new List<Land>();
                }
                
                Debug.Log($"找到 {playersInScene.Length} 个玩家对象");
            }
            else
            {
                Debug.LogWarning("场景中没有找到CharacterMovement对象");
            }
        }
    }
    
    // 获取玩家对象
    public CharacterMovement GetPlayer(int playerIndex)
    {
        // 检查索引是否有效
        if (playerIndex >= 0 && playerIndex < playerList.Count)
        {
            return playerList[playerIndex];
        }
        
        Debug.LogError($"无效的玩家索引: {playerIndex}");
        return null;
    }

    // 处理购买地块
    public bool PurchaseLand(Land land, int playerIndex)
    {
        CharacterMovement player = GetPlayer(playerIndex);
        
        // 检查条件
        if (player == null)
        {
            Debug.LogError($"找不到索引为 {playerIndex} 的玩家");
            return false;
        }
        
        if (land == null)
        {
            Debug.LogError("尝试购买空地块");
            return false;
        }
        
        if (land.ownerIndex != -1)
        {
            Debug.LogWarning($"地块 {land.landName} 已被玩家 {land.ownerIndex} 拥有");
            if (uiManager != null)
            {
                uiManager.ShowEventMessage($"地块 {land.landName} 已被玩家 {land.ownerIndex + 1} 拥有");
            }
            return false;
        }
        
        if (player.money < land.purchasePrice)
        {
            Debug.LogWarning($"玩家 {playerIndex} 资金不足，无法购买价格为 {land.purchasePrice} 的地块");
            if (uiManager != null)
            {
                uiManager.ShowEventMessage($"资金不足，无法购买 {land.landName}");
            }
            return false;
        }
        
        // 扣除金钱
        player.money -= land.purchasePrice;
        UpdatePlayerMoney(playerIndex, player.money);
        
        // 设置地块所有权
        Color playerColor = GetPlayerColor(player);
        land.PurchaseProperty(playerIndex, playerColor);
        
        // 将地块添加到玩家拥有列表
        if (!playerOwnedLands.ContainsKey(playerIndex))
        {
            playerOwnedLands[playerIndex] = new List<Land>();
        }
        playerOwnedLands[playerIndex].Add(land);
        
        // 购买成功提示
        if (uiManager != null)
        {
            uiManager.ShowEventMessage($"玩家 {playerIndex + 1} 成功购买了 {land.landName}");
        }
        
        Debug.Log($"玩家 {playerIndex} 成功购买了地块 {land.landName}");
        return true;
    }
    
    // 处理过路费支付
    public void PlayerPayToll(int payerIndex, int receiverIndex, int amount, Land land)
    {
        CharacterMovement payer = GetPlayer(payerIndex);
        CharacterMovement receiver = GetPlayer(receiverIndex);
        
        // 检查玩家对象
        if (payer == null || receiver == null)
        {
            Debug.LogError("支付过路费时找不到玩家对象");
            return;
        }
        
        // 检查是否有足够的钱支付
        if (payer.money >= amount)
        {
            // 扣除付款方金钱
            payer.money -= amount;
            UpdatePlayerMoney(payerIndex, payer.money);
            
            // 增加接收方金钱
            receiver.money += amount;
            UpdatePlayerMoney(receiverIndex, receiver.money);
            
            // 显示交易信息
            if (uiManager != null)
            {
                uiManager.ShowEventMessage($"玩家{payerIndex + 1}支付{amount}过路费给玩家{receiverIndex + 1}（{land.landName}）");
            }
            
            Debug.Log($"玩家{payerIndex}支付{amount}过路费给玩家{receiverIndex}");
        }
        else
        {
            // 处理破产情况
            HandleBankruptcy(payerIndex, receiverIndex, amount, land);
        }
    }
    
    // 更新玩家金钱UI显示
    private void UpdatePlayerMoney(int playerIndex, int amount)
    {
        if (uiManager != null)
        {
            uiManager.UpdatePlayerMoney(playerIndex, amount);
        }
    }
    
    // 获取玩家颜色
    private Color GetPlayerColor(CharacterMovement player)
    {
        // 尝试从玩家渲染器获取颜色
        Renderer renderer = player.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            return renderer.material.color;
        }
        
        // 默认颜色
        return new Color[] { Color.red, Color.blue, Color.green, Color.yellow }[
            playerList.IndexOf(player) % 4];
    }
    
    // 处理破产
    private void HandleBankruptcy(int bankruptPlayerIndex, int creditorIndex, int debtAmount, Land land)
    {
        if (uiManager != null)
        {
            uiManager.ShowEventMessage($"玩家{bankruptPlayerIndex + 1}资金不足，无法支付{debtAmount}过路费，已破产!");
        }
        
        Debug.Log($"玩家{bankruptPlayerIndex}破产");
        
        // 简单破产处理：释放所有地块
        if (playerOwnedLands.ContainsKey(bankruptPlayerIndex))
        {
            foreach (Land ownedLand in new List<Land>(playerOwnedLands[bankruptPlayerIndex]))
            {
                ownedLand.ownerIndex = -1;
                ownedLand.level = 0;
                ownedLand.UpdateAppearance();
            }
            playerOwnedLands[bankruptPlayerIndex].Clear();
        }
    }
    
    // 添加玩家到列表
    public void AddPlayer(CharacterMovement player)
    {
        if (player != null && !playerList.Contains(player))
        {
            int playerIndex = playerList.Count;
            playerList.Add(player);
            playerOwnedLands[playerIndex] = new List<Land>();
            Debug.Log($"添加玩家到列表，索引: {playerIndex}");
        }
    }
    
    // 获取玩家拥有的地块列表
    public List<Land> GetPlayerOwnedLands(int playerIndex)
    {
        if (playerOwnedLands.ContainsKey(playerIndex))
        {
            return playerOwnedLands[playerIndex];
        }
        return new List<Land>();
    }

    public int GetStationCount(int playerIndex)
    {
        int count = 0;
        foreach (Land land in GetPlayerOwnedLands(playerIndex))
        {
            if (land.group == "Station")
                count++;
        }
        return count;
    }

    public int GetUtilityCount(int playerIndex)
    {
        int count = 0;
        foreach (Land land in GetPlayerOwnedLands(playerIndex))
        {
            if (land.group == "Utilities")
                count++;
        }
        return count;
    }

    public void PurchaseLand(int playerIndex, Land land, Color playerColor)
    {
        // 处理地块购买逻辑
        CharacterMovement player = GetPlayer(playerIndex);
        if (player != null && player.money >= land.purchasePrice)
        {
            player.money -= land.purchasePrice;
            land.PurchaseProperty(playerIndex, playerColor);
            
            // 更新玩家金钱显示
            UIManager.Instance.UpdatePlayerMoney(playerIndex, player.money);
        }
    }

    public void ReleaseAllLands(int playerIndex)
    {
        // 找到所有地块
        Land[] allLands = FindObjectsByType<Land>(FindObjectsSortMode.None);
        
        // 释放指定玩家拥有的地块
        foreach (Land land in allLands)
        {
            if (land.ownerIndex == playerIndex)
            {
                // 重置地块所有权
                land.ownerIndex = -1;
                land.level = 0;
                land.isMortgaged = false;
                land.UpdateAppearance();
            }
        }
    }
} 