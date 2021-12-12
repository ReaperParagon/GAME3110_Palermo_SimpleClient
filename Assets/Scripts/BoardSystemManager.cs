using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardSystemManager : MonoBehaviour
{
    GameObject gameOverText, returnToMenuButton;
    GameObject gotoReplayButton;

    GameObject tictactoeBoard;
    public List<GameObject> tictactoeSquareButtonList = new List<GameObject>();

    public int OurTeam;

    GameObject networkedClient, gameSystemManager;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

        foreach (GameObject go in allObjects)
        {
            if (go.name == "Client")
                networkedClient = go;
            else if (go.name == "TicTacToeBoard")
                tictactoeBoard = go;
            else if (go.name == "GameOverText")
                gameOverText = go;
            else if (go.name == "ReturnToMenuButton")
                returnToMenuButton = go;
            else if (go.name == "GoToReplayButton")
                gotoReplayButton = go;
            else if (go.GetComponent<GameSystemManager>() != null)
                gameSystemManager = go;
        }

        returnToMenuButton.GetComponent<Button>().onClick.AddListener(gameSystemManager.GetComponent<GameSystemManager>().GoToMainMenu);
        gotoReplayButton.GetComponent<Button>().onClick.AddListener(gameSystemManager.GetComponent<GameSystemManager>().ViewReplays);

        for (int i = 0; i < tictactoeBoard.transform.childCount; i++)
        {
            int index = i;
            tictactoeSquareButtonList.Add(tictactoeBoard.transform.GetChild(index).gameObject);
            tictactoeSquareButtonList[i].GetComponent<Button>().onClick.AddListener(delegate { TicTacToeSquareButtonPressed(index); });
        }
    }

    public void ChangeState(int newState)
    {
        gameOverText.SetActive(false);
        gotoReplayButton.SetActive(false);
        returnToMenuButton.SetActive(false);

        DisplayBoard(false);

        
        if (newState == GameStates.TicTacToe)
        {
            // Show the board and Reset it
            ResetBoard();
            DisplayBoard(true);

            // If an observer, show return to menu button and disable tiles
            if (OurTeam == TeamSignifier.None)
            {
                returnToMenuButton.SetActive(true);
                SetBoardInteractable(false);
            }
        }
        else if (newState == GameStates.GameEnd)
        {
            // Show Board and Buttons, Disable Tiles
            SetBoardInteractable(false);
            DisplayBoard(true);

            gameOverText.SetActive(true);
            returnToMenuButton.SetActive(true);

            // If an observer, don't show go to replay button, they won't have the replay anyway
            if (OurTeam != TeamSignifier.None)
            {
                gotoReplayButton.SetActive(true);
            }

        }
    }

    public void DisplayBoard(bool showBoard)
    {
        foreach (GameObject tile in tictactoeSquareButtonList)
        {
            tile.SetActive(showBoard);
        }
    }

    public void ResetBoard()
    {
        // Set all the button's texts back to nothing
        foreach (GameObject tile in tictactoeSquareButtonList)
        {
            tile.transform.GetChild(0).GetComponent<Text>().text = "";
        }
    }

    public void SetBoardInteractable(bool interactable)
    {
        // Set all tiles to disabled
        foreach (GameObject tile in tictactoeSquareButtonList)
        {
            tile.GetComponent<Button>().interactable = interactable;
        }
    }


    public void SetTurn(int turn)
    {
        // If we are an observer and not on a team, no need to check other things
        if (OurTeam == TeamSignifier.None)
        {
            // Disable squares
            SetBoardInteractable(false);
            return;
        }

        if (turn == TurnSignifier.MyTurn)
        {
            // Enable squares
            foreach (GameObject tile in tictactoeSquareButtonList)
            {
                // Check if there is something in that square
                if (tile.transform.GetChild(0).GetComponent<Text>().text == "")
                    tile.GetComponent<Button>().interactable = true;
            }
        }
        else if (turn == TurnSignifier.TheirTurn)
        {
            // Disable squares
            SetBoardInteractable(false);
        }
    }

    public void SetBoardTile(int index, int team)
    {
        if (team == TeamSignifier.O)
            tictactoeSquareButtonList[index].transform.GetChild(0).GetComponent<Text>().text = "O";
        else if (team == TeamSignifier.X)
            tictactoeSquareButtonList[index].transform.GetChild(0).GetComponent<Text>().text = "X";
        else if (team == TeamSignifier.None)
            tictactoeSquareButtonList[index].transform.GetChild(0).GetComponent<Text>().text = "";
    }

    public void TicTacToeSquareButtonPressed(int index)
    {
        // Set to not our turn
        SetTurn(TurnSignifier.TheirTurn);

        // Update board
        SetBoardTile(index, OurTeam);

        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.TicTacToePlay + "," + index);
    }

    public void SetWinLoss(int winLoss)
    {
        if (winLoss == WinStates.OsWin)
        {
            gameOverText.GetComponent<Text>().text = "Team O Wins";
        }
        else if (winLoss == WinStates.XsWin)
        {
            gameOverText.GetComponent<Text>().text = "Team X Wins";
        }
        else if (winLoss == WinStates.Tie)
        {
            gameOverText.GetComponent<Text>().text = "It's a Tie.";
        }
    }
}

static public class TurnSignifier
{
    public const int MyTurn = 0;
    public const int TheirTurn = 1;
    public const int Observer = 2;
}

public static class WinStates
{
    public const int ContinuePlay = 100;
    public const int OsWin = 0;
    public const int XsWin = 1;
    public const int Tie = 2;
}

public static class TeamSignifier
{
    public const int None = -1;
    public const int O = 0;
    public const int X = 1;
}
