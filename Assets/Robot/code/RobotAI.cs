using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAI : MonoBehaviour
{
    public float moveSpeed = 3f;
    private Animator anim;
    private List<Node> path;
    private int pathIndex = 0;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // 🌟 수정됨: 도착 시 실행할 Action 추가
    public void MoveToPath(List<Node> newPath, float tileSize, Vector2 offset, System.Action onReachDestination)
    {
        path = newPath;
        pathIndex = 0;
        StopAllCoroutines();
        StartCoroutine(FollowPath(tileSize, offset, onReachDestination));
    }

    IEnumerator FollowPath(float tileSize, Vector2 offset, System.Action onReachDestination)
    {
        while (pathIndex < path.Count)
        {
            float targetX = (path[pathIndex].gridX * tileSize) - offset.x;
            float targetY = -(path[pathIndex].gridY * tileSize) + offset.y;
            Vector3 targetPos = transform.parent.position + new Vector3(targetX, targetY, 0);

            Vector3 moveDir = (targetPos - transform.position).normalized;
            if (anim != null)
            {
                anim.SetFloat("DirX", moveDir.x);
                anim.SetFloat("DirY", moveDir.y);
            }

            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }

            pathIndex++;
        }
        
        // 🌟 도착 완료 시 콜백 실행!
        onReachDestination?.Invoke();
    }
}