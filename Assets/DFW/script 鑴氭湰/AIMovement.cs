using UnityEngine;
using System.Collections;

// AI移动类 - 继承自基础移动类，添加AI特有功能
public class AIMovement : CharacterMovement
{
    [Header("AI特定属性")]
    public int aiPlayerIndex = 1; // AI玩家索引，默认为1（玩家通常为0）
    
    // 重写基类的Start方法
    protected override void Start()
    {
        // 调用基类的Start方法
        base.Start();
        
        // AI特有的初始化逻辑可以在这里添加
    }
    
    // 重写移动完成方法 - AI移动完成后自动触发地块事件
    protected override void OnMoveComplete()
    {
        Debug.Log($"<color=yellow>AI移动完成，当前位置：{currentIndex}</color>");
        // 触发地块事件
        Land currentLand = null;
        if (waypoints != null && waypoints.Length > 0 && currentIndex < waypoints.Length)
        {
            currentLand = waypoints[currentIndex].GetComponent<Land>();
        }
        
        if (currentLand != null)
        {
            // AI触发地块事件
            currentLand.OnPlayerLanded(aiPlayerIndex);
            
            // 或者通知回合管理器AI已完成移动
            TurnManager turnManager = FindAnyObjectByType<TurnManager>();
            if (turnManager != null)
            {
                turnManager.HandleLandEvent(currentLand, gameObject);
            }
        }
    }

    // 在AIMovement.cs文件中修改MoveSteps方法
    public override void MoveSteps(int steps)
    {
        Debug.Log($"<color=yellow>AI尝试移动{steps}步</color>");
        Debug.Log($"AI {gameObject.name} 开始移动 {steps} 步");
        base.MoveSteps(steps);
    }

    // 修改基类CharacterMovement.cs中的MoveCoroutine方法
    protected override IEnumerator MoveCoroutine(int steps)
    {
        isMoving = true;
        
        if (gameObject.CompareTag("Player") == false) // 仅对AI进行日志
            Debug.Log($"AI移动开始，目标步数: {steps}");
        
        // 设置动画状态
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }
        
        int targetIndex = currentIndex + steps;
        
        // 处理路径循环（如超过长度则回到起点）
        if (targetIndex >= waypoints.Length)
            targetIndex %= waypoints.Length;

        while (currentIndex != targetIndex)
        {
            // 计算下一个路径点索引
            int nextIndex = (currentIndex + 1) % waypoints.Length;
            Vector3 targetPos = waypoints[nextIndex].position;
            
            if (gameObject.CompareTag("Player") == false) // 仅对AI进行日志
                Debug.Log($"AI正在移动到路径点 {nextIndex}");
              
            // 移动到下一个路径点
            while (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }

            transform.position = targetPos;
            currentIndex = nextIndex;
            
            if (gameObject.CompareTag("Player") == false) // 仅对AI进行日志
                Debug.Log($"AI到达路径点 {currentIndex}");
        }
        
        // 设置动画状态
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }
        
        isMoving = false;
        
        if (gameObject.CompareTag("Player") == false) // 仅对AI进行日志
            Debug.Log($"AI移动完成，当前位置: {currentIndex}");
        
        // 移动完成后调用事件处理（可由子类重写）
        OnMoveComplete();
    }
    public void MoveToIndex(int targetIndex)
{
    currentIndex = targetIndex;
    transform.position = TurnManager.Instance.waypoints[targetIndex].position;
    isMoving = false;
}
} 