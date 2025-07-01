using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MainSceneElements", menuName = "ScriptableObjects/MainSceneElements", order = 2)]
public class MainSceneElements : ScriptableObject
{
    [Header("Canvas")]
    public Canvas mainSceneCanvasPrefab;
}