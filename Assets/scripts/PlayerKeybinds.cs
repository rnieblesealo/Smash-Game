using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Keybinds", menuName = "Scriptable Objects/PlayerKeybinds")]
public class PlayerKeybinds : ScriptableObject
{
    [Header("Keybinds")]
    public string buttonAxis;
    public KeyCode jump;
    public KeyCode shoot;
    public KeyCode drop;
    public KeyCode reload;
    public KeyCode phaseDown;
}