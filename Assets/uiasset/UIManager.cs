using UnityEngine;

public class UIManager : MonoBehaviour
{
    // 현재 열려있는 메뉴 버튼(과 그 패널)을 기억
    private MenuButton currentMenu = null;

    public void ToggleMenu(MenuButton clickedMenu)
    {
        // 1. 이미 열려있는 창의 버튼을 다시 누른 경우 (닫기)
        if (currentMenu == clickedMenu)
        {
            currentMenu.targetPanel.SetActive(false); // 창 닫기
            currentMenu.ChangeToOriginal();           // 이미지를 원래 아이콘으로 복구
            currentMenu = null;                       // 현재 열린 창 없음 상태로 변경
        }
        // 2. 다른 버튼을 누르거나, 처음 버튼을 누른 경우 (새 창 열기)
        else
        {
            // 이미 다른 창이 열려있다면 먼저 닫고 이미지 원상복구
            if (currentMenu != null)
            {
                currentMenu.targetPanel.SetActive(false);
                currentMenu.ChangeToOriginal();
            }

            // 새로운 창 열고 이미지를 X 모양으로 변경
            clickedMenu.targetPanel.SetActive(true);
            clickedMenu.ChangeToX();
            currentMenu = clickedMenu;                // 현재 열린 창 갱신
        }
    }
}