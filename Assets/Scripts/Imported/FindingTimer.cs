using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FindingTimer : MonoBehaviour
{
    // Start is called before the first frame update
    public int time = 15;
    public int Findingtime { get; set; }
    public TextMeshProUGUI _text;
    void Start()
    {
        Findingtime = time;
        _text.text = Findingtime.ToString();
        
    }
    private void OnEnable()
    {
        Findingtime = time;
        _text.text = Findingtime.ToString();
    }

    

    public void Timer()
    {
        StartCoroutine(Timing());
    }
    IEnumerator Timing()
    {
        Findingtime--;
        yield return new WaitForSeconds(1f);
        _text.text = Findingtime.ToString();
        if(Findingtime>0) StartCoroutine(Timing());
    }
}
