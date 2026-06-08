using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Q-Learning 학습 결과(Q테이블)를 관리하는 싱글턴.
/// StageManager 오브젝트에 같이 붙이거나, 별도 빈 오브젝트에 부착.
/// </summary>
public class QTableManager : MonoBehaviour
{
    public static QTableManager Instance;

    [Header("학습 설정")]
    public int defaultEpisodes = 500; // 기본 학습 에피소드 수

    // 저장소: "robotId_stageIndex" → (Q테이블 flat배열, state 수)
    private Dictionary<string, double[]> savedQTables = new Dictionary<string, double[]>();
    private Dictionary<string, int> savedQStates = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ─── 키 생성 ───
    string MakeKey(int robotId, int stageIndex) => $"{robotId}_{stageIndex}";

    // ─── 학습 여부 확인 ───
    public bool HasTrainedData(int robotId, int stageIndex)
    {
        return savedQTables.ContainsKey(MakeKey(robotId, stageIndex));
    }

    // ─── 시작점/끝점 찾기 헬퍼 ───
    bool FindStartGoal(int[,] maze, out int startState, out int goalState)
    {
        startState = -1; goalState = -1;
        int sizeX = StageMapData.SizeX;
        int sizeY = StageMapData.SizeY;
        for (int y = 0; y < sizeY; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                if (maze[y, x] == 3) startState = y * sizeX + x;
                if (maze[y, x] == 4) goalState = y * sizeX + x;
            }
        }
        return startState >= 0 && goalState >= 0;
    }

    // ─── 누적 학습 (강화 시스템용) ───
    /// <summary>
    /// 기존 Q테이블을 초기화하지 않고 episodes 만큼 추가 학습.
    /// epsilon이 낮을수록 탐험을 줄이고 최적 경로에 집중.
    /// </summary>
    public bool TrainStageIncremental(int robotId, int stageIndex, int episodes, double epsilon)
    {
        StageMapData mapData = StageManager.Instance?.GetMapData(robotId, stageIndex);
        if (mapData == null) return false;

        int[,] maze = mapData.GetMaze();
        int sizeX = StageMapData.SizeX;
        int sizeY = StageMapData.SizeY;

        if (!FindStartGoal(maze, out int startState, out int goalState)) return false;

        string key = MakeKey(robotId, stageIndex);

        QLearning q = new QLearning();
        q.mazeWidth = sizeX;
        q.mazeHeight = sizeY;

        // 기존 학습 데이터가 있으면 이어서, 없으면 새로
        if (savedQTables.ContainsKey(key))
            q.DeserializeQ(savedQTables[key], savedQStates[key]);
        else
            q.MakeQ(maze);

        q.TrainIncremental(maze, startState, goalState, episodes, epsilon);

        savedQTables[key] = q.SerializeQ();
        savedQStates[key] = sizeX * sizeY;
        return true;
    }

    // ─── 오메가 전체 스테이지 누적 강화 ───
    /// <summary>
    /// 오메가(8번)의 5개 스테이지를 모두 episodes 만큼 추가 학습.
    /// DataManager.TrainOmega()에서 호출.
    /// </summary>
    public bool TrainOmegaAllStages(int episodes, double epsilon)
    {
        bool anySuccess = false;
        for (int i = 0; i < 5; i++)
        {
            if (TrainStageIncremental(8, i, episodes, epsilon))
                anySuccess = true;
        }
        return anySuccess;
    }

    // ─── 스테이지 학습 실행 ───
    /// <summary>
    /// 지정된 로봇+스테이지의 맵을 불러와 Q-Learning 학습 후 저장.
    /// 반환값: 학습 성공 여부
    /// </summary>
    public bool TrainStage(int robotId, int stageIndex, int episodes = -1)
    {
        if (episodes < 0) episodes = defaultEpisodes;

        StageMapData mapData = StageManager.Instance?.GetMapData(robotId, stageIndex);
        if (mapData == null)
        {
            Debug.LogWarning($"[QTableManager] 맵 데이터 없음: robotId={robotId}, stage={stageIndex}");
            return false;
        }

        int[,] maze = mapData.GetMaze();
        int sizeX = StageMapData.SizeX;
        int sizeY = StageMapData.SizeY;

        // 시작점/끝점 찾기
        int startState = -1, goalState = -1;
        for (int y = 0; y < sizeY; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                if (maze[y, x] == 3) startState = y * sizeX + x;
                if (maze[y, x] == 4) goalState = y * sizeX + x;
            }
        }

        if (startState < 0 || goalState < 0)
        {
            Debug.LogWarning($"[QTableManager] 시작점/끝점을 찾을 수 없음!");
            return false;
        }

        // Q-Learning 학습
        QLearning q = new QLearning();
        q.Train(maze, startState, goalState, episodes);

        // 경로 추출 가능한지 검증
        List<Node> testPath = q.GetPath(maze, startState, goalState);
        if (testPath.Count < 2)
        {
            Debug.LogWarning($"[QTableManager] 학습 후 경로 추출 실패! 에피소드를 늘려보세요.");
            return false;
        }

        // 경로의 마지막이 목적지인지 확인
        Node last = testPath[testPath.Count - 1];
        if (last.gridX != goalState % sizeX || last.gridY != goalState / sizeX)
        {
            Debug.LogWarning($"[QTableManager] 학습 부족: 경로가 목적지에 도달하지 못함. (에피소드: {episodes})");
            // 저장은 하되 경고
        }

        // Q테이블 저장
        string key = MakeKey(robotId, stageIndex);
        savedQTables[key] = q.SerializeQ();
        savedQStates[key] = sizeX * sizeY;

        Debug.Log($"[QTableManager] 학습 완료! robotId={robotId}, stage={stageIndex}, 경로길이={testPath.Count}");
        return true;
    }

    // ─── 저장된 Q테이블로 경로 추출 ───
    /// <summary>
    /// 학습된 Q테이블에서 최적 경로를 Node 리스트로 반환.
    /// 학습 데이터가 없으면 null 반환.
    /// </summary>
    public List<Node> GetTrainedPath(int robotId, int stageIndex)
    {
        string key = MakeKey(robotId, stageIndex);
        if (!savedQTables.ContainsKey(key)) return null;

        StageMapData mapData = StageManager.Instance?.GetMapData(robotId, stageIndex);
        if (mapData == null) return null;

        int[,] maze = mapData.GetMaze();
        int sizeX = StageMapData.SizeX;
        int sizeY = StageMapData.SizeY;

        // 시작점/끝점 찾기
        int startState = -1, goalState = -1;
        for (int y = 0; y < sizeY; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                if (maze[y, x] == 3) startState = y * sizeX + x;
                if (maze[y, x] == 4) goalState = y * sizeX + x;
            }
        }

        // Q테이블 복원
        QLearning q = new QLearning();
        q.mazeWidth = sizeX;
        q.mazeHeight = sizeY;
        q.DeserializeQ(savedQTables[key], savedQStates[key]);

        return q.GetPath(maze, startState, goalState);
    }

    // ─── 특정 학습 데이터 삭제 ───
    public void ClearTrainedData(int robotId, int stageIndex)
    {
        string key = MakeKey(robotId, stageIndex);
        savedQTables.Remove(key);
        savedQStates.Remove(key);
    }

    // ─── 전체 삭제 ───
    public void ClearAll()
    {
        savedQTables.Clear();
        savedQStates.Clear();
    }
}