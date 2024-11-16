using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DataSaver : MonoBehaviour
{
    private static DataSaver instance;

    public static DataSaver Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public int gameMode { get; set; }
    public TextMeshProUGUI debug;
    public void setDebug(string s)
    {
        debug.text = debug.text+ s;
    }
}
