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
        myImage = GetComponent<Image>();
        originalSprite = myImage.sprite;
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
        if(selectedSprite != null) myImage.sprite = selectedSprite; 
    }
    
    public void ChangeToOriginal() 
    { 
        if(originalSprite != null) myImage.sprite = originalSprite; 
    }
}