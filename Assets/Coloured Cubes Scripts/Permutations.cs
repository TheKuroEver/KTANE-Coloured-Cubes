using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class Permutations : MonoBehaviour
{
    private int[] _permutation;
    private Cycle[] _disjointCycles;

    public int[] Permutation { get { return _permutation; } }
    public Cycle[] DisjointCycles { get { return _disjointCycles; } }

    public void GeneratePermutation(int numOfElements, int maxCycleLength)
    {
        List<int> unusedElements;
        int position;

        if (numOfElements < 1) { throw new ArgumentException("Cannot generate a permutation with a non-positive number of elements."); }

        _permutation = new int[numOfElements];
        unusedElements = Enumerable.Range(0, _permutation.Length).ToList(); // I don't know how else to do this concisely.

        for (int i = 0; i < numOfElements; i++)
        {
            position = Rnd.Range(0, unusedElements.Count());
            _permutation[i] = unusedElements[position];
            unusedElements.RemoveAt(position);
        }

        _disjointCycles = GetDisjointCycles(maxCycleLength);
    }

    private Cycle[] GetDisjointCycles(int maxCycleLength)
    { 
        List<int> newCycle;
        var visitedElements = new List<int>();
        var disjointCycles = new List<Cycle>();
        int lastAddedElement;

        for (int i = 0; i < _permutation.Length; i++)
        {
            if (visitedElements.Contains(i) || _permutation[i] == i) { continue; }

            newCycle = new List<int>() { i };

            do
            {
                lastAddedElement = newCycle[newCycle.Count() - 1];
                visitedElements.Add(lastAddedElement);
                newCycle.Add(_permutation[lastAddedElement]);
            }
            while (!visitedElements.Contains(_permutation[lastAddedElement]));

            newCycle.RemoveAt(0); // Removes the one of the duplicated endpoints.
            disjointCycles.Add(new Cycle(newCycle.ToArray()));
        }

        return disjointCycles.ToArray();
    }
}

public class Cycle
{
    private int[] _elements;

    public Cycle(int[] elements)
    {
        if (elements.Distinct().Count() != elements.Length) { throw new ArgumentException("A cycle cannot include duplicates."); }

        _elements = elements;
    }

    public int Permute(int element)
    {
        int index;

        // Typically, trying to cycle an element that is not in the cycle returns back the element. However, in this context that
        // would mean that something has gone wrong elsewhere, so we throw an exception.
        if (!_elements.Contains(element)) { throw new KeyNotFoundException("Tried to permute an element that is not contained in the cycle."); }

        index = (Array.IndexOf(_elements, element) + 1) % _elements.Length;
        return _elements[index];
    }

    public override string ToString()
    {
        string cycleAsString;

        if (_elements.Length > 9) { return BigCycleToString(); }

        cycleAsString = "( ";

        foreach (int element in _elements)
        {
            cycleAsString += ((Position)element).ToString() + " -> ";
        }

        return cycleAsString + ((Position)_elements[0]).ToString() + " )";
    }

    private string BigCycleToString()
    {
        string cycleAsString = "( ";

        foreach (int element in _elements)
        {
            cycleAsString += element.ToString() + " -> ";
        }

        return cycleAsString + _elements[0].ToString() + " )";
    }
}