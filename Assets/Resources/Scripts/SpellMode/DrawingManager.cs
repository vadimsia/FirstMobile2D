using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Resources.Scripts.Player; // Access to PlayerStatsHandler

namespace Resources.Scripts.SpellMode
{
    public class DrawingManager : MonoBehaviour
    {
        [Header("Start Mode")]
        [Tooltip("Drawing starts when the button is pressed.")]
        public Button drawButton;

        [Header("UI Elements")]
        public Text feedbackText;

        [Header("Line Prefab")]
        [Tooltip("Prefab with a LineRenderer component for displaying the drawn line.")]
        public GameObject drawingLinePrefab;

        [Header("LineRenderer Settings")]
        public Color drawingLineColor = Color.red;
        public float drawingLineWidth = 0.1f;
        public int drawingLineSortingOrder = 100;

        [Header("Drawing Settings")]
        [Tooltip("Maximum drawing time (seconds)")]
        public float maxDrawingTime = 5f;
        [Tooltip("Minimum distance between points to be recorded")]
        public float movementThreshold = 0.1f;
        [Tooltip("Time without movement (seconds) after which the player is considered to have stopped")]
        public float stopTimeThreshold = 0.5f;
        [Tooltip("Minimum number of points required for gesture recognition")]
        public int minPointsForRecognition = 10;
        [Tooltip("Minimum total path length (in world units) for a valid sign")]
        public float minTotalPathLength = 1.0f;

        [Header("Recognition Settings")]
        public IconGestureRecognizer gestureRecognizer; // Renamed to follow naming conventions
        public int resampleCount = 64;
        public int smoothingWindow = 3;
        // 'normalizeRotation' is omitted in the call below because its default is used

        [Header("Player Stats")]
        [Tooltip("Reference to the component managing the player's mana.")]
        public PlayerStatsHandler playerStatsHandler;

        // Internal variables (redundant default initializations removed)
        private bool isDrawing;
        private float drawingTimer;
        private float noMovementTimer;
        private Vector3 lastRecordedPosition;
        private List<Vector3> drawnPoints = new List<Vector3>();
        private LineRenderer currentLine;
        private bool hasMoved;

        void Start()
        {
            if (drawButton != null)
                drawButton.onClick.AddListener(StartDrawing);

            lastRecordedPosition = transform.position;
        }

        void Update()
        {
            if (!isDrawing)
                return;

            drawingTimer += Time.deltaTime;
            Vector3 currentPos = transform.position;
            float distanceSinceLast = Vector3.Distance(currentPos, lastRecordedPosition);

            if (distanceSinceLast >= movementThreshold)
            {
                noMovementTimer = 0f;
                hasMoved = true;
                drawnPoints.Add(currentPos);
                lastRecordedPosition = currentPos;
                UpdateDrawingLine();
            }
            else
            {
                noMovementTimer += Time.deltaTime;
                if (hasMoved && noMovementTimer >= stopTimeThreshold)
                {
                    EndDrawing();
                    return;
                }
            }

            if (drawingTimer >= maxDrawingTime)
            {
                EndDrawing();
            }
        }

        /// <summary>
        /// Starts the drawing process.
        /// </summary>
        public void StartDrawing()
        {
            if (currentLine != null)
            {
                Destroy(currentLine.gameObject);
                currentLine = null;
            }

            isDrawing = true;
            drawingTimer = 0f;
            noMovementTimer = 0f;
            hasMoved = false;
            drawnPoints.Clear();
            lastRecordedPosition = transform.position;
            drawnPoints.Add(lastRecordedPosition);

            if (drawingLinePrefab != null)
            {
                GameObject lineObj = Instantiate(drawingLinePrefab, Vector3.zero, Quaternion.identity);
                currentLine = lineObj.GetComponent<LineRenderer>();
                if (currentLine != null)
                {
                    currentLine.useWorldSpace = true;
                    currentLine.startWidth = drawingLineWidth;
                    currentLine.endWidth = drawingLineWidth;
                    currentLine.startColor = drawingLineColor;
                    currentLine.endColor = drawingLineColor;
                    currentLine.sortingOrder = drawingLineSortingOrder;
                    currentLine.positionCount = 1;
                    currentLine.SetPosition(0, lastRecordedPosition);
                }
            }
            else
            {
                Debug.LogError("drawingLinePrefab is not assigned!");
            }

            if (feedbackText != null)
                feedbackText.text = "Drawing...";
        }

        /// <summary>
        /// Ends the drawing process and processes the gesture.
        /// </summary>
        public void EndDrawing()
        {
            if (!isDrawing)
                return;

            isDrawing = false;
            if (feedbackText != null)
                feedbackText.text = "Processing...";

            if (drawnPoints.Count < minPointsForRecognition)
            {
                if (feedbackText != null)
                    feedbackText.text = "Skill not recognized (not enough points)";
                return;
            }

            float totalLength = 0f;
            for (int i = 1; i < drawnPoints.Count; i++)
            {
                totalLength += Vector3.Distance(drawnPoints[i - 1], drawnPoints[i]);
            }
            if (totalLength < minTotalPathLength)
            {
                if (feedbackText != null)
                    feedbackText.text = "Skill not recognized (sign too small)";
                return;
            }

            // Convert 3D drawn points to 2D
            List<Vector2> points2D = new List<Vector2>();
            foreach (Vector3 pt in drawnPoints)
                points2D.Add(new Vector2(pt.x, pt.y));

            // The NormalizePoints method uses its default for 'normalizeRotation' (true)
            List<Vector2> normalizedInput = GestureUtils.NormalizePoints(points2D, resampleCount, smoothingWindow);
            Debug.Log("Normalized Input: " + Vector2ListToString(normalizedInput));

            // Recognize the gesture
            SignTemplateIcon recognized = gestureRecognizer.Recognize(normalizedInput);
            if (recognized != null)
            {
                ExecuteSkill(recognized);
                if (feedbackText != null)
                    feedbackText.text = "Skill: " + recognized.id;
            }
            else
            {
                if (feedbackText != null)
                    feedbackText.text = "Skill not recognized";
            }
        }

        /// <summary>
        /// Updates the LineRenderer positions to reflect the drawn points.
        /// </summary>
        private void UpdateDrawingLine()
        {
            if (currentLine == null)
                return;

            currentLine.positionCount = drawnPoints.Count;
            for (int i = 0; i < drawnPoints.Count; i++)
            {
                currentLine.SetPosition(i, drawnPoints[i]);
            }
        }

        /// <summary>
        /// Executes the skill linked to the recognized template, checking player's mana.
        /// </summary>
        private void ExecuteSkill(SignTemplateIcon template)
        {
            // Check if the player has enough mana
            if (playerStatsHandler != null)
            {
                if (!playerStatsHandler.UseMana(template.manaCost))
                {
                    if (feedbackText != null)
                        feedbackText.text = "Not enough mana";
                    return;
                }
            }
            if (template.skillPrefab != null)
            {
                Instantiate(template.skillPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("SkillPrefab not assigned for sign " + template.id);
            }
        }

        /// <summary>
        /// Converts a list of Vector2 points to a formatted string.
        /// </summary>
        private string Vector2ListToString(List<Vector2> points)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (Vector2 p in points)
            {
                sb.AppendFormat("({0:F2}, {1:F2}) ", p.x, p.y);
            }
            return sb.ToString();
        }
    }
}
