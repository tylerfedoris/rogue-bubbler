using System.Collections.Generic;
using System.Linq;

public static class Helpers
{
    public static List<int> GenerateRandomUniqueIndexes(int pickCount, int numElements)
    {
        if (numElements <= 0 || pickCount <= 0)
        {
            return new List<int>();
        }
        
        var validChoices = Enumerable.Range(0, numElements).ToList();
        var randomNumbers = new List<int>();
        
        if (pickCount > validChoices.Count)
        {
            pickCount = validChoices.Count;
        }

        for (var i = 0; i < pickCount; i++)
        {
            int choiceIndex = UnityEngine.Random.Range(0, validChoices.Count);
            randomNumbers.Add(validChoices[choiceIndex]);
            validChoices.RemoveAt(choiceIndex);
        }

        return randomNumbers;
    }
}
