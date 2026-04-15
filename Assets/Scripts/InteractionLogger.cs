using System;
using System.IO;
using UnityEngine;

public class InteractionLogger : MonoBehaviour
{
    public static InteractionLogger Instance { get; private set; }

    [Tooltip("Optional PieceHandler to auto-subscribe to. Leave null to log manually.")]
    public PieceHandler pieceHandler;

    private string filePath;
    private StreamWriter writer;

    private const string Header =
        "timestamp,event_type,piece_glyph_id,piece_name,contact_id,screen_x,screen_y,rotation_deg,target_step_index,was_correct,difficulty,recipe_name";

    void Awake()
    {
        // Singleton check MUST run before OpenFile — otherwise a duplicate
        // instance opens a second CSV and then self-destructs without
        // disposing the writer, leaking both the file handle and the log.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        OpenFile();
    }

    void OnEnable()
    {
        Subscribe();
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    void OnDestroy()
    {
        Unsubscribe();
        CloseFile();
        if (Instance == this) Instance = null;
    }

    private void OpenFile()
    {
        try
        {
            string ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            filePath = Path.Combine(Application.persistentDataPath, $"interaction_log_{ts}.csv");
            writer = new StreamWriter(filePath, append: false);
            writer.WriteLine(Header);
            writer.Flush();
            Debug.Log($"[InteractionLogger] Logging to {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[InteractionLogger] Failed to open log file: {e.Message}");
        }
    }

    private void CloseFile()
    {
        try
        {
            writer?.Flush();
            writer?.Dispose();
            writer = null;
        }
        catch { }
    }

    private void Subscribe()
    {
        if (pieceHandler == null) return;
        pieceHandler.OnPiecePlaced += HandlePlaced;
        pieceHandler.OnPieceLifted += HandleLifted;
        pieceHandler.OnPieceMoved += HandleMoved;
    }

    private void Unsubscribe()
    {
        if (pieceHandler == null) return;
        pieceHandler.OnPiecePlaced -= HandlePlaced;
        pieceHandler.OnPieceLifted -= HandleLifted;
        pieceHandler.OnPieceMoved -= HandleMoved;
    }

    private float CurrentRotation(int contactId)
    {
        return pieceHandler != null ? pieceHandler.GetOrientationDegrees(contactId) : 0f;
    }

    private void HandlePlaced(int glyphId, Vector2 pos, int contactId)
    {
        LogEvent("placed", glyphId, PieceIdentifier.GetPieceName(glyphId), contactId, pos, CurrentRotation(contactId), -1, false, CurrentDifficulty(), CurrentRecipeName());
    }

    private void HandleLifted(int glyphId, Vector2 pos, int contactId)
    {
        LogEvent("lifted", glyphId, PieceIdentifier.GetPieceName(glyphId), contactId, pos, CurrentRotation(contactId), -1, false, CurrentDifficulty(), CurrentRecipeName());
    }

    private void HandleMoved(int glyphId, Vector2 pos, int contactId)
    {
        LogEvent("moved", glyphId, PieceIdentifier.GetPieceName(glyphId), contactId, pos, CurrentRotation(contactId), -1, false, CurrentDifficulty(), CurrentRecipeName());
    }

    private static string CurrentDifficulty()
    {
        return GameManager.Instance != null ? (GameManager.Instance.CurrentDifficulty ?? "") : "";
    }

    private static string CurrentRecipeName()
    {
        return (GameManager.Instance != null && GameManager.Instance.CurrentRecipe != null)
            ? GameManager.Instance.CurrentRecipe.name
            : "";
    }

    public void LogEvent(
        string eventType,
        int glyphId,
        string pieceName,
        int contactId,
        Vector2 screenPos,
        float rotationDeg,
        int targetStepIndex,
        bool wasCorrect,
        string difficulty,
        string recipeName)
    {
        if (writer == null) return;

        try
        {
            string line = string.Join(",",
                DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                Escape(eventType),
                glyphId.ToString(),
                Escape(pieceName),
                contactId.ToString(),
                screenPos.x.ToString("F2"),
                screenPos.y.ToString("F2"),
                rotationDeg.ToString("F2"),
                targetStepIndex.ToString(),
                wasCorrect ? "true" : "false",
                Escape(difficulty),
                Escape(recipeName)
            );
            writer.WriteLine(line);
            writer.Flush();
        }
        catch (Exception e)
        {
            Debug.LogError($"[InteractionLogger] Write failed: {e.Message}");
        }
    }

    private static string Escape(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        if (s.Contains(",") || s.Contains("\""))
        {
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }
        return s;
    }
}
