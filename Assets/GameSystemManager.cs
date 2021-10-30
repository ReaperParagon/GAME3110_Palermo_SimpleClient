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

    GameObject networkedClient;

    public int OurTeam;

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
        }

        submitButton.GetComponent<Button>().onClick.AddListener(SubmitButtonPressed);
        loginToggle.GetComponent<Toggle>().onValueChanged.AddListener(LoginToggleChanged);
        createToggle.GetComponent<Toggle>().onValueChanged.AddListener(CreateToggleChanged);
        joinGameRoomButton.GetComponent<Button>().onClick.AddListener(JoinGameRoomButtonPressed);

        for (int i = 0; i < tictactoeBoard.transform.childCount; i++)
        {
            int index = i;
            tictactoeSquareButtonList.Add(tictactoeBoard.transform.GetChild(index).gameObject);
            tictactoeSquareButtonList[i].GetComponent<Button>().onClick.AddListener(delegate { TicTacToeSquareButtonPressed(index); } );
        }


        ChangeState(GameStates.LoginMenu);
    }

    // Update is called once per frame
    void Update()
    {
        
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
            // tictactoeSquareButton.SetActive(true);
            foreach (var square in tictactoeSquareButtonList)
            {
                square.SetActive(true);
            }
        }
    }

    public void JoinGameRoomButtonPressed()
    {
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.JoinQueueForGameRoom + "");
        ChangeState(GameStates.WaitingInQueueForOtherPlayer);
    }

    public void SetTurn(int turn)
    {
        if (turn == TurnSignifier.MyTurn)
        {
            // Enable squares
            foreach (var square in tictactoeSquareButtonList)
            {
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
}