using UnityEngine;
using Zenject;

public class MainSceneInstaller : MonoInstaller<MainSceneInstaller>
{
    public override void InstallBindings()
    {
        MainSceneElements mainSceneElements = Resources.Load<MainSceneElements>("UI/SO/MainSceneElements");
        Container.Bind<MainSceneElements>().FromInstance(mainSceneElements);
    }
}