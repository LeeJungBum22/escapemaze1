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
    [Tooltip("PPU 16일 때 1.0은 너무 큽니다. 0.2 ~ 0.5 사이를 추천합니다.")]
    [Range(0.01f, 2.0f)] 
    public float displayScale = 0.3f; // 0.3 정도로 낮춰서 시작해보세요.

    void Start()
    {
        GenerateMaze();
    }

    // 인스펙터에서 값이 바뀔 때마다 즉시 확인하고 싶다면 아래 주석을 해제하세요.
    // void OnValidate() { if(floorPrefab1 != null) GenerateMaze(); }

    public void GenerateMaze()
    {
        // 1. 기존 생성물 삭제 (중복 생성 방지)
        foreach (Transform child in transform) {
            if (Application.isPlaying) Destroy(child.gameObject);
            else DestroyImmediate(child.gameObject);
        }

        // 2. 미로 데이터 생성
        int[,] maze = new int[sizey, sizex];
        maze = InitBoard(maze);
        maze = GeneratedByBinaryTree(maze);

        // 3. 타일 간격 계산 (PPU 16일 때 원본 크기는 1.0입니다)
        float originalTileSize = 1f; 
        SpriteRenderer sr = floorPrefab1.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            originalTileSize = sr.sprite.bounds.size.x; 
        }

        // 4. 배율이 적용된 실제 배치 간격
        float scaledTileSize = originalTileSize * displayScale;

        // 5. 중앙 정렬 오프셋
        float offsetX = (sizex - 1) * scaledTileSize / 2f;
        float offsetY = (sizey - 1) * scaledTileSize / 2f;

        // 틈새 방지용 미세 보정 (1.001)
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

                floorObj.transform.localScale = finalScale;
                floorObj.transform.parent = this.transform;
                floorObj.GetComponent<SpriteRenderer>().sortingOrder = 0;

                // 7. 벽 생성
                if (maze[y1, x1] == 1)
                {
                    GameObject wallObj = Instantiate(wallPrefab, spawnPos, Quaternion.identity);
                    wallObj.transform.localScale = finalScale;
                    wallObj.transform.parent = this.transform;
                    wallObj.GetComponent<SpriteRenderer>().sortingOrder = 1;
                }
            }
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