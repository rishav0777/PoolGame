using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Holes : MonoBehaviour
{
    private GameManagement gameManagement;
    private GameObject myBox, oppBox;
    public Sprite boxfillSprite;
    

    private void Start()
    {
        gameManagement = GameObject.FindGameObjectWithTag("canvas").GetComponent<GameManagement>();
        myBox = gameManagement.myBox;
        oppBox = gameManagement.OppBox;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "ball")
        {
            Debug.Log("hole colloided with ball");
            //collision.gameObject.SetActive(false);
            StartCoroutine(Deactivate(collision));
            gameManagement.IsMoreChance = true;
        }
        if (collision.tag == "strikerBall")
        {
            collision.gameObject.transform.position = new Vector2(0, 0);
            gameManagement.ExaustedTime();
        }
    }

    IEnumerator Deactivate(Collider2D collision)
    {
        if (gameManagement.GetTurn() == Turn.mine) myBox.transform.GetChild(gameManagement.myPoint++).GetComponent<Image>().sprite = boxfillSprite;
        else if (gameManagement.GetTurn() == Turn.opp) oppBox.transform.GetChild(gameManagement.oppPoint++).GetComponent<Image>().sprite = boxfillSprite;
        yield return new WaitForSeconds(0.1f);
        GameObject[] balls = gameManagement.GetBallCollection();
        int val = 0;
        
        collision.gameObject.SetActive(false);
        for (int j = 0; j < balls.Length; j++)
        {
            if (balls[j] == collision.gameObject)
            {

            }
        }

    }

   
}
