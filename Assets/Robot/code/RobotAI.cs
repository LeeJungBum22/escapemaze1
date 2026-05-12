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

    public void MoveToPath(List<Node> newPath, float tileSize, Vector2 offset)
    {
        path = newPath;
        pathIndex = 0;
        StopAllCoroutines();
        StartCoroutine(FollowPath(tileSize, offset));
    }

    IEnumerator FollowPath(float tileSize, Vector2 offset)
    {
        while (pathIndex < path.Count)
        {
            // 배열 인덱스를 유니티 월드 좌표로 변환
            float targetX = (path[pathIndex].gridX * tileSize) - offset.x;
            float targetY = -(path[pathIndex].gridY * tileSize) + offset.y;
            Vector3 targetPos = new Vector3(targetX, targetY, 0);

            // 이동 방향 계산 (애니메이션용)
            Vector3 moveDir = (targetPos - transform.position).normalized;
            anim.SetFloat("DirX", moveDir.x);
            anim.SetFloat("DirY", moveDir.y);

            // 실제 이동
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }

            pathIndex++;
        }
    }
}