using UnityEngine;

public class Node
{
    public bool walkable;    // 갈 수 있는 길인가?
    public int gridX, gridY; // 배열 상의 좌표
    public int gCost;        // 시작점에서 현재 노드까지의 거리
    public int hCost;        // 현재 노드에서 목적지까지의 예상 거리
    public Node parent;      // 경로 추적을 위한 부모 노드

    public Node(bool _walkable, int _gridX, int _gridY)
    {
        walkable = _walkable;
        gridX = _gridX;
        gridY = _gridY;
    }

    // F = G + H (최종 비용)
    public int fCost { get { return gCost + hCost; } }
}