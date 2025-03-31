using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("面板引用")]
    public GameObject purchasePanel;  // 购买面板
    public GameObject upgradePanel;   // 升级面板
    public GameObject bankPanel;            // 银行主面板
    public GameObject mortgageListPanel;    // 抵押列表面板
    public GameObject unmortgageListPanel;  // 解除抵押列表面板
    
    [Header("购买面板组件")]
    public TextMeshProUGUI landNameText;     // 地块名称
    public TextMeshProUGUI landPriceText;    // 地块价格
    public TextMeshProUGUI landTollText;     // 过路费
    public Button purchaseButton;            // 购买按钮
    public Button cancelPurchaseButton;      // 取消按钮
    
    [Header("升级面板组件")]
    public TextMeshProUGUI upgradeLandNameText;      // 地块名称
    public TextMeshProUGUI currentLevelText;         // 当前等级
    public TextMeshProUGUI upgradeCostText;          // 升级费用
    public TextMeshProUGUI nextLevelRentText;        // 下一级租金
    public TextMeshProUGUI maxLevelTotalCostText;    // 升满费用
    public Button upgradeButton;                     // 升级按钮
    public Button cancelUpgradeButton;               // 取消按钮
    
    [Header("银行面板组件")]
    public Button mortgageButton;           // 抵押按钮
    public Button unmortgageButton;         // 解除抵押按钮
    public Button closeBankButton;          // 关闭银行面板按钮
    public Transform mortgageListContent;   // 抵押列表内容区
    public Transform unmortgageListContent; // 解除抵押列表内容区
    public GameObject landItemPrefab;       // 地产项预制体
    
    [Header("银行面板文本")]
    public TextMeshProUGUI landText;    // 地块名称
    public TextMeshProUGUI ownerText;   // 所有者
    public TextMeshProUGUI valueText;   // 价值
    
    [Header("玩家金钱UI")]
    public TextMeshProUGUI[] playerMoneyTexts;  // 为每个玩家创建一个金钱文本
    
    [Header("卡片UI")]
    public GameObject cardPanel;            // 卡片面板
    public TextMeshProUGUI cardTitleText;   // 卡片标题
    public TextMeshProUGUI cardDescText;    // 卡片描述
    public Button closeCardButton;          // 关闭卡片按钮
    
    // 当前相关数据
    public Land currentLand { get; set; }
    private int currentPlayerIndex;
    
    // 单例实例
    public static UIManager Instance { get; private set; }
    
    // 添加一个静态计数器
    private static int instanceCount = 0;
    
    private void Awake()
    {
        // 设置单例
        if (Instance == null)
        {
            Instance = this;
            instanceCount++; // 增加计数
            Debug.Log($"UIManager实例数量: {instanceCount}");
            
            // 暂时注释掉，测试是否出现多个实例
            // DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Debug.Log($"发现多余的UIManager实例！当前实例数: {instanceCount+1}");
            Destroy(gameObject);
        }
        
        // 隐藏所有面板
        HideAllPanels();
    }
    
    // 隐藏所有UI面板
    private void HideAllPanels()
    {
        if (purchasePanel) purchasePanel.SetActive(false);
        if (upgradePanel) upgradePanel.SetActive(false);
        if (bankPanel) bankPanel.SetActive(false);
        if (mortgageListPanel) mortgageListPanel.SetActive(false);
        if (unmortgageListPanel) unmortgageListPanel.SetActive(false);
    }
    
    // 显示购买提示
    public void ShowPurchasePrompt(Land land, int playerIndex)
    {
        currentLand = land;
        currentPlayerIndex = playerIndex;
        
        // 添加调试输出查看地块名称
        Debug.Log($"地块名称: '{land.landName}'");
        
        // 确保直接赋值，不使用格式化字符串
        landNameText.text = land.landName;  // 不要写成"Land name"或$"Land name"
        landPriceText.text = $"£{land.purchasePrice}";
        landTollText.text = $"£{land.toll}";
        
        // 检查玩家是否有足够的钱
        CharacterMovement player = GameManager.Instance.GetPlayer(playerIndex);
        bool canAfford = player != null && player.money >= land.purchasePrice;
        
        // 更新购买按钮状态
        purchaseButton.interactable = canAfford;
        
        // 显示面板
        purchasePanel.SetActive(true);
        
        // 设置按钮事件
        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(OnPurchaseConfirmed);
        
        cancelPurchaseButton.onClick.RemoveAllListeners();
        cancelPurchaseButton.onClick.AddListener(OnPurchaseCancelled);
    }
    
    // 显示升级提示
    public void ShowUpgradePrompt(Land land, int playerIndex)
    {
        currentLand = land;
        currentPlayerIndex = playerIndex;
        
        // 计算下一级租金
        int nextLevelRent = 0;
        switch (land.level + 1)
        {
            case 1: nextLevelRent = land.rent1House; break;
            case 2: nextLevelRent = land.rent2Houses; break;
            case 3: nextLevelRent = land.rent3Houses; break;
            case 4: nextLevelRent = land.rent4Houses; break;
            case 5: nextLevelRent = land.rentHotel; break;
        }
        
        // 计算升满级总费用
        int remainingLevels = land.maxLevel - land.level;
        int totalCost = land.upgradeCost * remainingLevels;
        
        // 更新UI文本
        upgradeLandNameText.text = land.landName;
        currentLevelText.text = $"当前等级: {GetLevelText(land.level)}";
        upgradeCostText.text = $"升级费用: £{land.upgradeCost}";
        nextLevelRentText.text = $"下一级租金: £{nextLevelRent}";
        maxLevelTotalCostText.text = $"升满总费用: £{totalCost}";
        
        // 检查玩家是否有足够的钱
        CharacterMovement player = GameManager.Instance.GetPlayer(playerIndex);
        bool canAfford = player != null && player.money >= land.upgradeCost;
        
        // 更新升级按钮状态
        upgradeButton.interactable = canAfford;
        
        // 显示面板
        upgradePanel.SetActive(true);
        
        // 设置按钮事件
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnUpgradeConfirmed);
        
        cancelUpgradeButton.onClick.RemoveAllListeners();
        cancelUpgradeButton.onClick.AddListener(OnUpgradeCancelled);
    }
    
    // 获取等级文本表示
    private string GetLevelText(int level)
    {
        switch (level)
        {
            case 0: return "无建筑";
            case 1: return "1栋房子";
            case 2: return "2栋房子";
            case 3: return "3栋房子";
            case 4: return "4栋房子";
            case 5: return "酒店";
            default: return $"等级{level}";
        }
    }
    
    // 购买确认
    private void OnPurchaseConfirmed()
    {
        if (currentLand != null && GameManager.Instance != null)
        {
            CharacterMovement player = GameManager.Instance.GetPlayer(currentPlayerIndex);
            Color playerColor = player.GetComponentInChildren<Renderer>()?.material.color ?? Color.white;
            
            // 执行购买
            GameManager.Instance.PurchaseLand(currentPlayerIndex, currentLand, playerColor);
            
            // 显示消息
            ShowEventMessage($"{player.name}购买了{currentLand.landName}");
        }
        
        HideAllPanels();
    }
    
    // 取消购买
    private void OnPurchaseCancelled()
    {
        HideAllPanels();
    }
    
    // 升级确认
    private void OnUpgradeConfirmed()
    {
        if (currentLand != null)
        {
            // 执行升级
            bool success = currentLand.UpgradeProperty(currentPlayerIndex);
            
            if (success)
            {
                // 显示升级成功消息
                CharacterMovement player = GameManager.Instance.GetPlayer(currentPlayerIndex);
                ShowEventMessage($"{player.name}将{currentLand.landName}升级至{GetLevelText(currentLand.level)}");
            }
        }
        
        HideAllPanels();
    }
    
    // 取消升级
    private void OnUpgradeCancelled()
    {
        HideAllPanels();
    }
    
    // 显示事件消息
    public void ShowEventMessage(string message)
    {
        // 你可以添加一个事件消息显示面板
        Debug.Log(message);
    }

    // 添加初始化方法
    public void InitializePlayerMoneyUI(int playerCount)
    {
        if (playerMoneyTexts == null || playerMoneyTexts.Length < playerCount)
        {
            Debug.LogWarning("玩家金钱UI文本数量不足");
            return;
        }
        
        // 初始隐藏所有文本
        for (int i = 0; i < playerMoneyTexts.Length; i++)
        {
            if (playerMoneyTexts[i] != null)
            {
                playerMoneyTexts[i].gameObject.SetActive(i < playerCount);
                playerMoneyTexts[i].text = "玩家" + (i+1) + ": £0";
            }
        }
    }
    
    // 完善已有的UpdatePlayerMoney方法
    public void UpdatePlayerMoney(int playerIndex, int amount)
    {
        if (playerIndex >= 0 && playerIndex < playerMoneyTexts.Length && playerMoneyTexts[playerIndex] != null)
        {
            playerMoneyTexts[playerIndex].text = $"玩家{playerIndex + 1}: £{amount}";
        }
        Debug.Log($"玩家{playerIndex}的金钱更新为{amount}");
    }

    private void Start()
    {
        // 添加银行按钮监听
        if (mortgageButton) mortgageButton.onClick.AddListener(ShowMortgagePanel);
        if (unmortgageButton) unmortgageButton.onClick.AddListener(ShowUnmortgagePanel);
        if (closeBankButton) closeBankButton.onClick.AddListener(() => bankPanel.SetActive(false));
        
        // 添加YES和NO按钮点击事件
        Transform yesBtn = bankPanel.transform.Find("YES");
        Transform noBtn = bankPanel.transform.Find("NO");
        
        if (yesBtn && yesBtn.GetComponent<Button>())
        {
            yesBtn.GetComponent<Button>().onClick.AddListener(OnMortgageConfirmed);
        }
        
        if (noBtn && noBtn.GetComponent<Button>())
        {
            noBtn.GetComponent<Button>().onClick.AddListener(() => bankPanel.SetActive(false));
        }

        if (closeCardButton) 
            closeCardButton.onClick.AddListener(() => cardPanel.SetActive(false));
    }

    public void ShowBankPanel(Land land = null)
    {
        if (land != null)
        {
            currentLand = land;
            
            // 设置银行面板上的文本内容
            // 例如：
            landText.text = land.landName;
            ownerText.text = $"玩家{land.ownerIndex + 1}";
            valueText.text = 
                land.isMortgaged ? 
                $"解除抵押: £{(int)(land.purchasePrice/2*1.1f)}" : 
                $"抵押价值: £{land.purchasePrice/2}";
        }
        
        HideAllPanels();
        bankPanel.SetActive(true);
    }

    private void ShowMortgagePanel()
    {
        mortgageListPanel.SetActive(true);
        unmortgageListPanel.SetActive(false);
        UpdateMortgageList();
    }

    private void ShowUnmortgagePanel()
    {
        mortgageListPanel.SetActive(false);
        unmortgageListPanel.SetActive(true);
        UpdateUnmortgageList();
    }

    private void UpdateMortgageList()
    {
        // 清空列表
        foreach (Transform child in mortgageListContent)
        {
            Destroy(child.gameObject);
        }
        
        // 获取当前玩家可抵押地产
        int currentPlayerIndex = TurnManager.Instance.CurrentPlayerIndex;
        List<Land> ownedLands = GameManager.Instance.GetPlayerOwnedLands(currentPlayerIndex);
        
        foreach (Land land in ownedLands)
        {
            if (!land.isMortgaged)
            {
                CreateLandItem(land, mortgageListContent, true);
            }
        }
    }

    private void UpdateUnmortgageList()
    {
        // 清空列表
        foreach (Transform child in unmortgageListContent)
        {
            Destroy(child.gameObject);
        }
        
        // 获取当前玩家已抵押地产
        int currentPlayerIndex = TurnManager.Instance.CurrentPlayerIndex;
        List<Land> ownedLands = GameManager.Instance.GetPlayerOwnedLands(currentPlayerIndex);
        
        foreach (Land land in ownedLands)
        {
            if (land.isMortgaged)
            {
                CreateLandItem(land, unmortgageListContent, false);
            }
        }
    }

    private void CreateLandItem(Land land, Transform parent, bool isMortgage)
    {
        GameObject item = Instantiate(landItemPrefab, parent);
        
        // 获取组件
        TextMeshProUGUI landNameText = item.transform.Find("LandNameText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI priceText = item.transform.Find("PriceText").GetComponent<TextMeshProUGUI>();
        Button actionButton = item.transform.Find("ActionButton").GetComponent<Button>();
        
        // 设置文本
        landNameText.text = land.landName;
        
        if (isMortgage)
        {
            int mortgageValue = land.purchasePrice / 2;
            priceText.text = $"抵押价值: £{mortgageValue}";
            actionButton.GetComponentInChildren<TextMeshProUGUI>().text = "抵押";
            
            // 抵押按钮事件
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => {
                Bank.Instance.MortgageLand(land, TurnManager.Instance.CurrentPlayerIndex);
                UpdateMortgageList();
                UpdateUnmortgageList();
            });
        }
        else
        {
            int unmortgageCost = (int)(land.purchasePrice / 2 * 1.1f);
            priceText.text = $"解除抵押费用: £{unmortgageCost}";
            actionButton.GetComponentInChildren<TextMeshProUGUI>().text = "解除抵押";
            
            // 解除抵押按钮事件
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => {
                Bank.Instance.UnmortgageLand(land, TurnManager.Instance.CurrentPlayerIndex);
                UpdateMortgageList();
                UpdateUnmortgageList();
            });
        }
    }

    // 处理抵押/解除抵押确认
    private void OnMortgageConfirmed()
    {
        if (currentLand == null) return;
        
        if (currentLand.isMortgaged)
        {
            // 解除抵押
            Bank.Instance.UnmortgageLand(currentLand, currentLand.ownerIndex);
        }
        else
        {
            // 抵押
            Bank.Instance.MortgageLand(currentLand, currentLand.ownerIndex);
        }
        
        bankPanel.SetActive(false);
    }

    // 显示卡片UI
    public void ShowCardUI(Card card)
    {
        if (cardPanel == null || cardTitleText == null || cardDescText == null) return;
        
        cardTitleText.text = card.title;
        cardDescText.text = card.description;
        
        // 根据卡片类型设置不同的颜色
        Color chanceColor = new Color(1, 0.6f, 0, 1);  // 橙色
        Color communityColor = new Color(0, 0.6f, 1, 1);  // 蓝色
        
        cardPanel.GetComponent<Image>().color = 
            card.type == Card.CardType.Chance ? chanceColor : communityColor;
        
        cardPanel.SetActive(true);
    }
}