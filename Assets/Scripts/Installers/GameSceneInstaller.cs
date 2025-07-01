using UnityEngine;
using Zenject;

public class GameSceneInstaller : MonoInstaller<GameSceneInstaller>
{
    public override void InstallBindings()
    {
        GameSceneElements gameSceneElements = Resources.Load<GameSceneElements>("UI/SO/GameSceneElements");
        Container.Bind<GameSceneElements>().FromInstance(gameSceneElements);

        Container.BindInterfacesAndSelfTo<GameManager>().FromComponentInNewPrefab(gameSceneElements.gameplayManager).AsSingle().NonLazy();
    }
}