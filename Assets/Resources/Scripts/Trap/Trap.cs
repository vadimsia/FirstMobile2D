using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Resources.Scripts.Enemy;
using Resources.Scripts.Player;

namespace Resources.Scripts.Trap
{
    public class Trap : MonoBehaviour
    {
        [SerializeField] private float slowMultiplier = 0.5f;       // 50% скорости
        [SerializeField] private float slowDuration = 3f;           // длительность эффекта
        [SerializeField] private float playerDelay = 2f;            // задержка для игрока
        private float spawnTime;

        // Для хранения запущенных корутин задержки для игроков
        private Dictionary<Collider2D, Coroutine> playerDelayCoroutines = new Dictionary<Collider2D, Coroutine>();

        private void Start()
        {
            // Ловушки можно ставить только на сцене "FirstPartScene"
            if (SceneManager.GetActiveScene().name != "FirstPartScene")
            {
                Destroy(gameObject);
                return;
            }

            spawnTime = Time.time;
            // Ловушка исчезает через 5 секунд
            Destroy(gameObject, 5f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy"))
            {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.ApplySlow(slowMultiplier, slowDuration);
                    Debug.Log($"Trap: Enemy '{other.gameObject.name}' slowed immediately.");
                }
            }
            else if (other.CompareTag("Player"))
            {
                // Если ловушка существует более 2 секунд – замедление применяется сразу (например, при повторном входе)
                if (Time.time - spawnTime >= playerDelay)
                {
                    PlayerController player = other.GetComponent<PlayerController>();
                    if (player != null)
                    {
                        player.ApplySlow(slowMultiplier, slowDuration);
                        Debug.Log($"Trap: Player '{other.gameObject.name}' slowed immediately on re-entry.");
                    }
                }
                else
                {
                    // Если ловушка установлена недавно, запускаем задержку в 2 секунды
                    if (!playerDelayCoroutines.ContainsKey(other))
                    {
                        Coroutine coroutine = StartCoroutine(PlayerDelayCoroutine(other));
                        playerDelayCoroutines.Add(other, coroutine);
                    }
                }
            }
        }

        private IEnumerator PlayerDelayCoroutine(Collider2D playerCollider)
        {
            yield return new WaitForSeconds(playerDelay);
            Collider2D trapCollider = GetComponent<Collider2D>();
            if (trapCollider != null && playerCollider != null && trapCollider.IsTouching(playerCollider))
            {
                PlayerController player = playerCollider.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.ApplySlow(slowMultiplier, slowDuration);
                    Debug.Log($"Trap: Player '{playerCollider.gameObject.name}' slowed after delay.");
                }
            }
            if (playerCollider != null)
            {
                playerDelayCoroutines.Remove(playerCollider);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (playerDelayCoroutines.ContainsKey(other))
                {
                    StopCoroutine(playerDelayCoroutines[other]);
                    playerDelayCoroutines.Remove(other);
                    Debug.Log($"Trap: Player '{other.gameObject.name}' exited trap before slow effect applied.");
                }
            }
        }
    }
}
