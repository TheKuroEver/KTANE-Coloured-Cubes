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

	private ColouredItem _buttonToWatch;

	private string _defaultText;
	private string _overrideText;

	private bool _showColour;
	private bool _showOverride;

	public string DefaultText { set { _defaultText = value; UpdateText(); } }

	public void UpdateText()
    {
		if (_showOverride) { _screenText.text = _overrideText; }
		else if (_showColour) { _screenText.text = _buttonToWatch.ColourName; }
		else { _screenText.text = _defaultText; }
    }

	public void DisplayColourName(ColouredItem buttonToWatch)
    {
		_showColour = true;
		_buttonToWatch = buttonToWatch;
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

public interface ColouredItem
{
	string ColourName { get; }
}