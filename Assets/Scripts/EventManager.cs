using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventManager
{
    private static Dictionary<string, Action> eventTable = new();

    public static void Subscribe(string eventName, Action listener)
    {
        if (!eventTable.ContainsKey(eventName))
            eventTable[eventName] = listener;
        else
            eventTable[eventName] += listener;
    }

    public static void Unsubscribe(string eventName, Action listener)
    {
        if (eventTable.ContainsKey(eventName))
            eventTable[eventName] -= listener;
    }

    public static void TriggerEvent(string eventName)
    {
        if (eventTable.TryGetValue(eventName, out Action action))
        {
            action?.Invoke();
        }
    }
}
