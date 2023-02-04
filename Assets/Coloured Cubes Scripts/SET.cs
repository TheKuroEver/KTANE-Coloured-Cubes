using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class Set : MonoBehaviour
{
    private List<SetValue> _mostRecentCorrectValues;
    private List<SetValue> _allowedValues;

    public List<SetValue> MostRecentCorrectValues { get { return _mostRecentCorrectValues; } }

    public SetValue[] GenerateSETValuesWithOneSet(int parameterCount, int setValueCount)
    {
        List<SetValue> generatedValues;

        if (parameterCount < 1) { throw new ArgumentException("Cannot generate SET values with fewer than 1 parameter."); }
        if (setValueCount < 3) { throw new ArgumentException("Cannot generate a match with fewer than 3 SETs."); }

        _allowedValues = GetAllPossibleSETValues(parameterCount);
        generatedValues = new List<SetValue>();

        return new SetValue[2];
    }

    private List<SetValue> GetAllPossibleSETValues(int parameterCount)
    {
        var possibleValues = new List<SetValue>() { new SetValue(0), new SetValue(1), new SetValue(2) };

        return AddParameters(possibleValues, parameterCount - 1);
    }
    
    private List<SetValue> AddParameters(List<SetValue> valuesSoFar, int numOfParameters)
    {
        var newValues = new List<SetValue>();

        if (numOfParameters == 0) { return valuesSoFar; }

        foreach (SetValue value in valuesSoFar)
        {
            for (int i = 0; i < 3; i++)
            {
                newValues.Add(new SetValue(new List<int>() { i }.Concat(value.Values).ToList()));
            }
        }

        return AddParameters(newValues, numOfParameters - 1);
    }

    private void GenerateCorrectValues()
    {
        int position;
        _mostRecentCorrectValues = new List<SetValue>();

        while (_mostRecentCorrectValues.Count() < 2)
        {
            position = Rnd.Range(0, _allowedValues.Count());
            _mostRecentCorrectValues.Add(_allowedValues[position]);
            _allowedValues.RemoveAt(position);
        }

        _mostRecentCorrectValues.Add(FindSetWith(_mostRecentCorrectValues[0], _mostRecentCorrectValues[1]));
        _allowedValues.Remove(_mostRecentCorrectValues[2]);

        Debug.Log(_mostRecentCorrectValues[0].ToString() + " " + _mostRecentCorrectValues[1].ToString() + " " + _mostRecentCorrectValues[2].ToString());
    }

    private SetValue FindSetWith(SetValue valueOne, SetValue valueTwo)
    {
        List<int> valueThreeParameters;

        if (valueOne.Values.Length != valueTwo.Values.Length) { throw new RankException("Cannot find a set with values with differing parameter counts."); }

        valueThreeParameters = new List<int>();

        for (int i = 0; i < valueOne.Values.Length; i++)
        {
            valueThreeParameters.Add((6 - valueOne.Values[i] - valueTwo.Values[i]) % 3);
        }

        return new SetValue(valueThreeParameters);
    }

}

public class SetValue
{

    private int[] _values;

    public int[] Values { get { return _values; } }

    public SetValue(List<int> values)
    {
        _values = values.ToArray();
    }

    public SetValue(params int[] values)
    {
        _values = values;
    }

    public override string ToString()
    {
        string valueAsString = "";

        foreach (int value in _values)
        {
            valueAsString += value.ToString();
        }

        return valueAsString;
    }

}