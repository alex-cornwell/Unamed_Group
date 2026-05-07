using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenController : MonoBehaviour
{
public void StartGame() { SceneManager.LoadScene("CutsceneIntro"); }
public void QuitGame() { Application.Quit(); }
}