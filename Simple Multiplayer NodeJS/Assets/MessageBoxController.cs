using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBoxController : MonoBehaviour
{

    public GameObject messageBox;
    public Text chatText;
    // Start is called before the first frame update
    void Start()
    {
        messageBox.SetActive(false);
    }


    public void ShowMessage(string msg){
        Debug.Log("ShowMessage "+messageBox.activeInHierarchy);
        // 이미 활성화 되어있는경우
        if (!messageBox.activeInHierarchy){
            messageBox.SetActive(true);
        }
        StartCoroutine(ShowMessageAndClose(msg,3f));
    }


    private IEnumerator ShowMessageAndClose(string msg,float sec){

        Debug.Log("ShowMessageAndClose "+messageBox.activeInHierarchy);
        

        chatText.text = msg;
        yield return new WaitForSeconds(sec);

        
        messageBox.SetActive(false);
        chatText.text = "";

    }



}
