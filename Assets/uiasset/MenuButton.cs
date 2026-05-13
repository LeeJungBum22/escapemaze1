using UnityEngine;
using UnityEngine.UI; // 이미지를 다루기 위해 필수!

public class MenuButton : MonoBehaviour
{
    [Header("연결 설정")]
    public UIManager uiManager;     // 지휘관인 UIManager
    public GameObject targetPanel;  // 이 버튼이 열고 닫을 창

    [Header("이미지 설정")]
    public Sprite xSprite;          // 열려있을 때 보여줄 X 표시 이미지

    private Sprite originalSprite;  // 원래 아이콘 이미지 (자동으로 기억함)
    private Image myImage;

    void Start()
    {
        myImage = GetComponent<Image>();
        originalSprite = myImage.sprite; // 게임 시작 시점의 이미지를 원래 이미지로 저장
    }

    // 🌟 인스펙터의 On Click() 에 이 함수를 연결할 겁니다!
    public void OnClickThisButton()
    {
        if (uiManager != null)
        {
            uiManager.ToggleMenu(this); // 매니저에게 "나(버튼) 눌렸어!" 라고 넘겨줌
        }
    }

    // UIManager가 호출할 이미지 변경 함수들
    public void ChangeToX() { myImage.sprite = xSprite; }
    public void ChangeToOriginal() { myImage.sprite = originalSprite; }
}