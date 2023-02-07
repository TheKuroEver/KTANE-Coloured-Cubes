using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class ColouredCubesModule : MonoBehaviour
{
	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMBombModule Module;

	public ColouredCube[] Cubes;
	public StageLight[] StageLights;
	public ScreenButton Screen;

	public Set SET;
	public PermsManager PermsGenerator;

	private static int ModuleIdCounter = 1;
	private int ModuleId;
	private bool ModuleSolved = false;

	private StageInfo[] _stages = new StageInfo[3];
	// private Cycle[] _stageTwoCycles;
	private SetValue _stageThreeHiddenValue;

	private int counter = 0;

	void Awake()
    {
		ModuleId = ModuleIdCounter++;

		AssignInteractionHandlers();
	}

	void AssignInteractionHandlers()
    {
		foreach (ColouredCube cube in Cubes)
		{
			cube.GetComponent<KMSelectable>().OnInteract += delegate () { CubePress(cube); return false; };
			cube.GetComponent<KMSelectable>().OnHighlight += delegate () { Screen.DisplayColourName(cube); };
			cube.GetComponent<KMSelectable>().OnHighlightEnded += delegate () { Screen.StopDisplayingColourName(); };
		}

		foreach (StageLight light in StageLights)
		{
			light.GetComponent<KMSelectable>().OnInteract += delegate () { StageLightPress(light); return false; };
			light.GetComponent<KMSelectable>().OnHighlight += delegate () { Screen.DisplayColourName(light); };
			light.GetComponent<KMSelectable>().OnHighlightEnded += delegate () { Screen.StopDisplayingColourName(); };
		}

		Screen.GetComponent<KMSelectable>().OnInteract += delegate () { ScreenPress(); return false; };
	}

	void Start()
	{
		GenerateStages();
		Debug.LogFormat("[Coloured Cubes #{0}] Press the screen the start.", ModuleId);
	}

	void GenerateStages()
    {
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
		int[] stageTwoPositions = stageOnePositions;
		int[] stageThreePositions = stageOnePositions;

		for (int i = 0; i < 3; i++)
        {
			stageOneColours[i] = new Color(i == stageOneRed ? 1 : 0, i == stageOneGreen ? 1 : 0, i == stageOneBlue ? 1 : 0);
			stageTwoColours[i] = new Color(i == stageTwoRed ? 1 : 0, i == stageTwoGreen ? 1 : 0, i == stageTwoBlue ? 1 : 0);
		}

		_stageThreeHiddenValue = SET.FindSetWith(new SetValue(stageOneRed, stageOneGreen, stageOneBlue, stageTwoSize), new SetValue(stageTwoRed, stageTwoGreen, stageTwoBlue, stageTwoSize));

		_stages[0] = new StageInfo(SET.GenerateSETValuesWithOneSet(4, 9), SET.MostRecentCorrectPositions.ToArray(), stageOnePositions, stageOneColours);
		_stages[1] = new StageInfo(SET.GenerateSETValuesWithOneSet(4, 9), SET.MostRecentCorrectPositions.ToArray(), stageTwoPositions, stageTwoColours);
		_stages[2] = new StageInfo(SET.GenerateSETValuesWithOneSet(4, 9, _stageThreeHiddenValue), SET.MostRecentCorrectPositions.ToArray(), stageThreePositions);

		// _stageTwoCycles = PermsManager.GenerateCycles();
	}

	void CubePress(ColouredCube cube)
    {

    }

	void StageLightPress(StageLight light)
    {

    }

	void ScreenPress()
    {

	}

	void DoStageOneLogging()
    {
		int[] correctPositions = _stages[0].CorrectPositions;

		Debug.LogFormat("[Coloured Cubes #{0}] Set values are in red-green-blue-size order.", ModuleId);
		Debug.LogFormat("[Coloured Cubes #{0}] Stage 1:", ModuleId);

		for (int i = 0; i < 9; i++)
        {
			Debug.LogFormat("[Coloured Cubes #{0}] {1} is a {2} {3} cube. Its actual values are {4}.", ModuleId, (Position)i, (Size)Cubes[i].Size, Cubes[i].ColourName.ToLower(), _stages[0].AllValues[i]);
		}

		Debug.LogFormat("[Coloured Cubes #{0}] {1}, {2}, and {3} form a set!", ModuleId, (Position)correctPositions[0], (Position)correctPositions[1], (Position)correctPositions[2]);
    }

	private class StageInfo
	{ 
		private readonly SetValue[] _allValues;
		private readonly int[] _correctPositions;
		private readonly int[] _truePositions;
		private readonly Color[] _stageLightColours;

		public SetValue[] AllValues { get { return _allValues; } }
		public int[] CorrectPositions { get { return _correctPositions; } }
		public int[] TruePositions { get { return _truePositions; } }
		public Color[] StageLightColours { get { return _stageLightColours; } }

		public StageInfo(SetValue[] allValues, int[] correctPositions, int[] truePositions, Color[] stageLightColours = null)
        {
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
}
