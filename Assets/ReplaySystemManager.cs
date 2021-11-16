using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ReplaySystemManager : MonoBehaviour
{
    public GameObject replayStepPrefab;

    public GameObject replayStepsPanel;
    GameObject tictactoeBoard;
    List<GameObject> tictactoeSquareButtonList = new List<GameObject>();
    List<GameObject> replayStepsButtonList = new List<GameObject>();
    List<string> replayStepBoardStates = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

        foreach (GameObject go in allObjects)
        {
            if (go.name == "ReplayPanel")
                replayStepsPanel = go;
            else if (go.name == "TicTacToeBoard")
                tictactoeBoard = go;
        }

        for (int i = 0; i < tictactoeBoard.transform.childCount; i++)
        {
            int index = i;
            tictactoeSquareButtonList.Add(tictactoeBoard.transform.GetChild(index).gameObject);
        }

    }

    public void SaveReplay(string replayInfo)
    {
        // Separate each step of the replay
        string[] steps = replayInfo.Split(';');

        // Setup string for storing current board state
        int[] boardState = new int[9];

        for (int i = 0; i < boardState.Length; i++)
        {
            boardState[i] = TeamSignifier.None;
        }

        // Get / Create a new name for the saved file
        string name = "TEST";

        // Save information to a local file
        StreamWriter sw = new StreamWriter(Application.dataPath + Path.DirectorySeparatorChar + name + ".txt");

        for (int i = 0; i < steps.Length; i++)
        {
            // Get the individual move information
            Debug.Log(replayInfo);
            Debug.Log(steps.Length);
            string[] info = steps[i].Split('.');

            var boardIndex = int.Parse(info[0]);
            var team = int.Parse(info[1]);
            var time = "TIME";

            // Write the move information
            sw.WriteLine(ReplaySignifiers.MoveInformation + "," + team + "," + time + "," + boardIndex + ",");

            // Write the board state information after adding info to board state array
            boardState[boardIndex] = team;

            for (int j = 0; j < boardState.Length; j++)
            {
                sw.WriteLine(ReplaySignifiers.BoardState + "," + j + "," + boardState[j]);
            }
        }

        sw.Close();

        // Add file to a list of replay files
    }

    public void LoadReplayInformation()
    {
        // Remove any previous buttons
        var content = replayStepsPanel.transform.GetChild(0).GetChild(0);

        foreach (var step in replayStepsButtonList)
        {
            Destroy(step);
        }

        //for (int i = content.childCount - 1; i >= 0; i--)
        //{
        //    Destroy(content.GetChild(i).gameObject);
        //}

        // Reset the step instructions list
        replayStepBoardStates.Clear();

        // Find the correct file name
        string name = "TEST";

        // Create new replay step buttons
        StreamReader sr = new StreamReader(Application.dataPath + Path.DirectorySeparatorChar + name + ".txt");

        string line;
        while((line = sr.ReadLine()) != null)
        {
            string[] csv = line.Split(',');

            var signifier = int.Parse(csv[0]);

            // Indicating the Team that played the move
            if (signifier == ReplaySignifiers.MoveInformation)
            {
                var team = int.Parse(csv[1]);
                var time = csv[2];
                var move = csv[3];

                // Add a child to the content
                GameObject step = Instantiate(replayStepPrefab);
                step.transform.SetParent(content);
                var text = step.transform.GetChild(0).GetComponent<Text>();

                // Append the Team name to the text
                if (team == TeamSignifier.O)
                    text.text = "Team O: ";
                else if (team == TeamSignifier.X)
                    text.text = "Team X: ";

                // Append the current move to the text
                text.text += move;

                // Append the time taken to the text
                text.text += " - " + time.ToString() + "s";

                // Create board state
                replayStepBoardStates.Add("");
            }
            else
            // Indicating the state of a specific position on the board
            if (signifier == ReplaySignifiers.BoardState)
            {
                var childIndex = content.childCount - 1;
                var boardIndex = int.Parse(csv[1]);
                var team = int.Parse(csv[2]);

                // Save the information into a List according to the current child
                replayStepBoardStates[childIndex] += boardIndex + "," + team + ",";
            }
        }

        sr.Close();

        // Add functionality to created Buttons
        for (int i = 0; i < content.childCount; i++)
        {
            int index = i;
            var replayStep = content.transform.GetChild(index).gameObject;
            replayStep.GetComponent<Button>().onClick.AddListener(delegate { LoadReplayStep(index); });
        }
    }

    public void LoadReplayStep(int index)
    {
        // Get the board state information
        var boardInfo = replayStepBoardStates[index];
        string[] csv = boardInfo.Split(',');

        // Load board information into the board
        for (int i = 0; i < csv.Length - 1; i += 2)
        {
            var boardIndex = int.Parse(csv[i]);
            var team = int.Parse(csv[i + 1]);

            if (team == TeamSignifier.O)
                tictactoeSquareButtonList[boardIndex].transform.GetChild(0).GetComponent<Text>().text = "O";
            else if (team == TeamSignifier.X)
                tictactoeSquareButtonList[boardIndex].transform.GetChild(0).GetComponent<Text>().text = "X";
            else if (team == TeamSignifier.None)
                tictactoeSquareButtonList[boardIndex].transform.GetChild(0).GetComponent<Text>().text = "";

        }
    }

}



public static class ReplaySignifiers
{
    public const int MoveInformation = 1;    // Team that played, Time taken, current move
    public const int BoardState = 2;     // Position, Team
}