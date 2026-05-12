using UnityEngine;
using System.Collections.Generic;

public class Make : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject floorPrefab1; 
    public GameObject floorPrefab2; 
    public GameObject wallPrefab;
    public GameObject markerPrefab; 
    public GameObject robotPrefab;  

    [Header("Maze Grid Settings")]
    public int sizex = 17; 
    public int sizey = 9;  

    [Header("Display Settings")]
    [Range(0.01f, 2.0f)] 
    public float displayScale = 0.3f;

    private int[,] maze;
    private float scaledTileSize;
    private float offsetX;
    private float offsetY;
    private Vector3 finalScale; // 로봇에게도 적용하기 위해 전역 변수화

    void Start()
    {
        GenerateMaze();
    }

    public void GenerateMaze()
    {
        // 1. 기존 생성물 삭제
        foreach (Transform child in transform) {
            if (Application.isPlaying) Destroy(child.gameObject);
            else DestroyImmediate(child.gameObject);
        }

        // 2. 미로 데이터 생성
        maze = new int[sizey, sizex];
        maze = InitBoard(maze);
        maze = GeneratedByBinaryTree(maze);

        // 3. 출발지/도착지 좌표 설정
        Vector2Int startPos = FindEmptySpace(1, 1, 1, 1);
        maze[startPos.y, startPos.x] = 3;

        Vector2Int endPos = FindEmptySpace(sizex - 2, sizey - 2, -1, -1);
        maze[endPos.y, endPos.x] = 4;

        // 4. 타일 크기 및 오프셋 계산
        float originalTileSize = 1f; 
        SpriteRenderer sr = floorPrefab1.GetComponent<SpriteRenderer>();
        if (sr != null) originalTileSize = sr.sprite.bounds.size.x; 

        scaledTileSize = originalTileSize * displayScale;
        offsetX = (sizex - 1) * scaledTileSize / 2f;
        offsetY = (sizey - 1) * scaledTileSize / 2f;
        
        // 틈새 방지용 미세 보정값 (로봇에게도 동일하게 적용)
        finalScale = Vector3.one * displayScale * 1.001f;

        // 5. 맵 생성 (바닥, 벽, 마커)
        for (int y = 0; y < sizey; y++)
        {
            for (int x = 0; x < sizex; x++)
            {
                Vector3 spawnPos = GetWorldPos(x, y);

                // 바닥 생성
                GameObject floorObj = Instantiate((x + y) % 2 == 0 ? floorPrefab1 : floorPrefab2, spawnPos, Quaternion.identity);
                floorObj.transform.localScale = finalScale;
                floorObj.transform.parent = this.transform;
                floorObj.GetComponent<SpriteRenderer>().sortingOrder = 0;

                // 벽 생성
                if (maze[y, x] == 1)
                {
                    GameObject wallObj = Instantiate(wallPrefab, spawnPos, Quaternion.identity);
                    wallObj.transform.localScale = finalScale;
                    wallObj.transform.parent = this.transform;
                    wallObj.GetComponent<SpriteRenderer>().sortingOrder = 1;
                }
                // 마커 생성
                else if (maze[y, x] == 3 || maze[y, x] == 4)
                {
                    GameObject markerObj = Instantiate(markerPrefab, spawnPos, Quaternion.identity);
                    markerObj.transform.localScale = finalScale;
                    markerObj.transform.parent = this.transform;
                    markerObj.GetComponent<SpriteRenderer>().sortingOrder = 2;
                }
            }
        }

        // 6. 로봇 생성 및 이동 시작
        StartAStarRobot(startPos, endPos);
    }

    Vector3 GetWorldPos(int x, int y)
    {
        float posX = (x * scaledTileSize) - offsetX;
        float posY = -(y * scaledTileSize) + offsetY;
        return new Vector3(posX, posY, 0);
    }

    Vector2Int FindEmptySpace(int startX, int startY, int moveX, int moveY)
    {
        for (int y = startY; y >= 1 && y < sizey - 1; y += moveY)
        {
            for (int x = startX; x >= 1 && x < sizex - 1; x += moveX)
            {
                if (maze[y, x] == 0) return new Vector2Int(x, y);
            }
        }
        return new Vector2Int(startX, startY);
    }

    void StartAStarRobot(Vector2Int start, Vector2Int end)
    {
        GameObject robot = Instantiate(robotPrefab, GetWorldPos(start.x, start.y), Quaternion.identity);
        robot.transform.localScale = finalScale * 1.5f; 
        
        SpriteRenderer robotSr = robot.GetComponent<SpriteRenderer>();
        if (robotSr != null) robotSr.sortingOrder = 5; 

        Pathfinding pathfinder = GetComponent<Pathfinding>();
        if (pathfinder != null)
        {
            // 탐색 시작! (코루틴 실행 후, 완료되면 콜백 함수인 delegate 안의 내용이 실행됨)
            pathfinder.StartVisualSearch(maze, start, end, scaledTileSize, new Vector2(offsetX, offsetY), delegate(List<Node> path) 
            {
                if (path != null)
                {
                    RobotAI ai = robot.GetComponent<RobotAI>();
                    if (ai != null)
                    {
                        // 탐색 연출이 다 끝난 뒤, 실제 로봇이 경로를 따라 이동 시작
                        ai.MoveToPath(path, scaledTileSize, new Vector2(offsetX, offsetY));
                    }
                }
            });
        }
    }

    int[,] InitBoard(int[,] maze)
    {
        for (int y = 0; y < sizey; y++)
        {
            for (int x = 0; x < sizex; x++)
            {
                if (x % 2 == 0 || y % 2 == 0) maze[y, x] = 1;
                else maze[y, x] = 0;
            }
        }
        return maze;
    }

    int[,] GeneratedByBinaryTree(int[,] maze)
    {
        for (int y = 1; y < sizey; y += 2)
        {
            for (int x = 1; x < sizex; x += 2)
            {
                if (x >= sizex - 2 && y >= sizey - 2) continue;
                if (x >= sizex - 2) { if (y + 1 < sizey - 1) maze[y + 1, x] = 0; continue; }
                if (y >= sizey - 2) { if (x + 1 < sizex - 1) maze[y, x + 1] = 0; continue; }

                int rand = Random.Range(0, 2);
                if (rand == 0) maze[y + 1, x] = 0;
                else maze[y, x + 1] = 0;
            }
        }
        return maze;
    }
}