using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    public GameObject ball;
    public float forceValue = 10f;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ball.GetComponent<Rigidbody2D>().AddForce(forceValue * Vector2.down, ForceMode2D.Impulse);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            ball.GetComponent<Rigidbody2D>().AddForce(forceValue * Vector2.up, ForceMode2D.Impulse);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            ball.GetComponent<Rigidbody2D>().AddForce(forceValue * Vector2.right, ForceMode2D.Impulse);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            ball.GetComponent<Rigidbody2D>().AddForce(forceValue * Vector2.left, ForceMode2D.Impulse);
        }
    }
}
