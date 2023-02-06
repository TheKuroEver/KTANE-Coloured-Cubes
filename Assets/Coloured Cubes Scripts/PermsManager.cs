using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class PermsManager : MonoBehaviour
{
    
}

public class Cycle
{
    private int[] _elements;

    public Cycle(int[] elements)
    {
        if (elements.Distinct() != elements) { throw new ArgumentException("A cycle cannot include duplicates."); }

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
}
