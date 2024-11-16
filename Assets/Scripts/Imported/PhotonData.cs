using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class PhotonData : MonoBehaviourPunCallbacks
{
    public GameManagement game;
    
   
    // Start is called before the first frame update
    [PunRPC]
    void PhotonStep(bool step)
    {
        Debug.Log("photonstep " + step);
        DataSaver.Instance.setDebug("photonstep " + step);
        if(step)game.UpdatePhotonStrikerReleaseStatus();  
    }
    [PunRPC]
    void PhotonReset(bool step)
    {
        Debug.Log("photonreset " + step);
        if (step) game.MakePhotonReset();
        
    }

   
    
}
