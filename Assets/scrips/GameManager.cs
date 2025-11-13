using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Timer")]
    public TextMeshProUGUI timerText;
    public float gameTime = 120f;
    private float currentTime;
    private bool gameActive = true;

    [Header("UI References")]
    public TMP_InputField textInputField; // 改用 TMP_InputField
    public Button submitButton;
    public Transform chatContainer;
    public GameObject playerMessagePrefab;
    public GameObject systemMessagePrefab;
    public ScrollRect chatScrollRect;

    [Header("Customer Report")]
    public TextMeshProUGUI[] customerValueTexts = new TextMeshProUGUI[3]; // 陰德值, 業力值, 誠意度
    public Button[] increaseButtons = new Button[3];
    public Button[] decreaseButtons = new Button[3];
    public Button confirmButton;
    private int[] customerValues = { 60, 60, 60 };

    [Header("Reference Questions")]
    public Button[] referenceButtons = new Button[5];
    public ScrollRect referenceScrollRect;
    private string[] referenceQuestions =  {
        "汝行街上，見一老嫗跌倒，身旁五千金鈔隨風散。若只得一瞬之舉，汝先救人，抑或先拾錢？",
        "汝見一孩以假幣買食，攤主未覺。若揭穿，孩餓；若不言，誤教。汝何選？",
        "友人誤將重物砸壞汝物，卻以為無人知曉。汝心明白真相，是當面直言，還是靜默讓他安？",
        "汝於雨夜撞傷野貓，車無損，人未見。汝會停車查看，抑或遠去以免麻煩？",
        "汝在工廠，知上司指示排放廢水入河，若揭發，恐遭解雇；若隱瞞，公司利潤倍增。汝何解？"
    };

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip buttonClickSound;
    public AudioClip submitSound;
    public AudioClip timerWarningSound;

    private void Start()
    {
        InitializeGame();
        SetupEventListeners();
    }

    private void InitializeGame()
    {
        currentTime = gameTime;
        UpdateTimerDisplay();
        UpdateCustomerValueDisplays();

        // 設置初始提示文字
        if (textInputField != null && textInputField.placeholder != null)
        {
            TextMeshProUGUI placeholderText = textInputField.placeholder as TextMeshProUGUI;
            if (placeholderText != null)
            {
                placeholderText.text = "請輸入文字情境考題，或點擊複製參考題型";
            }
        }

        // 初始化參考題按鈕
        for (int i = 0; i < referenceButtons.Length; i++)
        {
            int index = i; // 避免閉包問題
            referenceButtons[i].onClick.AddListener(() => SelectReferenceQuestion(index));
        }
    }

    private void SetupEventListeners()
    {
        // 提交按鈕
        submitButton.onClick.AddListener(SubmitMessage);

        // 客戶報表按鈕
        for (int i = 0; i < 3; i++)
        {
            int index = i; // 避免閉包問題
            increaseButtons[i].onClick.AddListener(() => ModifyCustomerValue(index, 1));
            decreaseButtons[i].onClick.AddListener(() => ModifyCustomerValue(index, -1));
        }

        confirmButton.onClick.AddListener(ConfirmCustomerValues);

        // 輸入框 Enter 鍵提交
        textInputField.onEndEdit.AddListener((string text) => {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SubmitMessage();
            }
        });
    }

    private void Update()
    {
        if (gameActive)
        {
            UpdateTimer();
        }
    }

    private void UpdateTimer()
    {
        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            EndGame();
        }

        UpdateTimerDisplay();

        // 最後10秒警告音效
        if (currentTime <= 10f && currentTime > 9f)
        {
            if (timerWarningSound != null && !audioSource.isPlaying)
            {
                audioSource.PlayOneShot(timerWarningSound);
            }
        }
    }

    private void UpdateTimerDisplay()
    {
        int totalSeconds = Mathf.FloorToInt(currentTime);
        timerText.text = $"倒計時：{totalSeconds}秒";

        // 最後30秒變紅色
        if (currentTime <= 30f)
        {
            timerText.color = Color.red;
        }
        else
        {
            timerText.color = Color.white;
        }
    }

    public void SubmitMessage()
    {
        if (!gameActive || string.IsNullOrEmpty(textInputField.text.Trim())) return;

        string messageText = textInputField.text.Trim();

        // 播放音效
        if (submitSound != null)
        {
            audioSource.PlayOneShot(submitSound);
        }

        // 創建玩家訊息
        CreateChatMessage(messageText, true);

        // 清空輸入框
        textInputField.text = "";
        textInputField.Select();
        textInputField.ActivateInputField();

        // 延遲回復系統訊息
        StartCoroutine(DelayedSystemResponse());
    }

    private IEnumerator DelayedSystemResponse()
    {
        yield return new WaitForSeconds(1f);

        if (gameActive)
        {
            // 模擬其他電腦回覆的訊息
            string[] responses = {
                "收到你的訊息了！",
                "這個情況很有趣呢",
                "我來看看這個問題...",
                "了解了，謝謝分享",
                "這讓我想到一個類似的情況"
            };

            string randomResponse = responses[UnityEngine.Random.Range(0, responses.Length)];
            CreateChatMessage(randomResponse, false);
        }
    }

    private void CreateChatMessage(string message, bool isPlayer)
    {
        GameObject messagePrefab = isPlayer ? playerMessagePrefab : systemMessagePrefab;
        GameObject messageObj = Instantiate(messagePrefab, chatContainer);
        
        // 設置基本屬性
        messageObj.SetActive(true);
        messageObj.name = isPlayer ? $"PlayerMsg_{message}" : $"SystemMsg_{message}";
        
        // 尋找文字組件並設置
        TextMeshProUGUI messageText = messageObj.GetComponentInChildren<TextMeshProUGUI>();
        if (messageText != null)
        {
            messageText.text = message;
            
            // 根據訊息類型設置樣式
            if (isPlayer)
            {
                messageText.alignment = TextAlignmentOptions.MidlineRight; // 玩家靠右
                messageText.color = Color.white;
            }
            else
            {
                messageText.alignment = TextAlignmentOptions.MidlineLeft; // 系統靠左
                messageText.color = new Color(0.2f, 0.2f, 0.2f, 1f); // 深灰色
            }
            
            // 確保文字自動換行
            messageText.enableWordWrapping = true;
            messageText.overflowMode = TextOverflowModes.Overflow;
        }
        
        // 強制刷新佈局以確保高度正確計算
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(messageObj.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatContainer as RectTransform);
        
        Debug.Log($"創建訊息: {(isPlayer ? "玩家" : "系統")} - {message}");
        
        // 自動滾動到底部
        StartCoroutine(ScrollToBottom());
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        chatScrollRect.verticalNormalizedPosition = 0f;
    }

    public void SelectReferenceQuestion(int index)
    {
        if (!gameActive || index >= referenceQuestions.Length) return;

        // 播放音效
        if (buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }

        // 將參考題目放入輸入框
        textInputField.text = referenceQuestions[index];
        textInputField.Select();
        textInputField.ActivateInputField();
    }

    private void ModifyCustomerValue(int index, int change)
    {
        if (!gameActive) return;

        customerValues[index] = Mathf.Clamp(customerValues[index] + change, 0, 100);
        UpdateCustomerValueDisplays();

        // 播放音效
        if (buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

    private void UpdateCustomerValueDisplays()
    {
        for (int i = 0; i < customerValues.Length && i < customerValueTexts.Length; i++)
        {
            if (customerValueTexts[i] != null)
            {
                // 只顯示數字，設置為黃色、36大小、粗體
                customerValueTexts[i].text = $"<color=yellow><size=36><b>{customerValues[i]}</b></size></color>";

                // 確保組件設置正確
                customerValueTexts[i].fontSize = 36;
                customerValueTexts[i].color = Color.yellow;
                customerValueTexts[i].fontStyle = FontStyles.Bold;
                customerValueTexts[i].alignment = TextAlignmentOptions.Center;
            }
        }
    }

    public void ConfirmCustomerValues()
    {
        if (!gameActive) return;

        // 播放音效
        if (buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }

        // 顯示確認訊息
        string confirmMessage = $"客戶報表已儲存 - 陰德值:{customerValues[0]} 業力值:{customerValues[1]} 誠意度:{customerValues[2]}";
        StartCoroutine(ShowConfirmationMessage(confirmMessage));
    }

    private IEnumerator ShowConfirmationMessage(string message)
    {
        // 可以在這裡添加確認訊息的UI顯示
        Debug.Log(message);
        yield return new WaitForSeconds(2f);
    }

    private void EndGame()
    {
        gameActive = false;

        // 禁用所有互動元素
        submitButton.interactable = false;
        textInputField.interactable = false;
        confirmButton.interactable = false;

        for (int i = 0; i < increaseButtons.Length; i++)
        {
            increaseButtons[i].interactable = false;
            decreaseButtons[i].interactable = false;
        }

        for (int i = 0; i < referenceButtons.Length; i++)
        {
            referenceButtons[i].interactable = false;
        }

        // 顯示遊戲結束訊息
        CreateChatMessage("時間結束！感謝您的參與。", false);

        Debug.Log("遊戲結束！");
    }

    // 公開方法供其他腳本調用
    public bool IsGameActive()
    {
        return gameActive;
    }

    public float GetRemainingTime()
    {
        return currentTime;
    }

    public int[] GetCustomerValues()
    {
        return customerValues;
    }
}