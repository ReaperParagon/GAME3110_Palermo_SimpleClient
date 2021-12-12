using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbySystemManager : MonoBehaviour
{
    GameObject submitButton, userNameInput, passwordInput, createToggle, loginToggle;
    GameObject textNameInfo, textPassordInfo;

    GameObject joinGameRoomButton, viewReplayButton, refreshRoomsButton;
    GameObject gameRoomPanel, gameRoomScrollGroup;
    List<GameObject> gameRoomButtonList = new List<GameObject>();

    GameObject networkedClient, gameSystemManager;

    public GameObject gameRoomPrefab;

    // Start is called before the first frame update
    void Awake()
    {
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
            else if (go.name == "ViewReplayButton")
                viewReplayButton = go;
            else if (go.name == "RefreshGameRoomsButton")
                refreshRoomsButton = go;
            else if (go.name == "GameRoomPanel")
                gameRoomPanel = go;
            else if (go.name == "GameRoomScrollGroup")
                gameRoomScrollGroup = go;
            else if (go.GetComponent<GameSystemManager>() != null)
                gameSystemManager = go;
        }

        submitButton.GetComponent<Button>().onClick.AddListener(SubmitButtonPressed);
        loginToggle.GetComponent<Toggle>().onValueChanged.AddListener(LoginToggleChanged);
        createToggle.GetComponent<Toggle>().onValueChanged.AddListener(CreateToggleChanged);
        joinGameRoomButton.GetComponent<Button>().onClick.AddListener(gameSystemManager.GetComponent<GameSystemManager>().JoinGameRoom);

        viewReplayButton.GetComponent<Button>().onClick.AddListener(gameSystemManager.GetComponent<GameSystemManager>().ViewReplays);
        refreshRoomsButton.GetComponent<Button>().onClick.AddListener(AskForRooms);

        gameRoomPrefab = Resources.Load("Prefabs/GameRoomPrefab") as GameObject;
    }

    public void ChangeState(int newState)
    {
        // Disable all
        joinGameRoomButton.SetActive(false);
        viewReplayButton.SetActive(false);
        refreshRoomsButton.SetActive(false);
        gameRoomPanel.SetActive(false);

        submitButton.SetActive(false);
        userNameInput.SetActive(false);
        passwordInput.SetActive(false);
        createToggle.SetActive(false);
        loginToggle.SetActive(false);

        textNameInfo.SetActive(false);
        textPassordInfo.SetActive(false);

        if (newState == GameStates.MainMenu)
        {
            joinGameRoomButton.SetActive(true);
            viewReplayButton.SetActive(true);
            refreshRoomsButton.SetActive(true);
            gameRoomPanel.SetActive(true);

            // Refresh Rooms
            AskForRooms();
        }
        else if (newState == GameStates.LoginMenu)
        {

            submitButton.SetActive(true);
            userNameInput.SetActive(true);
            passwordInput.SetActive(true);
            createToggle.SetActive(true);
            loginToggle.SetActive(true);

            textNameInfo.SetActive(true);
            textPassordInfo.SetActive(true);
        }
    }

    public void SubmitButtonPressed()
    {
        // We want to send login information to the server
        string n = userNameInput.GetComponent<InputField>().text;
        string p = passwordInput.GetComponent<InputField>().text;

        string msg;

        if (createToggle.GetComponent<Toggle>().isOn)
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

    public void AskForRooms()
    {
        // Remove all rooms from room panel
        for (int i = gameRoomScrollGroup.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(gameRoomScrollGroup.transform.GetChild(i).gameObject);
        }

        // Reset the room buttons list
        gameRoomButtonList.Clear();

        // Ask server for room list
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.GetGameRoomList + ",");
    }

    public void CreateRoom(int index, int spectatorCount)
    {
        // Add a room to the game room container
        GameObject room = Instantiate(gameRoomPrefab);
        room.transform.SetParent(gameRoomScrollGroup.transform);
        Text text = room.transform.GetChild(0).GetComponent<Text>();
        gameRoomButtonList.Add(room);

        // Append information to the text
        text.text = "Game Room " + (index + 1);

        // Append the spectator count
        text.text += " | Watching: " + spectatorCount;

        // Add functionality to created Button
        Button spectateButton = room.transform.GetChild(1).GetComponent<Button>();
        spectateButton.onClick.AddListener(delegate { JoinRoomAsObserver(index); });
    }

    private void JoinRoomAsObserver(int index)
    {
        // Send Message to Server to join the room as an observer
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.SpectateGame + "," + index);
    }

    
}
