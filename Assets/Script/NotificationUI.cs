using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class NotificationUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI notifyText;

    [Header("Animation Settings")]
    [SerializeField] private float slideDistance = 100f;
    [SerializeField] private float slideInDuration = 0.35f;
    [SerializeField] private float stayDuration = 2.0f;
    [SerializeField] private float slideOutDuration = 0.35f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 hiddenPos;
    private Vector2 visiblePos;
    private Coroutine currentCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        visiblePos = rectTransform.anchoredPosition;
        hiddenPos = visiblePos + new Vector2(0f, slideDistance);

        rectTransform.anchoredPosition = hiddenPos;
        canvasGroup.alpha = 0f;
    }

    public void ShowNotification(string message)
    {
        if (notifyText != null)
        {
            notifyText.text = message;
        }

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(AnimateNotification());
    }

    private IEnumerator AnimateNotification()
    {
        float elapsed = 0f;
        while (elapsed < slideInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideInDuration);

            float ease = 1f - Mathf.Pow(1f - t, 3f);

            rectTransform.anchoredPosition = Vector2.Lerp(hiddenPos, visiblePos, ease);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        rectTransform.anchoredPosition = visiblePos;
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(stayDuration);

        elapsed = 0f;
        while (elapsed < slideOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideOutDuration);
            float ease = t * t;

            rectTransform.anchoredPosition = Vector2.Lerp(visiblePos, hiddenPos, ease);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        rectTransform.anchoredPosition = hiddenPos;
        canvasGroup.alpha = 0f;
        currentCoroutine = null;
    }
}