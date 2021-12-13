using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ReplaySystemManager : MonoBehaviour
{
    public GameObject replayStepPrefab;

    GameObject replayStepsPanel, replayDropDown, backToMenuButton, replayScollGroup;
    GameObject replayAnimationButton;

    List<GameObject> replayStepsButtonList = new List<GameObject>();
    List<string> replayStepBoardStates = new List<string>();

    const string IndexFilePath = "replayIndex.txt";
    public int lastIndexUsed;
    public List<string> replayNames;
    LinkedList<NameAndIndex> replayNameAndIndices = new LinkedList<NameAndIndex>();

    GameObject gameSystemManager, boardSystemManager;
    IEnumerator replayAnimationCoroutine;

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
            else if (go.name == "ReplayScrollGroup")
                replayScollGroup = go;
            else if (go.name == "PlayReplayButton")
                replayAnimationButton = go;
            else if (go.GetComponent<GameSystemManager>() != null)
                gameSystemManager = go;
            else if (go.GetComponent<BoardSystemManager>() != null)
                boardSystemManager = go;
        }

        replayStepPrefab = Resources.Load("Prefabs/ReplayStep") as GameObject;

        backToMenuButton.GetComponent<Button>().onClick.AddListener(gameSystemManager.GetComponent<GameSystemManager>().GoToMainMenu);
        replayAnimationButton.GetComponent<Button>().onClick.AddListener(delegate { PlayCurrentReplay(); });
        replayDropDown.GetComponent<Dropdown>().onValueChanged.AddListener(delegate { LoadDropDownChanged(); });

        // Clear Drop down options
        replayDropDown.GetComponent<Dropdown>().ClearOptions();
    }

    public void ChangeState(int newState)
    {
        replayStepsPanel.SetActive(false);
        replayDropDown.SetActive(false);
        backToMenuButton.SetActive(false);
        replayAnimationButton.SetActive(false);

        if (newState == GameStates.Replay)
        {
            // Show TicTacToe board
            boardSystemManager.GetComponent<BoardSystemManager>().DisplayBoard(true);

            // Show replay panel
            replayStepsPanel.SetActive(true);
            backToMenuButton.SetActive(true);
            replayDropDown.SetActive(true);
            replayAnimationButton.SetActive(true);
        }
    }

    public void LoadReplaysIntoDropDown()
    {
        // Reset stored replay information
        replayNameAndIndices = new LinkedList<NameAndIndex>();

        // Get the replays from the index file, save into replayNameAndIndices
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

        // Add Names for use in drop down
        replayNames = new List<string>();

        foreach (NameAndIndex nameAndIndex in replayNameAndIndices)
        {
            replayNames.Add(nameAndIndex.name);
        }

        // Add Replays to Dropdown
        Dropdown dropdown = replayDropDown.GetComponent<Dropdown>();
        dropdown.options.Clear();

        foreach (string option in replayNames)
        {
            dropdown.options.Add(new Dropdown.OptionData(option));
        }

        // Change Replay dropdown to the latest replay
        dropdown.value = dropdown.options.Count - 1;

        // Load that replay
        LoadDropDownChanged();
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

    // CHANGE THIS ON THE SERVER TO NOT USE THOSE DELIMITERS
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

            int boardIndex = int.Parse(info[0]);
            int team = int.Parse(info[1]);

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

        // Add to the drop down
        LoadReplaysIntoDropDown();
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
        // Stop Replay coroutine if it is running
        if (replayAnimationCoroutine != null)
            StopCoroutine(replayAnimationCoroutine);

        // Remove any previous buttons
        for (int i = replayScollGroup.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(replayScollGroup.transform.GetChild(i).gameObject);
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

            int signifier = int.Parse(csv[0]);

            // Indicating the Team that played the move
            if (signifier == ReplaySignifiers.MoveInformation)
            {
                int team = int.Parse(csv[1]);
                int move = int.Parse(csv[2]);

                // Add a child to the scroll group
                GameObject step = Instantiate(replayStepPrefab);
                step.transform.SetParent(replayScollGroup.transform);
                Text text = step.transform.GetChild(0).GetComponent<Text>();
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
                int childIndex = replayStepBoardStates.Count - 1;
                int boardIndex = int.Parse(csv[1]);
                int team = int.Parse(csv[2]);

                // Save the information into a List according to the current child
                replayStepBoardStates[childIndex] += boardIndex + "," + team + ",";
            }
        }

        sr.Close();

        // Add functionality to created Buttons
        for (int i = 0; i < replayStepBoardStates.Count; i++)
        {
            int index = i;
            GameObject replayStep = replayStepsButtonList[index];
            replayStep.GetComponent<Button>().onClick.AddListener(() => LoadReplayStep(index));
        }
    }

    public void LoadReplayStep(int index)
    {
        // Get the board state information
        string boardInfo = replayStepBoardStates[index];
        string[] csv = boardInfo.Split(',');

        // Load board information into the board
        for (int i = 0; i < csv.Length - 1; i += 2)
        {
            int boardIndex = int.Parse(csv[i]);
            int team = int.Parse(csv[i + 1]);

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

    public void ResetReplayList()
    {
        // Reset local index file
        StreamWriter sw = new StreamWriter(Application.dataPath + Path.DirectorySeparatorChar + IndexFilePath);

        sw.WriteLine("");

        sw.Close();

        // Reset our last index used and our stored replay information, so we can overwrite the files
        lastIndexUsed = 0;
        replayNameAndIndices = new LinkedList<NameAndIndex>();
    }

    public void PlayCurrentReplay()
    {
        // Set and Start Coroutine
        replayAnimationCoroutine = ReplayAnimationCoroutine();
        StartCoroutine(replayAnimationCoroutine);
    }

    IEnumerator ReplayAnimationCoroutine()
    {
        // For each step... 
        for (int i = 0; i < replayStepBoardStates.Count; i++)
        {
            // Load the replay step
            LoadReplayStep(i);

            // Wait for 1 second
            yield return new WaitForSeconds(1.0f);
        }

        // Unset our coroutine
        replayAnimationCoroutine = null;
    }

}


public class NameAndIndex
{
    public string name;
    public int index;

    public NameAndIndex(int Index, string Name)
    {
        index = Index;
        name = Name;
    }
}

static public class ReplayReadSignifier
{
    public const int LastUsedIndexSignifier = 1;
    public const int IndexAndNameSignifier = 2;
    public const int ResetLocalReplayFiles = 100;
}

public static class ReplaySignifiers
{
    public const int MoveInformation = 1;    // Team that played, current move
    public const int BoardState = 2;     // Position, Team
}