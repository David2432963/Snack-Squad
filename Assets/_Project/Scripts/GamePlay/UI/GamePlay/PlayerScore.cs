using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScore : MonoBehaviour
{
    [SerializeField] private Text textName;
    [SerializeField] private Text textScore;

    public void SetPlayer(EPlayerType playerType, int index, int score, Color textColor)
    {
        textName.text = $"{index + 1}. {playerType}";
        textScore.text = score.ToString();
        textName.color = textColor;
        textScore.color = textColor;
    }
}
