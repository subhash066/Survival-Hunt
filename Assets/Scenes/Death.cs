using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Needed for TextMeshPro

public class Death : MonoBehaviour
{
    public TMP_Text countdownText; // Drag a TextMeshProUGUI element here in Inspector

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine(RespawnCountdown());
    }

    System.Collections.IEnumerator RespawnCountdown()
    {
        int timeLeft = 5;
        while (timeLeft > 0)
        {
            countdownText.text = "Respawning in " + timeLeft;

            if (timeLeft == 1)
            {
                // Immediately load scene when countdown hits 1
                SceneManager.LoadScene("SampleScene");
                yield break;
            }

            yield return new WaitForSeconds(1f);
            timeLeft--;
        }
    }
}
