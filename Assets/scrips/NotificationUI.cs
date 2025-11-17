using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotificationUI : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI messageText;
    public Image backgroundImage;
    public CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.5f;
    public float movementDistance = 50f;
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Colors")]
    public Color infoColor = new Color(0.3f, 0.6f, 1f, 0.9f);
    public Color successColor = new Color(0.3f, 0.8f, 0.3f, 0.9f);
    public Color warningColor = new Color(1f, 0.6f, 0.2f, 0.9f);
    public Color errorColor = new Color(1f, 0.3f, 0.3f, 0.9f);

    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private bool isInitialized = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        // 初始設為透明
        canvasGroup.alpha = 0f;
    }

    public void Initialize(string message, NotificationType type, float displayTime)
    {
        if (isInitialized) return;

        isInitialized = true;
        originalPosition = rectTransform.localPosition;

        // 設置訊息文字
        if (messageText != null)
        {
            messageText.text = message;
        }

        // 設置背景顏色
        SetNotificationColor(type);

        // 設置初始位置（稍微上移）
        rectTransform.localPosition = originalPosition + Vector3.up * movementDistance;

        // 開始動畫序列
        StartCoroutine(NotificationSequence(displayTime));
    }

    private void SetNotificationColor(NotificationType type)
    {
        Color targetColor = infoColor;

        switch (type)
        {
            case NotificationType.Info:
                targetColor = infoColor;
                break;
            case NotificationType.Success:
                targetColor = successColor;
                break;
            case NotificationType.Warning:
                targetColor = warningColor;
                break;
            case NotificationType.Error:
                targetColor = errorColor;
                break;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = targetColor;
        }

        // 根據背景顏色調整文字顏色
        if (messageText != null)
        {
            float brightness = targetColor.r * 0.299f + targetColor.g * 0.587f + targetColor.b * 0.114f;
            messageText.color = brightness > 0.5f ? Color.black : Color.white;
        }
    }

    private IEnumerator NotificationSequence(float displayTime)
    {
        // 淡入動畫
        yield return StartCoroutine(FadeIn());

        // 等待顯示時間
        yield return new WaitForSeconds(displayTime);

        // 淡出動畫
        yield return StartCoroutine(FadeOut());

        // 銷毀物件
        Destroy(gameObject);
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Vector3 startPosition = rectTransform.localPosition;

        while (elapsedTime < fadeInDuration)
        {
            float progress = elapsedTime / fadeInDuration;
            float curvedProgress = animationCurve.Evaluate(progress);

            // 透明度動畫
            canvasGroup.alpha = curvedProgress;

            // 位置動畫
            rectTransform.localPosition = Vector3.Lerp(startPosition, originalPosition, curvedProgress);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;
        rectTransform.localPosition = originalPosition;
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Vector3 startPosition = rectTransform.localPosition;
        Vector3 endPosition = originalPosition + Vector3.up * movementDistance * 0.5f;

        while (elapsedTime < fadeOutDuration)
        {
            float progress = elapsedTime / fadeOutDuration;
            float curvedProgress = animationCurve.Evaluate(progress);

            // 透明度動畫
            canvasGroup.alpha = 1f - curvedProgress;

            // 位置動畫
            rectTransform.localPosition = Vector3.Lerp(startPosition, endPosition, curvedProgress);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    public void ForceClose()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    // 點擊通知可以立即關閉
    public void OnNotificationClick()
    {
        ForceClose();
    }

    // 為通知添加彈跳效果
    public void AddBounceEffect()
    {
        StartCoroutine(BounceEffect());
    }

    private IEnumerator BounceEffect()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 bounceScale = originalScale * 1.1f;

        float bounceTime = 0.2f;
        float elapsedTime = 0f;

        // 放大
        while (elapsedTime < bounceTime * 0.5f)
        {
            float progress = elapsedTime / (bounceTime * 0.5f);
            transform.localScale = Vector3.Lerp(originalScale, bounceScale, progress);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 縮小
        elapsedTime = 0f;
        while (elapsedTime < bounceTime * 0.5f)
        {
            float progress = elapsedTime / (bounceTime * 0.5f);
            transform.localScale = Vector3.Lerp(bounceScale, originalScale, progress);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }
}