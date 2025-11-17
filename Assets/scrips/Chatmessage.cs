using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatMessage : MonoBehaviour
{
    [Header("Message Components")]
    public TextMeshProUGUI messageText;
    public Image backgroundImage;
    public LayoutElement layoutElement;

    [Header("Animation Settings")]
    public float fadeInDuration = 0.5f;
    public AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private CanvasGroup canvasGroup;
    private bool isPlayer;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 初始設為透明
        canvasGroup.alpha = 0f;
    }

    public void Initialize(string message, bool isPlayerMessage, Color? customColor = null)
    {
        isPlayer = isPlayerMessage;
        messageText.text = message;

        // 設置訊息對齊
        if (isPlayerMessage)
        {
            // 玩家訊息靠左
            messageText.alignment = TextAlignmentOptions.Left;
            if (backgroundImage != null)
            {
                backgroundImage.color = customColor ?? new Color(0.2f, 0.6f, 1f, 0.8f); // 藍色背景
            }
        }
        else
        {
            // 系統訊息靠右
            messageText.alignment = TextAlignmentOptions.Right;
            if (backgroundImage != null)
            {
                backgroundImage.color = customColor ?? new Color(0.8f, 0.8f, 0.8f, 0.8f); // 灰色背景
            }
        }

        // 開始淡入動畫
        StartCoroutine(FadeInAnimation());
    }

    private IEnumerator FadeInAnimation()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            float progress = elapsedTime / fadeInDuration;
            canvasGroup.alpha = fadeInCurve.Evaluate(progress);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    public void SetMessageStyle(MessageStyle style)
    {
        switch (style)
        {
            case MessageStyle.Player:
                SetPlayerStyle();
                break;
            case MessageStyle.System:
                SetSystemStyle();
                break;
            case MessageStyle.Warning:
                SetWarningStyle();
                break;
            case MessageStyle.Success:
                SetSuccessStyle();
                break;
        }
    }

    private void SetPlayerStyle()
    {
        messageText.color = Color.white;
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(0.2f, 0.6f, 1f, 0.8f);
        }
        messageText.alignment = TextAlignmentOptions.Left;
    }

    private void SetSystemStyle()
    {
        messageText.color = Color.black;
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(0.9f, 0.9f, 0.9f, 0.8f);
        }
        messageText.alignment = TextAlignmentOptions.Right;
    }

    private void SetWarningStyle()
    {
        messageText.color = Color.white;
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(1f, 0.6f, 0.2f, 0.8f); // 橙色警告
        }
        messageText.alignment = TextAlignmentOptions.Center;
    }

    private void SetSuccessStyle()
    {
        messageText.color = Color.white;
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(0.2f, 0.8f, 0.2f, 0.8f); // 綠色成功
        }
        messageText.alignment = TextAlignmentOptions.Center;
    }

    public void UpdateMessage(string newMessage)
    {
        messageText.text = newMessage;

        // 重新計算佈局
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    public void HighlightMessage(float duration = 2f)
    {
        StartCoroutine(HighlightEffect(duration));
    }

    private IEnumerator HighlightEffect(float duration)
    {
        Color originalColor = backgroundImage != null ? backgroundImage.color : Color.white;
        Color highlightColor = new Color(1f, 1f, 0.3f, 0.8f); // 黃色高亮

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float progress = Mathf.PingPong(elapsedTime * 2f, 1f);

            if (backgroundImage != null)
            {
                backgroundImage.color = Color.Lerp(originalColor, highlightColor, progress * 0.5f);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = originalColor;
        }
    }
}

public enum MessageStyle
{
    Player,
    System,
    Warning,
    Success
}