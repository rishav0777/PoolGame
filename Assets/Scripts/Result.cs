using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Result : MonoBehaviour
{

    public TextMeshProUGUI WinText;
   public void ShowResult(string t)
    {
        WinText.text = t;
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(sceneBuildIndex: 0);
    }
}
