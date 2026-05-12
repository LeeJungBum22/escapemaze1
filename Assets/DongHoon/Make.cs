using UnityEngine;

public class Make : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject floorPrefab1; 
    public GameObject floorPrefab2; 
    public GameObject wallPrefab;   

    [Header("Maze Grid Settings")]
    public int sizex = 17; 
    public int sizey = 9;  

    [Header("Display Settings")]
    [Range(0.1f, 5.0f)]
    public float displayScale = 1.0f; // 전체 미로의 출력 크기

    void Start()
    {
        GenerateMaze();
    }

    public void GenerateMaze()
    {
        // 1. 기존 생성된 미로 삭제 (실시간 수정 대응)
        foreach (Transform child in transform) {
            if(Application.isPlaying) Destroy(child.gameObject);
            else DestroyImmediate(child.gameObject);
        }

        // 2. 미로 데이터 생성
        int[,] maze = new int[sizey, sizex];
        maze = InitBoard(maze);
        maze = GeneratedByBinaryTree(maze);

        // 3. 타일의 원본 크기 측정 (Sprite의 실제 크기)
        float originalTileSize = 1f; 
        SpriteRenderer sr = floorPrefab1.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // 배율 1일 때 Sprite가 월드에서 차지하는 가로 길이
            originalTileSize = sr.sprite.bounds.size.x; 
        }

        // 4. 실제 배치 간격 계산
        float scaledTileSize = originalTileSize * displayScale;

        // 5. 중앙 정렬 오프셋
        float offsetX = (sizex - 1) * scaledTileSize / 2f;
        float offsetY = (sizey - 1) * scaledTileSize / 2f;

        // [중요] 틈새 방지를 위한 미세 보정값 (1.001배)
        // 타일을 아주 미세하게 겹치게 하여 1픽셀 선이 보이는 현상을 막습니다.
        Vector3 finalScale = Vector3.one * displayScale * 1.001f;

        for (int y1 = 0; y1 < sizey; y1++)
        {
            for (int x1 = 0; x1 < sizex; x1++)
            {
                float posX = (x1 * scaledTileSize) - offsetX;
                float posY = -(y1 * scaledTileSize) + offsetY;
                Vector3 spawnPos = new Vector3(posX, posY, 0);

                // 6. 바닥 생성
                GameObject floorObj;
                if ((x1 + y1) % 2 == 0)
                    floorObj = Instantiate(floorPrefab1, spawnPos, Quaternion.identity);
                else
                    floorObj = Instantiate(floorPrefab2, spawnPos, Quaternion.identity);

                floorObj.transform.localScale = finalScale; // 보정된 크기 적용
                floorObj.transform.parent = this.transform;
                floorObj.GetComponent<SpriteRenderer>().sortingOrder = 0;

                // 7. 벽 생성
                if (maze[y1, x1] == 1)
                {
                    GameObject wallObj = Instantiate(wallPrefab, spawnPos, Quaternion.identity);
                    wallObj.transform.localScale = finalScale; // 벽도 동일하게 적용
                    wallObj.transform.parent = this.transform;
                    wallObj.GetComponent<SpriteRenderer>().sortingOrder = 1;
                }
            }
        }
    }

    // --- 미로 알고리즘 (기존 로직 유지) ---
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
        for (int y = 0; y < sizey; y++)
        {
            for (int x = 0; x < sizex; x++)
            {
                if (x % 2 == 0 || y % 2 == 0) continue;
                if (x == sizex - 2 && y == sizey - 2) continue;
                if (x == sizex - 2) { maze[y + 1, x] = 0; continue; }
                if (y == sizey - 2) { maze[y, x + 1] = 0; continue; }

                int rand = Random.Range(0, 2);
                if (rand == 0) maze[y + 1, x] = 0;
                else maze[y, x + 1] = 0;
            }
        }
        return maze;
    }
}