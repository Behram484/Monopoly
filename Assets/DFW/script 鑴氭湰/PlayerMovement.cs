using UnityEngine;
using System.Collections;

// 玩家移动类 - 继承自基础移动类，添加玩家特有功能
public class PlayerMovement : CharacterMovement
{
    // 重写基类的Start方法
    protected override void Start()
    {
        // 调用基类的Start方法
        base.Start();
        
        // 玩家特有的初始化逻辑
        if (animator == null)
        {
            Debug.LogError("Animator组件未找到！");
        }
    }
    
    // 重写移动完成方法 - 玩家移动后可能需要执行特定操作
    protected override void OnMoveComplete()
    {
        // 玩家移动完成后的逻辑
        // 例如：等待玩家输入，或触发UI界面等
        
        // 获取当前地块
        Land currentLand = null;
        if (waypoints != null && waypoints.Length > 0 && currentIndex < waypoints.Length)
        {
            currentLand = waypoints[currentIndex].GetComponent<Land>();
        }
        
        // 如果存在地块，可以手动触发地块事件（这里不像AI自动触发）
        if (currentLand != null)
        {
            // 此处可以添加玩家特有的地块交互逻辑
            // 例如通知GameManager玩家已到达某地块
            currentLand.OnPlayerLanded(0); // 假设玩家索引为0
        }
    }
    public void MoveToIndex(int targetIndex)
{
    currentIndex = targetIndex;
    transform.position = TurnManager.Instance.waypoints[targetIndex].position;
    isMoving = false;
}

} 