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

	void Awake()
    {
		ModuleId = ModuleIdCounter++;

		AssignInteractionMethods();
	}

	void AssignInteractionMethods()
    {
		foreach (ColouredCube cube in Cubes)
		{
			cube.GetComponentInParent<KMSelectable>().OnInteract += delegate () { CubePress(cube); return false; };
			// cube.GetComponentInParent<KMSelectable>().OnHighlight += delegate () { Screen.ShowColour(cube.ColourAsName); };
			// cube.GetComponentInParent<KMSelectable>().OnHighlightEnded += delegate () { Screen.StopShowingColour(); };
		}

		foreach (StageLight light in StageLights)
		{
			light.GetComponentInParent<KMSelectable>().OnInteract += delegate () { StageLightPress(light); return false; };
			// light.GetComponentInParent<KMSelectable>().OnHighlight += delegate () { Screen.ShowColour(light.ColourAsName); };
			// light.GetComponentInParent<KMSelectable>().OnHighlightEnded += delegate () { Screen.StopShowingColour(); };
		}

		Screen.GetComponentInParent<KMSelectable>().OnInteract += delegate () { ScreenPress(); return false; };
	}

	void Start()
	{

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

		for (int i = 0; i < 3; i++)
        {
			stageOneColours[i] = new Color(i == stageOneRed ? 1 : 0, i == stageOneGreen ? 1 : 0, i == stageOneBlue ? 1 : 0);
			stageTwoColours[i] = new Color(i == stageTwoRed ? 1 : 0, i == stageTwoGreen ? 1 : 0, i == stageTwoBlue ? 1 : 0);
		}

		_stageThreeHiddenValue = SET.FindSetWith(new SetValue(stageOneRed, stageOneGreen, stageOneBlue, stageTwoSize), new SetValue(stageTwoRed, stageTwoGreen, stageTwoBlue, stageTwoSize));

		_stages[0] = new StageInfo(SET.GenerateSETValuesWithOneSet(4, 9), SET.MostRecentCorrectValues.ToArray(), stageOneColours);
		_stages[1] = new StageInfo(SET.GenerateSETValuesWithOneSet(4, 9), SET.MostRecentCorrectValues.ToArray(), stageTwoColours);
		_stages[2] = new StageInfo(SET.GenerateSETValuesWithOneSet(4, 9, _stageThreeHiddenValue), SET.MostRecentCorrectValues.ToArray());

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

	private class StageInfo
	{ 
		private readonly SetValue[] _allValues;
		private readonly SetValue[] _correctValues;
		private readonly Color[] _stageLightColours;

		public SetValue[] AllValues { get { return _allValues; } }
		public SetValue[] CorrectValues { get { return _correctValues; } }
		public Color[] StageLightColours { get { return _stageLightColours; } }

		public StageInfo(SetValue[] allValues, SetValue[] correctValues, Color[] stageLightColours = null)
        {
			if (allValues.Length != 9) { throw new ArgumentException("A stage needs exactly 9 set values."); }
			if (correctValues.Length != 3) { throw new ArgumentException("A stage needs exactly 3 correct set values."); }
			if (stageLightColours.Length != 3) { throw new ArgumentException("A stage needs exactly 3 stage light colours."); }

			_allValues = allValues;
			_correctValues = correctValues;
			_stageLightColours = stageLightColours;

			if (_stageLightColours == null) { _stageLightColours = new Color[] { Color.black, Color.black, Color.black }; }
        }
	}
}
