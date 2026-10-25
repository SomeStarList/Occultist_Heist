using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Link_Scene : MonoBehaviour
{
    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void ToTutotial()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Tutorial");
    }

    public void ToLevel01()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level_01");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
