using UnityEngine;
using UnityEditor;

public class RemoveEvents
{
    [MenuItem("Tools/모든 애니메이션 이벤트 삭제")]
    public static void RemoveAnimationEvents()
    {
        // 프로젝트 창에서 선택한 애니메이션 클립들을 가져옵니다.
        Object[] selectedObjects = Selection.GetFiltered(typeof(AnimationClip), SelectionMode.DeepAssets);
        int count = 0;

        foreach (Object obj in selectedObjects)
        {
            AnimationClip clip = obj as AnimationClip;
            if (clip != null)
            {
                // 해당 클립의 이벤트를 '빈 배열'로 덮어씌워 싹 날려버립니다.
                AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);
                EditorUtility.SetDirty(clip);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"<color=lime>총 {count}개의 애니메이션 클립에서 범인(이벤트)을 소탕했습니다!</color>");
    }
}