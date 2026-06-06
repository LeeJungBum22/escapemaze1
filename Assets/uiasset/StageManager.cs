using UnityEngine;

/// <summary>
/// 씬에 빈 오브젝트 만들고 부착.
/// 맵 데이터 보관 및 제공 역할.
/// </summary>
public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("캐릭터별 스테이지 맵 (9캐릭터 × 5스테이지)")]
    public StageMapData[] 알파_IDA   = new StageMapData[5];
    public StageMapData[] 베타_Dij   = new StageMapData[5];
    public StageMapData[] 감마_BFS   = new StageMapData[5];
    public StageMapData[] 델타_AStar = new StageMapData[5];
    public StageMapData[] 엡실론_Best= new StageMapData[5];
    public StageMapData[] 제타_OJPS  = new StageMapData[5];
    public StageMapData[] 에타_JPS   = new StageMapData[5];
    public StageMapData[] 세타_Trace = new StageMapData[5];
    public StageMapData[] 오메가_RL  = new StageMapData[5];

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// StageMap.cs에서 호출 — robotId와 stageIndex(0-based)로 맵 데이터 반환
    /// </summary>
    public StageMapData GetMapData(int robotId, int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= 5) return null;

        StageMapData[] maps = robotId switch
        {
            0 => 알파_IDA,
            1 => 베타_Dij,
            2 => 감마_BFS,
            3 => 델타_AStar,
            4 => 엡실론_Best,
            5 => 제타_OJPS,
            6 => 에타_JPS,
            7 => 세타_Trace,
            8 => 오메가_RL,
            _ => null
        };

        return maps?[stageIndex];
    }
}
