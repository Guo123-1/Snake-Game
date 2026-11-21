using TMPro;
using UnityEngine;

public class GameHUD : MonoBehaviour
{
    public Snake snake;
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI bestText;

    int lastScore = -1;
    int lastBest = -1;

    void Start()
    {
        RefreshBest();          // 开局先摆一次
    }
    void Update()
    {
        if (snake.Score != lastScore)
        {
            lastScore = snake.Score;
            scoreText.text = $"得分：{lastScore}";
        }

        if (snake.Best != lastBest)
            RefreshBest();      // 破纪录后自动跟上，不用在别处记得刷

        if (snake.IsDead && !gameOverPanel.activeSelf)
            gameOverPanel.SetActive(true);
    }

    void RefreshBest()
    {
        lastBest = snake.Best;
        bestText.text = $"最高：{lastBest}";
    }
}


