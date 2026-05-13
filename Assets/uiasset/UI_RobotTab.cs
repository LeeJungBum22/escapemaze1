using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UI_RobotTab : MonoBehaviour
{
    [Header("설정")]
    public GameObject robotItemPrefab; // 로봇 칸 프리팹
    public Transform contentParent;    // ScrollView의 Content

    void OnEnable()
    {
        RefreshAll();
    }

    public void RefreshAll()
    {
        // 🌟 방어 코드 1: DataManager가 아직 안 켜졌으면 튕기지 말고 그냥 넘어감
        if (DataManager.Instance == null) return;

        // 🌟 방어 코드 2: 인스펙터 연결을 깜빡했다면 에러 대신 경고 메시지 띄움
        if (contentParent == null || robotItemPrefab == null)
        {
            Debug.LogWarning("UI_RobotTab: 인스펙터 창에 Prefab이나 Content Parent가 연결되지 않았습니다!");
            return;
        }

        // 1. 기존 목록 삭제
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. 데이터 가져와서 정렬
        var sortedList = DataManager.Instance.myRobots
            .OrderBy(r => r.robotId)
            .ThenByDescending(r => r.level)
            .ThenByDescending(r => r.star)
            .ToList();

        // 3. 프리팹 생성 및 데이터 연결
        foreach (var robot in sortedList)
        {
            GameObject item = Instantiate(robotItemPrefab, contentParent);
            item.GetComponent<UI_RobotItem>().Setup(robot, this);
        }
    }
}