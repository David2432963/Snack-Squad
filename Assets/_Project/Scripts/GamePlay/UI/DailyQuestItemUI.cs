using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DailyQuestItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image questIcon;
    [SerializeField] private TextMeshProUGUI questNameText;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI timeRemainingText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Button claimRewardButton;
    [SerializeField] private Image completedIcon;
    [SerializeField] private TextMeshProUGUI rewardText;

    [Header("Visual Settings")]
    [SerializeField] private Color completedColor = Color.green;
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color expiredColor = Color.gray;

    private DailyQuestProgress questProgress;
    private bool isCompleted;
    private CanvasGroup canvasGroup;

    public DailyQuestProgress QuestProgress => questProgress;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (claimRewardButton != null)
        {
            claimRewardButton.onClick.AddListener(OnClaimRewardClicked);
        }
    }

    private void OnDestroy()
    {
        if (claimRewardButton != null)
        {
            claimRewardButton.onClick.RemoveListener(OnClaimRewardClicked);
        }
    }

    public void Initialize(DailyQuestProgress questProgress, bool isCompleted)
    {
        this.questProgress = questProgress;
        this.isCompleted = isCompleted;

        UpdateDisplay();
        UpdateProgress();
        UpdateClaimButton();
    }

    private void UpdateDisplay()
    {
        if (questProgress?.Quest == null) return;

        var quest = questProgress.Quest;

        // Update texts
        if (questNameText != null)
        {
            questNameText.text = quest.QuestName;
        }

        if (questDescriptionText != null)
        {
            questDescriptionText.text = quest.QuestDescription;
        }

        if (rewardText != null)
        {
            string rewardString = "";
            if (quest.GoldReward > 0)
            {
                rewardString += $"+{quest.GoldReward} Gold";
            }
            rewardText.text = rewardString;
        }

        // Update visual state based on completion status
        UpdateVisualState();
    }

    public void UpdateProgress()
    {
        if (questProgress == null) return;

        // Update progress text
        if (progressText != null)
        {
            progressText.text = questProgress.GetProgressText();
        }

        // Update progress slider
        if (progressSlider != null)
        {
            progressSlider.DOValue(questProgress.Progress, 0.3f);
        }

        // Update time remaining
        if (timeRemainingText != null)
        {
            timeRemainingText.text = questProgress.GetTimeRemainingText();
        }

        // Update visual state
        UpdateVisualState();
        UpdateClaimButton();
    }

    private void UpdateVisualState()
    {
        if (questProgress == null) return;

        Color targetColor;
        
        if (questProgress.IsExpired)
        {
            targetColor = expiredColor;
        }
        else if (questProgress.IsCompleted)
        {
            targetColor = completedColor;
        }
        else
        {
            targetColor = activeColor;
        }

        // Update background color
        var backgroundImage = GetComponent<Image>();
        if (backgroundImage != null)
        {
            backgroundImage.DOColor(targetColor, 0.3f);
        }

        // Show/hide completed icon
        if (completedIcon != null)
        {
            completedIcon.gameObject.SetActive(questProgress.IsCompleted);
            if (questProgress.IsCompleted)
            {
                completedIcon.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 5, 1f);
            }
        }

        // Update alpha for expired quests
        if (canvasGroup != null)
        {
            float targetAlpha = questProgress.IsExpired ? 0.5f : 1f;
            canvasGroup.DOFade(targetAlpha, 0.3f);
        }
    }

    private void UpdateClaimButton()
    {
        if (claimRewardButton == null) return;

        bool canClaim = questProgress != null && questProgress.IsCompleted && !questProgress.IsRewardClaimed;
        claimRewardButton.gameObject.SetActive(canClaim);

        if (canClaim)
        {
            // Add pulsing animation to draw attention
            claimRewardButton.transform.DOScale(1.1f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }

    public void SetCompleted(bool completed)
    {
        isCompleted = completed;
        UpdateProgress();
        
        if (completed)
        {
            // Celebration animation
            transform.DOShakePosition(0.5f, new Vector3(5, 5, 0), 10, 90, false, true);
        }
    }

    public void SetRewardClaimed(bool claimed)
    {
        if (questProgress != null)
        {
            UpdateClaimButton();
            
            if (claimed)
            {
                // Fade out the claim button with animation
                if (claimRewardButton != null)
                {
                    claimRewardButton.transform.DOScale(0f, 0.3f)
                        .OnComplete(() => claimRewardButton.gameObject.SetActive(false));
                }
            }
        }
    }

    private void OnClaimRewardClicked()
    {
        if (questProgress != null && questProgress.IsCompleted && !questProgress.IsRewardClaimed)
        {
            questProgress.ClaimReward();
            
            // Visual feedback
            if (claimRewardButton != null)
            {
                claimRewardButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 1f);
            }
        }
    }

    private void Update()
    {
        // Update time remaining text periodically
        if (timeRemainingText != null && questProgress != null)
        {
            timeRemainingText.text = questProgress.GetTimeRemainingText();
        }
    }
}