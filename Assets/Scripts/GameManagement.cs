using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public class GameManagement : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
{
    private GameObject stick;
    private GameObject strikerBall;
    private Scrollbar striker;

    public float forceMag = 100f;
    private bool ishandle = true;
    private Vector2 initialDir;
    private Vector2 finalDir;

    private float currentAngle = 0;
    private bool reset = false;

    private LineRenderer line;
    private LayerMask layerMask;
    private Turn turn;
    public GameObject myTimer, oppTimer;
    public bool IsMoreChance { get; set; }
    private GameObject[] ballCollection;
    private GameObject[] holeCollection;

    public int myPoint { get; set; }
    public int oppPoint { get; set; }

    private bool botPlay = false;
    public GameObject myBox { get; set; }
    public GameObject OppBox { get; set; }
    public GameObject resultPanel;

    private void Start()
    {
        myPoint = 0;oppPoint = 0;
        if (DataSaver.Instance.gameMode == 0 && !PhotonNetwork.IsMasterClient)
        {
            turn = Turn.opp;
        }
        else
        {
            reset = true;
            turn = Turn.mine;
        }

        stick = GameObject.FindGameObjectWithTag("stick");
        strikerBall = GameObject.FindGameObjectWithTag("strikerBall");
        striker = GameObject.Find("Scrollbar").GetComponent<Scrollbar>();
        layerMask = LayerMask.GetMask("strikerBall");
        line = GetComponent<LineRenderer>();
        IsMoreChance = true;
        ballCollection = GameObject.FindGameObjectsWithTag("ball");
        holeCollection = GameObject.FindGameObjectsWithTag("hole");

        myBox = GameObject.Find("myBox");
        OppBox = GameObject.Find("oppBox");
        
    }
    PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
    private void Update()
    {
        if (turn == Turn.mine)
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    initialDir = touch.position;
                    pointerEventData.position = new Vector2(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
                    if (ishandle) IsPointerOverUIObject(pointerEventData);
                }
                else if (touch.phase == TouchPhase.Moved && ishandle)
                {
                    finalDir = touch.position;
                    SetStickDirection(initialDir, finalDir);
                    ShowDirection();
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    if (!ishandle) ReleaseStriker();
                    else
                    {
                        finalDir = touch.position;
                        SetStickDirection(initialDir, finalDir);
                    }
                }
            }


            if (Input.GetMouseButtonDown(0))
            {
                initialDir = Input.mousePosition;
                pointerEventData.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                if (ishandle) IsPointerOverUIObject(pointerEventData);
            }
            else if (Input.GetMouseButton(0) && ishandle)
            {
                finalDir = Input.mousePosition;
                SetStickDirection(initialDir, finalDir);
                ShowDirection();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (!ishandle) ReleaseStriker();
                else
                {
                    finalDir = Input.mousePosition;
                    SetStickDirection(initialDir, finalDir);
                }
            }
        }


        if (reset && strikerBall.GetComponent<Rigidbody2D>().velocity.magnitude < 0.08) ResetStriker();
        if (botPlay ) BotGamePlay();
        if (DataSaver.Instance.gameMode == 0 && turn == Turn.opp) ShowDirection();
        
        if(myPoint==8 || oppPoint == 8)
        {
            resultPanel.SetActive(true);
            if (myPoint == 8) resultPanel.GetComponent<Result>().ShowResult("You Win");
            else resultPanel.GetComponent<Result>().ShowResult("You Lose");
        }

    }

    public void ResetStriker()
    {
        ishandle = true;
        reset = false;
        strikerBall.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        if (turn == Turn.mine && DataSaver.Instance.gameMode == 0)
            photonView.RPC("PhotonReset", RpcTarget.Others, true);

        if (!IsMoreChance) SetTurn();

        Vector2 pos = new Vector2(strikerBall.transform.localPosition.x , strikerBall.transform.localPosition.y );
        stick.transform.localPosition = pos;
        currentAngle = stick.transform.localRotation.z;
        
        ShowDirection();

        if (IsMoreChance)
        {
            InitilizeTimer(); if (turn == Turn.opp && DataSaver.Instance.gameMode != 0) botPlay = true;
            ballCollection = GameObject.FindGameObjectsWithTag("ball");
        }
        IsMoreChance = false;
        
    }

    public void OnStrikerCompress()
    {
        if (turn == Turn.opp) { striker.enabled = false; }
        Vector2 pos = new Vector2(strikerBall.transform.localPosition.x, strikerBall.transform.localPosition.y);
        stick.transform.localPosition = pos;

        float value = striker.value;

        stick.transform.GetChild(0).transform.localPosition = new Vector2(stick.transform.GetChild(0).transform.localPosition.x, 
            -4.5f-(4f/1f)*(value));
    }



    public void ReleaseStriker()
    {
        Debug.Log("relese striker");
        if (turn == Turn.mine && DataSaver.Instance.gameMode == 0) 
            photonView.RPC("PhotonStep", RpcTarget.Others, true);

        float value = striker.value;
        float force = value * forceMag;
        striker.value = 0;
        line.positionCount = 0;

        Vector2 forceDir = (stick.transform.position - stick.transform.GetChild(0).transform.position).normalized;
        strikerBall.GetComponent<Rigidbody2D>().AddForce(forceDir * force, ForceMode2D.Impulse);
        reset = true;
        CloseTimer();
        circle.SetActive(false);
    }

    private void IsPointerOverUIObject(PointerEventData pointerEventData)
    {
        List<RaycastResult> list = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, list);
        for (int j = 0; j < list.Count; j++)
        {
            if (list[j].gameObject.tag == "handle")
            {
                ishandle = false;
                list.Clear();
            }
        }
    }


    public void SetStickDirection(Vector2 initial,Vector2 final)
    {
        Vector2 commonPoint = stick.transform.position;
        Vector2 a = initial - commonPoint;
        Vector2 b = final - commonPoint;

        float angle = Vector2.Angle(a, b);
        Vector3 cross = Vector3.Cross(a, b);
        int dir = cross.z > 0 ? 1 : -1;

        
        currentAngle += angle*dir;
        //Debug.Log("angle " + currentAngle + " dir " + dir);

        stick.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, currentAngle));
        initialDir = finalDir;
    }
    public GameObject circle;



    
    public void ShowDirection()
    {
        line.positionCount = 2;
        Vector2 Dir = (stick.transform.position - stick.transform.GetChild(0).transform.position).normalized;
        line.SetPosition(0, stick.transform.position);
        RaycastHit2D hit = Physics2D.Raycast(stick.transform.position, Dir, Mathf.Infinity,~layerMask);
        if (hit.collider != null)
        {
            float x1 = hit.point.x,y1=hit.point.y;
            float x2 = stick.transform.GetChild(0).transform.position.x;
            float y2 = stick.transform.GetChild(0).transform.position.y;
            float d = Mathf.Abs(Mathf.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)))-0.2f;
            float x = (x2 * 0.2f + x1 * d) / (d + 0.2f);
            float y = (y2 * 0.2f + y1 * d) / (d + 0.2f);
            line.SetPosition(1, hit.point);
            circle.SetActive(true);circle.transform.position = new Vector2(x, y);
            
            if(hit.collider.gameObject.tag=="ball") ShowMovDir(x,y,hit.collider.gameObject);

        }
    }
    public float radius = 0.4f;
    public void ShowMovDir(float x2,float y2,GameObject obj)
    {
        line.positionCount += 2;
        line.SetPosition(2,obj.transform.position);
        float x1 = obj.transform.position.x;
        float y1 = obj.transform.position.y;
        float d = Mathf.Abs(Mathf.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)));
        float x = x1 + radius * (x1 - x2) / d;
        float y = y1 + radius * (y1 - y2) / d;
        line.SetPosition(3, new Vector3(x,y,0));
    }

    public void SetTurn(bool fl=true)
    {
        if (turn == Turn.mine)
        {
            turn = Turn.opp;
            if (DataSaver.Instance.gameMode != 0) botPlay = true;
            
        }
        else
        {
            turn = Turn.mine;
            striker.enabled = true;
        }
        if (DataSaver.Instance.gameMode == 0) PhotonOwnerShip(PhotonNetwork.LocalPlayer.ActorNumber);
        InitilizeTimer();

    }
    public void InitilizeTimer()
    {
        if (turn == Turn.opp)
        {
            myTimer.SetActive(false);
            oppTimer.SetActive(true);
        }
        else
        {
            myTimer.SetActive(true);
            oppTimer.SetActive(false);
        }
    }
    public void CloseTimer()
    {
        myTimer.SetActive(false);
        oppTimer.SetActive(false);
    }

    public void ExaustedTime()
    {
        IsMoreChance = false;
        if (DataSaver.Instance.gameMode == 0 && turn == Turn.opp) return;
        ResetStriker();
    }

    public List<PhotonView> photonview = new List<PhotonView>();
    public void PhotonOwnerShip(int actorNo)
    {
        Debug.Log("request ownership");
        // if (photonView.IsMine) photonView.TransferOwnership(actorNo);
        if (!photonview[0].IsMine && turn==Turn.mine) 
            for(int j=0;j<photonview.Count;j++) photonview[j].RequestOwnership();
    }
    public void UpdatePhotonStrikerReleaseStatus()
    {
        line.positionCount = 0;
        CloseTimer();
        circle.SetActive(false);
    }
    public void MakePhotonReset()
    {
        reset = true;
    }








    
    public class BestOption
    {
        public float value;
        public GameObject ball;
        public GameObject hole;
    }
    public void BotGamePlay()
    {
        botPlay = false;
        BestOption option = GetNearestBall();
        Debug.Log("option " + option.ball.name + " " + option.hole.name);

        float x1 = option.hole.transform.position.x, x2 = option.ball.transform.position.x;
        float y1 = option.hole.transform.position.y, y2 = option.ball.transform.position.y;
        float m = (y2 - y1) / (x2 - x1);

        Vector2 a = new Vector2(x2 + (m * 0.4f) / Mathf.Sqrt(m * m + 1), y2 + 0.4f / Mathf.Sqrt(m * m + 1));

        Vector2 aa = stick.transform.GetChild(0).transform.position- stick.transform.position;
        Vector2 bb = option.ball.transform.position - stick.transform.position;

        float angle = 180- Vector2.Angle(aa, bb);
        Vector3 cross = Vector3.Cross(aa, bb);
        int dir = cross.z < 0 ? 1 : -1;

        Debug.Log(angle +" "+ dir+" inirot "+ stick.transform.rotation.eulerAngles.z);
        stick.transform.Rotate(new Vector3(0f, 0f, angle * dir));
        Debug.Log(angle + " " + dir + " firot " + stick.transform.rotation.eulerAngles.z);
        ShowDirection();
        striker.value = 0.8f;
        OnStrikerCompress();
        Invoke("ReleaseStriker", 3f);
    }

    public BestOption GetNearestBall()
    {
        BestOption option=new BestOption();
        option.value = 100000000;
        option.ball = ballCollection[0];
        option.hole = holeCollection[0];

        for(int i = 0; i < ballCollection.Length; i++)
        {
            for(int j = 0; j < holeCollection.Length; j++)
            {
                if (CheckForBetweenColloiders(ballCollection[i].transform.position, holeCollection[j].transform.position, ballCollection[i])) continue;
                float dist = Vector2.Distance(ballCollection[i].transform.position, holeCollection[j].transform.position);
                float angle = GetAngleOFBallWithHole(ballCollection[i].transform.position, holeCollection[j].transform.position);
                //Debug.Log(holeCollection[j].name+" "+ ballCollection[i].name+" " + dist + " " + angle);
                if ((dist - angle) < option.value)
                {
                    option.value = dist - angle;
                    option.ball = ballCollection[i];
                    option.hole = holeCollection[j];
                }
            }
        }
        return option;
    }
    public float GetAngleOFBallWithHole(Vector2 ballPos, Vector2 holePos)
    {
        Vector2 aa = strikerBall.transform.position;
        Vector2 a = aa - ballPos;
        Vector2 b = holePos - ballPos;

        float angle = Vector2.Angle(a, b);
        if (angle > 100) return angle;
        return 0;
    }
    public bool CheckForBetweenColloiders(Vector2 ballPos,Vector2 holePos,GameObject ball)
    {
        //line.positionCount = 7;
        
        float x1 = holePos.x,x2=ballPos.x;
        float y1 = holePos.y,y2=ballPos.y;
        float m = -1.0f/((y2 - y1) / (x2 - x1));

        Vector2 a = new Vector2(x2 + (m * 0.2f) / Mathf.Sqrt(m * m + 1), y2 + 0.2f / Mathf.Sqrt(m * m + 1));
        Vector2 b = new Vector2(x1 - (m * 0.2f) / Mathf.Sqrt(m * m + 1), y1 - 0.2f / Mathf.Sqrt(m * m + 1));
        Vector2 c = new Vector2(x2 - (m * 0.2f) / Mathf.Sqrt(m * m + 1), y2 - 0.2f / Mathf.Sqrt(m * m + 1));
        Vector2 d = new Vector2(x1 + (m * 0.2f) / Mathf.Sqrt(m * m + 1), y1 + 0.2f / Mathf.Sqrt(m * m + 1));
       // line.SetPosition(2, a);line.SetPosition(3, c); line.SetPosition(4, b); line.SetPosition(5, d); line.SetPosition(6, a);
        ball.layer = LayerMask.NameToLayer("Default");
        Collider2D[] collider = Physics2D.OverlapAreaAll(a, b,LayerMask.GetMask("ball"));
        ball.layer= LayerMask.NameToLayer("ball");
        if (collider.Length > 0) return true;
        return false;
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        Debug.Log("ownership requested");
        if (targetView.Owner == PhotonNetwork.LocalPlayer && turn==Turn.opp && photonview[0].IsMine)
        {
            targetView.TransferOwnership(requestingPlayer);
        }
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        Debug.Log("ownership successfully transfered");
    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
        Debug.Log("ownership transfered failed");
    }

    public Turn GetTurn()
    {
        return turn;
    }
    public GameObject[] GetBallCollection()
    {
        return ballCollection;
    }
}
