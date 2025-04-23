using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Resources.Scripts.Player;      // Access to player stats
using Resources.Scripts.SpellMode.Skills;
using Resources.Scripts.UI;          // For ManaSpendEffect

namespace Resources.Scripts.SpellMode
{
    /// <summary>
    /// Manages gesture drawing, recognition, and execution of spell skills.
    /// </summary>
    public class DrawingManager : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Start Mode")]
        [Tooltip("Drawing starts when the button is pressed.")]
        [SerializeField] private Button drawButton;

        [Header("UI Elements")]
        [SerializeField] private Text feedbackText;

        [Header("Line Prefab")]
        [Tooltip("Prefab with a LineRenderer component for displaying the drawn line.")]
        [SerializeField] private GameObject drawingLinePrefab;

        [Header("LineRenderer Settings")]
        [SerializeField] private Color drawingLineColor = Color.red;
        [SerializeField] private float drawingLineWidth = 0.1f;
        [SerializeField] private int drawingLineSortingOrder = 100;

        [Header("Drawing Settings")]
        [Tooltip("Maximum drawing time in seconds.")]
        [SerializeField] private float maxDrawingTime = 5f;
        [Tooltip("Minimum distance between points to be recorded.")]
        [SerializeField] private float movementThreshold = 0.1f;
        [Tooltip("Time without movement after which the player is considered stopped.")]
        [SerializeField] private float stopTimeThreshold = 0.5f;
        [Tooltip("Minimum number of points required for recognition.")]
        [SerializeField] private int minPointsForRecognition = 10;
        [Tooltip("Minimum total path length for a valid gesture.")]
        [SerializeField] private float minTotalPathLength = 1.0f;

        [Header("Mana Spend FX")]
        [SerializeField] private GameObject manaSpendEffectPrefab;
        [SerializeField] private RectTransform manaBarUI;
        [SerializeField] private Canvas mainCanvas;

        [Header("Recognition Settings")]
        [SerializeField] private IconGestureRecognizer gestureRecognizer;
        [SerializeField] private int resampleCount = 64;
        [SerializeField] private int smoothingWindow = 3;

        [Header("Player Stats")]
        [Tooltip("Reference to the component managing player's mana.")]
        [SerializeField] private PlayerStatsHandler playerStatsHandler;

        [Header("Combo Settings")]
        [Tooltip("Allow multiple gestures in one drawing session.")]
        [SerializeField] private bool enableComboMode = true;
        [Tooltip("Maximum time allowed between gestures in a combo session.")]
        [SerializeField] private float comboTimeWindow = 3f;

        #endregion

        #region Private State

        private bool isDrawing;
        private float drawingTimer;
        private float noMovementTimer;
        private float comboTimer;

        private Vector3 lastRecordedPosition;
        private readonly List<Vector3> drawnPoints = new List<Vector3>();
        private LineRenderer currentLine;
        private bool hasMoved;

        private readonly List<SignTemplateIcon> recognizedSigns = new List<SignTemplateIcon>();

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            if (drawButton != null)
                drawButton.onClick.AddListener(StartDrawing);

            lastRecordedPosition = transform.position;
        }

        private void Update()
        {
            if (!isDrawing)
                return;

            drawingTimer += Time.deltaTime;
            Vector3 currentPos = transform.position;
            float distance = Vector3.Distance(currentPos, lastRecordedPosition);

            if (distance >= movementThreshold)
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
                    if (enableComboMode)
                    {
                        ProcessPartialDrawing();
                        drawnPoints.Clear();
                        noMovementTimer = 0f;
                    }
                    else
                    {
                        EndDrawing();
                        return;
                    }
                }
            }

            if (drawingTimer >= maxDrawingTime)
                EndDrawing();

            if (recognizedSigns.Count > 0)
            {
                comboTimer += Time.deltaTime;
                if (comboTimer >= comboTimeWindow)
                    EndDrawing();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the drawing session.
        /// </summary>
        public void StartDrawing()
        {
            if (currentLine != null)
            {
                Destroy(currentLine.gameObject);
                currentLine = null;
            }

            isDrawing = true;
            drawingTimer = noMovementTimer = comboTimer = 0f;
            hasMoved = false;
            drawnPoints.Clear();
            recognizedSigns.Clear();

            lastRecordedPosition = transform.position;
            drawnPoints.Add(lastRecordedPosition);

            if (drawingLinePrefab != null)
            {
                GameObject lineObj = Instantiate(drawingLinePrefab);
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
                else
                {
                    Debug.LogError("[DrawingManager] drawingLinePrefab has no LineRenderer!");
                }
            }
            else
            {
                Debug.LogError("[DrawingManager] drawingLinePrefab is not assigned.");
            }

            if (feedbackText != null)
                feedbackText.text = "Drawing...";
        }

        /// <summary>
        /// Finalizes the drawing session and executes recognized skills.
        /// </summary>
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

            // 1) Sum up all mana costs
            int totalManaCost = 0;
            foreach (var template in recognizedSigns)
                totalManaCost += Mathf.RoundToInt(template.manaCost);

            Debug.Log($"[DrawingManager] Attempt to spend {totalManaCost} mana");

            // 2) Consume and spawn the FX
            if (playerStatsHandler != null && playerStatsHandler.UseMana(totalManaCost))
            {
                ShowManaSpendEffect(totalManaCost);

                // 3) Spawn each skill effect
                foreach (var template in recognizedSigns)
                    ExecuteSkill(template, isCombo);

                if (feedbackText != null)
                    feedbackText.text = isCombo ? "Combo Skill Activated!" : "Skill Activated!";
            }
            else
            {
                if (feedbackText != null)
                    feedbackText.text = "Not enough mana";
            }

            recognizedSigns.Clear();
            drawnPoints.Clear();
            comboTimer = 0f;
        }

        #endregion

        #region Private Methods

        private void ProcessPartialDrawing()
        {
            if (drawnPoints.Count < minPointsForRecognition)
                return;

            float totalLength = 0f;
            for (int i = 1; i < drawnPoints.Count; i++)
                totalLength += Vector3.Distance(drawnPoints[i - 1], drawnPoints[i]);

            if (totalLength < minTotalPathLength)
                return;

            var points2D = new List<Vector2>(drawnPoints.Count);
            foreach (var pt in drawnPoints)
                points2D.Add(new Vector2(pt.x, pt.y));

            var normalized = GestureUtils.NormalizePoints(points2D, resampleCount, smoothingWindow);
            Debug.Log($"Normalized Input: {Vector2ListToString(normalized)}");

            var recognized = gestureRecognizer.Recognize(normalized);
            if (recognized != null)
            {
                recognizedSigns.Add(recognized);
                comboTimer = 0f;
                if (feedbackText != null)
                    feedbackText.text = $"Added: {recognized.id}";
            }
        }

        private void ExecuteSkill(SignTemplateIcon template, bool isCombo)
        {
            if (template.skillPrefab == null)
                return;

            var skillObj = Instantiate(template.skillPrefab, transform.position, Quaternion.identity);
            if (!isCombo)
                return;

            var skillComp = skillObj.GetComponent<SkillBase>();
            if (skillComp == null)
                return;

            string idLower = template.id.ToLower();
            if (idLower.Contains("circle") && skillComp is PushEnemiesSkill push)
                push.pushForce *= 1.5f;
            else if (idLower.Contains("square") && skillComp is SlowEnemiesSkill slow)
                slow.slowDuration *= 1.5f;
            else if (idLower.Contains("triangle") && skillComp is SpeedBoostSkill speed)
                speed.boostDuration += 50f;
        }

        private void UpdateDrawingLine()
        {
            if (currentLine == null)
                return;

            currentLine.positionCount = drawnPoints.Count;
            for (int i = 0; i < drawnPoints.Count; i++)
                currentLine.SetPosition(i, drawnPoints[i]);
        }

        /// <summary>
        /// Spawns the floating "-XX" and sends it toward the mana bar.
        /// Uses the new FindFirstObjectByType API instead of the deprecated FindObjectOfType.
        /// </summary>
        private void ShowManaSpendEffect(int amount)
        {
            Debug.Log("[DrawingManager] Instantiating ManaSpendEffect");

            if (manaSpendEffectPrefab == null)
            {
                Debug.LogError("[DrawingManager] manaSpendEffectPrefab is not assigned!");
                return;
            }

            // Use the newer, non-deprecated API to find a Canvas in the scene
            if (mainCanvas == null)
            {
                mainCanvas = Object.FindFirstObjectByType<Canvas>();
                if (mainCanvas == null)
                {
                    Debug.LogError("[DrawingManager] Could not find any Canvas in the scene!");
                    return;
                }
            }

            if (manaBarUI == null)
            {
                Debug.LogError("[DrawingManager] manaBarUI is not assigned!");
                return;
            }

            var effectGo = Instantiate(
                manaSpendEffectPrefab,
                mainCanvas.transform,
                worldPositionStays: false
            );
            effectGo.transform.localScale = Vector3.one;

            var effect = effectGo.GetComponent<ManaSpendEffect>();
            if (effect != null)
            {
                effect.Initialize(amount, transform.position, manaBarUI);
            }
            else
            {
                Debug.LogError("[DrawingManager] Prefab does not contain a ManaSpendEffect component!");
            }
        }

        /// <summary>
        /// Converts a list of Vector2 points to a readable string.
        /// </summary>
        private string Vector2ListToString(List<Vector2> points)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var p in points)
                sb.AppendFormat("({0:F2}, {1:F2}) ", p.x, p.y);
            return sb.ToString();
        }

        #endregion
    }
}
