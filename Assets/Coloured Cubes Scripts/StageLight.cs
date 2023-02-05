using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class StageLight : MonoBehaviour
{
	[SerializeField] private MeshRenderer stageLightMaterial;
	private string _colourName = "Black";

	public string ColourName { get { return _colourName; } }

	private readonly Dictionary<string, string> BinaryColourValuesToName = new Dictionary<string, string>()
	{
		{ "000", "Black" },
		{ "001", "Blue" },
		{ "010", "Green" },
		{ "011", "Cyan" },
		{ "100", "Red" },
		{ "101", "Magenta" },
		{ "110", "Green" },
		{ "111", "White" }
	};

	public void SetColour(Color colour)
    {
		stageLightMaterial.material.color = colour;
		_colourName = BinaryColourValuesToName[colour.r.ToString() + colour.g.ToString() + colour.b.ToString()];
    }
}
