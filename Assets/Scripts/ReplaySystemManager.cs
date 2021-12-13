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
    List<string> replayStepsList = new List<string>();

    const string currentReplayFilePath = "currentReplayFile.txt";
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

    public void LoadReplayIndicesIntoDropDown()
    {
        Dropdown dropdown = replayDropDown.GetComponent<Dropdown>();
        dropdown.options.Clear();

        foreach (NameAndIndex nameAndIndex in replayNameAndIndices)
        {
            // Add that to the dropdown
            dropdown.options.Add(new Dropdown.OptionData(nameAndIndex.name));
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

        if (indexToLoad != -1)
            gameSystemManager.GetComponent<GameSystemManager>().AskForReplay(indexToLoad);
    }

    public void ResetIndicesList()
    {
        // Reset stored replay information
        replayNameAndIndices = new LinkedList<NameAndIndex>();
    }

    public void AddIndexToList(int index)
    {
        replayNameAndIndices.AddLast(new NameAndIndex(index, index.ToString()));

        // Load our new Indices into our dropdown
        LoadReplayIndicesIntoDropDown();
    }

    public void ResetReplayStepsList()
    {
        replayStepsList = new List<string>();
    }

    public void AddReplayStepIntoList(string replayStep)
    {
        // Put replay step into our list
        replayStepsList.Add(replayStep);
    }

    public void SaveStepsAsReplay()
    {
        // Setup string for storing current board state
        int[] boardState = new int[9];

        for (int i = 0; i < boardState.Length; i++)
        {
            boardState[i] = TeamSignifier.None;
        }

        // Save information to a local file
        StreamWriter sw = new StreamWriter(Application.dataPath + Path.DirectorySeparatorChar + currentReplayFilePath);

        for (int i = 0; i < replayStepsList.Count; i++)
        {
            // Get the individual move information
            Debug.Log(replayStepsList[i]);
            string[] info = replayStepsList[i].Split(',');

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

        LoadReplayInformation();
    }

    public void LoadReplayInformation()
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
        StreamReader sr = new StreamReader(Application.dataPath + Path.DirectorySeparatorChar + currentReplayFilePath);

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
}

public static class ReplaySignifiers
{
    public const int MoveInformation = 1;    // Team that played, current move
    public const int BoardState = 2;     // Position, Team
}