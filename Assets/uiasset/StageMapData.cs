using UnityEngine;

/// <summary>
/// 각 캐릭터별 스테이지 맵 데이터 ScriptableObject
/// Assets에서 우클릭 → Create → StageMap → StageMapData 로 생성
/// </summary>
[CreateAssetMenu(fileName = "StageMapData", menuName = "StageMap/StageMapData")]
public class StageMapData : ScriptableObject
{
    [Header("기본 설정")]
    public int robotId;        // 0:알파 ~ 8:오메가
    public int stageIndex;     // 0~4 (1~5단계)
    public float limitTime;    // 제한시간 (초)

    [Header("맵 데이터 (17x11 = 187개, 0=길 1=벽 3=시작 4=끝)")]
    public int[] mapData = new int[17 * 11];

    // 맵 크기 고정
    public const int SizeX = 17;
    public const int SizeY = 11;

    /// <summary>
    /// 1차원 배열을 2차원으로 변환
    /// </summary>
    public int[,] GetMaze()
    {
        int[,] maze = new int[SizeY, SizeX];
        for (int y = 0; y < SizeY; y++)
            for (int x = 0; x < SizeX; x++)
                maze[y, x] = mapData[y * SizeX + x];
        return maze;
    }
}