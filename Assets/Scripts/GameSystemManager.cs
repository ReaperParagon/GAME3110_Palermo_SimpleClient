using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystemManager : MonoBehaviour
{
    GameObject networkedClient;
    GameObject boardSystemManager, replaySystemManager, chatSystemManager, lobbySystemManager;

    void Awake()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

        foreach (GameObject go in allObjects)
        {
            if (go.name == "Client")
                networkedClient = go;
            else if (go.GetComponent<ReplaySystemManager>() != null)
                replaySystemManager = go;
            else if (go.GetComponent<ChatSystemManager>() != null)
                chatSystemManager = go;
            else if (go.GetComponent<LobbySystemManager>() != null)
                lobbySystemManager = go;
            else if (go.GetComponent<BoardSystemManager>() != null)
                boardSystemManager = go;
        }
    }

    public void ProcessMessage(string msg, int id)
    {
        // Setup comma separated values
        string[] csv = msg.Split(',');
        int signifier = int.Parse(csv[0]);

        if (signifier == ServerToClientSignifiers.AccountCreationComplete)
        {
            ChangeState(GameStates.MainMenu);
        }
        else if (signifier == ServerToClientSignifiers.LoginComplete)
        {
            ChangeState(GameStates.MainMenu);
        }
        else if (signifier == ServerToClientSignifiers.GameStart)
        {
            // Set our team
            boardSystemManager.GetComponent<BoardSystemManager>().OurTeam = int.Parse(csv[1]);

            // Change the state
            ChangeState(GameStates.TicTacToe);

            // Set whose turn it is
            boardSystemManager.GetComponent<BoardSystemManager>().SetTurn(int.Parse(csv[1]));

        }
        else if (signifier == ServerToClientSignifiers.OpponentPlayed)
        {
            // Set the X or O in the right place
            var location = int.Parse(csv[1]);
            var team = int.Parse(csv[2]);
            var continuePlay = int.Parse(csv[3]);

            boardSystemManager.GetComponent<BoardSystemManager>().SetBoardTile(location, team);

            // Set to your turn if we are continuing playing
            if (continuePlay == WinStates.ContinuePlay)
                boardSystemManager.GetComponent<BoardSystemManager>().SetTurn(TurnSignifier.MyTurn);
        }
        else if (signifier == ServerToClientSignifiers.GameOver)
        {
            // Change the state
            ChangeState(GameStates.GameEnd);

            var outcome = int.Parse(csv[1]);

            // Tell board system that the game is over and to display end game information
            boardSystemManager.GetComponent<BoardSystemManager>().SetWinLoss(outcome);

        }
        else if (signifier == ServerToClientSignifiers.TextMessage)
        {
            // Display the message in the chat
            chatSystemManager.GetComponent<ChatSystemManager>().DisplayMessage(csv[1]);
        }
        else if (signifier == ServerToClientSignifiers.ReplayInformation)
        {
            var replayInstructionCheck = csv[1];

            // Check if we are being told to reset our replay files so we can get the new ones...
            if (replayInstructionCheck == ReplayReadSignifier.ResetLocalReplayFiles.ToString())
            {
                replaySystemManager.GetComponent<ReplaySystemManager>().ResetReplayList();
                return;
            }

            // Tell the ReplaySystemManager to save this information
            replaySystemManager.GetComponent<ReplaySystemManager>().SaveReplay(csv[1]);
        }
        else if (signifier == ServerToClientSignifiers.ServerList)
        {
            int roomID = int.Parse(csv[1]);
            int observerCount = int.Parse(csv[2]);

            // Add the servers to the main menu's list
            lobbySystemManager.GetComponent<LobbySystemManager>().CreateRoom(roomID, observerCount);
        }
    }

    public void ChangeState(int newState)
    {
        boardSystemManager.GetComponent<BoardSystemManager>().ChangeState(newState);
        replaySystemManager.GetComponent<ReplaySystemManager>().ChangeState(newState);
        chatSystemManager.GetComponent<ChatSystemManager>().ChangeState(newState);
        lobbySystemManager.GetComponent<LobbySystemManager>().ChangeState(newState);
    }
    public void JoinGameRoom()
    {
        // The server the client is waiting to join a room
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.JoinQueueForGameRoom + "");

        // Go to waiting state
        ChangeState(GameStates.WaitingInQueueForOtherPlayer);
    }

    public void GoToMainMenu()
    {
        // Tell server that client has left the room
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.LeaveRoom + "");

        // Return to the main menu
        ChangeState(GameStates.MainMenu);
    }

    public void ViewReplays()
    {
        // Tell server client has left the room
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.LeaveRoom + "");

        // Disable squares and Reset board
        boardSystemManager.GetComponent<BoardSystemManager>().SetBoardInteractable(false);
        boardSystemManager.GetComponent<BoardSystemManager>().ResetBoard();

        // Get all available replays from the server for our client (based on the name associated with our client ID)
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.RequestReplays + "");

        // Change State to replay scene
        ChangeState(GameStates.Replay);

        // Load the Replay information
        // replaySystemManager.GetComponent<ReplaySystemManager>().LoadReplayInformation(replaySystemManager.GetComponent<ReplaySystemManager>().lastIndexUsed);

    }
}




static public class GameStates
{
    public const int LoginMenu = 1;
    public const int MainMenu = 2;
    public const int WaitingInQueueForOtherPlayer = 3;
    public const int TicTacToe = 4;
    public const int GameEnd = 5;
    public const int Replay = 6;
}