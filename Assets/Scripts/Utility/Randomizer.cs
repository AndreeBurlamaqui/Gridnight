using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Utility script for randomization related to lists and arrays
/// </summary>
public static class Randomizer
{
    #region ARRAY

    public static T RandomContent<T>(this T[] array)
    {
        return array.RandomContent(array.Length);
    }

    public static T RandomContent<T>(this T[] array, int maxLength)
    {
        if (array == null)
            return default(T);

        return array[Random.Range(0, maxLength)];
    }

    public static T RandomContent<T>(this T[] array, System.Func<T, bool> match)
    {
        T possibleContent = default(T);
        if (array == null)
        {
            return possibleContent;
        }

        var randomOrder = CreateRandomOrder(array.Length, true);
        for (int r = 0; r < randomOrder.Length; r++)
        {
            possibleContent = array[randomOrder[r]];
            if (match(possibleContent))
            {
                break;
            }
        }

        return possibleContent;
    }

    public static List<T> Gacha<T>(this T[] objs, int number, bool onlyUnique, List<float> probabilities = null, List<T> exclude = null)
    {
        if (probabilities == null)
        {
            probabilities = new List<float>();
            foreach (var obj in objs)
            {
                probabilities.Add(1f / objs.Length);
            }
        }

        Debug.Assert(probabilities.Count == objs.Length);

        // get random thresholds
        var thresholds = new List<float>();
        var accumulator = 0f;
        for (var i = 0; i < probabilities.Count; i++)
        {
            accumulator += probabilities[i];
            thresholds.Add(accumulator);
        }

        var outputs = new List<T>();
        // try to find the right objects with a limited number of iterations
        for (var k = 0; k < number * 100; k++)
        {
            var val = Random.Range(0f, accumulator);
            for (var i = 0; i < thresholds.Count; i++)
            {
                if (val < thresholds[i])
                {
                    if (onlyUnique && outputs.Contains(objs[i]))  // check for repeaters
                    {
                        break;
                    }

                    if (exclude != null && exclude.Contains(objs[i]))  // don't use excluded items
                    {
                        break;
                    }
                    outputs.Add(objs[i]);
                    break;
                }
            }

            if (outputs.Count >= number)
            {
                return outputs;
            }
        }

        throw new System.Exception("could not find enough none repeating items");
    }
    public static T[] Shuffle<T>(this T[] objs, bool inPlace = false)
    {
        List<T> newObjs;
        if (inPlace)
        {
            newObjs = objs.ToList();
        }
        else
        {
            newObjs = new List<T>(objs);
        }

        for (int i = 0; i < newObjs.Count; i++)
        {
            var temp = newObjs[i];
            int randomIndex = Random.Range(i, newObjs.Count);
            newObjs[i] = newObjs[randomIndex];
            newObjs[randomIndex] = temp;
        }

        return newObjs.ToArray();
    }
    /// <summary>
    /// Create a random array of int
    /// </summary>
    /// <param name="size">Size of the array</param>
    /// <param name="uniqueList">Values needs to be unique? As in, it'll not have the same number twice</param>
    public static int[] CreateRandomOrder(int size, bool uniqueList)
    {
        int[] newOrder = new int[size];
        int filledIndex = 0;
        int tryCount = 0; // To avoid infinite loops
        int tryLimit = size * 2;
        for (int c = 0; filledIndex < size && tryCount < tryLimit; c++)
        {
            int rng = Random.Range(0, size);

            if (uniqueList)
            { // If we're searching only for unique values
                tryCount++;

                if (newOrder.Contains(rng))
                {
                    // Try again
                    continue;
                }
            }

            //Debug.Log($"Filling index {c} with RNG {rng}");
            newOrder[filledIndex] = rng;
            filledIndex++;
            tryCount = 0; // Reset try count
        }

        return newOrder;
    }
    #endregion

    #region LIST

    public static T RandomContent<T>(this List<T> list)
    {
        if (list == null)
            return default(T);

        return list[Random.Range(0, list.Count)];
    }

    public static List<T> Shuffle<T>(this List<T> objs, bool inPlace = false)
    {
        List<T> newObjs;
        if (inPlace)
        {
            newObjs = objs;
        }
        else
        {
            newObjs = new List<T>(objs);
        }

        for (int i = 0; i < newObjs.Count; i++)
        {
            var temp = newObjs[i];
            int randomIndex = Random.Range(i, newObjs.Count);
            newObjs[i] = newObjs[randomIndex];
            newObjs[randomIndex] = temp;
        }

        return newObjs;
    }

    public static List<T> Gacha<T>(this List<T> objs, int number, List<float> probabilities = null, List<T> exclude = null)
    {
        if (probabilities == null)
        {
            probabilities = new List<float>();
            foreach (var obj in objs)
            {
                probabilities.Add(1f / objs.Count);
            }
        }

        Debug.Assert(probabilities.Count == objs.Count);

        // get random thresholds
        var thresholds = new List<float>();
        var accumulator = 0f;
        for (var i = 0; i < probabilities.Count; i++)
        {
            accumulator += probabilities[i];
            thresholds.Add(accumulator);
        }

        var outputs = new List<T>();
        // try to find the right objects with a limited number of iterations
        for (var k = 0; k < number * 100; k++)
        {
            var val = Random.Range(0f, accumulator);
            for (var i = 0; i < thresholds.Count; i++)
            {
                if (val < thresholds[i])
                {
                    if (outputs.Contains(objs[i]))  // check for repeaters
                    {
                        break;
                    }
                    if (exclude != null && exclude.Contains(objs[i]))  // don't use excluded items
                    {
                        break;
                    }
                    outputs.Add(objs[i]);
                    break;
                }
            }

            if (outputs.Count >= number)
            {
                return outputs;
            }
        }
        throw new System.Exception("could not find enough none repeating items");
    }

    public static T Gacha<T>(List<T> objs, List<float> probabilities = null)
    {
        if (probabilities == null)
        {
            probabilities = new List<float>();
            foreach (var obj in objs)
            {
                probabilities.Add(1f / objs.Count);
            }
        }

        Debug.Assert(probabilities.Count == objs.Count);

        // get random thresholds
        var thresholds = new List<float>();
        var accumulator = 0f;
        for (var i = 0; i < probabilities.Count; i++)
        {
            accumulator += probabilities[i];
            thresholds.Add(accumulator);
        }

        // pick obj
        var val = Random.Range(0f, accumulator);
        for (var i = 0; i < thresholds.Count; i++)
        {
            if (val < thresholds[i])
            {
                return objs[i];
            }
        }

        Debug.LogError("gacha did not find an object, this should not have happened");
        return objs[0];
    }

    #endregion

    #region DICTIONARY

    public static T2 RandomValue<T1, T2>(this Dictionary<T1, T2> dictionary)
    {
        if (dictionary == null)
            return default(T2);

        return dictionary.Values.ElementAt(Random.Range(0, dictionary.Count));
    }

    public static int RandomKey<T1, T2>(this Dictionary<T1, T2> dictionary)
    {
        if (dictionary == null)
            return 0;

        return Random.Range(0, dictionary.Count);
    }
    public static T2 RandomValueExcept<T1, T2>(this Dictionary<T1, T2> dictionary, IEnumerable<T1> exceptionKey)
    {
        if (dictionary == null)
            return default(T2);

        var exceptKeys = dictionary.Keys.Except(exceptionKey);
        return dictionary[exceptionKey.ElementAt(Random.Range(0, exceptKeys.Count()))];
    }

    #endregion

    public static bool RandomBool() => Random.value < 0.5f;
}
