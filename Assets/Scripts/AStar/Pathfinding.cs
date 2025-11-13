using System;
using System.Collections.Generic;

public class Pathfinding
{
    public static List<T> Astar<T>(T start, Func<T, bool> satisfies,
        Func<T, List<T>> getNeighbors, Func<T, T, float> getCost, Func<T, float> getHeuristic) where T : class
    {
        PriorityQueue<T> frontier = new();
        frontier.Enqueue(start, 0);
        Dictionary<T, T> cameFrom = new();
        Dictionary<T, float> costSoFar = new();
        cameFrom.Add(start, null);
        costSoFar.Add(start, 0);

        while (!frontier.IsEmpty)
        {
            var current = frontier.Dequeue();

            if (satisfies(current))
            {
                List<T> path = new List<T>();
                while (current != null)
                {
                    path.Add(current);
                    current = cameFrom[current];
                }
                path.Reverse();
                return path;
            }
            var neighbors = getNeighbors(current);

            for (int i = 0; i < neighbors.Count; i++)
            {
                var next = neighbors[i];
                var newCost = costSoFar[current] + getCost(current, next);
                if (!cameFrom.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    var priority = newCost + getHeuristic(next);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }
        return null;
    }
    
    public static List<T> AstarPS<T>(T start, Func<T, bool> satisfies, Func<T, List<T>> getNeighbors, 
        Func<T, T, float> getCost, Func<T, float> getHeuristic, Func<T, T, bool> lineOfSight) where T : class
    {
        var path = Astar(start, satisfies, getNeighbors, getCost, getHeuristic);
        int current = 0;
        while (current + 2 < path.Count)
        {
            if (lineOfSight(path[current], path[current + 2]))
                path.RemoveAt(current + 1);
            else
                current++;
        }
        return path;

    }
}
