using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] HttpAuthHandler handler;
    public GameObject gameOverCanvas;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
    }

    public void GameOver()
    {
        Debug.Log(PlayerPrefs.GetInt("highscore"));
        if (Score.score > PlayerPrefs.GetInt("highscore"))
        {
            handler.UpdateHighScore();
            //manager.UpdateHighScore();
        }
        gameOverCanvas.SetActive(true);
        Time.timeScale = 0;
    }
    public void Replay()
    {
        SceneManager.LoadScene("game");
    }
}
