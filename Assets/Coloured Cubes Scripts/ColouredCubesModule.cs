using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using System.Net.Configuration;
using NUnit.Framework.Constraints;
using Newtonsoft.Json.Converters;

public class ColouredCubesModule : MonoBehaviour {
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;

    public ColouredCube[] Cubes;
    public StageLight[] StageLights;
    public ScreenButton Screen;
    public Transform CubeGrid;

    public Set SET;
    public Permutations PermGenerator;

    private const int _moveTrackCount = 5;

    private List<Func<IEnumerator>> _stageThreeAnimations = new List<Func<IEnumerator>>();

    private readonly string[] _strikeSounds = new string[] {
        "genius",
        "glossy road",
        "hang on",
        "I'm taking it with a grain of salt",
        "inchresting",
        "incorrect",
        "Negative Beeps",
        "kuro analysis",
        "microwave interactive",
        "oke",
        "plays fault",
        "probably thinking",
        "smarts",
        "so stupid",
        "stupid",
        "sure",
        "sureee",
        "well done",
        "wot",
        "yeah",
        "yep",
        "yepmhm",
        "genius (1)",
        "Gloucester_road",
        "goodfish",
        "grain_of_salt",
        "interesting",
        "liemuh",
        "so_stupid",
        "Sierra_interacts_with_the_oven",
        "sorry",
        "yep_I_was_probably_thinking_that",
        "yepmhm (1)",
        "yo"
    };
    private readonly string[] _solveSounds = new string[] {
        "amazing",
        "awesome",
        "congratulations",
        "fantastic",
        "green",
        "kuro cornetto",
        "kuro noodle",
        "kuro spaghetti",
        "nice",
        "yay long",
        "yay short"
    };
    private readonly string[] _stageThreeSounds = new string[] {
        "nyoom",
        "spin",
        "to the trains",
        "weeee"
    };
    private readonly string[] _twitchCubeCommandList = new string[] {
        "0",
        "1",
        "2",
        "3",
        "4",
        "5",
        "6",
        "7",
        "8"
    };
    private readonly string[] _twitchButtonCommandList = new string[] {
        "S1",
        "S2",
        "S3",
        "SCREEN"
    };
    private readonly string[] _sizeChartColours = new string[] {
        "210",
        "201",
        "021",
        "120",
        "012",
        "102",
        "211",
        "121",
        "112",
        "212",
        "221",
        "122"
    };
    private readonly int[] _sizeChartSizes = new int[] { 0, 1, 0, 1, 2, 1, 0, 1, 0 };

    private static int s_moduleIdCounter = 1;
    private int _moduleId;
    private bool _moduleSolved = false;

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
    private bool _gridMoving = false;

    void Awake() {
        _moduleId = s_moduleIdCounter++;
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

        _stageThreeAnimations.Add(XFlip);
        _stageThreeAnimations.Add(ZFlip);
        _stageThreeAnimations.Add(Spiral);

        Screen.DefaultText = "Start";
        Debug.LogFormat("[Coloured Cubes #{0}] Press the screen the start.", _moduleId);
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
        _stageThreeHiddenValue = SET.FindSetWith(new SetValue(stageOneRed, stageOneGreen, stageOneBlue, stageOneSize), new SetValue(stageTwoRed, stageTwoGreen, stageTwoBlue, stageTwoSize));

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
            Audio.PlaySoundAtTransform("Deselect", cube.GetComponent<Transform>());

            if (_possibleSubmission) { DisableSubmission(); }
        } else if (!_possibleSubmission) {
            cube.SetHighlight(true);
            _selectedPositions.Add(cube.Position);
            Audio.PlaySoundAtTransform("Select", cube.GetComponent<Transform>());

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

        _displayingSizeChart = false;

        if (light.name == "Stage1Light" && _internalStage >= 1 && _displayedStage != 1) {
            StartCoroutine(StageOneAnimation());
        } else if (light.name == "Stage2Light" && _internalStage >= 2) {
            StartCoroutine(StageTwoAnimation());
        } else if (light.name == "Stage3Light" && _internalStage >= 3) {
            StartCoroutine(StageThreeAnimation());
        }
    }


    void ScreenPress() {
        if (!_allowButtonInteraction) { return; }

        if (_possibleSubmission) {
            HandleSubmission();
            return;
        }

        if (_internalStage == 0) {
            StartCoroutine(StageOneAnimation());
        } else if (!_displayingSizeChart) {
            ShowSizeChart();
        } else {
            HideSizeChart();
        }
    }

    void HandleSubmission() {
        if (_selectedPositions.Any((position) => !_stages[_internalStage - 1].CorrectPositions.Contains(position))) {
            Strike();
            return;
        }

        Debug.LogFormat("[Coloured Cubes #{0}] You submitted the correct cubes.", _moduleId);
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
        Debug.LogFormat("[Coloured Cubes #{0}] Strike! You selected {1}, which was incorrect.", _moduleId, selectedCubes);


        Audio.PlaySoundAtTransform(_strikeSounds[Rnd.Range(0, _strikeSounds.Length)], Module.GetComponent<Transform>());
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
        _allowButtonInteraction = false;
        _moduleSolved = true;
        Audio.PlaySoundAtTransform(_solveSounds[Rnd.Range(0, _solveSounds.Length)], Module.GetComponent<Transform>());

        StartCoroutine(SolveAnimation());
        Debug.LogFormat("[Coloured Cubes #{0}] -=-==-=-", _moduleId);
        Debug.LogFormat("[Coloured Cubes #{0}] Module solved!", _moduleId);
        Module.HandlePass();
    }

    IEnumerator SolveAnimation() {
        var solvedSetValues = new SetValue[9];

        for (int i = 0; i < 9; i++) {
            solvedSetValues[i] = new SetValue("0202");
        }

        DeselectCubes();
        ColouredCube.AssignSetValues(Cubes, solvedSetValues);
        do { yield return null; } while (ColouredCube.AreBusy(Cubes));
        ColouredCube.SetHiddenStates(Cubes, true);
    }

    void ShowSizeChart() {
        int colourNumber = Rnd.Range(0, _sizeChartColours.Length);
        var setValues = new SetValue[9];

        for (int i = 0; i < 9; i++) {
            setValues[i] = new SetValue(_sizeChartColours[colourNumber] + _sizeChartSizes[i].ToString());
        }

        StartCoroutine(SizeChartAnimation(setValues));
        _displayingSizeChart = true;
    }

    IEnumerator SizeChartAnimation(SetValue[] setValues) {
        Audio.PlaySoundAtTransform("Move " + Rnd.Range(1, _moveTrackCount).ToString(), Module.GetComponent<Transform>());
        Screen.EnableOverrideText("...");
        _allowButtonInteraction = false;
        _allowCubeInteraction = false;

        DeselectCubes();
        ColouredCube.AssignSetValues(Cubes, setValues);
        do { yield return null; } while (ColouredCube.AreBusy(Cubes));

        StageLight.SetColours(StageLights, _stages[2].StageLightColours);
        Screen.DefaultText = "Size Chart";
        Screen.DisableOverrideText();
        _allowButtonInteraction = true;
        Audio.PlaySoundAtTransform("Move " + Rnd.Range(1, _moveTrackCount).ToString(), Module.GetComponent<Transform>());
    }

    void HideSizeChart() {
        _displayingSizeChart = false;

        if (_displayedStage == 1) {
            StartCoroutine(StageOneAnimation());
        } else if (_displayedStage == 2) {
            StartCoroutine(StageTwoAnimation(showCycles: false));
        } else {
            StartCoroutine(StageThreeAnimation());
        }
    }


    IEnumerator StageOneAnimation() {
        Audio.PlaySoundAtTransform("Move " + Rnd.Range(1, _moveTrackCount).ToString(), Module.GetComponent<Transform>());
        Screen.EnableOverrideText("...");
        _allowButtonInteraction = false;
        _allowCubeInteraction = false;

        StageLight.SetColours(StageLights, _stages[2].StageLightColours); // Stage 3 light colours are all black.

        DeselectCubes();
        ColouredCube.SetHiddenStates(Cubes, false);
        do { yield return null; } while (ColouredCube.AreBusy(Cubes)); // This causes the coroutine to wait until the cubes stop moving.

        ColouredCube.AssignSetValues(Cubes, _stages[0].AllValues, _stages[0].TruePositions);
        do { yield return null; } while (ColouredCube.AreBusy(Cubes));

        StageLight.SetColours(StageLights, _stages[0].StageLightColours);
        Screen.DefaultText = "Stage 1";
        _displayedStage = 1;
        Screen.DisableOverrideText();
        _allowButtonInteraction = true;
        Audio.PlaySoundAtTransform("Move " + Rnd.Range(1, _moveTrackCount).ToString(), Module.GetComponent<Transform>());

        if (_internalStage == 0) {
            DoStageOneLogging();
            _internalStage = 1;
        }

        if (_internalStage == 1) { _allowCubeInteraction = true; }
    }

    void DoStageOneLogging() {
        int[] correctPositions = _stages[0].CorrectPositions;

        Debug.LogFormat("[Coloured Cubes #{0}] Set values are in red-green-blue-size order.", _moduleId);
        Debug.LogFormat("[Coloured Cubes #{0}] Stage light colours are in 0-1-2 order.", _moduleId);
        Debug.LogFormat("[Coloured Cubes #{0}] -=-==-=-", _moduleId);
        Debug.LogFormat("[Coloured Cubes #{0}] Stage 1:", _moduleId);
        Debug.LogFormat("[Coloured Cubes #{0}] The stage lights display {1}, {2}, and {3}.", _moduleId, StageLights[0].ColourName.ToLower(), StageLights[1].ColourName.ToLower(), StageLights[2].ColourName.ToLower());

        for (int i = 0; i < 9; i++) {
            Debug.LogFormat("[Coloured Cubes #{0}] {1} is a {2} {3} cube. Its actual values are {4}.", _moduleId, (Position)i, (Size)Cubes[i].Size, Cubes[i].ColourName.ToLower(), _stages[0].AllValues[i]);
        }

        Debug.LogFormat("[Coloured Cubes #{0}] {1}, {2}, and {3} form a set!", _moduleId, (Position)correctPositions[0], (Position)correctPositions[1], (Position)correctPositions[2]);
    }

    IEnumerator StageTwoAnimation(bool showCycles = true) {
        Audio.PlaySoundAtTransform("Move " + Rnd.Range(1, _moveTrackCount).ToString(), Module.GetComponent<Transform>());
        Screen.EnableOverrideText("...");
        _allowButtonInteraction = false;
        _allowCubeInteraction = false;

        StageLight.SetColours(StageLights, _stages[2].StageLightColours);
        DeselectCubes();

        if (showCycles) {
            ColouredCube.ShrinkAndMakeWhite(Cubes); 
            do { yield return null; } while (ColouredCube.AreBusy(Cubes));

            foreach (Cycle cycle in _stageTwoCycles) {
                // Reveals cubes that are in the cycle and hides the rest.
                ColouredCube.SetHiddenStates(Cubes, Cubes.Select((cube, index) => !cycle.Elements.Contains(index)).ToArray());
                Audio.PlaySoundAtTransform("Move " + Rnd.Range(1, _moveTrackCount).ToString(), Module.GetComponent<Transform>());
                do { yield return null; } while (ColouredCube.AreBusy(Cubes));

                foreach (int position in cycle.Elements) {
                    Cubes[position].SetPosition(cycle.Permute(position));
                }
                do { yield return null; } while (ColouredCube.AreBusy(Cubes));

                ReorderCubes();
            }

            Audio.PlaySoundAtTransform("Move " + Rnd.Range(1, _moveTrackCount).ToString(), Module.GetComponent<Transform>());
            ColouredCube.SetHiddenStates(Cubes, false);
            do { yield return null; } while (ColouredCube.AreBusy(Cubes));
        }

        ColouredCube.AssignSetValues(Cubes, _stages[1].AllValues, _stages[1].TruePositions);
        do { yield return null; } while (ColouredCube.AreBusy(Cubes));

        StageLight.SetColours(StageLights, _stages[1].StageLightColours);
        Screen.DefaultText = "Stage 2";
        _displayedStage = 2;
        Screen.DisableOverrideText();
        _allowButtonInteraction = true;
        Audio.PlaySoundAtTransform("Move " + Rnd.Range(1, _moveTrackCount).ToString(), Module.GetComponent<Transform>());

        if (_internalStage == 1) {
            DoStageTwoLogging();
            _internalStage = 2;
        }

        if (_internalStage == 2) { _allowCubeInteraction = true; }
    }

    void DoStageTwoLogging() {
        int[] correctPositions = _stages[1].CorrectPositions;

        Debug.LogFormat("[Coloured Cubes #{0}] -=-==-=-", _moduleId);
        Debug.LogFormat("[Coloured Cubes #{0}] Stage 2:", _moduleId);
        Debug.LogFormat("[Coloured Cubes #{0}] The stage lights display {1}, {2}, and {3}.", _moduleId, StageLights[0].ColourName.ToLower(), StageLights[1].ColourName.ToLower(), StageLights[2].ColourName.ToLower());
        Debug.LogFormat("[Coloured Cubes #{0}] The cycles displayed are:", _moduleId);

        foreach (Cycle cycle in _stageTwoCycles) {
            Debug.LogFormat("[Coloured Cubes #{0}] {1}", _moduleId, cycle.ToString());
        }

        for (int i = 0; i < 9; i++) {
            Debug.LogFormat("[Coloured Cubes #{0}] {1} is a {2} {3} cube. Its original position was {4}. Its actual values are {5}.", _moduleId, (Position)i, (Size)Cubes[i].Size, Cubes[i].ColourName.ToLower(), (Position)_stages[1].TruePositions[i], _stages[1].AllValues[i]);
        }

        Debug.LogFormat("[Coloured Cubes #{0}] {1}, {2}, and {3} form a set!", _moduleId, (Position)correctPositions[0], (Position)correctPositions[1], (Position)correctPositions[2]);
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
        Audio.PlaySoundAtTransform("Move " + Rnd.Range(1, _moveTrackCount).ToString(), Module.GetComponent<Transform>());
        Screen.EnableOverrideText("...");
        _allowButtonInteraction = false;
        _allowCubeInteraction = false;

        StageLight.SetColours(StageLights, _stages[2].StageLightColours);

        DeselectCubes();
        StartCoroutine(_stageThreeAnimations[Rnd.Range(0, _stageThreeAnimations.Count())]());
        Audio.PlaySoundAtTransform(_stageThreeSounds[Rnd.Range(0, _stageThreeSounds.Length)], Module.GetComponent<Transform>());
        _gridMoving = true;

        ColouredCube.AssignSetValues(Cubes, _stages[2].AllValues, _stages[2].TruePositions);
        do { yield return null; } while (ColouredCube.AreBusy(Cubes) || _gridMoving);

        Screen.DefaultText = "Stage 3";
        _displayedStage = 3;
        Screen.DisableOverrideText();
        _allowButtonInteraction = true;
        Audio.PlaySoundAtTransform("Move " + Rnd.Range(1, _moveTrackCount).ToString(), Module.GetComponent<Transform>());

        if (_internalStage == 2) {
            DoStageThreeLogging();
            _internalStage = 3;
        }

        _allowCubeInteraction = true;
    }

    void DoStageThreeLogging() {
        int[] correctPositions = _stages[2].CorrectPositions;

        Debug.LogFormat("[Coloured Cubes #{0}] -=-==-=-", _moduleId);
        Debug.LogFormat("[Coloured Cubes #{0}] Stage 3:", _moduleId);

        for (int i = 0; i < 9; i++) {
            Debug.LogFormat("[Coloured Cubes #{0}] {1} is a {2} {3} cube. Its true position is {4}. Its actual values are {5}.", _moduleId, (Position)i, (Size)Cubes[i].Size, Cubes[i].ColourName.ToLower(), (Position)_stages[2].TruePositions[i], _stages[2].AllValues[i]);
        }

        Debug.LogFormat("[Coloured Cubes #{0}] The hidden set value is {1}", _moduleId, _stageThreeHiddenValue);
        Debug.LogFormat("[Coloured Cubes #{0}] {1} and {2} form a set with this value!", _moduleId, (Position)correctPositions[0], (Position)correctPositions[1]);
    }

    IEnumerator XFlip() {
        float elapsedTime = 0;
        float transitionTime = 1;
        float transitionProgress;
        float oldX = CubeGrid.localPosition.x;
        float oldZ = CubeGrid.localPosition.z;
        int rotationDirection = Rnd.Range(0, 2) * 2 - 1; // Gives 1 or -1.

        yield return null;

        while (elapsedTime / transitionTime <= 1) {
            elapsedTime += Time.deltaTime;
            transitionProgress = Mathf.Min(elapsedTime / transitionTime, 1);
            CubeGrid.Rotate(new Vector3(rotationDirection * Time.deltaTime * 360 / transitionTime, 0, 0));
            CubeGrid.localPosition = new Vector3(oldX, 0.1f * Mathf.Sin(transitionProgress * Mathf.PI), oldZ);
            yield return null;
        }

        CubeGrid.localRotation = new Quaternion(0, 0, 0, 0);
        _gridMoving = false;
    }

    IEnumerator ZFlip() {
        float elapsedTime = 0;
        float transitionTime = 1;
        float transitionProgress;
        float oldX = CubeGrid.localPosition.x;
        float oldZ = CubeGrid.localPosition.z;
        int rotationDirection = Rnd.Range(0, 2) * 2 - 1;

        yield return null;

        while (elapsedTime / transitionTime <= 1) {
            elapsedTime += Time.deltaTime;
            transitionProgress = Mathf.Min(elapsedTime / transitionTime, 1);
            CubeGrid.Rotate(new Vector3(0, 0, rotationDirection * Time.deltaTime * 360 / transitionTime));
            CubeGrid.localPosition = new Vector3(oldX, 0.1f * Mathf.Sin(transitionProgress * Mathf.PI), oldZ);
            yield return null;
        }

        CubeGrid.localRotation = new Quaternion(0, 0, 0, 0);
        _gridMoving = false;
    }

    IEnumerator Spiral() {
        float elapsedTime = 0;
        float transitionTime = 1;
        float transitionProgress;
        float oldX = CubeGrid.localPosition.x;
        float oldZ = CubeGrid.localPosition.z;
        float radius;
        float newX;
        float newZ;
        int rotationCount = Rnd.Range(1, 4);
        int rotationDirection = Rnd.Range(0, 2) * 2 - 1;

        yield return null;

        while (elapsedTime / transitionTime <= 1) {
            elapsedTime += Time.deltaTime;
            transitionProgress = Mathf.Min(elapsedTime / transitionTime, 1);
            radius = 0.2f * Mathf.Sin(transitionProgress * Mathf.PI);
            newX = oldX + radius * Mathf.Cos(transitionProgress * Mathf.PI * rotationCount * 2);
            newZ = oldZ + radius * Mathf.Sin(transitionProgress * Mathf.PI * rotationCount * 2) * rotationDirection;
            CubeGrid.localPosition = new Vector3(newX, 0, newZ);
            yield return null;
        }

        CubeGrid.localPosition = new Vector3(oldX, 0, oldZ);
        _gridMoving = false;
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
    private readonly string TwitchHelpMessage = @"Use !{0} 0-8 in reading order to select/deselect cubes. Use !{0} s1/2/3 to press stage lights. " +
                                                    "Use !{0} screen to press the screen button. Chain commands together with spaces.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command) {
        string[] commandList;

        command = command.Trim().ToUpper();
        commandList = command.Split(' ');
        yield return null;

        if (commandList.Any(c => !_twitchCubeCommandList.Contains(c) && !_twitchButtonCommandList.Contains(c))) {
            yield return "sendtochaterror Invalid command!";
            yield break;
        }

        foreach (string instruction in commandList) {
            if (!_allowButtonInteraction) {
                yield return "sendtochaterror Cannot execute command '" + instruction + "' as module is busy. Stopping execution.";
                yield break;
            }

            if (!_allowCubeInteraction && _twitchCubeCommandList.Contains(instruction)) {
                yield return "sendtochaterror Cannot select cube " + instruction + " as cubes are not currently selectable. " +
                                "Make sure the module is displaying the *current* stage. Stopping execution.";
                yield break;
            }

            if (_twitchCubeCommandList.Contains(instruction)) {
                Cubes[int.Parse(instruction)].GetComponent<KMSelectable>().OnInteract();
            } else if (instruction == "SCREEN") {
                Screen.GetComponent<KMSelectable>().OnInteract();
            } else {
                StageLights[instruction[1] - '1'].GetComponent<KMSelectable>().OnInteract();
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator TwitchHandleForcedSolve() {
        bool done = false;

        yield return null;

        if (_moduleSolved) {
            yield break;
        }

        // Wait for module to be idle.
        do {
            yield return new WaitForSeconds(1f);
        } while (!_allowButtonInteraction);

        if (_internalStage == 0) {
            Screen.GetComponent<KMSelectable>().OnInteract();
        } else if (_internalStage != _displayedStage || _displayingSizeChart) {
            StageLights[_internalStage - 1].GetComponent<KMSelectable>().OnInteract();
        }

        // Using ToArray() to copy the list, as this list is modified on interaction.
        foreach (int i in _selectedPositions.ToArray()) {
            Cubes[i].GetComponent<KMSelectable>().OnInteract();
            yield return new WaitForSeconds(0.1f);
        }

        while (!done) {
            do {
                yield return new WaitForSeconds(1f);
            } while (!_allowCubeInteraction);

            foreach (int i in _stages[_internalStage - 1].CorrectPositions) {
                Cubes[i].GetComponent<KMSelectable>().OnInteract();
                yield return new WaitForSeconds(0.1f);
            }

            if (_internalStage == 3) {
                done = true;
            }
            
            Screen.GetComponent<KMSelectable>().OnInteract();
        }
    }
}
