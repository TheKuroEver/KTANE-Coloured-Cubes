using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
public class ColouredCube : MonoBehaviour
{
    [SerializeField] private GameObject _cube;
    [SerializeField] private Transform _cubeTransform;
    [SerializeField] private MeshRenderer _cubeRenderer;

    private const float _biggestCubeSize = 0.028f;
    private const float _topLeftCubeXValue = -0.056f;
    private const float _topLeftCubeZValue = 0.014f;
    private const float _revealedYValue = 0.013f;
    private const float _distanceBetweenCubes = 0.035f;

    private int[] _position = new int[2]; // (row, column), with top left being (0, 0);
    private string _colourName = "Gray";

    private bool _isHidden = true;
    private bool _isMoving = false;
    private bool _isHiding = false;
    private bool _isChangingColour = false;

    public string ColourName { get { return _colourName; } }
    public bool IsBusy { get { return _isMoving || _isChangingColour || _isHiding; } }

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

    void Start()
    {
        GetPositionFromName();
        _cubeTransform.localPosition = new Vector3(_topLeftCubeXValue + _position[1] * _distanceBetweenCubes, _revealedYValue - 0.05f, _topLeftCubeZValue - _position[0] * _distanceBetweenCubes);
        _cube.SetActive(false);
    }

    private void GetPositionFromName()
    {
        _position[0] = "123".IndexOf(_cubeTransform.name[1]);
        _position[1] = "ABC".IndexOf(_cubeTransform.name[0]);
    }

    public static bool AreBusy(ColouredCube[] cubes)
    {
        return cubes.Any(cube => cube.IsBusy);
    }

    public static void SetHiddenStates(ColouredCube[] cubes, bool newState, float transitionTime = 1)
    {
        foreach (ColouredCube cube in cubes)
        {
            cube.SetHiddenState(newState, transitionTime);
        }
    }

    public static void SetHiddenStates(ColouredCube[] cubes, bool[] newStates, float transitionTime = 1)
    {
        if (cubes.Length != newStates.Length) { throw new RankException("Number of cubes and number of states to set do not match."); }

        for (int i = 0; i < cubes.Length; i++)
        {
            cubes[i].SetHiddenState(newStates[i], transitionTime);
        }
    }

    private void SetActive(bool state = true)
    {
        _cube.SetActive(state);
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
}
