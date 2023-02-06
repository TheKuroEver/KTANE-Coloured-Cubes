using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
public class ColouredCube : MonoBehaviour, IColouredItem
{
    [SerializeField] private GameObject _cube;
    [SerializeField] private Transform _cubeTransform;
    [SerializeField] private MeshRenderer _cubeRenderer;

    private const float _transitionTime = 1;
    private const float _biggestCubeSize = 0.028f;
    private const float _topLeftCubeXValue = -0.056f;
    private const float _topLeftCubeZValue = 0.014f;
    private const float _revealedYValue = 0.013f;
    private const float _distanceBetweenCubes = 0.035f;

    private int _position; // Position in reading order, starting from 0.
    private int _size = 2;
    private string _colourName = "Gray";

    private bool _isHidden = true;
    private bool _isMoving = false;
    private bool _isHiding = false;
    private bool _isChangingColour = false;
    private bool _isChangingSize = false;

    private readonly Dictionary<string, string> TernaryColourValuesToName = new Dictionary<string, string>()
    {
        { "000", "Black" },
        { "001", "Indigo" },
        { "002", "Blue" },
        { "010", "Forest" },
        { "011", "Teal" },
        { "012", "Azure" },
        { "020", "Green" },
        { "021", "Jade" },
        { "022", "Cyan" },
        { "100", "Maroon" },
        { "101", "Plum" },
        { "102", "Violet" },
        { "110", "Olive" },
        { "111", "Gray" },
        { "112", "Maya" },
        { "120", "Lime" },
        { "121", "Mint" },
        { "122", "Aqua" },
        { "200", "Red" },
        { "201", "Rose" },
        { "202", "Magenta" },
        { "210", "Orange" },
        { "211", "Salmon" },
        { "212", "Pink" },
        { "220", "Yellow" },
        { "221", "Cream" },
        { "222", "White" }
    };

    public string ColourName { get { return _colourName; } }
    public bool IsBusy { get { return _isMoving || _isChangingColour || _isHiding || _isChangingSize; } }

    void Start()
    {
        GetPositionFromName();
        // _cubeTransform.localPosition = new Vector3(_topLeftCubeXValue + _position[1] * _distanceBetweenCubes, _revealedYValue - 0.05f, _topLeftCubeZValue - _position[0] * _distanceBetweenCubes);
        // _cube.SetActive(false);
        Debug.Log(_cubeTransform.name + ":" + (Position)_position);
    }

    private void SetActive(bool state = true)
    {
        _cube.SetActive(state);
    }

    private void GetPositionFromName()
    {
        _position = 3 * "123".IndexOf(_cubeTransform.name[1]) + "ABC".IndexOf(_cubeTransform.name[0]);
    }

    public static bool AreBusy(ColouredCube[] cubes)
    {
        return cubes.Any(cube => cube.IsBusy);
    }

    // Hiding code.
    public static void SetHiddenStates(ColouredCube[] cubes, bool newState, float transitionTime = _transitionTime)
    {
        foreach (ColouredCube cube in cubes)
        {
            cube.SetHiddenState(newState, transitionTime);
        }
    }

    public static void SetHiddenStates(ColouredCube[] cubes, bool[] newStates, float transitionTime = _transitionTime)
    {
        if (cubes.Length != newStates.Length) { throw new RankException("Number of cubes and number of states to set do not match."); }

        for (int i = 0; i < cubes.Length; i++)
        {
            cubes[i].SetHiddenState(newStates[i], transitionTime);
        }
    }

    private void SetHiddenState(bool newState, float transitionTime)
    {
        if (newState == _isHidden) { return; }
        if (_isHiding) { return; }

        _isHiding = true;
        _isHidden = newState;
        SetActive();
        StartCoroutine(HidingAnimation(newState, transitionTime));
    }

    private IEnumerator HidingAnimation(bool makeHidden, float transitionTime)
    {
        float elapsedTime = 0;
        float transitionProgress;
        float newYValue = makeHidden ? _revealedYValue - 0.05f : _revealedYValue;
        float oldYValue = _cubeTransform.localPosition.y;
        float yValueDifference = newYValue - oldYValue;

        yield return null;

        while (elapsedTime <= transitionTime)
        {
            elapsedTime += Time.deltaTime;
            transitionProgress = Mathf.Min(elapsedTime / transitionTime, 1);
            _cubeTransform.localPosition = new Vector3(_cubeTransform.localPosition.x, oldYValue + transitionProgress * yValueDifference, _cubeTransform.localPosition.z);
            yield return null;
        }

        _isHiding = false;
        SetActive(!makeHidden);
    }

    // Colour code.
    private void SetColour(string newColour)
    {
        if (TernaryColourValuesToName[newColour] == _colourName) { return; }
        if (_isChangingColour) { return; }

        _isChangingColour = true;
        _colourName = TernaryColourValuesToName[newColour];
        StartCoroutine(ColourChangeAnimation(newColour));
    }

    private IEnumerator ColourChangeAnimation(string newColour)
    {
        float elapsedTime = 0;
        float transitionProgress;
        float oldRed = _cubeRenderer.material.color.r;
        float oldGreen = _cubeRenderer.material.color.g;
        float oldBlue = _cubeRenderer.material.color.b;
        float redDifference = 0.5f * (newColour[0] - '0') - oldRed;
        float greenDifference = 0.5f * (newColour[1] - '0') - oldGreen;
        float blueDifference = 0.5f * (newColour[2] - '0') - oldBlue;

        yield return null;

        while (elapsedTime / _transitionTime <= 1)
        {
            elapsedTime += Time.deltaTime;
            transitionProgress = Mathf.Min(elapsedTime / _transitionTime, 1);
            _cubeRenderer.material.color = new Color(oldRed + transitionProgress * redDifference, oldGreen + transitionProgress * greenDifference, oldBlue + transitionProgress * blueDifference);
            yield return null;
        }

        _isChangingColour = false;
    }

    // Size code.
    private void SetSize(int newSize)
    {
        if (_size == newSize) { return; }
        if (_isChangingSize) { return; }

        _isChangingSize = true;
        _size = newSize;
        StartCoroutine(SizeChangeAnimation(newSize));
    }

    private IEnumerator SizeChangeAnimation(int newSize)
    {
        float elapsedTime = 0;
        float transitionProgress;
        float oldSize = _cubeTransform.localScale.x;
        float sizeDifference = (2 + newSize) / 4f * _biggestCubeSize - oldSize;
        float currentSize;

        yield return null;

        while (elapsedTime / _transitionTime <= 1)
        {
            elapsedTime += Time.deltaTime;
            transitionProgress = Mathf.Min(elapsedTime / _transitionTime, 1);
            currentSize = oldSize + transitionProgress * sizeDifference;
            _cubeTransform.localScale = new Vector3(currentSize, currentSize, currentSize);
            yield return null;
        }

        _isChangingSize = false;
    }

    // Position code.
    private void SetPosition(int newPosition)
    {
        if (_position == newPosition) { return; }
        if (_isMoving) { return; }

        _isMoving = true;
        _position = newPosition;
        StartCoroutine(MoveAnimation(GetRowColumn(newPosition)));
    }

    private int[] GetRowColumn(int positionNumber)
    {
        int row = positionNumber / 3;
        int column = positionNumber % 3;

        return new int[] { row, column };
    }

    private IEnumerator MoveAnimation(int[] newPosition)
    {
        float elapsedTime = 0;
        float transitionProgress;
        float oldX = _cubeTransform.localPosition.x;
        float oldZ = _cubeTransform.localPosition.z;
        float xDifference = _topLeftCubeXValue + newPosition[1] * _distanceBetweenCubes - oldX;
        float zDifference = _topLeftCubeZValue - newPosition[0] * _distanceBetweenCubes - oldZ; // Minus since positive Z is up, not down.

        yield return null;

        while (elapsedTime / _transitionTime <= 1)
        {
            elapsedTime += Time.deltaTime;
            transitionProgress = Mathf.Min(elapsedTime / _transitionTime, 1);
            _cubeTransform.localPosition = new Vector3(oldX + transitionProgress * xDifference, _cubeTransform.localPosition.y, oldZ + transitionProgress * zDifference);
            yield return null;
        }

        _isMoving = false;
    }
}

public enum Position
{ 
    A1,
    B1,
    C1,
    A2,
    B2,
    C2,
    A3,
    B3,
    C3
}