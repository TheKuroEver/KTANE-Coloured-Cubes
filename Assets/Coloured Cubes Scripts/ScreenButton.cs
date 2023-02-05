using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class ScreenButton : MonoBehaviour
{
	[SerializeField] private TextMesh _screenText;

	private string _defaultText;
	private string _colourText;
	private string _overrideText;

	private bool _showColour;
	private bool _showOverride;

	public string DefaultText { set { _defaultText = value; UpdateText(); } }

	private void UpdateText()
    {
		if (_showOverride) { _screenText.text = _overrideText; }
		else if (_showColour) { _screenText.text = _colourText; }
		else { _screenText.text = _defaultText; }
    }

	public void DisplayColourName(string colourName)
    {
		_showColour = true;
		_colourText = colourName;
		UpdateText();
    }

	public void StopDisplayingColourName()
    {
		_showColour = false;
		UpdateText();
    }

	public void EnableOverrideText(string text)
    {
		_showOverride = true;
		_overrideText = text;
		UpdateText();
    }

	public void DisableOverrideText()
    {
		_showOverride = false;
		UpdateText();
    }
}
