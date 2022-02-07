using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatController : MonoBehaviour
{
    public InputField chatInput;
    public Text chatText;
    // Start is called before the first frame update
    
    void Start()
    {
        Debug.Log("ChatController initiate");
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if(chatInput.text.Length > 0){
                sendMessage(chatInput.text);
                chatText.text+="[나] >> "+chatInput.text+"\n";
                chatInput.text ="";
                chatInput.ActivateInputField();
            }
        }
    }

    public void addMessage(string text){
        chatText.text+=text;
    }

    private void sendMessage(string text){
        NetworkManager n = NetworkManager.instance.GetComponent<NetworkManager>();
        n.CommandChatMessage(text);
    }

}
