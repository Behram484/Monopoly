using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DiceSystem : MonoBehaviour
{
    [Header("UI组件")]
    public Button diceButton;
    public TMP_Text resultText;

    [Header("骰子参数")]
    public int minValue = 1;
    public int maxValue = 6;
    public float rollDuration = 2f;

    public bool IsRollComplete { get; private set; }

    void Start()
    {
        diceButton.interactable = true;
        resultText.gameObject.SetActive(false);
        
        // 添加点击事件监听
        diceButton.onClick.AddListener(RollDice);
    }

    public void EnableDice()//激活骰子
    {
        Debug.Log("骰子系统被启用");
        diceButton.interactable = true;//激活按钮
        resultText.gameObject.SetActive(true);//激活文本
        IsRollComplete = false;//骰子未投掷完成
    }

    public void RollDice()//投掷骰子
    {
        if(resultText != null && !resultText.gameObject.activeInHierarchy)
            resultText.gameObject.SetActive(true);
        StartCoroutine(RollDiceCoroutine());//调用下面的RollDiceCoroutine协程
    }

    private IEnumerator RollDiceCoroutine()// 投掷骰子的协程
    {
        diceButton.interactable = false;//投掷骰子后，按钮不可用
        
        // 骰子动画
        float timer = 0;
        while (timer < rollDuration)
        {
            int dice1 = Random.Range(minValue, maxValue + 1);//第一个骰子
            int dice2 = Random.Range(minValue, maxValue + 1);//第二个骰子
            int sun = dice1 + dice2;//两个骰子的和
            resultText.text = sun.ToString();//显示和
            timer += Time.deltaTime;
            yield return null;
        }

        // 最终结果
       int finalResult = int.Parse(resultText.text);//把已经变成文本的sum转换成整数
        resultText.text = finalResult.ToString();//显示和
        // 从TurnManager获取当前玩家
        TurnManager turnManager = FindAnyObjectByType<TurnManager>();
        if (turnManager != null)
        {
            turnManager.MoveCurrentPlayer(finalResult);
            Debug.Log($"<color=green>骰子结果：{finalResult}，通知TurnManager移动玩家</color>");
        }
        else
        {
            Debug.LogError("找不到TurnManager!");
        }
        
        // 触发移动
        IsRollComplete = true;
        
        resultText.gameObject.SetActive(true);
    }

    public void ResetDiceState()
    {
        IsRollComplete = false;
        Debug.Log("骰子状态已重置");
    }
}