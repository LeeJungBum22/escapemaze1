using UnityEngine;

public class SubTabManager : MonoBehaviour
{
    private SubTabButton currentTab = null;

    [Header("기본으로 열려있을 탭")]
    public SubTabButton defaultTab;

    // 강화 창이 열릴 때마다 자동으로 첫 번째 탭(기본 탭)을 열어줍니다.
    void OnEnable()
    {
        if (defaultTab != null)
        {
            SelectTab(defaultTab);
        }
    }

    public void SelectTab(SubTabButton clickedTab)
    {
        // 1. 이미 열려있는 탭을 다시 누른 경우 -> 아무것도 안 함 (창이 닫히지 않게 방지)
        if (currentTab == clickedTab) return;

        // 2. 다른 탭이 열려있다면 기존 탭 닫기
        if (currentTab != null)
        {
            currentTab.targetPanel.SetActive(false);
            currentTab.ChangeToOriginal();
        }

        // 3. 새로운 탭 열기
        clickedTab.targetPanel.SetActive(true);
        clickedTab.ChangeToSelected();
        currentTab = clickedTab;
    }
}