using UnityEngine;
using Resources.Scripts.Player;

namespace Resources.Scripts.Obstacles
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class TreeTrigger : MonoBehaviour
    {
        [Tooltip("На сколько единиц поднять порядок рендеринга у игрока, когда он в триггере")]
        [SerializeField] private int orderOffset = 1;

        private CircleCollider2D triggerCollider;

        private void Awake()
        {
            // Берём только CircleCollider2D и гарантируем, что он — триггер
            triggerCollider = GetComponent<CircleCollider2D>();
            triggerCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var playerSR = other.GetComponent<SpriteRenderer>();
            if (playerSR == null) return;

            // Хранилище оригинального порядка игрока
            var keeper = other.GetComponent<PlayerSortingKeeper>();
            if (keeper == null)
                keeper = other.gameObject.AddComponent<PlayerSortingKeeper>();

            keeper.CacheOriginalOrder(playerSR.sortingOrder);

            // Ищем SpriteRenderer дерева — на том же объекте или в родителе
            var treeSR = GetComponent<SpriteRenderer>() 
                         ?? GetComponentInParent<SpriteRenderer>();
            if (treeSR == null) return;

            // Поднимаем игрока над деревом
            playerSR.sortingOrder = treeSR.sortingOrder + orderOffset;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var playerSR = other.GetComponent<SpriteRenderer>();
            var keeper = other.GetComponent<PlayerSortingKeeper>();
            if (playerSR == null || keeper == null) return;

            // Восстанавливаем базовый порядок
            playerSR.sortingOrder = keeper.OriginalOrder;
            keeper.ClearCache();
        }
    }
}
