using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StepCardUI : MonoBehaviour
{
    [Header("References")]
    public RectTransform hitArea;
    public TMP_Text displayText;
    public Image background;
    public Image checkmark;
    public Image glowBorder;

    [Header("Colors")]
    public Color defaultColor = new Color(1f, 0.98f, 0.92f);
    public Color selectedColor = new Color(1f, 0.95f, 0.6f);
    public Color completedColor = new Color(0.7f, 1f, 0.7f);
    public Color wrongColor = new Color(1f, 0.6f, 0.6f);

    public int StepIndex { get; private set; }
    public RecipeStep Step { get; private set; }
    public bool IsCompleted { get; private set; }
    public bool IsSelected { get; private set; }

    private Coroutine wrongFlashRoutine;

    void Reset()
    {
        hitArea = GetComponent<RectTransform>();
    }

    void Awake()
    {
        if (hitArea == null) hitArea = GetComponent<RectTransform>();
        if (checkmark != null) checkmark.enabled = false;
        if (glowBorder != null) glowBorder.enabled = false;
    }

    public void SetStep(RecipeStep step)
    {
        Step = step;
        StepIndex = step != null ? step.index : -1;
        IsCompleted = false;
        IsSelected = false;

        if (displayText != null && step != null)
        {
            displayText.text = step.displayText;
        }
        if (background != null) background.color = defaultColor;
        if (checkmark != null) checkmark.enabled = false;
        if (glowBorder != null) glowBorder.enabled = false;
        transform.localScale = Vector3.one;
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        if (IsCompleted) return;

        if (glowBorder != null) glowBorder.enabled = selected;
        if (background != null) background.color = selected ? selectedColor : defaultColor;
        transform.localScale = selected ? Vector3.one * 1.05f : Vector3.one;
    }

    public void SetCompleted(bool completed)
    {
        IsCompleted = completed;
        if (completed)
        {
            if (background != null) background.color = completedColor;
            if (checkmark != null) checkmark.enabled = true;
            if (glowBorder != null) glowBorder.enabled = false;
            if (displayText != null && Step != null && !string.IsNullOrEmpty(Step.completedText))
            {
                displayText.text = Step.completedText;
            }
            transform.localScale = Vector3.one;
        }
    }

    public void FlashWrong()
    {
        if (wrongFlashRoutine != null) StopCoroutine(wrongFlashRoutine);
        wrongFlashRoutine = StartCoroutine(WrongFlash());
    }

    private IEnumerator WrongFlash()
    {
        if (background == null) yield break;
        Color start = background.color;
        background.color = wrongColor;
        yield return new WaitForSeconds(0.5f);
        background.color = IsCompleted ? completedColor : (IsSelected ? selectedColor : defaultColor);
        wrongFlashRoutine = null;
    }

    public bool ContainsScreenPoint(Vector2 screenPos, Camera uiCamera)
    {
        if (hitArea == null) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(hitArea, screenPos, uiCamera);
    }
}
