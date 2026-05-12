using UnityEngine;
using UnityEditor;
using UnityEngine.U2D.Animation;
using System.Collections.Generic;
using System.Linq;

public class SpriteLibraryGenerator : EditorWindow
{
    public Texture2D spriteSheet;
    public SpriteLibraryAsset mainLibrary;

    private string[] categories = {
        "Walk_down", "Walk_bottom_right", "Walk_right", "Walk_top_right",
        "Walk_up", "Walk_top_left", "Walk_left", "Walk_bottom_left"
    };

    [MenuItem("Tools/Sprite Library Generator")]
    public static void ShowWindow() => GetWindow<SpriteLibraryGenerator>("캐릭터 생성기");

    void OnGUI()
    {
        EditorGUILayout.HelpBox("1. 시트가 'Multiple'로 슬라이스 되었는지 확인하세요.\n2. 부모 템플릿(Main Library)을 반드시 넣어주세요.", MessageType.Info);
        
        spriteSheet = (Texture2D)EditorGUILayout.ObjectField("스프라이트 시트", spriteSheet, typeof(Texture2D), false);
        mainLibrary = (SpriteLibraryAsset)EditorGUILayout.ObjectField("부모 템플릿 (상속)", mainLibrary, typeof(SpriteLibraryAsset), false);

        if (GUILayout.Button("새 캐릭터 라이브러리 생성", GUILayout.Height(40)))
        {
            if (spriteSheet == null) { Debug.LogError("스프라이트 시트가 선택되지 않았습니다!"); return; }
            CreateAndFillAsset();
        }
    }

    void CreateAndFillAsset()
    {
        string defaultName = spriteSheet.name + "_Library";
        string path = EditorUtility.SaveFilePanelInProject("라이브러리 저장", defaultName, "asset", "새로운 라이브러리 이름을 입력하세요.");

        if (string.IsNullOrEmpty(path)) return;

        // 1. 스프라이트 추출 시 Null 제외 처리
        string sheetPath = AssetDatabase.GetAssetPath(spriteSheet);
        Object[] objects = AssetDatabase.LoadAllAssetsAtPath(sheetPath);
        List<Sprite> sprites = objects.OfType<Sprite>()
            .Where(s => s != null) // Null 인 스프라이트 제외
            .OrderBy(s => {
                string[] parts = s.name.Split('_');
                int.TryParse(parts[parts.Length - 1], out int num);
                return num;
            }).ToList();

        if (sprites.Count < 24) {
            Debug.LogError($"[오류] 슬라이스된 스프라이트가 부족합니다. (찾은 개수: {sprites.Count}/24). Sprite Editor에서 슬라이스가 잘 되었는지 확인하세요!");
            return;
        }

        // 2. 에셋 생성 및 초기화
        SpriteLibraryAsset newAsset = ScriptableObject.CreateInstance<SpriteLibraryAsset>();
        
        if (mainLibrary != null)
        {
            SerializedObject so = new SerializedObject(newAsset);
            var mainProp = so.FindProperty("m_MainLibraryAsset");
            if (mainProp != null) {
                mainProp.objectReferenceValue = mainLibrary;
                so.ApplyModifiedProperties();
            }
        }

        // 3. 데이터 주입 시 안전 확인 (여기가 67번 줄 부근입니다)
        string[] labelOrder = { "1", "0", "2" };
        int spriteIndex = 0;

        for (int i = 0; i < categories.Length; i++)
        {
            for (int f = 0; f < 3; f++)
            {
                if (spriteIndex < sprites.Count)
                {
                    Sprite currentSprite = sprites[spriteIndex];
                    if (currentSprite != null && newAsset != null) {
                        newAsset.AddCategoryLabel(currentSprite, categories[i], labelOrder[f]);
                    }
                    else {
                        Debug.LogWarning($"[주의] {spriteIndex}번째 스프라이트가 Null입니다. 건너뜁니다.");
                    }
                    spriteIndex++;
                }
            }
        }

        AssetDatabase.CreateAsset(newAsset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = newAsset;
        Debug.Log($"<color=lime>✔ {newAsset.name} 생성 완료!</color>");
    }
}