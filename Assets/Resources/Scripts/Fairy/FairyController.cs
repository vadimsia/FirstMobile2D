using UnityEngine;

namespace Resources.Scripts.Fairy
{
    public class FairyController : MonoBehaviour
    {
        [SerializeField, Range(1, 5)] private int maxMoveRadius = 2;
        [SerializeField, Range(3, 15)] private int speed = 5;

        private Vector3 startPosition;
        private Vector3 targetPosition;

        
        public void Init(Vector3 startPosition)
        {
            targetPosition = startPosition = startPosition;
        }

        private void UpdateTargetPosition()
        {
            targetPosition = startPosition - new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0);
            targetPosition *= Random.Range(3, maxMoveRadius);
        }

        private void Update()
        {
            if ((transform.position - targetPosition).magnitude < 0.1f) {
                UpdateTargetPosition();
            }

            transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
        }


        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}
