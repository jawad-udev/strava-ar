using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneService : MonoBehaviour
{
    [SerializeField] private SceneReference gameScene;
    [SerializeField] private SceneReference mainScene;

    public void LoadGameScene()
    {
        SceneManager.LoadSceneAsync(gameScene);
    }

    public void LoadMainScene()
    {
        SceneManager.LoadSceneAsync(mainScene);
    }
}
