using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameSceneElements", menuName = "ScriptableObjects/GameSceneElements", order = 2)]
public class GameSceneElements : ScriptableObject
{
    [Header("Canvas")]
    public Canvas gameSceneCanvasPrefab;

    [Header("Game Play Prefabs")]
    public GameManager gameplayManager;
}