using UnityEngine;

namespace Resources.Scripts.Player
{
    /// <summary>
    /// Хранит оригинальный sortingOrder персонажа, чтобы вернуть его при выходе из триггера.
    /// </summary>
    public class PlayerSortingKeeper : MonoBehaviour
    {
        public int OriginalOrder { get; private set; }
        private bool hasCached;

        /// <summary>
        /// Сохраняет исходный порядок, первый раз при входе в любой триггер.
        /// </summary>
        public void CacheOriginalOrder(int order)
        {
            if (!hasCached)
            {
                OriginalOrder = order;
                hasCached = true;
            }
        }

        /// <summary>
        /// Сброс кеша, чтобы можно было снова сохранить при повторном триггере.
        /// </summary>
        public void ClearCache()
        {
            hasCached = false;
        }
    }
}