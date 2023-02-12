using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class StageLight : MonoBehaviour, IColouredItem {
    [SerializeField] private MeshRenderer _stageLightRenderer;
    [SerializeField] private int _stageNumber;

    private string _colourName = "Black";

    private static readonly Dictionary<string, string> BinaryColourValuesToName = new Dictionary<string, string>()
    {
        { "000", "Black" },
        { "001", "Blue" },
        { "010", "Green" },
        { "011", "Cyan" },
        { "100", "Red" },
        { "101", "Magenta" },
        { "110", "Yellow" },
        { "111", "White" }
    };

    public string ColourName { get { return _colourName; } }
    public int StageNumber { get { return _stageNumber; } }

    void Awake() {
        _stageLightRenderer = GetComponent<MeshRenderer>();
    }

    public static void SetColours(StageLight[] lights, Color[] colours) {
        for (int i = 0; i < 3; i++) {
            lights[i]._stageLightRenderer.material.color = colours[i];
            lights[i]._colourName = BinaryColourValuesToName[colours[i].r.ToString() + colours[i].g.ToString() + colours[i].b.ToString()];
        }
    }
}
