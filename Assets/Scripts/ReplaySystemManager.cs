using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ReplaySystemManager : MonoBehaviour
{
    public GameObject replayStepPrefab;

    GameObject replayStepsPanel, replayDropDown, backToMenuButton;

    List<GameObject> replayStepsButtonList = new List<GameObject>();
    List<string> replayStepBoardStates = new List<string>();

    const string IndexFilePath = "replayIndex.txt";
    public int lastIndexUsed;
    public List<string> replayNames;
    LinkedList<NameAndIndex> replayNameAndIndices;

    GameObject gameSystemManager, boardSystemManager;

    // Start is called before the first frame update
    void Awake()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

        foreach (GameObject go in allObjects)
        {
            if (go.name == "BackToMenuButton")
                backToMenuButton = go;
            else if (go.name == "ReplayPanel")
                replayStepsPanel = go;
            else if (go.name == "ReplayDropDown")
                replayDropDown = go;
            else if (go.name == "ReplayPanel")
                replayStepsPanel = go;
            else if (go.name == "ReplayDropDown")
                replayDropDown = go;
            else if (go.GetComponent<GameSystemManager>() != null)
                gameSystemManager = go;
            else if (go.GetComponent<BoardSystemManager>() != null)
                boardSystemManager = go;
        }

        replayStepPrefab = Resources.Load("Prefabs/ReplayStep") as GameObject;

        backToMenuButton.GetComponent<Button>().onClick.AddListener(gameSystemManager.GetComponent<GameSystemManager>().GoToMainMenu);
        replayDropDown.GetComponent<Dropdown>().onValueChanged.AddListener(delegate { LoadDropDownChanged(); });

        LoadReplays();

    }

    public void ChangeState(int newState)
    {
        replayStepsPanel.SetActive(false);
        replayDropDown.SetActive(false);
        backToMenuButton.SetActive(false);

        if (newState == GameStates.Replay)
        {
            // Show TicTacToe board
            boardSystemManager.GetComponent<BoardSystemManager>().DisplayBoard(true);

            // Show replay panel
            replayStepsPanel.SetActive(true);
            backToMenuButton.SetActive(true);
            replayDropDown.SetActive(true);

            // Update Replay Names
            LoadReplays();

            // Add Replays to Dropdown
            Dropdown dropdown = replayDropDown.GetComponent<Dropdown>();
            dropdown.options.Clear();

            foreach (string option in replayNames)
            {
                dropdown.options.Add(new Dropdown.OptionData(option));
            }

            // Change Replay dropdown to the latest replay
            dropdown.value = dropdown.options.Count - 1;
        }
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
        boardSystemManager.GetComponent<BoardSystemManager>().ResetBoard();

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
        string saveReplayName = lastIndexUsed.ToString();
        replayNameAndIndices.AddLast(new NameAndIndex(lastIndexUsed, saveReplayName));

        // Save information to a local file
        StreamWriter sw = new StreamWriter(Application.dataPath + Path.DirectorySeparatorChar + lastIndexUsed + ".txt");

        for (int i = 0; i < steps.Length; i++)
        {
            // Get the individual move information
            string[] info = steps[i].Split('.');

            var boardIndex = int.Parse(info[0]);
            var team = int.Parse(info[1]);

            // Write the move information
            sw.WriteLine(ReplaySignifiers.MoveInformation + "," + team + "," + boardIndex + ",");

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

        int turnNumber = 0;

        string line;
        while((line = sr.ReadLine()) != null)
        {
            string[] csv = line.Split(',');

            var signifier = int.Parse(csv[0]);

            // Indicating the Team that played the move
            if (signifier == ReplaySignifiers.MoveInformation)
            {
                var team = int.Parse(csv[1]);
                var move = int.Parse(csv[2]);

                // Add a child to the content
                GameObject step = Instantiate(replayStepPrefab);
                step.transform.SetParent(content);
                var text = step.transform.GetChild(0).GetComponent<Text>();
                replayStepsButtonList.Add(step);

                // Append the Team name to the text
                if (team == TeamSignifier.O)
                    text.text = "Turn " + (++turnNumber) + " | Team O: ";
                else if (team == TeamSignifier.X)
                    text.text = "Turn " + (++turnNumber) + " | Team X: ";

                // Append the current move to the text
                text.text += GetMoveFromNumber(move);

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

            boardSystemManager.GetComponent<BoardSystemManager>().SetBoardTile(boardIndex, team);
        }
    }

    private string GetMoveFromNumber(int index)
    {
        string tile;

        // Top Row
        if (index < 3)
            tile = "Top ";
        else
        // Bottom Row
        if (index > 5)
            tile = "Bottom ";
        else
        // Middle
            tile = "Middle ";

        int col = index % 3;

        // Left
        if (col == 0)
            tile += "Left";
        else
        // Center
        if (col == 1)
            tile += "Center";
        else
        // Right
        if (col == 2)
            tile += "Right";


        return tile;
    }

}



public static class ReplaySignifiers
{
    public const int MoveInformation = 1;    // Team that played, current move
    public const int BoardState = 2;     // Position, Team
}