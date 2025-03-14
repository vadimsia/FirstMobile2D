using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Resources.Scripts.Enemy;
using Resources.Scripts.Player;

namespace Resources.Scripts.TrapSkill
{
    public class Trap : MonoBehaviour
    {
        [SerializeField] private float slowMultiplier = 0.5f; // 50% speed reduction
        [SerializeField] private float slowDuration = 3f;     // Slow effect duration
        [SerializeField] private float playerDelay = 2f;      // Delay before slowing the player

        private float spawnTime;
        private readonly Dictionary<Collider2D, Coroutine> playerDelayCoroutines = new();

        private void Start()
        {
            if (!IsInCorrectScene()) return;

            spawnTime = Time.time;
            Destroy(gameObject, 5f); // The trap disappears after 5 seconds
        }

        private bool IsInCorrectScene()
        {
            if (SceneManager.GetActiveScene().name == "FirstPartScene") return true;

            Destroy(gameObject);
            return false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy"))
            {
                other.GetComponent<EnemyController>()?.ApplySlow(slowMultiplier, slowDuration);
                Debug.Log($"Trap: Enemy '{other.gameObject.name}' slowed immediately.");
            }
            else if (other.CompareTag("Player"))
            {
                HandlePlayerEntry(other);
            }
        }

        private void HandlePlayerEntry(Collider2D playerCollider)
        {
            if (Time.time - spawnTime >= playerDelay)
            {
                playerCollider.GetComponent<PlayerController>()?.ApplySlow(slowMultiplier, slowDuration);
                Debug.Log($"Trap: Player '{playerCollider.gameObject.name}' slowed immediately on re-entry.");
            }
            else if (!playerDelayCoroutines.ContainsKey(playerCollider))
            {
                playerDelayCoroutines[playerCollider] = StartCoroutine(PlayerDelayCoroutine(playerCollider));
            }
        }

        private IEnumerator PlayerDelayCoroutine(Collider2D playerCollider)
        {
            yield return new WaitForSeconds(playerDelay);
            
            if (playerCollider != null && GetComponent<Collider2D>().IsTouching(playerCollider))
            {
                playerCollider.GetComponent<PlayerController>()?.ApplySlow(slowMultiplier, slowDuration);
                Debug.Log($"Trap: Player '{playerCollider.gameObject.name}' slowed after delay.");
            }

            playerDelayCoroutines.Remove(playerCollider);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") && playerDelayCoroutines.TryGetValue(other, out Coroutine coroutine))
            {
                StopCoroutine(coroutine);
                playerDelayCoroutines.Remove(other);
                Debug.Log($"Trap: Player '{other.gameObject.name}' exited trap before slow effect applied.");
            }
        }
    }
}
