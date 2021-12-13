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
            int location = int.Parse(csv[1]);
            int team = int.Parse(csv[2]);
            int continuePlay = int.Parse(csv[3]);

            boardSystemManager.GetComponent<BoardSystemManager>().SetBoardTile(location, team);

            // Set to your turn if we are continuing playing
            if (continuePlay == WinStates.ContinuePlay)
                boardSystemManager.GetComponent<BoardSystemManager>().SetTurn(TurnSignifier.MyTurn);
        }
        else if (signifier == ServerToClientSignifiers.GameOver)
        {
            // Change the state
            ChangeState(GameStates.GameEnd);

            int outcome = int.Parse(csv[1]);

            // Tell board system that the game is over and to display end game information
            boardSystemManager.GetComponent<BoardSystemManager>().SetWinLoss(outcome);

        }
        else if (signifier == ServerToClientSignifiers.TextMessage)
        {
            // Display the message in the chat
            chatSystemManager.GetComponent<ChatSystemManager>().DisplayMessage(csv[1]);
        }
        else if (signifier == ServerToClientSignifiers.ReplayIndexList)
        {
            int replayTransferSignifier = int.Parse(csv[1]);

            // Check to see if we need to reset the list, or add a number to the index list
            if (replayTransferSignifier == ReplayTransferSignifiers.ResetIndexList)
            {
                replaySystemManager.GetComponent<ReplaySystemManager>().ResetIndicesList();
            }
            else
            if (replayTransferSignifier == ReplayTransferSignifiers.IndexNumber)
            {
                int indexToAdd = int.Parse(csv[2]);

                replaySystemManager.GetComponent<ReplaySystemManager>().AddIndexToList(indexToAdd);
            }
        }
        else if (signifier == ServerToClientSignifiers.ReplayInformation)
        {
            int replayTransferSignifier = int.Parse(csv[1]);

            if (replayTransferSignifier == ReplayTransferSignifiers.StartReplayStep)
            {
                // Reset Replay step list
                replaySystemManager.GetComponent<ReplaySystemManager>().ResetReplayStepsList();
            }
            else
            if (replayTransferSignifier == ReplayTransferSignifiers.ReplayStep)
            {
                // Add step to the list
                string location = csv[2];
                string team = csv[3];

                replaySystemManager.GetComponent<ReplaySystemManager>().AddReplayStepIntoList(location + "," + team);
            }
            else
            if (replayTransferSignifier == ReplayTransferSignifiers.EndReplaySteps)
            {
                // Save step list as a replay
                replaySystemManager.GetComponent<ReplaySystemManager>().SaveStepsAsReplay();
            }
        }
        else if (signifier == ServerToClientSignifiers.GameRoomList)
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
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.RequestReplayList + "");

        // Change State to replay scene
        ChangeState(GameStates.Replay);
    }

    public void AskForReplay(int index)
    {
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.RequestReplayByIndex + "," + index);
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


public static class ClientToServerSignifiers
{
    public const int CreateAccount = 1;
    public const int Login = 2;
    public const int JoinQueueForGameRoom = 3;

    public const int TicTacToePlay = 4;
    public const int LeaveRoom = 5;

    public const int TextMessage = 6;
    public const int RequestReplayList = 7;
    public const int RequestReplayByIndex = 8;
    public const int GetGameRoomList = 9;
    public const int SpectateGame = 10;
}

public static class ServerToClientSignifiers
{
    public const int LoginComplete = 1;
    public const int LoginFailed = 2;
    public const int AccountCreationComplete = 3;
    public const int AccountCreationFailed = 4;

    public const int OpponentPlayed = 5;
    public const int GameStart = 6;
    public const int GameOver = 7;

    public const int TextMessage = 8;
    public const int ReplayIndexList = 9;
    public const int ReplayInformation = 10;
    public const int GameRoomList = 11;
}

public static class ReplayTransferSignifiers
{
    public const int ResetIndexList = 1;
    public const int IndexNumber = 2;
    public const int StartReplayStep = 3;
    public const int ReplayStep = 4;
    public const int EndReplaySteps = 5;
}