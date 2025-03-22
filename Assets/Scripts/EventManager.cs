using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager 
{
    public static event UnityAction GameStart;

    public static void OnGameStart() => GameStart?.Invoke();

}
