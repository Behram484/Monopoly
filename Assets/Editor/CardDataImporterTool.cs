using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;

public class CardImporterTool : EditorWindow
{
    private TextAsset csvFile;
    private string savePath = "Assets/Resources/Cards";
    
    [MenuItem("工具/卡片导入工具")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(CardImporterTool), false, "卡片导入工具");
    }
    
    void OnGUI()
    {
        GUILayout.Label("卡片数据导入工具", EditorStyles.boldLabel);
        
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV文件", csvFile, typeof(TextAsset), false);
        savePath = EditorGUILayout.TextField("保存路径", savePath);
        
        if(GUILayout.Button("导入卡片"))
        {
            ImportCards();
        }
    }
    
    void ImportCards()
    {
        if (csvFile == null)
        {
            EditorUtility.DisplayDialog("错误", "请选择CSV文件", "确定");
            return;
        }

        // 确保目录存在
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        string[] lines = csvFile.text.Split('\n');
        bool isPotLuck = true; // 默认是机会卡
        int cardsCreated = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            
            // 跳过空行
            if (string.IsNullOrEmpty(line)) continue;
            
            // 检测当前处理的是哪种卡片
            if (line.Contains("Pot luck card data"))
            {
                isPotLuck = true;
                continue;
            }
            else if (line.Contains("Opportunity knocks card data"))
            {
                isPotLuck = false;
                continue;
            }
            
            // 跳过标题行
            if (line.Contains("Description") || line.Contains("Action")) continue;
            
            string[] values = line.Split(',');
            if (values.Length >= 2)
            {
                string description = values[0].Replace("\"", "").Trim();
                string action = values[1].Replace("\"", "").Trim();
                
                // 创建卡片资产
                CardData cardData = CreateCardAsset(description, action, isPotLuck);
                if (cardData != null)
                {
                    cardsCreated++;
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("成功", $"成功导入 {cardsCreated} 张卡片!", "确定");
    }

    private CardData CreateCardAsset(string description, string action, bool isPotLuck)
    {
        // 创建ScriptableObject
        CardData cardData = ScriptableObject.CreateInstance<CardData>();
        cardData.cardTitle = description.Length > 20 ? description.Substring(0, 20) + "..." : description;
        cardData.description = description;
        cardData.cardType = isPotLuck ? Card.CardType.Community : Card.CardType.Chance;
        
        // 解析动作类型和值
        ParseAction(cardData, action);
        
        // 设置唯一文件名
        string cardName = cardData.cardTitle.Replace(" ", "_").Replace(".", "").Replace(",", "");
        string type = isPotLuck ? "PotLuck" : "Opportunity";
        string assetPath = $"{savePath}/{type}_{cardName}.asset";
        
        // 创建资产文件
        AssetDatabase.CreateAsset(cardData, assetPath);
        return cardData;
    }

    private void ParseAction(CardData card, string action)
    {
        if (action.Contains("Bank pays player"))
        {
            card.actionType = Card.CardActionType.CollectMoney;
            string amountStr = action.Replace("Bank pays player ", "").Replace("£", "").Trim();
            int.TryParse(amountStr, out int amount);
            card.value = amount;
        }
        else if (action.Contains("Player pays") && action.Contains("bank"))
        {
            card.actionType = Card.CardActionType.PayMoney;
            string amountStr = action.Replace("Player pays ", "").Replace("to the bank", "").Replace("£", "").Trim();
            int.TryParse(amountStr, out int amount);
            card.value = amount;
        }
        else if (action.Contains("jail") || action.Contains("Go to jail"))
        {
            card.actionType = Card.CardActionType.MoveToPrison;
        }
        else if (action.Contains("from each player"))
        {
            card.actionType = Card.CardActionType.Birthday;
            string amountStr = action.Replace("Player receives ", "").Replace("from each player", "").Replace("£", "").Trim();
            int.TryParse(amountStr, out int amount);
            card.value = amount;
        }
        else if (action.Contains("Get out of jail"))
        {
            card.actionType = Card.CardActionType.GetOutOfPrison;
        }
        else if (action.Contains("backwards"))
        {
            card.actionType = Card.CardActionType.MoveBack;
        }
        else if (action.Contains("forwards") || action.Contains("Advance"))
        {
            card.actionType = Card.CardActionType.MoveForward;
        }
        else
        {
            // 默认收钱
            card.actionType = Card.CardActionType.CollectMoney;
            card.value = 0;
        }
    }
}
#endif
