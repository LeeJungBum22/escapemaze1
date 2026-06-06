using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class StageMap : MonoBehaviour
{
    [Header("Prefabs (Make.cs와 동일)")]
    public GameObject floorPrefab1;
    public GameObject floorPrefab2;
    public GameObject wallPrefab;
    public GameObject markerPrefab;
    public GameObject[] robotPrefabs = new GameObject[9];

    [Header("🌟 미로 생성 위치 (월드 공간 빈 오브젝트)")]
    public Transform mazeWorldOrigin;

    [Header("🌟 미로가 들어갈 UI 영역 (StageMapArea)")]
    public RectTransform stageMapArea;

    [Header("🌟 미로 보일 때 끌 배경 (MazeSpaces - 타이쿤 미로들)")]
    public GameObject panelBackground;

    [Header("🌟 battlepanel 자체의 주황 배경 Image")]
    public Image panelImage;

    [Header("🌟 추가로 끌 오브젝트 (Canvas1 - 공사중 텍스트들)")]
    public GameObject extraBackground;

    [Header("Maze Grid Settings")]
    public int sizex = 17;
    public int sizey = 11;
    [Range(0.01f, 1.8f)] public float displayScale = 0.8f;

    [Header("🌟 수동 위치 및 크기 조절")]
    public float manualOffsetX = -0.5f;
    public float manualScaleX = 0.05f;

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI stageInfoText;
    public GameObject selectUI;
    public GameObject stageUI;
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Button retryButton;
    public Button backButton;

    // 내부 상태
    private int[,] maze;
    private float scaledTileSize;
    private float stepX;
    private float stepY;
    private float offsetX;
    private float offsetY;
    private Vector3 finalScale;
    private Vector2Int startPos;
    private Vector2Int endPos;
    private Transform mazeContainer;
    private Coroutine stageCoroutine;
    private DataManager.RobotInstance currentRobot;

    void Awake()
    {
        mazeContainer = new GameObject("StageMazeContainer").transform;
        if (mazeWorldOrigin != null)
            mazeContainer.SetParent(mazeWorldOrigin);
        mazeContainer.localPosition = Vector3.zero;
    }

    void Start()
    {
        if (resultPanel != null) resultPanel.SetActive(false);

        if (retryButton != null)
            retryButton.onClick.AddListener(OnClickRetry);
        if (backButton != null)
            backButton.onClick.AddListener(OnClickBack);
    }

    void OnEnable()
    {
        if (selectUI != null) selectUI.SetActive(true);
        if (stageUI != null) stageUI.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);

        if (panelBackground != null) panelBackground.SetActive(true);
        if (panelImage != null) panelImage.enabled = true;
        if (extraBackground != null) extraBackground.SetActive(true);
    }

    void OnDisable()
    {
        if (panelBackground != null) panelBackground.SetActive(true);
        if (panelImage != null) panelImage.enabled = true;
        if (extraBackground != null) extraBackground.SetActive(true);

        if (stageCoroutine != null)
        {
            StopCoroutine(stageCoroutine);
            stageCoroutine = null;
        }

        // 🌟 Pathfinding 탐색도 중단
        if (mazeWorldOrigin != null)
        {
            Pathfinding pf = mazeWorldOrigin.GetComponent<Pathfinding>();
            if (pf != null)
            {
                pf.StopAllCoroutines();
                pf.ClearMarkers();
            }
        }

        ClearMaze();
    }

    public void StartStage(DataManager.RobotInstance robot)
    {
        currentRobot = robot;
        ShowStageUI();

        if (stageCoroutine != null) StopCoroutine(stageCoroutine);
        stageCoroutine = StartCoroutine(RunStage());
    }

    IEnumerator RunStage()
    {
        int stageIndex = currentRobot.currentStage - 1;

        StageMapData mapData = StageManager.Instance.GetMapData(currentRobot.robotId, stageIndex);
        if (mapData == null)
        {
            Debug.LogWarning($"[StageMap] 맵 데이터 없음: robotId={currentRobot.robotId}, stage={stageIndex}");
            ShowSelectUI();
            yield break;
        }

        if (stageInfoText != null)
            stageInfoText.text = $"스테이지 {stageIndex + 1} / 5";

        GenerateMazeMap(mapData);

        float remainTime = mapData.limitTime;

        bool isEscaped = false;
        bool isFailed = false;

        // 🌟 타이머를 경로 찾기 시작부터 표시
        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = $"{remainTime:F1}초";
        }

        StartRobotEscape(currentRobot,
            onSearchComplete: () => { },
            onComplete: () => { isEscaped = true; });

        // 🌟 탐색 + 이동 전체에 제한시간 적용
        while (!isEscaped && !isFailed)
        {
            remainTime -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = $"{remainTime:F1}초";
                timerText.color = remainTime <= 5f ? Color.red : Color.white;
            }

            if (remainTime <= 0f)
            {
                isFailed = true;
                break;
            }

            yield return null;
        }

        ClearMaze();
        if (timerText != null) timerText.gameObject.SetActive(false);

        if (isEscaped) OnClear(stageIndex);
        else OnFail();
    }

    void GenerateMazeMap(StageMapData data)
    {
        maze = data.GetMaze();

        for (int y = 0; y < sizey; y++)
            for (int x = 0; x < sizex; x++)
            {
                if (maze[y, x] == 3) startPos = new Vector2Int(x, y);
                if (maze[y, x] == 4) endPos = new Vector2Int(x, y);
            }

        float originalTileSize = 1f;
        SpriteRenderer sr = floorPrefab1.GetComponent<SpriteRenderer>();
        if (sr != null) originalTileSize = sr.sprite.bounds.size.x;

        if (stageMapArea != null && Camera.main != null)
        {
            Vector3[] corners = new Vector3[4];
            stageMapArea.GetWorldCorners(corners);

            Vector3 bl = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
            Vector3 tr = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

            float z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 wbl = Camera.main.ScreenToWorldPoint(new Vector3(bl.x, bl.y, z));
            Vector3 wtr = Camera.main.ScreenToWorldPoint(new Vector3(tr.x, tr.y, z));

            float areaWidth = Mathf.Abs(wtr.x - wbl.x);
            float areaHeight = Mathf.Abs(wtr.y - wbl.y);

            float scaleByWidth = areaWidth / (sizex * originalTileSize);
            float scaleByHeight = areaHeight / (sizey * originalTileSize);
            displayScale = Mathf.Min(scaleByWidth, scaleByHeight) * 1.08f;

            Vector3 areaCenter = (wbl + wtr) / 2f;
            mazeWorldOrigin.position = new Vector3(areaCenter.x + manualOffsetX, areaCenter.y, 0);
        }

        scaledTileSize = originalTileSize * displayScale;

        float baseScale = displayScale * 1.02f;
        finalScale = new Vector3(baseScale + manualScaleX, baseScale, baseScale);

        float ratioX = finalScale.x / baseScale;
        stepX = scaledTileSize * ratioX;
        stepY = scaledTileSize;
        offsetX = (sizex - 1) * stepX / 2f;
        offsetY = (sizey - 1) * stepY / 2f;

        for (int y = 0; y < sizey; y++)
        {
            for (int x = 0; x < sizex; x++)
            {
                Vector3 spawnPos = GetWorldPos(x, y);

                GameObject floorObj = Instantiate((x + y) % 2 == 0 ? floorPrefab1 : floorPrefab2, spawnPos, Quaternion.identity);
                floorObj.transform.localScale = finalScale;
                floorObj.transform.parent = mazeContainer;
                floorObj.GetComponent<SpriteRenderer>().sortingOrder = 10;

                if (maze[y, x] == 1)
                {
                    GameObject wallObj = Instantiate(wallPrefab, spawnPos, Quaternion.identity);
                    wallObj.transform.localScale = finalScale;
                    wallObj.transform.parent = mazeContainer;
                    wallObj.GetComponent<SpriteRenderer>().sortingOrder = 11;
                }
                else if (maze[y, x] == 3 || maze[y, x] == 4)
                {
                    GameObject markerObj = Instantiate(markerPrefab, spawnPos, Quaternion.identity);
                    markerObj.transform.localScale = finalScale;
                    markerObj.transform.parent = mazeContainer;
                    markerObj.GetComponent<SpriteRenderer>().sortingOrder = 12;
                }
            }
        }
    }

    void StartRobotEscape(DataManager.RobotInstance bot, System.Action onSearchComplete, System.Action onComplete)
    {
        GameObject prefab = robotPrefabs[bot.robotId];
        if (prefab == null)
        {
            Debug.LogWarning($"[StageMap] robotPrefabs[{bot.robotId}] 가 비어있어요!");
            onSearchComplete?.Invoke();
            onComplete?.Invoke();
            return;
        }

        GameObject robot = Instantiate(prefab, GetWorldPos(startPos.x, startPos.y), Quaternion.identity);
        robot.transform.localScale = finalScale * 1.5f;
        robot.transform.parent = mazeContainer;
        robot.GetComponent<SpriteRenderer>().sortingOrder = 15;

        Pathfinding pathfinder = mazeWorldOrigin.GetComponent<Pathfinding>();
        if (pathfinder != null)
        {
            var config = DataManager.Instance.robotConfigs[bot.robotId];
            pathfinder.selectedAlgorithm = config.algo;
            pathfinder.searchDelay = DataManager.Instance.GetFinalSearchDelay(bot);
            pathfinder.currentRobotId = bot.robotId;
            pathfinder.markerSortingOrder = 13; // 🌟 스테이지 마커는 타일 위에 보이게

            // 🌟 가로/세로 타일 간격 분리하여 전달
            pathfinder.StartVisualSearch(maze, startPos, endPos, stepX, stepY,
                new Vector2(offsetX, offsetY), delegate (List<Node> path)
                {
                    onSearchComplete?.Invoke();

                    if (robot == null) { onComplete?.Invoke(); return; }

                    if (path != null)
                    {
                        RobotAI ai = robot.GetComponent<RobotAI>();
                        if (ai != null)
                        {
                            ai.moveSpeed = DataManager.Instance.GetFinalMoveSpeed(bot);
                            ai.MoveToPath(path, stepX, stepY, new Vector2(offsetX, offsetY), onComplete);
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
        if (mazeContainer != null)
            foreach (Transform child in mazeContainer)
                Destroy(child.gameObject);

        if (mazeWorldOrigin != null)
        {
            Pathfinding pf = mazeWorldOrigin.GetComponent<Pathfinding>();
            if (pf != null) pf.ClearMarkers();
        }
    }

    void OnClear(int stageIndex)
    {
        currentRobot.stageClear[stageIndex] = true;
        if (stageIndex < 4)
            currentRobot.currentStage = stageIndex + 2;
        ShowResult(true, stageIndex);
    }

    void OnFail()
    {
        ShowResult(false, currentRobot.currentStage - 1);
    }

    void ShowResult(bool isCleared, int stageIndex)
    {
        if (panelBackground != null) panelBackground.SetActive(true);
        if (panelImage != null) panelImage.enabled = true;
        if (extraBackground != null) extraBackground.SetActive(true);

        if (resultPanel != null) resultPanel.SetActive(true);

        if (resultText != null)
        {
            if (isCleared)
                resultText.text = stageIndex >= 4
                    ? "<color=#FFD700>모든 스테이지 클리어!</color>"
                    : $"<color=#00FF00>스테이지 {stageIndex + 1} 클리어!</color>";
            else
                resultText.text = $"<color=#FF3333>시간 초과!\n스테이지 {stageIndex + 1} 실패</color>";
        }

        if (retryButton != null)
            retryButton.gameObject.SetActive(!isCleared);
    }

    void OnClickRetry()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
        if (stageCoroutine != null) StopCoroutine(stageCoroutine);
        ShowStageUI();
        stageCoroutine = StartCoroutine(RunStage());
    }

    void OnClickBack()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
        if (stageCoroutine != null) StopCoroutine(stageCoroutine);

        // 🌟 Pathfinding 탐색도 중단
        if (mazeWorldOrigin != null)
        {
            Pathfinding pf = mazeWorldOrigin.GetComponent<Pathfinding>();
            if (pf != null)
            {
                pf.StopAllCoroutines();
                pf.ClearMarkers();
            }
        }

        ClearMaze();
        ShowSelectUI();
    }

    void ShowSelectUI()
    {
        if (selectUI != null) selectUI.SetActive(true);
        if (stageUI != null) stageUI.SetActive(false);

        if (panelBackground != null) panelBackground.SetActive(true);
        if (panelImage != null) panelImage.enabled = true;
        if (extraBackground != null) extraBackground.SetActive(true);
    }

    void ShowStageUI()
    {
        if (selectUI != null) selectUI.SetActive(false);
        if (stageUI != null) stageUI.SetActive(true);

        if (panelBackground != null) panelBackground.SetActive(false);
        if (panelImage != null) panelImage.enabled = false;
        if (extraBackground != null) extraBackground.SetActive(false);
    }

    public Vector3 GetWorldPos(int x, int y)
    {
        float useStepX = stepX > 0 ? stepX : scaledTileSize;
        float useStepY = stepY > 0 ? stepY : scaledTileSize;
        float posX = (x * useStepX) - offsetX;
        float posY = -(y * useStepY) + offsetY;
        Vector3 basePos = mazeWorldOrigin != null ? mazeWorldOrigin.position : Vector3.zero;
        return basePos + new Vector3(posX, posY, 0);
    }
}