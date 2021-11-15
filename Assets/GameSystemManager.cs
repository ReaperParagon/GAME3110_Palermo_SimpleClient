using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSystemManager : MonoBehaviour
{

    GameObject submitButton, userNameInput, passwordInput, createToggle, loginToggle;
    GameObject textNameInfo, textPassordInfo;

    GameObject joinGameRoomButton;
    GameObject tictactoeBoard;
    List<GameObject> tictactoeSquareButtonList = new List<GameObject>();

    GameObject gameOverText, playAgainButton, watchReplayButton;

    GameObject textHistory, chatPanel;
    GameObject greetButton, ggButton, niceButton, oopsButton;
    GameObject inputMessageField, sendButton;

    GameObject networkedClient;

    public int OurTeam;
    public GameObject messagePrefab;

    // static GameObject Instance;

    // Start is called before the first frame update
    void Start()
    {
        // Instance = gameObject;

        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

        foreach (GameObject go in allObjects)
        {
            if (go.name == "UserNameInputField")
                userNameInput = go;
            else if (go.name == "PasswordInputField")
                passwordInput = go;
            else if (go.name == "SubmitButton")
                submitButton = go;
            else if (go.name == "CreateToggle")
                createToggle = go;
            else if (go.name == "LoginToggle")
                loginToggle = go;
            else if (go.name == "Client")
                networkedClient = go;
            else if (go.name == "JoinGameRoomButton")
                joinGameRoomButton = go;
            else if (go.name == "TextNameInfo")
                textNameInfo = go;
            else if (go.name == "TextPassInfo")
                textPassordInfo = go;
            else if (go.name == "TicTacToeBoard")
                tictactoeBoard = go;
            else if (go.name == "GameOverText")
                gameOverText = go;
            else if (go.name == "PlayAgainButton")
                playAgainButton = go;
            else if (go.name == "WatchReplayButton")
                watchReplayButton = go;
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

        submitButton.GetComponent<Button>().onClick.AddListener(SubmitButtonPressed);
        loginToggle.GetComponent<Toggle>().onValueChanged.AddListener(LoginToggleChanged);
        createToggle.GetComponent<Toggle>().onValueChanged.AddListener(CreateToggleChanged);
        joinGameRoomButton.GetComponent<Button>().onClick.AddListener(JoinGameRoomButtonPressed);
        playAgainButton.GetComponent<Button>().onClick.AddListener(PlayAgainButtonPressed);
        watchReplayButton.GetComponent<Button>().onClick.AddListener(WatchReplay);

        greetButton.GetComponent<Button>().onClick.AddListener(GreetButtonPressed);
        ggButton.GetComponent<Button>().onClick.AddListener(GGButtonPressed);
        niceButton.GetComponent<Button>().onClick.AddListener(NiceButtonPressed);
        oopsButton.GetComponent<Button>().onClick.AddListener(OopsButtonPressed);
        sendButton.GetComponent<Button>().onClick.AddListener(SendButtonPressed);


        for (int i = 0; i < tictactoeBoard.transform.childCount; i++)
        {
            int index = i;
            tictactoeSquareButtonList.Add(tictactoeBoard.transform.GetChild(index).gameObject);
            tictactoeSquareButtonList[i].GetComponent<Button>().onClick.AddListener(delegate { TicTacToeSquareButtonPressed(index); } );
        }


        ChangeState(GameStates.LoginMenu);
    }

    public void SubmitButtonPressed()
    {
        // We want to send login information to the server
        // Debug.Log("Submitted!");

        string n = userNameInput.GetComponent<InputField>().text;
        string p = passwordInput.GetComponent<InputField>().text;

        string msg;

        if(createToggle.GetComponent<Toggle>().isOn)
            msg = ClientToServerSignifiers.CreateAccount + "," + n + "," + p;
        else
            msg = ClientToServerSignifiers.Login + "," + n + "," + p;

        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(msg);

        Debug.Log(msg);
    }

    public void LoginToggleChanged(bool newValue)
    {
        Debug.Log("Login Changed!");
        createToggle.GetComponent<Toggle>().SetIsOnWithoutNotify(!newValue);
    }

    public void CreateToggleChanged(bool newValue)
    {
        Debug.Log("Create Changed!");
        loginToggle.GetComponent<Toggle>().SetIsOnWithoutNotify(!newValue);
    }

    public void ChangeState(int newState)
    {
        joinGameRoomButton.SetActive(false);
        submitButton.SetActive(false);
        userNameInput.SetActive(false);
        passwordInput.SetActive(false);
        createToggle.SetActive(false);
        loginToggle.SetActive(false);

        textNameInfo.SetActive(false);
        textPassordInfo.SetActive(false);

        gameOverText.SetActive(false);
        watchReplayButton.SetActive(false);
        playAgainButton.SetActive(false);

        textHistory.SetActive(false);
        chatPanel.SetActive(false);
        greetButton.SetActive(false);
        ggButton.SetActive(false);
        niceButton.SetActive(false);
        oopsButton.SetActive(false);

        // tictactoeSquareButton.SetActive(false);
        foreach (var square in tictactoeSquareButtonList)
        {
            square.SetActive(false);
        }

        if (newState == GameStates.LoginMenu)
        {
            submitButton.SetActive(true);
            userNameInput.SetActive(true);
            passwordInput.SetActive(true);
            createToggle.SetActive(true);
            loginToggle.SetActive(true);
            textNameInfo.SetActive(true);
            textPassordInfo.SetActive(true);
        }
        else if (newState == GameStates.MainMenu)
        {
            joinGameRoomButton.SetActive(true);
        }
        else if (newState == GameStates.WaitingInQueueForOtherPlayer)
        {
            // Back Button, loading UI
        }
        else if (newState == GameStates.TicTacToe)
        {
            // Set TicTacToe stuff to active
            foreach (var square in tictactoeSquareButtonList)
            {
                square.SetActive(true);
            }

            // Reset Messages
            var content = textHistory.transform.GetChild(0).GetChild(0);

            for (int i = content.childCount - 1; i >= 0; i--)
            {
                Destroy(content.GetChild(i).gameObject);
            }

            // Show Messages
            textHistory.SetActive(true);
            chatPanel.SetActive(true);
            greetButton.SetActive(true);
            ggButton.SetActive(true);
            niceButton.SetActive(true);
            oopsButton.SetActive(true);
        }
        else if (newState == GameStates.GameEnd)
        {
            // Show Game end stuff

            // Set TicTacToe stuff to active
            foreach (var square in tictactoeSquareButtonList)
            {
                square.SetActive(true);
            }

            gameOverText.SetActive(true);
            watchReplayButton.SetActive(true);
            playAgainButton.SetActive(true);

            // Show Messages
            textHistory.SetActive(true);
            chatPanel.SetActive(true);
            greetButton.SetActive(true);
            ggButton.SetActive(true);
            niceButton.SetActive(true);
            oopsButton.SetActive(true);
        }
    }

    public void ResetBoard()
    {
        // Set all the button's texts back to nothing
        foreach (var square in tictactoeSquareButtonList)
        {
            square.transform.GetChild(0).GetComponent<Text>().text = "";
        }
    }

    public void PlayAgainButtonPressed()
    {
        ResetBoard();

        // Tell server client has left the room
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.LeaveRoom + "");

        // Requeue
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.JoinQueueForGameRoom + "");

        // Change State
        ChangeState(GameStates.WaitingInQueueForOtherPlayer);

    }

    public void JoinGameRoomButtonPressed()
    {
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.JoinQueueForGameRoom + "");
        ChangeState(GameStates.WaitingInQueueForOtherPlayer);
    }

    public void WatchReplay()
    {
        ResetBoard();

        // TODO: implement Replay
    }

    public void SetWinLoss(int winLoss)
    {
        if (winLoss == WinStates.Win)
        {
            gameOverText.GetComponent<Text>().text = "You Won!";
        }
        else if (winLoss == WinStates.Loss)
        {
            gameOverText.GetComponent<Text>().text = "You Lost...";
        }
        else if (winLoss == WinStates.Tie)
        {
            gameOverText.GetComponent<Text>().text = "It's a Tie.";
        }
    }

    public void SetTurn(int turn)
    {
        if (turn == TurnSignifier.MyTurn)
        {
            // Enable squares
            foreach (var square in tictactoeSquareButtonList)
            {
                // Check if there is something in that square
                if (square.transform.GetChild(0).GetComponent<Text>().text == "")
                    square.GetComponent<Button>().interactable = true;
            }
        }
        else if (turn == TurnSignifier.TheirTurn)
        {
            // Disable squares
            foreach (var square in tictactoeSquareButtonList)
            {
                square.GetComponent<Button>().interactable = false;
            }
        }
    }

    public void SetOpponentPlay(int index, int team)
    {
        if (team == TeamSignifier.O)
            tictactoeSquareButtonList[index].transform.GetChild(0).GetComponent<Text>().text = "O";
        if (team == TeamSignifier.X)
            tictactoeSquareButtonList[index].transform.GetChild(0).GetComponent<Text>().text = "X";
    }

    public void TicTacToeSquareButtonPressed(int index)
    {
        // Set to not our turn
        SetTurn(TurnSignifier.TheirTurn);

        // Update board
        if (OurTeam == TeamSignifier.O)
            tictactoeSquareButtonList[index].transform.GetChild(0).GetComponent<Text>().text = "O";
        if (OurTeam == TeamSignifier.X)
            tictactoeSquareButtonList[index].transform.GetChild(0).GetComponent<Text>().text = "X";

        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.TicTacToePlay + "," + index);
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
            // Remove Commas
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

static public class TurnSignifier
{
    public const int MyTurn = 0;
    public const int TheirTurn = 1;
}
static public class GameStates
{
    public const int LoginMenu = 1;
    public const int MainMenu = 2;
    public const int WaitingInQueueForOtherPlayer = 3;

    public const int TicTacToe = 4;

    public const int GameEnd = 5;
}