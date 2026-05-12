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
            // 1. 기본 상대 좌표 계산
            float targetX = (path[pathIndex].gridX * tileSize) - offset.x;
            float targetY = -(path[pathIndex].gridY * tileSize) + offset.y;
            
            // 🌟 2. 수정됨: 부모(Space)의 월드 좌표를 더해 실제 도착 지점 확정
            Vector3 targetPos = transform.parent.position + new Vector3(targetX, targetY, 0);

            // 이동 방향 계산 (애니메이션용)
            Vector3 moveDir = (targetPos - transform.position).normalized;
            if (anim != null)
            {
                anim.SetFloat("DirX", moveDir.x);
                anim.SetFloat("DirY", moveDir.y);
            }

            // 실제 이동
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                // 데이터 매니저의 실시간 속도를 반영하고 싶다면 DataManager.Instance.GetFinalMoveSpeed()를 활용할 수 있습니다.
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }

            pathIndex++;
        }
    }
}