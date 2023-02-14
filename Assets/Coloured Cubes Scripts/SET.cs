using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class Set : MonoBehaviour {
    private List<SetValue> _mostRecentCorrectValues;
    private List<SetValue> _allowedValues;
    private int[] _mostRecentCorrectPositions;

    public int[] MostRecentCorrectPositions { get { return _mostRecentCorrectPositions; } }

    public SetValue[] GenerateSETValuesWithOneSet(int parameterCount, int setValueCount, SetValue hiddenValue = null) {
        List<SetValue> generatedValues;
        SetValue valueToAdd;
        int correctValueCount = 0;

        if (parameterCount < 1) { throw new ArgumentException("Cannot generate SET values with fewer than 1 parameter."); }
        if (setValueCount < 3) { throw new ArgumentException("Cannot generate a match with fewer than 3 SETs."); }

        generatedValues = new List<SetValue>();
        _allowedValues = GetAllPossibleSETValues(parameterCount);

        if (hiddenValue != null && !_allowedValues.Contains(hiddenValue)) { throw new ArgumentException("Tried to force an invalid hidden value."); }

        GenerateCorrectValuesAndPositions(hiddenValue, setValueCount);

        for (int i = 0; i < 9; i++) {
            if (_mostRecentCorrectPositions.Contains(i)) {
                valueToAdd = _mostRecentCorrectValues[correctValueCount];
                correctValueCount++;
            } else {
                valueToAdd = _allowedValues[Rnd.Range(0, _allowedValues.Count())];
            }

            RemoveExcessSets(valueToAdd, generatedValues);
            generatedValues.Add(valueToAdd);
            _allowedValues.Remove(valueToAdd);
        }

        return generatedValues.ToArray();
    }

    private List<SetValue> GetAllPossibleSETValues(int parameterCount) {
        var possibleValues = new List<SetValue>() { new SetValue(0), new SetValue(1), new SetValue(2) };

        return AddParameters(possibleValues, parameterCount - 1);
    }

    private List<SetValue> AddParameters(List<SetValue> valuesSoFar, int numOfParameters) {
        var newValues = new List<SetValue>();

        if (numOfParameters == 0) { return valuesSoFar; }

        foreach (SetValue value in valuesSoFar) {
            for (int i = 0; i < 3; i++) {
                newValues.Add(new SetValue(new List<int>() { i }.Concat(value.Values).ToList()));
            }
        }

        return AddParameters(newValues, numOfParameters - 1);
    }

    private void GenerateCorrectValuesAndPositions(SetValue hiddenValue, int setValueCount) {
        int position;
        _mostRecentCorrectValues = new List<SetValue>();

        if (hiddenValue != null) {
            _mostRecentCorrectValues.Add(hiddenValue);
            _allowedValues.Remove(hiddenValue);
            _mostRecentCorrectPositions = new int[] { Rnd.Range(0, setValueCount), Rnd.Range(0, setValueCount) };
        } else {
            _mostRecentCorrectPositions = new int[] { Rnd.Range(0, setValueCount), Rnd.Range(0, setValueCount), Rnd.Range(0, setValueCount) };
        }

        RemoveDuplicatePositions(setValueCount);

        while (_mostRecentCorrectValues.Count() < 2) {
            position = Rnd.Range(0, _allowedValues.Count());
            _mostRecentCorrectValues.Add(_allowedValues[position]);
            _allowedValues.RemoveAt(position);
        }

        _mostRecentCorrectValues.Add(FindSetWith(_mostRecentCorrectValues[0], _mostRecentCorrectValues[1]));
        _allowedValues.Remove(_mostRecentCorrectValues[2]);

        if (hiddenValue != null) { _mostRecentCorrectValues.RemoveAt(0); }
    }

    void RemoveDuplicatePositions(int setValueCount) {
        for (int i = 1; i < _mostRecentCorrectPositions.Length; i++) {
            while (_mostRecentCorrectPositions.ToList().GetRange(0, i).Contains(_mostRecentCorrectPositions[i])) {
                _mostRecentCorrectPositions[i]++;
                _mostRecentCorrectPositions[i] %= setValueCount;
            }
        }
    }

    public SetValue FindSetWith(SetValue valueOne, SetValue valueTwo) {
        List<int> valueThreeParameters;

        if (valueOne.Values.Length != valueTwo.Values.Length) { throw new RankException("Cannot find a set with values with differing parameter counts."); }

        valueThreeParameters = new List<int>();

        for (int i = 0; i < valueOne.Values.Length; i++) {
            valueThreeParameters.Add((6 - valueOne.Values[i] - valueTwo.Values[i]) % 3);
        }

        return new SetValue(valueThreeParameters);
    }

    private void RemoveExcessSets(SetValue lastAddedValue, List<SetValue> valuesSoFar) {
        foreach (SetValue value in valuesSoFar) {
            _allowedValues.Remove(FindSetWith(lastAddedValue, value));
        }
    }
}

public class SetValue : IEquatable<SetValue> {

    private int[] _values;

    public int[] Values { get { return _values; } }

    // There are three of these because I was stupid when I wrote this (I'm still stupid).
    public SetValue(List<int> values) {
        _values = values.ToArray();
        CheckValidity();
    }

    public SetValue(params int[] values) {
        _values = values;
        CheckValidity();
    }

    public SetValue(string values) {
        _values = values.Select((value) => value - '0').ToArray();
        CheckValidity();
    }

    private void CheckValidity() {
        foreach (int value in _values) {
            if (value > 2 || value < 0) { throw new ArgumentException("Set values must be in the range 0-2."); }
        }
    }

    public override string ToString() {
        string valueAsString = "";

        foreach (int value in _values) {
            valueAsString += value.ToString();
        }

        return valueAsString;
    }

    public bool Equals(SetValue other) {
        if (_values.Length != other.Values.Length) { return false; }

        return !_values.Where((element, index) => element != other.Values[index]).Any();
    }

}