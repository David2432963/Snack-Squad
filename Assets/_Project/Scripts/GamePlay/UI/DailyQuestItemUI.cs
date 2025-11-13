using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyQuestItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text questDescriptionText;
    [SerializeField] private Text progressText;
    [SerializeField] private Image progressSlider;
    [SerializeField] private Button claimRewardButton;
    [SerializeField] private Text rewardText;

    [Header("Button Sprites")]
    [SerializeField] private Sprite[] claimButtonSprites; // [0] = cannot claim, [1] = can claim

    private DailyQuestProgress questProgress;

    public DailyQuestProgress QuestProgress => questProgress;

    private void Awake()
    {
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

        UpdateDisplay();
        UpdateProgress();
        UpdateClaimButton();
    }

    private void UpdateDisplay()
    {
        if (questProgress?.Quest == null) return;

        var quest = questProgress.Quest;

        // Update texts
        if (questDescriptionText != null)
        {
            questDescriptionText.text = quest.QuestDescription;
        }

        if (rewardText != null)
        {
            string rewardString = "";
            if (quest.GoldReward > 0)
            {
                rewardString += $"+{quest.GoldReward}";
            }
            rewardText.text = rewardString;
        }

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
            progressSlider.fillAmount = questProgress.Progress;
        }

        // Update visual state
        UpdateClaimButton();
    }

    private void UpdateClaimButton()
    {
        if (claimRewardButton == null) return;

        bool canClaim = questProgress != null && questProgress.IsCompleted && !questProgress.IsRewardClaimed;

        // Always show the button
        claimRewardButton.gameObject.SetActive(true);

        // Update button sprite based on claim state
        if (claimButtonSprites != null && claimButtonSprites.Length >= 2)
        {
            var buttonImage = claimRewardButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                // If reward is claimed, always show first sprite (cannot claim)
                // If can claim, show second sprite (can claim)
                buttonImage.sprite = canClaim ? claimButtonSprites[1] : claimButtonSprites[0];
                claimRewardButton.GetComponentInChildren<Text>().text = questProgress.IsRewardClaimed ? "Claimed" : "Claim";
            }
        }

        // Enable/disable button interaction
        claimRewardButton.interactable = canClaim;
    }

    private void OnClaimRewardClicked()
    {
        if (questProgress != null && questProgress.IsCompleted && !questProgress.IsRewardClaimed)
        {
            questProgress.ClaimReward();
        }
        UpdateClaimButton();
    }
}