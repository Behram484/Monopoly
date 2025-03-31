using UnityEngine;
using System.Collections;

// 基础移动类 - 作为所有移动类的基类
public class CharacterMovement : MonoBehaviour
{
    [Header("路径配置")]
    public Transform[] waypoints;    // 路径点，按顺时针排列
    
    [Header("移动参数")]
    public float moveSpeed = 3f;     // 移动速度
    public int money = 200;         // 金钱数量
  
    [HideInInspector]
    public int currentIndex = 0;     // 当前所在路径点索引
    [HideInInspector]
    public bool isMoving = false;    // 移动状态标记
    
    protected Animator animator;     // 动画控制器
    
    public bool isBankrupt = false; // 是否破产
    
    // 虚拟的Start方法，可被子类重写
    protected virtual void Start()
    {
        // 尝试获取动画控制器
        animator = GetComponent<Animator>();
        
        // 初始位置设置
        if (waypoints != null && waypoints.Length > 0)
        {
            transform.position = waypoints[currentIndex].position;
        }
        
        // 设置初始动画状态
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }
    }
    
    // 移动指定步数
    public virtual void MoveSteps(int steps)
    {
        if (!isMoving)
        {
            StartCoroutine(MoveCoroutine(steps));
        }
    }
    
    // 核心移动协程
    protected virtual IEnumerator MoveCoroutine(int steps)
    {
        isMoving = true;
        
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
        }
        
        // 设置动画状态
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }
        
        isMoving = false;
        
        // 移动完成后调用事件处理（可由子类重写）
        OnMoveComplete();
    }
    
    // 移动完成后的虚拟方法，可被子类重写
    protected virtual void OnMoveComplete()
    {
        // 基类中不执行任何操作，由子类重写
    }
} 