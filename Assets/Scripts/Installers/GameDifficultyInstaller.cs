using UnityEngine;
using Zenject;


[CreateAssetMenu(fileName = "GameDifficultyInstaller", menuName = "Installers/GameDifficultyInstaller")]
public class GameDifficultyInstaller : ScriptableObjectInstaller<GameDifficultyInstaller>
{
    public DifficultyLevel difficultyLevel;
    public Difficulty normal;
    public Difficulty medium;
    public Difficulty hard;

    public override void InstallBindings()
    {
        switch (difficultyLevel)
        {
            case DifficultyLevel.Normal:
                Container.BindInstance(normal);
                break;
            case DifficultyLevel.Medium:
                Container.BindInstance(medium);
                break;
            default:
                Container.BindInstance(hard);
                break;
        }
    }
}