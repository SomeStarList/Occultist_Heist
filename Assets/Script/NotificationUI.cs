using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class NotificationUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI notifyText;

    [Header("Animation Settings")]
    [SerializeField] private float slideDistance = 100f; // ระยะที่เลื่อนลงมา (พิกเซล)
    [SerializeField] private float slideInDuration = 0.35f; // เวลาตอนเลื่อนลง
    [SerializeField] private float stayDuration = 2.0f;    // เวลาที่ค้างไว้ให้อ่าน
    [SerializeField] private float slideOutDuration = 0.35f; // เวลาตอนเลื่อนกลับและจางหาย

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 hiddenPos;
    private Vector2 visiblePos;
    private Coroutine currentCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // กำหนดตำแหน่งซ่อน (อยู่เหนือจอขึ้นไป) และตำแหน่งที่มองเห็น
        visiblePos = rectTransform.anchoredPosition;
        hiddenPos = visiblePos + new Vector2(0f, slideDistance);

        // รีเซ็ตสถานะเริ่มต้นให้ซ่อนอยู่
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
        // 1. เลื่อนลงมา + ค่อยๆ ชัดขึ้น (Slide In & Fade In)
        float elapsed = 0f;
        while (elapsed < slideInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideInDuration);
            // Ease-Out Back effect ให้มีความเด้งเบาๆ ตอนหยุด
            float ease = 1f - Mathf.Pow(1f - t, 3f);

            rectTransform.anchoredPosition = Vector2.Lerp(hiddenPos, visiblePos, ease);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        rectTransform.anchoredPosition = visiblePos;
        canvasGroup.alpha = 1f;

        // 2. หยุดค้างไว้ตามเวลาที่กำหนด
        yield return new WaitForSeconds(stayDuration);

        // 3. เลื่อนกลับขึ้นบน + ค่อยๆ จางหาย (Slide Out & Fade Out)
        elapsed = 0f;
        while (elapsed < slideOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideOutDuration);
            float ease = t * t; // Ease-In ให้ค่อยๆ เร่งความเร็วตอนถอยกลับ

            rectTransform.anchoredPosition = Vector2.Lerp(visiblePos, hiddenPos, ease);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        rectTransform.anchoredPosition = hiddenPos;
        canvasGroup.alpha = 0f;
        currentCoroutine = null;
    }
}