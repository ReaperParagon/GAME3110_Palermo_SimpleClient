using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ReplaySystemManager : MonoBehaviour
{
    public GameObject replayStepPrefab;

    public GameObject replayStepsPanel;
    public GameObject replayDropDown;
    GameObject tictactoeBoard;
    List<GameObject> tictactoeSquareButtonList = new List<GameObject>();
    List<GameObject> replayStepsButtonList = new List<GameObject>();
    List<string> replayStepBoardStates = new List<string>();

    const string IndexFilePath = "replayIndex.txt";
    public int lastIndexUsed;
    public string saveReplayName;
    public List<string> replayNames;
    LinkedList<NameAndIndex> replayNameAndIndices;

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
            else if (go.name == "ReplayDropDown")
                replayDropDown = go;
        }

        replayDropDown.GetComponent<Dropdown>().onValueChanged.AddListener(delegate { LoadDropDownChanged(); });

        for (int i = 0; i < tictactoeBoard.transform.childCount; i++)
        {
            int index = i;
            tictactoeSquareButtonList.Add(tictactoeBoard.transform.GetChild(index).gameObject);
        }

        LoadReplays();

    }

    public void LoadReplays()
    {
        replayNameAndIndices = new LinkedList<NameAndIndex>();

        if (File.Exists(Application.dataPath + Path.DirectorySeparatorChar + IndexFilePath))
        {
            StreamReader sr = new StreamReader(Application.dataPath + Path.DirectorySeparatorChar + IndexFilePath);

            string line;
            while ((line = sr.ReadLine()) != null)
            {
                Debug.Log(line);

                string[] csv = line.Split(',');
                int signifier = int.Parse(csv[0]);

                if (signifier == ReplayReadSignifier.LastUsedIndexSignifier)
                {
                    lastIndexUsed = int.Parse(csv[1]);
                }
                else if (signifier == ReplayReadSignifier.IndexAndNameSignifier)
                {
                    replayNameAndIndices.AddLast(new NameAndIndex(int.Parse(csv[1]), csv[2]));
                }
            }

            sr.Close();
        }

        replayNames = new List<string>();

        foreach (NameAndIndex nameAndIndex in replayNameAndIndices)
        {
            replayNames.Add(nameAndIndex.name);
        }

    }

    public void LoadDropDownChanged()
    {
        int menuIndex = replayDropDown.GetComponent<Dropdown>().value;
        List<Dropdown.OptionData> menuOptions = replayDropDown.GetComponent<Dropdown>().options;
        string value = menuOptions[menuIndex].text;
        ReplayDropDownChanged(value);
    }

    public void ReplayDropDownChanged(string selectedName)
    {
        ResetBoard();

        int indexToLoad = -1;

        foreach (NameAndIndex nameAndIndex in replayNameAndIndices)
        {
            if (nameAndIndex.name == selectedName)
                indexToLoad = nameAndIndex.index;
        }

        LoadReplayInformation(indexToLoad);
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
        lastIndexUsed++;
        saveReplayName = lastIndexUsed.ToString();
        replayNameAndIndices.AddLast(new NameAndIndex(lastIndexUsed, saveReplayName));

        // Save information to a local file
        StreamWriter sw = new StreamWriter(Application.dataPath + Path.DirectorySeparatorChar + lastIndexUsed + ".txt");

        for (int i = 0; i < steps.Length; i++)
        {
            // Get the individual move information
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
        SaveReplayToList();
    }

    public void SaveReplayToList()
    {
        StreamWriter sw = new StreamWriter(Application.dataPath + Path.DirectorySeparatorChar + IndexFilePath);

        sw.WriteLine(ReplayReadSignifier.LastUsedIndexSignifier + "," + lastIndexUsed);

        foreach (NameAndIndex nameAndIndex in replayNameAndIndices)
        {
            sw.WriteLine(ReplayReadSignifier.IndexAndNameSignifier + "," + nameAndIndex.index + "," + nameAndIndex.name);
        }

        sw.Close();
    }

    public void LoadReplayInformation(int indexToLoad)
    {
        // Remove any previous buttons
        var content = replayStepsPanel.transform.GetChild(0).GetChild(0);

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        // Reset the step instructions list
        replayStepBoardStates.Clear();
        replayStepsButtonList.Clear();

        // Create new replay step buttons
        StreamReader sr = new StreamReader(Application.dataPath + Path.DirectorySeparatorChar + indexToLoad + ".txt");

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
                replayStepsButtonList.Add(step);

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
                var childIndex = replayStepBoardStates.Count - 1;
                var boardIndex = int.Parse(csv[1]);
                var team = int.Parse(csv[2]);

                // Save the information into a List according to the current child
                replayStepBoardStates[childIndex] += boardIndex + "," + team + ",";
            }
        }

        sr.Close();

        // Add functionality to created Buttons
        for (int i = 0; i < replayStepBoardStates.Count; i++)
        {
            int index = i;
            var replayStep = replayStepsButtonList[index];
            replayStep.GetComponent<Button>().onClick.AddListener(() => LoadReplayStep(index));// delegate { LoadReplayStep(index); });
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

    public void ResetBoard()
    {
        for (int i = 0; i < tictactoeSquareButtonList.Count; i++)
        {
            tictactoeSquareButtonList[i].transform.GetChild(0).GetComponent<Text>().text = "";
        }
    }

}



public static class ReplaySignifiers
{
    public const int MoveInformation = 1;    // Team that played, Time taken, current move
    public const int BoardState = 2;     // Position, Team
}