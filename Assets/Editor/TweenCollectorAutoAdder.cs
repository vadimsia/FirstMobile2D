using UnityEditor;
using UnityEngine;
using Resources.Scripts.GameManagers;

public class TweenCollectorAutoAdder : EditorWindow
{
    private string searchFolder = "Assets";

    [MenuItem("Tools/Tween ► Добавить TweenGarbageCollector на префабы")]
    public static void ShowWindow()
    {
        GetWindow<TweenCollectorAutoAdder>("Auto Add TGCollector");
    }

    private void OnGUI()
    {
        GUILayout.Label("Добавление TweenGarbageCollector на префабы", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Поиск префабов в указанной папке и добавление компонента TweenGarbageCollector, если его нет.", MessageType.Info);
        searchFolder = EditorGUILayout.TextField("Папка поиска:", searchFolder);

        if (GUILayout.Button("Выполнить"))
            AddCollectorsToPrefabs();
    }

    private void AddCollectorsToPrefabs()
    {
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { searchFolder });
        int addedCount = 0;

        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var instance = PrefabUtility.LoadPrefabContents(path);

            if (instance.GetComponent<TweenGarbageCollector>() == null)
            {
                instance.AddComponent<TweenGarbageCollector>();
                addedCount++;
                Debug.Log($"[TweenCollector] Добавлен на: {path}");
                PrefabUtility.SaveAsPrefabAsset(instance, path);
            }

            PrefabUtility.UnloadPrefabContents(instance);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Готово", $"Добавлено компонентов: {addedCount}", "OK");
    }
}