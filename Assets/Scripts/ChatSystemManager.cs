using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatSystemManager : MonoBehaviour
{
    GameObject textHistory, chatPanel;
    GameObject greetButton, ggButton, niceButton, oopsButton;
    GameObject inputMessageField, sendButton;

    GameObject networkedClient;

    public GameObject messagePrefab;

    // Start is called before the first frame update
    void Awake()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

        foreach (GameObject go in allObjects)
        {
            if (go.name == "Client")
                networkedClient = go;
            else if (go.name == "TextHistory")
                textHistory = go;
            else if (go.name == "ChatPanel")
                chatPanel = go;
            else if (go.name == "GreetButton")
                greetButton = go;
            else if (go.name == "GGButton")
                ggButton = go;
            else if (go.name == "NiceButton")
                niceButton = go;
            else if (go.name == "OopsButton")
                oopsButton = go;
            else if (go.name == "InputMessage")
                inputMessageField = go;
            else if (go.name == "SendButton")
                sendButton = go;
        }

        greetButton.GetComponent<Button>().onClick.AddListener(GreetButtonPressed);
        ggButton.GetComponent<Button>().onClick.AddListener(GGButtonPressed);
        niceButton.GetComponent<Button>().onClick.AddListener(NiceButtonPressed);
        oopsButton.GetComponent<Button>().onClick.AddListener(OopsButtonPressed);
        sendButton.GetComponent<Button>().onClick.AddListener(SendButtonPressed);

        messagePrefab = Resources.Load("Prefabs/TextMessage") as GameObject;

    }

    public void ChangeState(int newState)
    {
        textHistory.SetActive(false);
        chatPanel.SetActive(false);
        greetButton.SetActive(false);
        ggButton.SetActive(false);
        niceButton.SetActive(false);
        oopsButton.SetActive(false);

        
        if (newState == GameStates.GameEnd)
        {
            // Show Message Options
            textHistory.SetActive(true);
            chatPanel.SetActive(true);
            greetButton.SetActive(true);
            ggButton.SetActive(true);
            niceButton.SetActive(true);
            oopsButton.SetActive(true);
        }
        else if (newState == GameStates.TicTacToe)
        {
            // Reset Messages
            var messageContainer = textHistory.transform.GetChild(0).GetChild(0);

            for (int i = messageContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(messageContainer.GetChild(i).gameObject);
            }

            // Show Message Options
            textHistory.SetActive(true);
            chatPanel.SetActive(true);
            greetButton.SetActive(true);
            ggButton.SetActive(true);
            niceButton.SetActive(true);
            oopsButton.SetActive(true);
        }
    }


    public void SendButtonPressed()
    {
        // Get text from Input field
        var inputField = inputMessageField.GetComponent<InputField>();
        var text = inputField.text;

        // Send Information to server
        SendTextMessage(text);

        // Reset Input field text
        inputField.text = "";
    }

    public void GreetButtonPressed()
    {
        SendTextMessage("Hello!");
    }

    public void GGButtonPressed()
    {
        SendTextMessage("Good Game!");
    }

    public void NiceButtonPressed()
    {
        SendTextMessage("Nice one!");
    }

    public void OopsButtonPressed()
    {
        SendTextMessage("Oops!");
    }

    public void SendTextMessage(string msg)
    {
        if (msg != "")
        {
            // Replace Commas with spaces
            var csv = msg.Split(',');
            string newMsg = "";

            foreach (var str in csv)
            {
                newMsg += str + " ";
            }

            // Send Message
            networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.TextMessage + "," + newMsg);
        }
    }

    public void DisplayMessage(string msg)
    {
        var content = textHistory.transform.GetChild(0).GetChild(0);
        var scrollbar = textHistory.transform.GetChild(1).GetComponent<Scrollbar>();

        // Display Message in Chat history
        GameObject text = Instantiate(messagePrefab);
        text.GetComponent<Text>().text = msg;
        text.transform.SetParent(content);

        // Scroll to bottom on message recieved
        scrollbar.value = 0;
    }
}
