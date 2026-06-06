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

    // 🌟 기존 호환용 (tileSize 하나로 가로세로 동일)
    public void MoveToPath(List<Node> newPath, float tileSize, Vector2 offset, System.Action onReachDestination)
    {
        MoveToPath(newPath, tileSize, tileSize, offset, onReachDestination);
    }

    // 🌟 가로/세로 타일 간격 분리 버전
    public void MoveToPath(List<Node> newPath, float tileSizeX, float tileSizeY, Vector2 offset, System.Action onReachDestination)
    {
        path = newPath;
        pathIndex = 0;
        StopAllCoroutines();
        StartCoroutine(FollowPath(tileSizeX, tileSizeY, offset, onReachDestination));
    }

    IEnumerator FollowPath(float tileSizeX, float tileSizeY, Vector2 offset, System.Action onReachDestination)
    {
        while (pathIndex < path.Count)
        {
            float targetX = (path[pathIndex].gridX * tileSizeX) - offset.x;
            float targetY = -(path[pathIndex].gridY * tileSizeY) + offset.y;
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

        onReachDestination?.Invoke();
    }
}