using TMPro;
using UnityEngine;

public class HighscoreText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;

    public void SetScore(int score)
    {
        if (scoreText != null) scoreText.text = score.ToString();
    }
}
