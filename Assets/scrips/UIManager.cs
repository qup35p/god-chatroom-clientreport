using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    [Header("Main Panels")]
    public CanvasGroup mainTransparentPanel;
    public CanvasGroup bottomInputPanel;

    [Header("Visual Effects")]
    public float panelFadeSpeed = 2f;
    public AnimationCurve fadeAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Button Effects")]
    public float buttonScaleEffect = 1.1f;
    public float buttonAnimationDuration = 0.2f;

    [Header("Input Field Effects")]
    public Color inputFocusColor = new Color(0.3f, 0.7f, 1f, 0.8f);
    public Color inputNormalColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);

    [Header("Notification System")]
    public GameObject notificationPrefab;
    public Transform notificationParent;
    public float notificationDisplayTime = 3f;

    private GameManager gameManager;
    private Coroutine currentPanelAnimation;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        InitializeUI();
        SetupButtonEffects();
    }

    private void InitializeUI()
    {
        // 設置初始透明度
        if (mainTransparentPanel != null)
        {
            mainTransparentPanel.alpha = 0f;
            StartCoroutine(FadePanelIn(mainTransparentPanel));
        }

        if (bottomInputPanel != null)
        {
            bottomInputPanel.alpha = 0f;
            StartCoroutine(FadePanelIn(bottomInputPanel, 0.5f));
        }
    }

    private void SetupButtonEffects()
    {
        // 為所有按鈕添加懸停效果
        Button[] allButtons = FindObjectsOfType<Button>();

        foreach (Button button in allButtons)
        {
            AddButtonEffect(button);
        }

        // 為輸入框添加焦點效果（支援兩種類型）
        InputField[] inputFields = FindObjectsOfType<InputField>();
        foreach (InputField inputField in inputFields)
        {
            AddInputFieldEffect(inputField);
        }

        TMP_InputField[] tmpInputFields = FindObjectsOfType<TMP_InputField>();
        foreach (TMP_InputField tmpInputField in tmpInputFields)
        {
            AddTMPInputFieldEffect(tmpInputField);
        }
    }

    private void AddButtonEffect(Button button)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // 滑鼠進入
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((data) => { OnButtonHover(button, true); });
        trigger.triggers.Add(pointerEnter);

        // 滑鼠離開
        EventTrigger.Entry pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) => { OnButtonHover(button, false); });
        trigger.triggers.Add(pointerExit);

        // 點擊
        EventTrigger.Entry pointerClick = new EventTrigger.Entry();
        pointerClick.eventID = EventTriggerType.PointerClick;
        pointerClick.callback.AddListener((data) => { OnButtonClick(button); });
        trigger.triggers.Add(pointerClick);
    }

    private void AddInputFieldEffect(InputField inputField)
    {
        Image backgroundImage = inputField.GetComponent<Image>();

        // Unity 傳統 InputField 使用不同的事件系統
        EventTrigger trigger = inputField.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = inputField.gameObject.AddComponent<EventTrigger>();
        }

        // 獲得焦點
        EventTrigger.Entry selectEntry = new EventTrigger.Entry();
        selectEntry.eventID = EventTriggerType.Select;
        selectEntry.callback.AddListener((data) => {
            // 檢查物件是否還存在
            if (this == null || backgroundImage == null) return;

            if (backgroundImage != null)
            {
                SafeStartCoroutine(ChangeImageColor(backgroundImage, inputFocusColor, 0.3f));
            }
        });
        trigger.triggers.Add(selectEntry);

        // 失去焦點
        EventTrigger.Entry deselectEntry = new EventTrigger.Entry();
        deselectEntry.eventID = EventTriggerType.Deselect;
        deselectEntry.callback.AddListener((data) => {
            // 檢查物件是否還存在
            if (this == null || backgroundImage == null) return;

            if (backgroundImage != null)
            {
                SafeStartCoroutine(ChangeImageColor(backgroundImage, inputNormalColor, 0.3f));
            }
        });
        trigger.triggers.Add(deselectEntry);
    }

    private void AddTMPInputFieldEffect(TMP_InputField tmpInputField)
    {
        Image backgroundImage = tmpInputField.GetComponent<Image>();

        tmpInputField.onSelect.AddListener((string value) => {
            // 檢查物件是否還存在
            if (this == null || backgroundImage == null) return;

            if (backgroundImage != null)
            {
                SafeStartCoroutine(ChangeImageColor(backgroundImage, inputFocusColor, 0.3f));
            }
        });

        tmpInputField.onDeselect.AddListener((string value) => {
            // 檢查物件是否還存在
            if (this == null || backgroundImage == null) return;

            if (backgroundImage != null)
            {
                SafeStartCoroutine(ChangeImageColor(backgroundImage, inputNormalColor, 0.3f));
            }
        });
    }

    private void OnButtonHover(Button button, bool isHovering)
    {
        if (!button.interactable) return;

        float targetScale = isHovering ? buttonScaleEffect : 1f;
        StartCoroutine(ScaleButton(button.transform, targetScale));

        // 改變顏色效果
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            Color targetColor = isHovering ?
                new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 1f) :
                new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 0.8f);

            StartCoroutine(ChangeImageColor(buttonImage, targetColor, buttonAnimationDuration));
        }
    }

    private void OnButtonClick(Button button)
    {
        if (!button.interactable) return;

        StartCoroutine(ButtonClickEffect(button.transform));
    }

    private IEnumerator ScaleButton(Transform buttonTransform, float targetScale)
    {
        Vector3 originalScale = buttonTransform.localScale;
        Vector3 targetScaleVector = Vector3.one * targetScale;

        float elapsedTime = 0f;

        while (elapsedTime < buttonAnimationDuration)
        {
            float progress = elapsedTime / buttonAnimationDuration;
            buttonTransform.localScale = Vector3.Lerp(originalScale, targetScaleVector, progress);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        buttonTransform.localScale = targetScaleVector;
    }

    private IEnumerator ButtonClickEffect(Transform buttonTransform)
    {
        Vector3 originalScale = buttonTransform.localScale;
        Vector3 clickScale = originalScale * 0.95f;

        // 縮小
        float elapsedTime = 0f;
        while (elapsedTime < buttonAnimationDuration * 0.5f)
        {
            float progress = elapsedTime / (buttonAnimationDuration * 0.5f);
            buttonTransform.localScale = Vector3.Lerp(originalScale, clickScale, progress);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 恢復
        elapsedTime = 0f;
        while (elapsedTime < buttonAnimationDuration * 0.5f)
        {
            float progress = elapsedTime / (buttonAnimationDuration * 0.5f);
            buttonTransform.localScale = Vector3.Lerp(clickScale, originalScale, progress);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        buttonTransform.localScale = originalScale;
    }

    private IEnumerator ChangeImageColor(Image image, Color targetColor, float duration)
    {
        Color originalColor = image.color;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            image.color = Color.Lerp(originalColor, targetColor, progress);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        image.color = targetColor;
    }

    private IEnumerator FadePanelIn(CanvasGroup panel, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        float elapsedTime = 0f;

        while (elapsedTime < 1f / panelFadeSpeed)
        {
            float progress = elapsedTime * panelFadeSpeed;
            panel.alpha = fadeAnimationCurve.Evaluate(progress);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        panel.alpha = 1f;
    }

    public void ShowNotification(string message, NotificationType type = NotificationType.Info)
    {
        if (notificationPrefab == null || notificationParent == null) return;

        GameObject notification = Instantiate(notificationPrefab, notificationParent);
        NotificationUI notificationUI = notification.GetComponent<NotificationUI>();

        if (notificationUI != null)
        {
            notificationUI.Initialize(message, type, notificationDisplayTime);
        }
    }

    public void ShowCustomerReportSaved()
    {
        ShowNotification("客戶報表已儲存！", NotificationType.Success);
    }

    public void ShowTimeWarning()
    {
        ShowNotification("注意：剩餘時間不多！", NotificationType.Warning);
    }

    public void ShowGameEnd()
    {
        ShowNotification("時間結束！", NotificationType.Info);

        // 淡出所有面板
        if (currentPanelAnimation != null)
        {
            StopCoroutine(currentPanelAnimation);
        }
        currentPanelAnimation = StartCoroutine(FadeOutAllPanels());
    }

    private IEnumerator FadeOutAllPanels()
    {
        float elapsedTime = 0f;
        float fadeOutDuration = 2f;

        float initialMainAlpha = mainTransparentPanel != null ? mainTransparentPanel.alpha : 0f;
        float initialBottomAlpha = bottomInputPanel != null ? bottomInputPanel.alpha : 0f;

        while (elapsedTime < fadeOutDuration)
        {
            float progress = elapsedTime / fadeOutDuration;
            float alpha = Mathf.Lerp(1f, 0.5f, progress); // 不完全淡出，保持一些可見性

            if (mainTransparentPanel != null)
            {
                mainTransparentPanel.alpha = Mathf.Lerp(initialMainAlpha, alpha, progress);
            }

            if (bottomInputPanel != null)
            {
                bottomInputPanel.alpha = Mathf.Lerp(initialBottomAlpha, alpha, progress);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    // 安全的協程啟動方法
    private void SafeStartCoroutine(System.Collections.IEnumerator routine)
    {
        if (this != null && gameObject != null && gameObject.activeInHierarchy)
        {
            StartCoroutine(routine);
        }
    }

    private void OnDestroy()
    {
        // 停止所有協程，避免在銷毀後繼續執行
        StopAllCoroutines();
    }

    // 公開方法供其他腳本調用
    public void SetUIInteractable(bool interactable)
    {
        if (mainTransparentPanel != null)
        {
            mainTransparentPanel.interactable = interactable;
        }

        if (bottomInputPanel != null)
        {
            bottomInputPanel.interactable = interactable;
        }
    }
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}