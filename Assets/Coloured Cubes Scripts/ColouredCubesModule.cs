using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class ColouredCubesModule : MonoBehaviour {
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;

    public ColouredCube[] Cubes;
    public StageLight[] StageLights;
    public ScreenButton Screen;

    public Set SET;
    public Permutations PermGenerator;

    private static int ModuleIdCounter = 1;
    private int ModuleId;
    private bool ModuleSolved = false;

    private KMSelectable _moduleSelectable;

    private StageInfo[] _stages = new StageInfo[3];
    private Cycle[] _stageTwoCycles;
    private int[] _stageTwoPermutation;
    private SetValue _stageThreeHiddenValue;

    private int _internalStage = 0;
    private int _displayedStage = 1;
    private int _selectionsNeededForSubmission = 3;

    private List<int> _selectedPositions = new List<int>();

    // Both _allowCubeInteraction and _allowButtonInteraction must be true for the cubes to be selectable.
    private bool _allowButtonInteraction = true;
    private bool _allowCubeInteraction = false;
    private bool _possibleSubmission = false;
    private bool _displayingSizeChart = false;

    void Awake() {
        ModuleId = ModuleIdCounter++;
        _moduleSelectable = Module.GetComponent<KMSelectable>();

        AssignInteractionHandlers();
    }

    void AssignInteractionHandlers() {
        foreach (ColouredCube cube in Cubes) {
            cube.GetComponent<KMSelectable>().OnInteract += delegate () { CubePress(cube); return false; };
            cube.GetComponent<KMSelectable>().OnHighlight += delegate () { Screen.DisplayColourName(cube); };
            cube.GetComponent<KMSelectable>().OnHighlightEnded += delegate () { Screen.StopDisplayingColourName(); };
        }

        foreach (StageLight light in StageLights) {
            light.GetComponent<KMSelectable>().OnInteract += delegate () { StageLightPress(light); return false; };
            light.GetComponent<KMSelectable>().OnHighlight += delegate () { Screen.DisplayColourName(light); };
            light.GetComponent<KMSelectable>().OnHighlightEnded += delegate () { Screen.StopDisplayingColourName(); };
        }

        Screen.GetComponent<KMSelectable>().OnInteract += delegate () { ScreenPress(); return false; };
    }

    void Start() {
        GenerateStages();
        Screen.DefaultText = "Start";
        Debug.LogFormat("[Coloured Cubes #{0}] Press the screen the start.", ModuleId);
    }

    void GenerateStages() {
        int stageOneRed = Rnd.Range(0, 3);
        int stageOneGreen = Rnd.Range(0, 3);
        int stageOneBlue = Rnd.Range(0, 3);
        int stageOneSize = Bomb.GetPortCount() % 3;
        Color[] stageOneColours = new Color[3];

        int stageTwoRed = Rnd.Range(0, 3);
        int stageTwoGreen = Rnd.Range(0, 3);
        int stageTwoBlue = Rnd.Range(0, 3);
        int stageTwoSize = Bomb.GetIndicators().Count() % 3;
        Color[] stageTwoColours = new Color[3];

        int[] stageOnePositions = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        int[] stageTwoPositions = new int[9];
        int[] stageThreePositions;

        for (int i = 0; i < 3; i++) {
            stageOneColours[i] = new Color(i == stageOneRed ? 1 : 0, i == stageOneGreen ? 1 : 0, i == stageOneBlue ? 1 : 0);
            stageTwoColours[i] = new Color(i == stageTwoRed ? 0 : 1, i == stageTwoGreen ? 0 : 1, i == stageTwoBlue ? 0 : 1);
        }

        PermGenerator.GeneratePermutation(9, 3);
        _stageTwoPermutation = PermGenerator.Permutation;
        _stageTwoCycles = PermGenerator.Cycles;

        for (int i = 0; i < 9; i++) {
            stageTwoPositions[_stageTwoPermutation[i]] = i;
        }

        stageThreePositions = stageOnePositions.Select((element, index) => FindPositionSet(element, stageTwoPositions[index])).ToArray();
        _stageThreeHiddenValue = SET.FindSetWith(new SetValue(stageOneRed, stageOneGreen, stageOneBlue, stageTwoSize), new SetValue(stageTwoRed, stageTwoGreen, stageTwoBlue, stageTwoSize));

        _stages[0] = new StageInfo(SET.GenerateSETValuesWithOneSet(4, 9), SET.MostRecentCorrectPositions.ToArray(), stageOnePositions, stageOneColours);
        _stages[1] = new StageInfo(SET.GenerateSETValuesWithOneSet(4, 9), SET.MostRecentCorrectPositions.ToArray(), stageTwoPositions, stageTwoColours);
        _stages[2] = new StageInfo(SET.GenerateSETValuesWithOneSet(4, 9, _stageThreeHiddenValue), SET.MostRecentCorrectPositions.ToArray(), stageThreePositions);
    }

    int FindPositionSet(int positionOne, int positionTwo) {
        int rowOne = positionOne / 3;
        int rowTwo = positionTwo / 3;
        int rowThree = SET.FindSetWith(new SetValue(rowOne), new SetValue(rowTwo)).Values[0];
        int columnOne = positionOne % 3;
        int columnTwo = positionTwo % 3;
        int columnThree = SET.FindSetWith(new SetValue(columnOne), new SetValue(columnTwo)).Values[0];

        return rowThree * 3 + columnThree;
    }

    void CubePress(ColouredCube cube) {
        if (!_allowButtonInteraction || !_allowCubeInteraction) { return; }

        if (_selectedPositions.Contains(cube.Position)) {
            cube.SetHighlight(false);
            _selectedPositions.Remove(cube.Position);
            if (_possibleSubmission) { DisableSubmission(); }
        } else if (!_possibleSubmission) {
            cube.SetHighlight(true);
            _selectedPositions.Add(cube.Position);
            if (!_possibleSubmission && _selectedPositions.Count() == _selectionsNeededForSubmission) { EnableSubmission(); }
        }
    }

    void EnableSubmission() {
        _possibleSubmission = true;
        Screen.EnableOverrideText("Submit");
    }

    void DisableSubmission() {
        _possibleSubmission = false;
        Screen.DisableOverrideText();
    }

    void StageLightPress(StageLight light) {
        if (!_allowButtonInteraction) { return; }
    }


    void ScreenPress() {
        if (!_allowButtonInteraction) { return; }

        if (_possibleSubmission) {
            HandleSubmission();
            return;
        }

        if (_internalStage == 0) {
            StartCoroutine(StageOneAnimation());
        }
    }

    void HandleSubmission() {
        if (_selectedPositions.Any((position) => !_stages[_internalStage - 1].CorrectPositions.Contains(position))) {
            Strike();
            return;
        }

        Debug.LogFormat("[Coloured Cubes #{0}] You submitted the correct cubes.", ModuleId);
        if (_internalStage == 1) {
            DisableSubmission();
            StartCoroutine(StageTwoAnimation());
        } else if (_internalStage == 2) {
            DisableSubmission();
            StartCoroutine(StageThreeAnimation());
            _selectionsNeededForSubmission = 2;
        } else {
            SolveModule();
        }
    }

    void Strike() {
        string selectedCubes = "";

        for (int i = 0; i < _selectedPositions.Count(); i++) {
            if (i == _selectedPositions.Count() - 1) {
                selectedCubes += "and " + (Position)_selectedPositions[i];
            } else {
                selectedCubes += (Position)_selectedPositions[i] + ", ";
            }
        }
        Debug.LogFormat("[Coloured Cubes #{0}] Strike! You selected {1}, which was incorrect.", ModuleId, selectedCubes);

        DeselectCubes();
        ColouredCube.StrikeFlash(Cubes);
        DisableSubmission();
        Module.HandleStrike();
    }

    void DeselectCubes() {
        _selectedPositions.Clear();
        ColouredCube.Deselect(Cubes);
    }

    void SolveModule() {
        Debug.LogFormat("[Coloured Cubes #{0}] -=-==-=-", ModuleId);
        Debug.LogFormat("[Coloured Cubes #{0}] Module solved!", ModuleId);
        Module.HandlePass();
        _allowButtonInteraction = false;
    }


    IEnumerator StageOneAnimation() {
        Screen.EnableOverrideText("...");
        _allowButtonInteraction = false;
        _allowCubeInteraction = false;

        DeselectCubes();
        ColouredCube.SetHiddenStates(Cubes, false);
        do { yield return null; } while (ColouredCube.AreBusy(Cubes)); // This causes the coroutine to wait until the cubes stop moving.

        ColouredCube.AssignSetValues(Cubes, _stages[0].AllValues, _stages[0].TruePositions);
        do { yield return null; } while (ColouredCube.AreBusy(Cubes));

        StageLight.SetColours(StageLights, _stages[0].StageLightColours);
        Screen.DefaultText = "Stage 1";
        Screen.DisableOverrideText();
        _allowButtonInteraction = true;

        if (_internalStage == 0) {
            DoStageOneLogging();
            _internalStage = 1;
        }

        if (_internalStage == 1) { _allowCubeInteraction = true; }
    }

    void DoStageOneLogging() {
        int[] correctPositions = _stages[0].CorrectPositions;

        Debug.LogFormat("[Coloured Cubes #{0}] Set values are in red-green-blue-size order.", ModuleId);
        Debug.LogFormat("[Coloured Cubes #{0}] Stage light colours are in 0-1-2 order.", ModuleId);
        Debug.LogFormat("[Coloured Cubes #{0}] -=-==-=-", ModuleId);
        Debug.LogFormat("[Coloured Cubes #{0}] Stage 1:", ModuleId);
        Debug.LogFormat("[Coloured Cubes #{0}] The stage lights display {1}, {2}, and {3}.", ModuleId, StageLights[0].ColourName.ToLower(), StageLights[1].ColourName.ToLower(), StageLights[2].ColourName.ToLower());

        for (int i = 0; i < 9; i++) {
            Debug.LogFormat("[Coloured Cubes #{0}] {1} is a {2} {3} cube. Its actual values are {4}.", ModuleId, (Position)i, (Size)Cubes[i].Size, Cubes[i].ColourName.ToLower(), _stages[0].AllValues[i]);
        }

        Debug.LogFormat("[Coloured Cubes #{0}] {1}, {2}, and {3} form a set!", ModuleId, (Position)correctPositions[0], (Position)correctPositions[1], (Position)correctPositions[2]);
    }

    IEnumerator StageTwoAnimation() {
        Screen.EnableOverrideText("...");
        _allowButtonInteraction = false;
        _allowCubeInteraction = false;

        DeselectCubes();
        ColouredCube.ShrinkAndMakeWhite(Cubes);
        do { yield return null; } while (ColouredCube.AreBusy(Cubes));

        foreach (Cycle cycle in _stageTwoCycles) {
            // Reveals cubes that are in the cycle and hides the rest.
            ColouredCube.SetHiddenStates(Cubes, Cubes.Select((cube, index) => !cycle.Elements.Contains(index)).ToArray());
            do { yield return null; } while (ColouredCube.AreBusy(Cubes));

            foreach (int position in cycle.Elements) {
                Cubes[position].SetPosition(cycle.Permute(position));
            }
            do { yield return null; } while (ColouredCube.AreBusy(Cubes));

            ReorderCubes();
        }

        ColouredCube.SetHiddenStates(Cubes, false);
        do { yield return null; } while (ColouredCube.AreBusy(Cubes));

        ColouredCube.AssignSetValues(Cubes, _stages[1].AllValues, _stages[1].TruePositions);
        do { yield return null; } while (ColouredCube.AreBusy(Cubes));

        StageLight.SetColours(StageLights, _stages[1].StageLightColours);
        Screen.DefaultText = "Stage 2";
        Screen.DisableOverrideText();
        _allowButtonInteraction = true;

        if (_internalStage == 1) {
            DoStageTwoLogging();
            _internalStage = 2;
        }

        if (_internalStage == 2) { _allowCubeInteraction = true; }
    }

    void DoStageTwoLogging() {
        int[] correctPositions = _stages[1].CorrectPositions;

        Debug.LogFormat("[Coloured Cubes #{0}] -=-==-=-", ModuleId);
        Debug.LogFormat("[Coloured Cubes #{0}] Stage 2:", ModuleId);
        Debug.LogFormat("[Coloured Cubes #{0}] The stage lights display {1}, {2}, and {3}.", ModuleId, StageLights[0].ColourName.ToLower(), StageLights[1].ColourName.ToLower(), StageLights[2].ColourName.ToLower());
        Debug.LogFormat("[Coloured Cubes #{0}] The cycles displayed are:", ModuleId);

        foreach (Cycle cycle in _stageTwoCycles) {
            Debug.LogFormat("[Coloured Cubes #{0}] {1}", ModuleId, cycle.ToString());
        }

        for (int i = 0; i < 9; i++) {
            Debug.LogFormat("[Coloured Cubes #{0}] {1} is a {2} {3} cube. Its original position was {4}. Its actual values are {5}.", ModuleId, (Position)i, (Size)Cubes[i].Size, Cubes[i].ColourName.ToLower(), (Position)_stages[1].TruePositions[i], _stages[1].AllValues[i]);
        }

        Debug.LogFormat("[Coloured Cubes #{0}] {1}, {2}, and {3} form a set!", ModuleId, (Position)correctPositions[0], (Position)correctPositions[1], (Position)correctPositions[2]);
    }

    void ReorderCubes() {
        var newCubeOrder = new ColouredCube[9];
        int row;
        int column;

        foreach (ColouredCube cube in Cubes) {
            newCubeOrder[cube.Position] = cube;

            row = cube.Position / 3;
            column = cube.Position % 3;
            _moduleSelectable.Children[4 * (row + 1) + column] = cube.GetComponent<KMSelectable>();
        }

        _moduleSelectable.UpdateChildren();
        Cubes = newCubeOrder;
    }

    IEnumerator StageThreeAnimation() {
        Screen.EnableOverrideText("...");
        _allowButtonInteraction = false;
        _allowCubeInteraction = false;

        DeselectCubes();
        ColouredCube.AssignSetValues(Cubes, _stages[2].AllValues, _stages[2].TruePositions);
        do { yield return null; } while (ColouredCube.AreBusy(Cubes));

        StageLight.SetColours(StageLights, _stages[2].StageLightColours);
        Screen.DefaultText = "Stage 3";
        Screen.DisableOverrideText();
        _allowButtonInteraction = true;

        if (_internalStage == 2) {
            DoStageThreeLogging();
            _internalStage = 3;
        }

        _allowCubeInteraction = true;
    }

    void DoStageThreeLogging() {
        int[] correctPositions = _stages[2].CorrectPositions;

        Debug.LogFormat("[Coloured Cubes #{0}] -=-==-=-", ModuleId);
        Debug.LogFormat("[Coloured Cubes #{0}] Stage 3:", ModuleId);

        for (int i = 0; i < 9; i++) {
            Debug.LogFormat("[Coloured Cubes #{0}] {1} is a {2} {3} cube. Its true position is {4}. Its actual values are {5}.", ModuleId, (Position)i, (Size)Cubes[i].Size, Cubes[i].ColourName.ToLower(), (Position)_stages[2].TruePositions[i], _stages[2].AllValues[i]);
        }

        Debug.LogFormat("[Coloured Cubes #{0}] The hidden set value is {1}", ModuleId, _stageThreeHiddenValue);
        Debug.LogFormat("[Coloured Cubes #{0}] {1} and {2} form a set with this value!", ModuleId, (Position)correctPositions[0], (Position)correctPositions[1]);
    }

    private class StageInfo {
        private readonly SetValue[] _allValues;
        private readonly int[] _correctPositions;
        private readonly int[] _truePositions;
        private readonly Color[] _stageLightColours;

        public SetValue[] AllValues { get { return _allValues; } }
        public int[] CorrectPositions { get { return _correctPositions; } }
        public int[] TruePositions { get { return _truePositions; } }
        public Color[] StageLightColours { get { return _stageLightColours; } }

        public StageInfo(SetValue[] allValues, int[] correctPositions, int[] truePositions, Color[] stageLightColours = null) {
            if (allValues.Length != 9) { throw new ArgumentException("A stage needs exactly 9 set values."); }
            if (correctPositions.Length != 2 && correctPositions.Length != 3) { throw new ArgumentException("A stage needs exactly 2 or 3 correct set values."); }
            if (stageLightColours != null && stageLightColours.Length != 3) { throw new ArgumentException("A stage needs exactly 3 stage light colours."); }

            _allValues = allValues;
            _correctPositions = correctPositions;
            _truePositions = truePositions;
            _stageLightColours = stageLightColours;

            if (_stageLightColours == null) { _stageLightColours = new Color[] { Color.black, Color.black, Color.black }; }
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command) {
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve() {
        yield return null;
    }
}
