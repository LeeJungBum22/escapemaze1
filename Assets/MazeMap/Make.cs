using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro; // TextMeshPro 사용

public class Make : MonoBehaviour
{
    [Header("Tycoon Settings")]
    public AlgorithmType assignedAlgorithm; // 이 공터에서 돌아갈 알고리즘
    
    // 🌟 수정됨: TextMeshPro -> TextMeshProUGUI (캔버스 UI용으로 변경)
    public TextMeshProUGUI statusText;          

    [Header("Prefabs")]
    public GameObject floorPrefab1; 
    public GameObject floorPrefab2; 
    public GameObject wallPrefab;
    public GameObject markerPrefab; 
    public GameObject robotPrefab;  

    [Header("Maze Grid Settings")]
    public int sizex = 17; 
    public int sizey = 9;  
    [Range(0.01f, 2.0f)] public float displayScale = 0.3f;

    private int[,] maze;
    private float scaledTileSize;
    private float offsetX;
    private float offsetY;
    private Vector3 finalScale; 

    private Vector2Int startPos;
    private Vector2Int endPos;
    
    private Transform mazeContainer; 

    void Start()
    {
        mazeContainer = new GameObject("MazeContainer").transform;
        mazeContainer.SetParent(this.transform);
        mazeContainer.localPosition = Vector3.zero;

        StartCoroutine(TycoonLoop());
    }

    IEnumerator TycoonLoop()
    {
        while (true)
        {
            var bestRobot = GetBestRobot();

            if (bestRobot == null)
            {
                statusText.text = "공사중...";
                statusText.gameObject.SetActive(true);
                yield return new WaitForSeconds(1f);
                continue; 
            }

            statusText.gameObject.SetActive(false); 
            GenerateMazeMap();

            bool isEscaped = false;
            StartRobotEscape(bestRobot, () => { isEscaped = true; });
            
            yield return new WaitUntil(() => isEscaped);

            bool isCrit;
            double goldReward = DataManager.Instance.GetFinalGoldReward(bestRobot, out isCrit);
            int diaReward = DataManager.Instance.CheckDiamondDropAmount();
            
            DataManager.Instance.AddGold(goldReward);
            bestRobot.mazeEscapeCount++; 

            string rewardText = "";
            if (isCrit) rewardText += "<color=#FF3333>치명타!</color>\n";
            rewardText += $"<color=#FFFF00>+{CurrencyFormatter.FormatKorean(goldReward).Replace("\n", "")}</color>";
            if (diaReward > 0) rewardText += $"\n<color=#FF3333>다이아 +{diaReward}개</color>";

            statusText.text = rewardText;
            statusText.gameObject.SetActive(true);

            yield return new WaitForSeconds(0.5f);

            ClearMaze();
            
            float cooldown = DataManager.Instance.GetFinalMazeRegenTime(bestRobot);
            while (cooldown > 0)
            {
                statusText.text = $"재생성 대기중...\n{cooldown:F1}초";
                cooldown -= Time.deltaTime;
                yield return null;
            }
        }
    }

    DataManager.RobotInstance GetBestRobot()
    {
        DataManager.RobotInstance best = null;
        int highestPower = -1;

        foreach (var r in DataManager.Instance.myRobots)
        {
            if (DataManager.Instance.robotConfigs[r.robotId].algo == assignedAlgorithm)
            {
                int power = (r.star * 100) + r.level;
                if (power > highestPower)
                {
                    highestPower = power;
                    best = r;
                }
            }
        }
        return best;
    }

    void GenerateMazeMap()
    {
        maze = new int[sizey, sizex];
        maze = InitBoard(maze);
        maze = GeneratedByBinaryTree(maze);

        startPos = FindEmptySpace(1, 1, 1, 1);
        maze[startPos.y, startPos.x] = 3;

        endPos = FindEmptySpace(sizex - 2, sizey - 2, -1, -1);
        maze[endPos.y, endPos.x] = 4;

        float originalTileSize = 1f; 
        SpriteRenderer sr = floorPrefab1.GetComponent<SpriteRenderer>();
        if (sr != null) originalTileSize = sr.sprite.bounds.size.x; 

        scaledTileSize = originalTileSize * displayScale;
        offsetX = (sizex - 1) * scaledTileSize / 2f;
        offsetY = (sizey - 1) * scaledTileSize / 2f;
        finalScale = Vector3.one * displayScale * 1.001f;

        for (int y = 0; y < sizey; y++)
        {
            for (int x = 0; x < sizex; x++)
            {
                Vector3 spawnPos = GetWorldPos(x, y);

                GameObject floorObj = Instantiate((x + y) % 2 == 0 ? floorPrefab1 : floorPrefab2, spawnPos, Quaternion.identity);
                floorObj.transform.localScale = finalScale;
                floorObj.transform.parent = mazeContainer; 
                floorObj.GetComponent<SpriteRenderer>().sortingOrder = 0;

                if (maze[y, x] == 1)
                {
                    GameObject wallObj = Instantiate(wallPrefab, spawnPos, Quaternion.identity);
                    wallObj.transform.localScale = finalScale;
                    wallObj.transform.parent = mazeContainer; 
                    wallObj.GetComponent<SpriteRenderer>().sortingOrder = 1;
                }
                else if (maze[y, x] == 3 || maze[y, x] == 4)
                {
                    GameObject markerObj = Instantiate(markerPrefab, spawnPos, Quaternion.identity);
                    markerObj.transform.localScale = finalScale;
                    markerObj.transform.parent = mazeContainer; 
                    markerObj.GetComponent<SpriteRenderer>().sortingOrder = 2;
                }
            }
        }
    }

    void StartRobotEscape(DataManager.RobotInstance bot, System.Action onComplete)
    {
        GameObject robot = Instantiate(robotPrefab, GetWorldPos(startPos.x, startPos.y), Quaternion.identity);
        robot.transform.localScale = finalScale * 1.5f; 
        robot.transform.parent = mazeContainer; 
        robot.GetComponent<SpriteRenderer>().sortingOrder = 5; 

        Pathfinding pathfinder = GetComponent<Pathfinding>();
        if (pathfinder != null)
        {
            pathfinder.selectedAlgorithm = assignedAlgorithm;
            pathfinder.searchDelay = DataManager.Instance.GetFinalSearchDelay(bot);

            pathfinder.StartVisualSearch(maze, startPos, endPos, scaledTileSize, new Vector2(offsetX, offsetY), delegate(List<Node> path) 
            {
                if (path != null)
                {
                    RobotAI ai = robot.GetComponent<RobotAI>();
                    if (ai != null)
                    {
                        ai.moveSpeed = DataManager.Instance.GetFinalMoveSpeed(bot);
                        ai.MoveToPath(path, scaledTileSize, new Vector2(offsetX, offsetY), onComplete);
                    }
                }
                else
                {
                    onComplete?.Invoke();
                }
            });
        }
    }

    void ClearMaze()
    {
        foreach (Transform child in mazeContainer)
        {
            Destroy(child.gameObject);
        }
        GetComponent<Pathfinding>().ClearMarkers();
    }

    public Vector3 GetWorldPos(int x, int y)
    {
        float posX = (x * scaledTileSize) - offsetX;
        float posY = -(y * scaledTileSize) + offsetY;
        return transform.position + new Vector3(posX, posY, 0);
    }

    Vector2Int FindEmptySpace(int startX, int startY, int moveX, int moveY) { for (int y = startY; y >= 1 && y < sizey - 1; y += moveY) { for (int x = startX; x >= 1 && x < sizex - 1; x += moveX) { if (maze[y, x] == 0) return new Vector2Int(x, y); } } return new Vector2Int(startX, startY); }
    int[,] InitBoard(int[,] maze) { for (int y = 0; y < sizey; y++) { for (int x = 0; x < sizex; x++) { if (x % 2 == 0 || y % 2 == 0) maze[y, x] = 1; else maze[y, x] = 0; } } return maze; }
    int[,] GeneratedByBinaryTree(int[,] maze) { for (int y = 1; y < sizey; y += 2) { for (int x = 1; x < sizex; x += 2) { if (x >= sizex - 2 && y >= sizey - 2) continue; if (x >= sizex - 2) { if (y + 1 < sizey - 1) maze[y + 1, x] = 0; continue; } if (y >= sizey - 2) { if (x + 1 < sizex - 1) maze[y, x + 1] = 0; continue; } int rand = Random.Range(0, 2); if (rand == 0) maze[y + 1, x] = 0; else maze[y, x + 1] = 0; } } return maze; }
}