using UnityEngine;

public class ScreenButton : MonoBehaviour {
    private TextMesh _screenText;

    private IColouredItem _buttonToWatch;

    private string _defaultText;
    private string _overrideText;

    private bool _showColour;
    private bool _showOverride;

    public string DefaultText { set { _defaultText = value; UpdateText(); } }

    void Awake() {
        _screenText = GetComponentInChildren<TextMesh>();
    }

    public void UpdateText() {
        if (_showOverride) {
            _screenText.text = _overrideText;
        } else if (_showColour) {
            _screenText.text = _buttonToWatch.ColourName;
        } else {
            _screenText.text = _defaultText;
        }
    }

    public void DisplayColourName(IColouredItem buttonToWatch) {
        _showColour = true;
        _buttonToWatch = buttonToWatch;
        UpdateText();
    }

    public void StopDisplayingColourName() {
        _showColour = false;
        UpdateText();
    }

    public void EnableOverrideText(string text) {
        _showOverride = true;
        _overrideText = text;
        UpdateText();
    }

    public void DisableOverrideText() {
        _showOverride = false;
        UpdateText();
    }
}

public interface IColouredItem {
    string ColourName { get; }
}