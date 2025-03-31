using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class LandDataImporter : EditorWindow
{
    private string csvFilePath = "";
    private LandData landData;

    [MenuItem("工具/地块数据导入工具")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(LandDataImporter), false, "地块数据导入工具");
    }

    void OnGUI()
    {
        GUILayout.Label("导入地块数据从Excel CSV文件", EditorStyles.boldLabel);
        
        landData = (LandData)EditorGUILayout.ObjectField("地块数据容器", landData, typeof(LandData), false);
        
        if (GUILayout.Button("选择CSV文件"))
        {
            csvFilePath = EditorUtility.OpenFilePanel("选择Excel导出的CSV文件", "", "csv");
        }
        
        EditorGUILayout.LabelField("选择的文件: " + (string.IsNullOrEmpty(csvFilePath) ? "未选择" : csvFilePath));
        
        if (!string.IsNullOrEmpty(csvFilePath) && landData != null)
        {
            if (GUILayout.Button("导入数据"))
            {
                ImportDataFromCSV();
            }
        }
    }

    private void ImportDataFromCSV()
    {
        try
        {
            string[] lines = File.ReadAllLines(csvFilePath);
            if (lines.Length < 4) // 至少需要4行
            {
                EditorUtility.DisplayDialog("错误", "CSV文件格式不正确", "确定");
                return;
            }
            
            landData.lands.Clear();
            
            // 标题在第3行（索引2）
            string[] headers = lines[2].Split(',');
            
            // 从第4行开始处理实际数据
            for (int i = 3; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                
                string[] values = lines[i].Split(',');
                if (values.Length < 5) continue; // 至少需要基本信息
                
                // 尝试解析位置
                int position;
                if (!int.TryParse(values[0], out position)) continue;
                
                LandInfo landInfo = new LandInfo();
                landInfo.position = position;
                landInfo.landName = values[1].Trim(); // Space/property
                
                // 处理Group (第3列索引为2)
                if (values.Length > 2)
                    landInfo.group = values[2].Trim();
                    
                // 处理Action (第4列索引为4)
                if (values.Length > 4)
                    landInfo.action = values[4].Trim();
                    
                // 处理Can be bought? (第5列索引为5)
                if (values.Length > 5)
                    landInfo.canBePurchased = values[5].Trim().ToLower() == "yes";
                    
                // 处理Cost (第7列索引为7)
                if (values.Length > 7)
                    int.TryParse(values[7], out landInfo.purchasePrice);
                    
                // 处理Rent (unimproved) (第8列索引为8)
                if (values.Length > 8)
                    int.TryParse(values[8], out landInfo.toll);
                    
                // 处理1 house到1 hotel (第10-14列)
                if (values.Length > 10)
                    int.TryParse(values[10], out landInfo.rent1House);
                if (values.Length > 11)
                    int.TryParse(values[11], out landInfo.rent2Houses);
                if (values.Length > 12)
                    int.TryParse(values[12], out landInfo.rent3Houses);
                if (values.Length > 13)
                    int.TryParse(values[13], out landInfo.rent4Houses);
                if (values.Length > 14)
                    int.TryParse(values[14], out landInfo.rentHotel);
                
                // 设置地块类型和升级费用
                SetLandTypeAndUpgradeCost(landInfo);
                
                landData.lands.Add(landInfo);
                Debug.Log($"添加地块: {landInfo.position}.{landInfo.landName}, 可购买={landInfo.canBePurchased}, 价格={landInfo.purchasePrice}");
            }
            
            EditorUtility.SetDirty(landData);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("成功", $"成功导入 {landData.lands.Count} 个地块数据", "确定");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"导入CSV错误: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("错误", $"导入失败: {e.Message}", "确定");
        }
    }
    
    private LandType ParseLandType(string typeStr)
    {
        switch (typeStr.ToLower())
        {
            case "normal": return LandType.Normal;
            case "commercial": return LandType.Commercial;
            case "event": return LandType.Event;
            case "start": return LandType.Start;
            case "jail": return LandType.Jail;
            case "hospital": return LandType.Hospital;
            case "chance": return LandType.Chance;
            case "tax": return LandType.Tax;
            default: return LandType.Normal;
        }
    }

    private void SetLandTypeAndUpgradeCost(LandInfo landInfo)
    {
        // 实现设置地块类型和升级费用的逻辑
        // 这里需要根据你的数据结构和逻辑来实现
        landInfo.landType = ParseLandType(landInfo.landName);
        landInfo.upgradeCost = 0; // 默认值，需要根据实际数据设置
    }
}
