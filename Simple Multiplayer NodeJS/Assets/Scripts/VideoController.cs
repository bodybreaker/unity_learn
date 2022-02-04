using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class VideoController : MonoBehaviour
{
    public GameObject popupView;
    // Start is called before the first frame update

    private InputField urlInput;

    void Start()
    {
        popupView.SetActive(false);
        urlInput = popupView.transform.Find("URLInput").GetComponent<InputField>();                
    }
    public void OpenURLPop(){
        popupView.SetActive(true);
    }

    public void CloseURLPop(){
        popupView.SetActive(false);
    }

    public void PlayURL(){
        if(urlInput.text.Length!=0){
            NetworkManager.instance.GetComponent<NetworkManager>().CommandPlayURL(urlInput.text);
        }
        popupView.SetActive(false);
    }
}
