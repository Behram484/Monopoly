using UnityEngine;

public class MapInitializer : MonoBehaviour
{
    public LandData landData;
    public Transform waypointManager;
    
    void Start()
    {
        if (landData == null || waypointManager == null) return;
        
        // 确保数据数量和路径点数量匹配
        if (landData.lands.Count > waypointManager.childCount)
        {
            Debug.LogWarning($"地块数据({landData.lands.Count})比路径点({waypointManager.childCount})多!");
        }
        
        // 为每个路径点上的Land组件应用数据
        for (int i = 0; i < Mathf.Min(landData.lands.Count, waypointManager.childCount); i++)
        {
            Transform waypoint = waypointManager.GetChild(i);
            Land land = waypoint.GetComponent<Land>();
            
            if (land != null)
            {
                land.ApplyLandInfo(landData.lands[i]);
                Debug.Log($"初始化地块: {land.landName}");
            }
        }
    }

#if UNITY_EDITOR
[UnityEditor.MenuItem("工具/立即应用地块数据")]
public static void ApplyLandDataNow()
{
    MapInitializer map = FindAnyObjectByType<MapInitializer>();
    if (map != null)
    {
        Debug.Log("手动应用地块数据");
        map.ApplyDataManually();
    }
}

public void ApplyDataManually()
{
    if (landData == null || waypointManager == null) 
    {
        Debug.LogError("landData或waypointManager未设置!");
        return;
    }
    
    Debug.Log($"开始应用数据: {landData.lands.Count}条记录到{waypointManager.childCount}个路径点");
    
    for (int i = 0; i < Mathf.Min(landData.lands.Count, waypointManager.childCount); i++)
    {
        Transform waypoint = waypointManager.GetChild(i);
        Land land = waypoint.GetComponent<Land>();
        
        if (land != null)
        {
            LandInfo info = landData.lands[i];
            Debug.Log($"应用数据到{waypoint.name}: 名称={info.landName}, 可购买={info.canBePurchased}");
            land.ApplyLandInfo(info);
        }
    }
}
#endif
}
