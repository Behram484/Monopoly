// Land.cs
using UnityEngine;
using TMPro;

// 地块类型
public enum LandType 
{ 
    Normal, 
    FreeParking,  // 改 free parking 为 FreeParking
    Event, 
    Start, 
    Jail, 
    PotLuck,      // 改 Pot Luck 为 PotLuck
    Opportunity, 
    Tax, 
    Station, 
    Utilities, 
    Community,
    Commercial,   // 添加缺少的枚举值
    Hospital,     // 添加缺少的枚举值
     Chance        // 添加缺少的枚举值
}

public class Land : MonoBehaviour
{
    [Header("基础属性")]
    public LandType landType = LandType.Normal;
    public string landName = "未命名地块";
    public string group;
    public bool canBePurchased = true; // 添加这行，默认勾选为可购买
    public int purchasePrice = 200; // 购买价格
    public int toll = 50;          // 过路费
    public int ownerIndex = -1;    // 拥有者ID，-1表示无主
    public int level = 0;          // 地块等级
    public int maxLevel = 5;       // 最大等级
    public int upgradeCost = 100;  // 升级费用
    
    [Header("视觉属性")]
    public Color ownerColor = Color.white;
    public Color defaultColor = Color.white;
    public SpriteRenderer landRenderer;
    
    [Header("事件属性")]
    public string eventMessage = "事件地块";
    public int eventReward = 0;
    public int eventPenalty = 0;
    
    [Header("税收属性")]
    public int taxAmount = 100;

    [Header("数据引用")]
    public LandInfo landInfo;
    
    private UIManager uiManager;

    [Header("升级租金")]
    public int rent1House;      // 1栋房子的租金
    public int rent2Houses;     // 2栋房子的租金
    public int rent3Houses;     // 3栋房子的租金
    public int rent4Houses;     // 4栋房子的租金
    public int rentHotel;       // 酒店的租金

    [Header("抵押状态")]
    public bool isMortgaged = false; // 是否已抵押

    // 初始化
    private void Awake()
    {
        if (landRenderer == null)
        {
            landRenderer = GetComponent<SpriteRenderer>();
        }
        
        // 设置默认颜色
        if (landRenderer != null)
        {
            defaultColor = landRenderer.color;
        }
    }

    private void Start()
    {
        // 使用单例模式:
        uiManager = UIManager.Instance;
        if (uiManager == null)
        {
            Debug.LogError($"地块 {landName} 无法找到UIManager.Instance!");
        }
        else
        {
            Debug.Log($"地块 {landName} 成功获取UIManager实例");
        }
        Debug.Log($"Land.Start uiManager是否为null: {uiManager == null}");
    }
    
    // 当玩家停留在地块上时触发
    public void OnPlayerLanded(int playerIndex)
    {
        DebugLog("OnPlayerLanded", $"玩家{playerIndex}落在地块{landName}上，类型:{landType}");
        
        // 判断是否是AI (在TurnManager中玩家通常是索引0)
        bool isAI = playerIndex > 0;
        
        switch (landType)
        {
            case LandType.Normal:
            case LandType.Commercial:
                // AI不显示购买UI
                if (!isAI)
                    HandlePropertyLand(playerIndex);
                else
                    HandleAIPropertyLand(playerIndex); // 新添加的AI处理方法
                break;
            case LandType.Event:
                TriggerEvent(playerIndex);
                break;
            case LandType.Start:
                GiveStartBonus(playerIndex);
                break;
            case LandType.Tax:
                CollectTax(playerIndex);
                break;
            case LandType.Chance:
                TriggerChance(playerIndex);
                break;
            // 其他类型的地块处理可以继续添加
        }
    }
    
    // 处理可购买地块
    private void HandlePropertyLand(int playerIndex)
    {
        // 如果地块无主且允许购买
        if (ownerIndex == -1 && canBePurchased)
        {
            Debug.Log("尝试显示购买UI");
            if (uiManager != null)
            {
                uiManager.ShowPurchasePrompt(this, playerIndex);
            }
            else
            {
                Debug.LogError("没有找到UIManager!");
            }
        }
        // 如果地块无主但不允许购买
        else if (ownerIndex == -1 && !canBePurchased)
        {
            // 可选：显示提示消息
            if (uiManager != null)
            {
                uiManager.ShowEventMessage($"{landName}不出售");
            }
        }
        // 如果地块是其他玩家的
        else if (ownerIndex != playerIndex)
        {
            // 计算过路费
            int currentToll = CalculateToll();
            
            // 通过GameManager处理支付
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerPayToll(playerIndex, ownerIndex, currentToll, this);
            }
        }
        // 如果是自己的地块
        else if (ownerIndex == playerIndex && level < maxLevel)
        {
            // 显示升级提示
            if (uiManager != null)
            {
                uiManager.ShowUpgradePrompt(this, playerIndex);
            }
        }
    }
    
    // 计算当前过路费
    public int CalculateToll(int diceValue = 0)
    {
        // 车站(Station)特殊处理
        if (landType == LandType.Station)
        {
            // 获取拥有者的车站数量
            int stationCount = GameManager.Instance.GetStationCount(ownerIndex);
            switch (stationCount)
            {
                case 1: return 25;
                case 2: return 50;
                case 3: return 100;
                case 4: return 200;
                default: return toll;
            }
        }
        
        // 公用事业(Utilities)特殊处理
        if (landType == LandType.Utilities)
        {
            // 获取拥有者的公用事业数量
            int utilityCount = GameManager.Instance.GetUtilityCount(ownerIndex);
            if (utilityCount == 1)
                return diceValue * 4;
            else if (utilityCount >= 2)
                return diceValue * 10;
            return toll;
        }
        
        // 普通地块按等级计算
        switch (level)
        {
            case 0: return toll;
            case 1: return rent1House;
            case 2: return rent2Houses;
            case 3: return rent3Houses;
            case 4: return rent4Houses;
            case 5: return rentHotel;
            default: return toll;
        }
    }
    
    // 升级地块
    public bool UpgradeProperty(int playerIndex)
    {
        // 检查是否是地块所有者
        if (ownerIndex != playerIndex || level >= maxLevel)
            return false;
        
        // 获取玩家对象
        CharacterMovement player = GameManager.Instance.GetPlayer(playerIndex);
        if (player == null || player.money < upgradeCost)
            return false;
        
        // 扣除升级费用
        player.money -= upgradeCost;
        level++;
        
        // 更新外观
        UpdateAppearance();
        return true;
    }
    
    // 购买地块
    public void PurchaseProperty(int playerIndex, Color playerColor)
    {
        ownerIndex = playerIndex;
        ownerColor = playerColor;
        UpdateAppearance();
    }
    
    // 更新地块外观
    public void UpdateAppearance()
    {
        if (landRenderer != null)
        {
            if (ownerIndex != -1)
            {
                if (isMortgaged)
                {
                    // 抵押状态显示（半透明）
                    Color mortgagedColor = ownerColor;
                    mortgagedColor.a = 0.5f;
                    landRenderer.color = mortgagedColor;
                }
                else
                {
                    landRenderer.color = ownerColor;
                    
                    // 根据等级显示不同数量的房子
                    // 可以实例化预制体或激活子对象来表示房子数量
                    // 例如：
                    // ShowHouseModels(level);
                }
            }
            else
            {
                landRenderer.color = defaultColor;
            }
        }
    }
    
    // 事件地块效果
    private void TriggerEvent(int playerIndex)
    {
        // 触发事件效果
        // 通过GameManager处理
        // GameManager.Instance.TriggerLandEvent(playerIndex, eventMessage, eventReward, eventPenalty);
    }
    
    // 起点奖励
    private void GiveStartBonus(int playerIndex)
    {
        // 给予玩家通过起点的奖励
        // GameManager.Instance.GiveStartBonus(playerIndex);
    }
    
    // 税收地块
    private void CollectTax(int playerIndex)
    {
        // 收取税款
        // GameManager.Instance.CollectTax(playerIndex, taxAmount);
    }
    
    // 机会地块
    private void TriggerChance(int playerIndex)
    {
        // 触发随机事件
        // GameManager.Instance.TriggerChanceCard(playerIndex);
    }

    // 添加方法来应用数据
    public void ApplyLandInfo(LandInfo info)
    {
        if (info == null) return;
        
        landName = info.landName;
        group = info.group;
        canBePurchased = info.canBePurchased;
        purchasePrice = info.purchasePrice;
        
        // 导入不同等级的过路费
        toll = info.toll;                // 基础租金(无房子)
        
        // 保存升级后的过路费值
        rent1House = info.rent1House;    // 1栋房子租金
        rent2Houses = info.rent2Houses;  // 2栋房子租金
        rent3Houses = info.rent3Houses;  // 3栋房子租金
        rent4Houses = info.rent4Houses;  // 4栋房子租金
        rentHotel = info.rentHotel;      // 酒店租金
        
        maxLevel = 5;  // 最高等级设为5（对应酒店）
        
        // 其他属性设置...

        if (info.group != null)
        {
            switch (info.group.ToLower())
            {
                case "brown":
                case "blue":
                    upgradeCost = 50;
                    break;
                case "purple":
                case "orange":
                    upgradeCost = 100;
                    break;
                case "red": 
                case "yellow":
                    upgradeCost = 150;
                    break;
                case "green":
                case "deep blue":
                    upgradeCost = 200;
                    break;
            }
        }
    }

    private LandType ParseLandType(string group)
    {
        // Implementation of ParseLandType method
        // This is a placeholder and should be replaced with the actual implementation
        // based on the group string.
        return LandType.Normal; // Placeholder return, actual implementation needed
    }

    private void OnMouseDown()
    {
        // 只有当地块有所有者并且是当前玩家的地块时才显示抵押面板
        if (ownerIndex != -1 && UIManager.Instance != null)
        {
            UIManager.Instance.currentLand = this;
            ShowMortgageInfo();
        }
    }

    private void ShowMortgageInfo()
    {
        if (UIManager.Instance == null || UIManager.Instance.bankPanel == null) return;
        
        // 使用UIManager中的引用而不是查找
        if (UIManager.Instance.landText) 
            UIManager.Instance.landText.text = landName;
        
        if (UIManager.Instance.ownerText) 
            UIManager.Instance.ownerText.text = $"玩家{ownerIndex + 1}";
        
        if (UIManager.Instance.valueText)
        {
            if (isMortgaged)
                UIManager.Instance.valueText.text = $"£{(int)(purchasePrice/2*1.1f)}";
            else
                UIManager.Instance.valueText.text = $"£{purchasePrice/2}";
        }
        
        UIManager.Instance.bankPanel.SetActive(true);
    }

    // 新添加的AI处理方法
    private void HandleAIPropertyLand(int playerIndex)
    {
        // 如果地块无主且允许购买，AI可以自动购买
        if (ownerIndex == -1 && canBePurchased)
        {
            // AI逻辑：例如随机决定是否购买
            if (Random.value > 0.5f && GameManager.Instance != null)
            {
                CharacterMovement player = GameManager.Instance.GetPlayer(playerIndex);
                if (player != null && player.money >= purchasePrice)
                {
                    // AI自动购买
                    Color aiColor = player.GetComponentInChildren<Renderer>()?.material.color ?? Color.white;
                    GameManager.Instance.PurchaseLand(playerIndex, this, aiColor);
                    
                    // 显示消息但不显示UI
                    if (uiManager != null)
                    {
                        uiManager.ShowEventMessage($"AI玩家{playerIndex}购买了{landName}");
                    }
                }
            }
        }
        // 其他情况不需要UI交互
    }

    private void DebugLog(string methodName, string message)
    {
        Debug.Log($"[Land] [{methodName}] {message}");
    }
}
