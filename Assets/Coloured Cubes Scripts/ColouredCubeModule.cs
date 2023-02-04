using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class ColouredCubeModule : MonoBehaviour
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

	void Awake()
    {
		ModuleId = ModuleIdCounter++;

		AssignInteractionMethods();
	}

	void Start()
    {
		SET.GenerateSETValuesWithOneSet(4, 9);
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

	void CubePress(ColouredCube cube)
    {

    }

	void StageLightPress(StageLight light)
    {

    }

	void ScreenPress()
    {

    }
}
