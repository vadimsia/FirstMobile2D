using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TrapPlacer : MonoBehaviour
{
    [Header("Настройки ловушки")]
    public GameObject trapPrefab;            // Префаб ловушки (с прикреплённым скриптом Trap)
    public Transform trapSpawnPoint;         // Точка появления ловушки (например, позиция игрока)
    public float cooldownTime = 10f;         // Охлаждение в секундах

    [Header("UI Элементы для отображения охлаждения")]
    public Image cooldownFillImage;        // Image с режимом Fill (например, radial)
    public Text cooldownText;              // Текст с оставшимися секундами

    private bool canPlace = true;
    private float cooldownTimer = 0f;

    private void Start()
    {
        // Отключаем UI-элементы кулдауна при старте игры
        if (cooldownFillImage != null)
            cooldownFillImage.gameObject.SetActive(false);
        if (cooldownText != null)
            cooldownText.gameObject.SetActive(false);
    }

    void Update()
    {
        // Если ловушка на кулдауне – обновляем таймер и UI
        if (!canPlace)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer < 0)
                cooldownTimer = 0f;

            if (cooldownFillImage != null)
            {
                // Заполняем от 1 к 0
                cooldownFillImage.fillAmount = cooldownTimer / cooldownTime;
            }
            if (cooldownText != null)
            {
                cooldownText.text = Mathf.Ceil(cooldownTimer).ToString();
            }
            if (cooldownTimer <= 0f)
            {
                canPlace = true;
                if (cooldownFillImage != null)
                {
                    cooldownFillImage.fillAmount = 0f;
                    cooldownFillImage.gameObject.SetActive(false);
                }
                if (cooldownText != null)
                {
                    cooldownText.text = "";
                    cooldownText.gameObject.SetActive(false);
                }
            }
        }

        // Пример: установка ловушки по нажатию клавиши T
        if (Input.GetKeyDown(KeyCode.T))
        {
            PlaceTrap();
        }
    }

    public void PlaceTrap()
    {
        // Ловушки можно ставить только на сцене "FirstPartScene"
        if (SceneManager.GetActiveScene().name != "FirstPartScene")
        {
            Debug.Log("Trap cannot be placed in this scene.");
            return;
        }
        if (canPlace)
        {
            Instantiate(trapPrefab, trapSpawnPoint.position, trapSpawnPoint.rotation);
            Debug.Log("Trap placed.");
            canPlace = false;
            cooldownTimer = cooldownTime;
            if (cooldownFillImage != null)
            {
                cooldownFillImage.gameObject.SetActive(true);
                cooldownFillImage.fillAmount = 1f;
            }
            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(true);
                cooldownText.text = cooldownTime.ToString();
            }
        }
        else
        {
            Debug.Log("Trap is on cooldown.");
        }
    }
}
