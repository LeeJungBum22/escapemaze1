using UnityEngine;
using UnityEngine.UI;

public class SubTabButton : MonoBehaviour
{
    [Header("연결 설정")]
    public SubTabManager tabManager;    // 이 탭들을 관리할 매니저
    public GameObject targetPanel;      // 이 버튼이 열어줄 내부 창 (로봇, 골드, 다이아 탭)

    [Header("이미지 설정 (선택사항)")]
    public Sprite selectedSprite;       // 눌렸을 때 바뀔 이미지 (밝아진 버튼 등)

    private Sprite originalSprite;
    private Image myImage;

    void Awake()
    {
        InitImage();
    }

    // 🌟 추가됨: 이미지를 못 찾아서 에러가 나는 것을 방지하는 초기화 함수
    private void InitImage()
    {
        if (myImage == null)
        {
            myImage = GetComponent<Image>();
            if (myImage != null) 
            {
                originalSprite = myImage.sprite;
            }
        }
    }

    public void OnClickThisTab()
    {
        if (tabManager != null)
        {
            tabManager.SelectTab(this);
        }
    }

    public void ChangeToSelected() 
    { 
        InitImage(); // 🌟 방어 코드: Awake보다 먼저 불렸더라도 여기서 안전하게 Image를 찾습니다.
        if (myImage != null && selectedSprite != null) 
        {
            myImage.sprite = selectedSprite; 
        }
    }
    
    public void ChangeToOriginal() 
    { 
        InitImage(); // 🌟 방어 코드
        if (myImage != null && originalSprite != null) 
        {
            myImage.sprite = originalSprite; 
        }
    }
}