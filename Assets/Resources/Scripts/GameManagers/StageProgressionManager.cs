using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Resources.Scripts.Data;
using Resources.Scripts.Player;

namespace Resources.Scripts.GameManagers
{
    [DefaultExecutionOrder(-100)]
    public class StageProgressionManager : MonoBehaviour
    {
        public static StageProgressionManager Instance { get; private set; }
        public static ArenaSettings CurrentArenaSettings { get; private set; }

        [Header("Доступные перки (через инспектор)")]
        [SerializeField] private PerkDefinition[] availablePerks = null!;

        [Header("UI панели перков")]
        [SerializeField] private GameObject perkSelectionPanelPrefab = null!;

        private int currentArenaIndex;
        private readonly List<PerkDefinition> selectedPerks = new();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <summary>
        /// Запускает текущий Stage: начинает с его первой арены.
        /// </summary>
        public void StartStage()
        {
            currentArenaIndex = 0;
            selectedPerks.Clear();
            LoadArena(currentArenaIndex);
        }

        private void LoadArena(int idx)
        {
            var data = GameStageManager.currentStageData;
            if (data == null || idx < 0
                || idx >= data.arenaSceneNames.Length
                || idx >= data.arenaSettingsList.Length)
            {
                Debug.LogError($"Невалидный индекс арены {idx}");
                return;
            }
            CurrentArenaSettings = data.arenaSettingsList[idx];
            SceneManager.LoadScene(data.arenaSceneNames[idx]);
        }

        private void LoadLabyrinth()
        {
            var data = GameStageManager.currentStageData;
            if (data == null)
            {
                Debug.LogError("StageData==null");
                return;
            }
            SceneManager.LoadScene(data.labyrinthSceneName);
        }

        public void OnArenaComplete()
        {
            var canvas = Object.FindFirstObjectByType<Canvas>();
            var panel = Instantiate(
                perkSelectionPanelPrefab,
                canvas != null ? canvas.transform : null,
                false);

            var opts = availablePerks
                .OrderBy(_ => Random.value)
                .Take(3)
                .ToArray();

            var ctrl = panel.GetComponent<Resources.Scripts.Menu.PerkSelectionController>();
            ctrl.Setup(opts);
        }

        public void OnPerkChosen(PerkDefinition perk)
        {
            selectedPerks.Add(perk);
            ApplyAllPerks();

            currentArenaIndex++;
            var data = GameStageManager.currentStageData;
            if (currentArenaIndex < data.arenaSceneNames.Length)
                LoadArena(currentArenaIndex);
            else
                LoadLabyrinth();
        }

        /// <summary>
        /// Вызывается после прохождения лабиринта.
        /// Пытаемся перейти к следующему StageData и запустить его.
        /// </summary>
        public void OnLabyrinthCompleted()
        {
            // Сначала попытаемся переключиться на следующий StageData
            bool hasNext = GameStageManager.Instance.LoadNextStage();
            if (hasNext)
            {
                // Запустить новую серию арен этого этапа
                StartStage();
            }
            else
            {
                // Все этапы пройдены — можно, например, показать экран победы
                Debug.Log("Поздравляем! Все этапы пройдены.");
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var data = GameStageManager.currentStageData;
            if (data != null)
            {
                var valid = data.arenaSceneNames.Concat(new[] { data.labyrinthSceneName });
                if (!valid.Contains(scene.name))
                {
                    selectedPerks.Clear();
                    var stats = Object.FindFirstObjectByType<PlayerStatsHandler>();
                    stats?.ResetStats();
                }
            }

            StartCoroutine(DelayedApplyAllPerks());
        }

        private IEnumerator DelayedApplyAllPerks()
        {
            yield return null;
            ApplyAllPerks();
        }

        private void ApplyAllPerks()
        {
            var stats = Object.FindFirstObjectByType<PlayerStatsHandler>();
            if (stats == null) return;

            stats.ResetStats();
            foreach (var p in selectedPerks)
                p.Apply(stats);
        }
    }
}
