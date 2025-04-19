using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Resources.Scripts.Player; // Доступ к PlayerStatsHandler
using Resources.Scripts.SpellMode.Skills;

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
        public IconGestureRecognizer gestureRecognizer;
        public int resampleCount = 64;
        public int smoothingWindow = 3;

        [Header("Player Stats")]
        [Tooltip("Reference to the component managing the player's mana.")]
        public PlayerStatsHandler playerStatsHandler;

        [Header("Combo Settings")]
        [Tooltip("Enable combo mode (multiple gestures in one drawing session)")]
        public bool enableComboMode = true;
        [Tooltip("Maximum time (seconds) allowed between individual gestures in a combo session")]
        public float comboTimeWindow = 3f;

        // Internal variables
        private bool isDrawing;
        private float drawingTimer;
        private float noMovementTimer;
        private Vector3 lastRecordedPosition;
        private List<Vector3> drawnPoints = new List<Vector3>();
        private LineRenderer currentLine;
        private bool hasMoved;

        // Накопление распознанных знаков (комбо)
        private List<SignTemplateIcon> recognizedSigns = new List<SignTemplateIcon>();
        private float comboTimer;

        // Накопление затраченной маны за сессию
        private float totalManaUsed;

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

                if (enableComboMode && hasMoved && noMovementTimer >= stopTimeThreshold)
                {
                    ProcessPartialDrawing();
                    noMovementTimer = 0f;
                    drawnPoints.Clear();
                }
                else if (!enableComboMode && hasMoved && noMovementTimer >= stopTimeThreshold)
                {
                    EndDrawing();
                    return;
                }
            }

            if (drawingTimer >= maxDrawingTime)
            {
                EndDrawing();
            }

            if (recognizedSigns.Count > 0)
            {
                comboTimer += Time.deltaTime;
                if (comboTimer >= comboTimeWindow)
                {
                    EndDrawing();
                }
            }
        }

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
            comboTimer = 0f;
            hasMoved = false;
            drawnPoints.Clear();
            recognizedSigns.Clear();
            totalManaUsed = 0f;
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

        public void EndDrawing()
        {
            if (!isDrawing)
                return;

            isDrawing = false;
            if (feedbackText != null)
                feedbackText.text = "Processing...";

            if (drawnPoints.Count >= minPointsForRecognition)
                ProcessPartialDrawing();

            if (recognizedSigns.Count == 0)
            {
                if (feedbackText != null)
                    feedbackText.text = "Skill not recognized";
                return;
            }

            bool isCombo = recognizedSigns.Count >= 2;

            foreach (SignTemplateIcon template in recognizedSigns)
                ExecuteSkill(template, isCombo);

            if (feedbackText != null)
            {
                string baseMessage = isCombo ? "Combo Skill Activated!" : "Skill Activated!";
                feedbackText.text = $"{baseMessage} Used {totalManaUsed:F0} mana.";
            }

            recognizedSigns.Clear();
            drawnPoints.Clear();
            comboTimer = 0f;
        }

        private void ProcessPartialDrawing()
        {
            if (drawnPoints.Count < minPointsForRecognition)
                return;

            float totalLength = 0f;
            for (int i = 1; i < drawnPoints.Count; i++)
                totalLength += Vector3.Distance(drawnPoints[i - 1], drawnPoints[i]);

            if (totalLength < minTotalPathLength)
                return;

            List<Vector2> points2D = new List<Vector2>();
            foreach (Vector3 pt in drawnPoints)
                points2D.Add(new Vector2(pt.x, pt.y));

            List<Vector2> normalizedInput = GestureUtils.NormalizePoints(points2D, resampleCount, smoothingWindow);
            Debug.Log("Normalized Input: " + Vector2ListToString(normalizedInput));

            SignTemplateIcon recognized = gestureRecognizer.Recognize(normalizedInput);
            if (recognized != null)
            {
                recognizedSigns.Add(recognized);
                comboTimer = 0f;
                if (feedbackText != null)
                    feedbackText.text = "Added: " + recognized.id;
            }
        }

        private void ExecuteSkill(SignTemplateIcon template, bool isCombo)
        {
            if (playerStatsHandler != null)
            {
                if (!playerStatsHandler.UseMana(template.manaCost))
                {
                    if (feedbackText != null)
                        feedbackText.text = "Not enough mana";
                    return;
                }
                totalManaUsed += template.manaCost;
                Debug.Log($"Used {template.manaCost:F0} mana for {template.id}");
            }

            if (template.skillPrefab != null)
            {
                GameObject skillObj = Instantiate(template.skillPrefab, transform.position, Quaternion.identity);
                if (isCombo)
                {
                    SkillBase skillComponent = skillObj.GetComponent<SkillBase>();
                    if (skillComponent != null)
                    {
                        if (template.id.ToLower().Contains("circle"))
                        {
                            PushEnemiesSkill pushSkill = skillComponent as PushEnemiesSkill;
                            if (pushSkill != null)
                                pushSkill.pushForce *= 1.5f;
                        }
                        else if (template.id.ToLower().Contains("square"))
                        {
                            SlowEnemiesSkill slowSkill = skillComponent as SlowEnemiesSkill;
                            if (slowSkill != null)
                                slowSkill.slowDuration *= 1.5f;
                        }
                        else if (template.id.ToLower().Contains("triangle"))
                        {
                            SpeedBoostSkill speedSkill = skillComponent as SpeedBoostSkill;
                            if (speedSkill != null)
                                speedSkill.boostDuration += 50f;
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("SkillPrefab not assigned for sign " + template.id);
            }
        }

        private void UpdateDrawingLine()
        {
            if (currentLine == null)
                return;

            currentLine.positionCount = drawnPoints.Count;
            for (int i = 0; i < drawnPoints.Count; i++)
                currentLine.SetPosition(i, drawnPoints[i]);
        }

        private string Vector2ListToString(List<Vector2> points)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (Vector2 p in points)
                sb.AppendFormat("({0:F2}, {1:F2}) ", p.x, p.y);
            return sb.ToString();
        }
    }
}