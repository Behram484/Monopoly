using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;      // 在这里决定被跟随的角色
    
    [Header("平滑参数")]
    public float smoothTime = 0.2f; // 平滑移动的速度
    public Vector3 offset = new Vector3(0, 0, -10); // 摄像机偏移

    private Vector3 velocity = Vector3.zero; // 当前速度
    void LateUpdate()
    {
        if (target != null)
        {
            // 计算目标位置
            Vector3 targetPosition = target.position + offset;
            
            // 使用平滑阻尼移动摄像机
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref velocity,
                smoothTime
            );
        }
    }
}
