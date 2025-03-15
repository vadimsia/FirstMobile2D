using System.Collections.Generic;
using UnityEngine;

namespace Resources.Scripts.Labyrinth
{
    /// <summary>
    /// Draws the solution path on the minimap using a LineRenderer.
    /// Make sure this GameObject is on the "MinimapOnly" layer so that it appears only on the minimap.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class LabyrinthMinimapSolutionPathDrawer : MonoBehaviour
    {
        [SerializeField] private Color lineColor = Color.green;
        [SerializeField] private float lineWidth = 0.1f;

        private LineRenderer lineRenderer;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            // Create a simple material for the line (using Sprites/Default shader)
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = 0;
        }

        /// <summary>
        /// Draws the solution path using a list of world-space positions.
        /// </summary>
        /// <param name="positions">A list of positions representing the solution path.</param>
        public void DrawSolutionPath(List<Vector3> positions)
        {
            if (positions == null || positions.Count == 0)
                return;

            lineRenderer.positionCount = positions.Count;
            for (int i = 0; i < positions.Count; i++)
            {
                lineRenderer.SetPosition(i, positions[i]);
            }
        }
    }
}