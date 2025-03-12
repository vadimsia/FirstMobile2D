using UnityEngine;

namespace Resources.Scripts.Fairy
{
    public class FairyController : MonoBehaviour
    {
        [SerializeField, Range(1, 5)] private int maxMoveRadius = 2;
        [SerializeField, Range(3, 15)] private int speed = 5;

        private Vector3 _startPosition;
        private Vector3 _targetPosition;

        
        public void Init(Vector3 startPosition)
        {
            _targetPosition = _startPosition = startPosition;
        }

        private void UpdateTargetPosition()
        {
            _targetPosition = _startPosition - new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0);
            _targetPosition *= Random.Range(3, maxMoveRadius);
        }

        private void Update()
        {
            if ((transform.position - _targetPosition).magnitude < 0.1f) {
                UpdateTargetPosition();
            }

            transform.position = Vector3.Lerp(transform.position, _targetPosition, speed * Time.deltaTime);
        }


        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}
