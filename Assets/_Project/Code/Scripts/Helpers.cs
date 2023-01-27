using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public static class Helpers
{
    public static List<int> GenerateRandomUniqueNumberList(int count, int min, int max)
    {
        var validChoices = Enumerable.Range(min, max).ToList();
        var randomNumbers = new List<int>();

        for (var i = 1; i <= count && validChoices.Count > 0; i++)
        {
            int choiceIndex = UnityEngine.Random.Range(0, validChoices.Count);
            randomNumbers.Add(validChoices[choiceIndex]);
            validChoices.RemoveAt(choiceIndex);
        }

        return randomNumbers;
    }
}
