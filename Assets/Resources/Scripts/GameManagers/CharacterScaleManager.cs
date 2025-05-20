using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Spine.Unity;
using UObject = UnityEngine.Object;

namespace Resources.Scripts.GameManagers
{
    [ExecuteAlways]
    public class CharacterScaleManager : MonoBehaviour, ICharacterFactory
    {
        [Serializable]
        public class CharacterScaleEntry
        {
            [Tooltip("Сюда префаб персонажа (Asset)")]
            public GameObject prefab;
            [Tooltip("Желаемый локальный масштаб")]
            public Vector3 scale = Vector3.one;
        }

        private static CharacterScaleManager instance;
        public static ICharacterFactory Factory
        {
            get
            {
                if (instance == null)
                    throw new InvalidOperationException(
                        "CharacterScaleManager не инициализирован");
                return instance;
            }
        }

        [Header("Настройки масштабов персонажей")]
        [SerializeField] private List<CharacterScaleEntry> scaleEntries = new List<CharacterScaleEntry>();
        private Dictionary<GameObject, Vector3> scaleLookup;

        private void Awake()
        {
            // Если мы в редакторе (ExecuteAlways), но НЕ в режиме Play — выходим сразу
            if (!Application.isPlaying)
                return;

            // Обработка singleton только в режиме Play
            if (instance != null && instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            BuildLookup();
        }

        private void OnValidate()
        {
            // Всегда обновляем lookup в редакторе при изменении данных
            BuildLookup();
        }

        private void BuildLookup()
        {
            scaleLookup = scaleEntries
                .Where(e => e.prefab != null)
                .ToDictionary(e => e.prefab, e => e.scale);
        }

        public GameObject CreateCharacter(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            var go = Instantiate(prefab, position, rotation);
            if (scaleLookup.TryGetValue(prefab, out var s))
                go.transform.localScale = s;
            return go;
        }

    #if UNITY_EDITOR
        public void ApplyScalesToScene()
        {
            BuildLookup();

            foreach (var entry in scaleEntries.Where(e => e.prefab != null))
            {
                // Сцена: SpriteRenderer
                foreach (var sr in UObject.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
                {
                    var src = PrefabUtility.GetCorrespondingObjectFromSource(sr.gameObject);
                    if (src == entry.prefab)
                    {
                        Undo.RecordObject(sr.transform, "Apply Character Scale");
                        sr.transform.localScale = entry.scale;
                        EditorUtility.SetDirty(sr.gameObject);
                    }
                }
                // Сцена: SkeletonAnimation
                foreach (var sk in UObject.FindObjectsByType<SkeletonAnimation>(FindObjectsSortMode.None))
                {
                    var src = PrefabUtility.GetCorrespondingObjectFromSource(sk.gameObject);
                    if (src == entry.prefab)
                    {
                        Undo.RecordObject(sk.transform, "Apply Character Scale");
                        sk.transform.localScale = entry.scale;
                        EditorUtility.SetDirty(sk.gameObject);
                    }
                }

                // Asset-префаб
                string assetPath = AssetDatabase.GetAssetPath(entry.prefab);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    var prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
                    prefabRoot.transform.localScale = entry.scale;
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }

            Debug.Log("ApplyScalesToScene: scene and prefab assets updated.", this);
        }
    #endif
    }

    public interface ICharacterFactory
    {
        GameObject CreateCharacter(GameObject prefab, Vector3 position, Quaternion rotation);
    }
}
