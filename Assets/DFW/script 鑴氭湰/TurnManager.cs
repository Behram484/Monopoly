using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//这个class管理回合逻辑比如玩家先走，做决定，然后AI走，做决定，然后下一个玩家走，循环往复
public class TurnManager : MonoBehaviour
{
    [Header("玩家列表")]
    public List<GameObject> players; // 按顺序包含玩家和AI（需拖拽赋值）
    
    [Header("骰子系统")]
    public DiceSystem playerDice;    // 玩家的骰子系统（拖拽赋值）
    
    [Header("路径点")]
    public Transform[] waypoints;    // 所有玩家共用的路径点
    
    [Header("UI管理器")]
    public UIManager uiManager;

    [SerializeField] private int jailIndex = 10;  // 监狱地块索引，需要设置

    private int currentPlayerIndex = 0;
    public int CurrentPlayerIndex { get { return currentPlayerIndex; } }

    public static TurnManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // 添加这段代码到TurnManager.cs的Start方法开始处
        GameObject waypointManagerObj = GameObject.Find("waypointManager");
        if (waypointManagerObj != null)
        {
            // 自动获取所有路径点
            waypoints = new Transform[waypointManagerObj.transform.childCount];
            for (int i = 0; i < waypointManagerObj.transform.childCount; i++)
            {
                waypoints[i] = waypointManagerObj.transform.GetChild(i);
            }
            Debug.Log($"自动获取了{waypoints.Length}个路径点");
        }
        
        Debug.Log($"初始化骰子系统: {(playerDice != null ? "成功" : "失败")}");
        playerDice.EnableDice();
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()//游戏循环，这里加入玩家决策，以及玩家回合结束时的"结束"按钮
    {
        while (true)
        {
            GameObject currentPlayer = players[currentPlayerIndex];//获取当前回合玩家从玩家列表的「0」开始
            Debug.Log($"当前回合玩家：{currentPlayer.name}");//打印当前回合玩家的名字，方便检查
            Debug.Log($"尝试激活骰子: {currentPlayer.name}是否是玩家: {currentPlayer.CompareTag("Player")}");

            // 玩家回合
            if (currentPlayer.CompareTag("Player"))
            {
                Debug.Log("玩家回合开始，激活骰子");
                
                // 确保这里正确调用了EnableDice
                if (playerDice != null)
                {
                    playerDice.EnableDice();
                }
                else
                {
                    Debug.LogError("playerDice引用为空!");
                }
                
                yield return new WaitUntil(() => playerDice.IsRollComplete);
                Debug.Log("玩家已完成骰子投掷");
                
                // 等待玩家移动完成
                PlayerMovement playerMov = currentPlayer.GetComponent<PlayerMovement>();
                yield return new WaitUntil(() => !playerMov.isMoving);
                Debug.Log("<color=green>玩家移动完成</color>");
                
                // 获取玩家当前位置
                int currentIndex = playerMov.currentIndex;
                Land currentLand = waypoints[currentIndex].GetComponent<Land>();
                
                // 仅调用一次地块事件处理
                HandleLandEvent(currentLand, currentPlayer);  // 只保留这一行
                
                playerDice.ResetDiceState();
            }
            // AI回合
            else
            {
                Debug.Log("AI回合开始: " + currentPlayer.name);
                AIMovement aiMov = currentPlayer.GetComponent<AIMovement>();
                if (aiMov == null) {
                    Debug.LogError("AI对象没有AIMovement组件!");
                    yield break;
                }
                int steps = Random.Range(2, 13);//随机步数，"step"这里是Ai用的，"movestep"是玩家用的，这里可能有逻辑错误，仔细检查
                aiMov.MoveSteps(steps);
                yield return new WaitUntil(() => !aiMov.isMoving);//等待AI移动完成
                
                // 获取AI当前位置
                int currentIndex = aiMov.currentIndex;//获取当前位置
                Land currentLand = waypoints[currentIndex].GetComponent<Land>();//获取当前位置的Land组件
                HandleLandEvent(currentLand, currentPlayer);//处理地块事件
            }

            // 切换到下一个玩家
            MoveNext();

            yield return new WaitForSeconds(1f);//等待1秒
        }
    }

    public void HandleLandEvent(Land land, GameObject lander)
    {
        if (land == null) return;

        int playerIndex = players.IndexOf(lander);
        
        switch (land.landType)
        {
            case LandType.Event:
                HandleRandomEvent(lander);
                break;
            case LandType.Chance:
                HandleChanceCard(playerIndex);
                break;
            case LandType.Community:  // 使用正确的类型
                HandleCommunityCard(playerIndex);
                break;
            default:
                HandlePropertyLogic(land, lander);
                break;
        }

        // 更新玩家UI
        if (lander.CompareTag("Player"))
        {
            uiManager.UpdatePlayerMoney(playerIndex, lander.GetComponent<PlayerMovement>().money);
        }
    }

    private void HandlePropertyLogic(Land land, GameObject lander)
    {
        if (land == null || lander == null)
        {
            Debug.LogWarning("Land或Lander为空，无法处理地块逻辑");
            return;
        }

        // 从players列表获取玩家索引
        int playerIndex = players.IndexOf(lander);
        if (playerIndex == -1)
        {
            Debug.LogWarning("找不到玩家索引");
            return;
        }

        // 检查是否是特殊地块，特殊地块不显示购买UI
        if (land.landType == LandType.Jail || land.landType == LandType.Hospital || 
            land.landType == LandType.Tax || land.landType == LandType.Start)
        {
            // 特殊地块只执行相应效果，不显示购买UI
            land.OnPlayerLanded(playerIndex);
            return;
        }

        // 普通地块调用OnPlayerLanded
        land.OnPlayerLanded(playerIndex);
    }

    private void HandleRandomEvent(GameObject lander)
    {
        int randomAmount = Random.Range(-100, 201);
        string message = "";

        if (lander.CompareTag("Player"))
        {
            PlayerMovement player = lander.GetComponent<PlayerMovement>();
            player.money += randomAmount;
            message = $"玩家遭遇随机事件！{(randomAmount >= 0 ? "获得" : "损失")} {Mathf.Abs(randomAmount)} 金币！";
        }
        else
        {
            AIMovement ai = lander.GetComponent<AIMovement>();
            ai.money += randomAmount;
            message = $"{lander.name} 遭遇随机事件！{(randomAmount >= 0 ? "获得" : "损失")} {Mathf.Abs(randomAmount)} 金币！";
        }

        Debug.Log(message);
        uiManager.ShowEventMessage(message); // 需要UIManager有对应方法
    }

    private void HandleChanceCard(int playerIndex)
    {
        Card card = CardManager.Instance.DrawChanceCard();
        CardManager.Instance.ExecuteCard(card, playerIndex);
    }

    private void HandleCommunityCard(int playerIndex)
    {
        Card card = CardManager.Instance.DrawCommunityCard();
        CardManager.Instance.ExecuteCard(card, playerIndex);
    }

    public void MoveCurrentPlayer(int steps)
    {
        if (currentPlayerIndex < players.Count)
        {
            GameObject currentPlayer = players[currentPlayerIndex];
            if (currentPlayer.CompareTag("Player"))
            {
                PlayerMovement playerMov = currentPlayer.GetComponent<PlayerMovement>();
                if (playerMov != null)
                {
                    playerMov.MoveSteps(steps);
                    Debug.Log($"<color=green>玩家移动{steps}步</color>");
                }
            }
        }
    }

    private void MoveNext()
    {
        // 切换到下一个玩家前先检查索引
        if (players != null && players.Count > 0)
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            Debug.Log($"切换到玩家 {currentPlayerIndex}");
        }
        else
        {
            Debug.LogError("没有玩家在列表中！");
        }
    }

    public void MovePlayerToPrison(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= players.Count) return;
        
        GameObject playerObj = players[playerIndex];
        if (playerObj.CompareTag("Player"))
        {
            PlayerMovement playerMov = playerObj.GetComponent<PlayerMovement>();
            playerMov.MoveToIndex(jailIndex);
            uiManager.ShowEventMessage($"玩家{playerIndex + 1}被送进监狱");
        }
        else
        {
            AIMovement aiMov = playerObj.GetComponent<AIMovement>();
            aiMov.MoveToIndex(jailIndex);
            uiManager.ShowEventMessage($"AI{playerIndex + 1}被送进监狱");
        }
    }
}





