using TMPro;
using UnityEngine;

namespace Resources.Scripts.Labyrinth
{
    public class LabyrinthCellPrefab : MonoBehaviour
    {
        [SerializeField] private GameObject topBorder;
        [SerializeField] private GameObject rightBorder;
        [SerializeField] private GameObject bottomBorder;
        [SerializeField] private GameObject leftBorder;
        [SerializeField] private TextMeshProUGUI arrayValue;
        
        public void Init(LabyrinthCell cell)
        {
            if (!cell.topBorder)
                Destroy(topBorder);

            if (!cell.rightBorder)
                Destroy(rightBorder);

            if (!cell.bottomBorder)
                Destroy(bottomBorder);

            if (!cell.leftBorder)
                Destroy(leftBorder);
        }
    }
}
