using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private void OnEnable()
    {
        PlayerInputs.OnEscapePressed += ExitGame;
        PlayerInputs.OnRestartPressed += RestartGame;
    }

    private void OnDisable()
    {
        PlayerInputs.OnEscapePressed -= ExitGame;
        PlayerInputs.OnRestartPressed -= RestartGame;
    }

    private void ExitGame()
    {
        Application.Quit();
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
