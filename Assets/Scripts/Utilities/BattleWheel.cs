using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BattleRoulette<T>
{
    private Dictionary<T, float> items = new Dictionary<T, float>();

    public void AddItem(T item, float weight)
    {
        if (items.ContainsKey(item)) items[item] = weight;
        else items.Add(item, weight);
    }

    public void UpdateWeight(T item, float newWeight)
    {
        if (items.ContainsKey(item)) items[item] = newWeight;
    }

    public T Run()
    {
        float totalWeight = items.Values.Sum();
        float randomValue = Random.Range(0, totalWeight);
        float cursor = 0f;

        foreach (var kvp in items)
        {
            cursor += kvp.Value;
            if (cursor >= randomValue) return kvp.Key;
        }
        return items.Keys.Last();
    }
}