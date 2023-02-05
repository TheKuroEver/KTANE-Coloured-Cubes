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

    private static List<ColouredCube> _instances = new List<ColouredCube>();

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
        _instances.Add(this);
        GetPositionFromName();
        // _cubeTransform.localPosition = new Vector3(_topLeftCubeXValue + _position[1] * _distanceBetweenCubes, _revealedYValue - 0.05f, _topLeftCubeZValue - _position[0] * _distanceBetweenCubes);

    }

    private void GetPositionFromName()
    {
        _position[0] = "123".IndexOf(_cubeTransform.name[1]);
        _position[1] = "ABC".IndexOf(_cubeTransform.name[0]);
    }

    public static bool AreBusy()
    {
        return _instances.Any(cube => cube.IsBusy);
    }

    public static void SetHiddenStates(ColouredCube[] cubes, bool[] newStates)
    {
        if (cubes.Length != newStates.Length) { throw new RankException("Number of cubes and number of states to set do not match."); }

        for (int i = 0; i < cubes.Length; i++)
        {
            cubes[i].SetHiddenState(newStates[i]);
        }
    }

    private void SetHiddenState(bool newState)
    {
        if (newState == _isHidden) { return; }
    }
}
