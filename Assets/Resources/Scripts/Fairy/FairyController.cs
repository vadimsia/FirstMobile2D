using UnityEngine;

namespace Resources.Scripts.Fairy
{
    /// <summary>
    /// Controls fairy behavior including movement and disappearance upon being collected.
    /// </summary>
    public class FairyController : MonoBehaviour
    {
        [SerializeField, Range(1, 5)]
        private int maxMoveRadius = 2;
        [SerializeField, Range(3, 15)]
        private int speed = 5;

        private Vector3 startPosition;
        private Vector3 targetPosition;

        /// <summary>
        /// Initializes the fairy with a starting position.
        /// </summary>
        /// <param name="startPosition">The initial position of the fairy.</param>
        public void Init(Vector3 startPosition)
        {
            this.startPosition = startPosition;
            targetPosition = startPosition;
        }

        /// <summary>
        /// Updates the target position for the fairy's movement.
        /// </summary>
        private void UpdateTargetPosition()
        {
            targetPosition = startPosition - new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
            targetPosition *= Random.Range(3, maxMoveRadius);
        }

        private void Update()
        {
            if ((transform.position - targetPosition).magnitude < 0.1f)
            {
                UpdateTargetPosition();
            }

            transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
        }

        /// <summary>
        /// Destroys the fairy from the scene when collected.
        /// </summary>
        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}