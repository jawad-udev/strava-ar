using System;
using System.Collections.Generic;
using ModestTree;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ProjectInstaller : MonoInstaller<ProjectInstaller>
{
    public override void InstallBindings()
    {
        SignalBusInstaller.Install(Container);

        GameElements gameElements = Resources.Load<GameElements>("UI/SO/GameElements");
        Container.Bind<GameElements>().FromInstance(gameElements);

        //Logic Bindings
        Container.Bind<Services>().FromNewComponentOnNewGameObject().WithGameObjectName("Services").AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<ServicesOrganizer>().AsSingle();

        Container.Bind<PlayerService>().AsSingle().NonLazy();
        Container.Bind<UIService>().AsSingle().NonLazy();
        Container.Bind<VibrationService>().AsSingle().NonLazy();
        Container.Bind<ScoreService>().AsSingle().NonLazy();
        Container.Bind<CameraService>().AsSingle().NonLazy();
        Container.Bind<UserService>().AsSingle().NonLazy();

        Container.BindInterfacesAndSelfTo<GameService>().FromComponentInNewPrefab(gameElements.gameService).AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<AudioService>().FromComponentInNewPrefab(gameElements.audioService).AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<SceneService>().FromComponentInNewPrefab(gameElements.sceneService).AsSingle().NonLazy();
        //Container.BindInterfacesAndSelfTo<UserService>().FromComponentInNewPrefab(gameElements.userService).AsSingle().NonLazy();

        Container.Bind<InputService>().FromNewComponentOnNewGameObject().WithGameObjectName("InputService").AsSingle()
            .OnInstantiated((InjectContext context, InputService x) => { x.gameObject.AddComponent<AutoParentToServices>(); }).NonLazy();

        Container.Bind<BackLogService>().FromNewComponentOnNewGameObject().WithGameObjectName("BackLogService").AsSingle()
            .OnInstantiated((InjectContext context, BackLogService x) => { x.gameObject.AddComponent<AutoParentToServices>(); }).NonLazy();

        Container.Bind<EffectService>().FromNewComponentOnNewGameObject().WithGameObjectName("EffectService").AsSingle()
            .OnInstantiated((InjectContext context, EffectService x) => { x.gameObject.AddComponent<AutoParentToServices>(); }).NonLazy();

        //Debug Console

        //Signals Bindings
        Container.DeclareSignal<ScoreUpdateSignal>();

        //Ui Bindings
        Container.Bind<Canvas>().FromComponentInNewPrefab(gameElements.globalCanvasPrefab).AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<UiOrganizer>().AsSingle();

        //Ui Screens
        Container.BindInterfacesAndSelfTo<SplashScreen>().FromComponentInNewPrefab(gameElements.splashScreen).AsSingle();
        Container.BindInterfacesAndSelfTo<HomeScreen>().FromComponentInNewPrefab(gameElements.homeScreen).AsSingle();
        Container.BindInterfacesAndSelfTo<GamePlayScreen>().FromComponentInNewPrefab(gameElements.gamePlayScreen).AsSingle();
        Container.BindInterfacesAndSelfTo<LoginScreen>().FromComponentInNewPrefab(gameElements.loginScreen).AsSingle();

        //Ui Popups
        Container.BindInterfacesAndSelfTo<CommonPopup>().FromComponentInNewPrefab(gameElements.commonPopup).AsSingle();
        Container.BindInterfacesAndSelfTo<GameFailPopup>().FromComponentInNewPrefab(gameElements.gameFailPopup).AsSingle();
        Container.BindInterfacesAndSelfTo<GameWinPopup>().FromComponentInNewPrefab(gameElements.gameWinPopup).AsSingle();
        Container.BindInterfacesAndSelfTo<GameLosePopup>().FromComponentInNewPrefab(gameElements.gameLosePopup).AsSingle();
        Container.BindInterfacesAndSelfTo<PausePopup>().FromComponentInNewPrefab(gameElements.pausePopup).AsSingle();
        Container.BindInterfacesAndSelfTo<ProfilePopup>().FromComponentInNewPrefab(gameElements.profilePopup).AsSingle();
        Container.BindInterfacesAndSelfTo<SettingsPopup>().FromComponentInNewPrefab(gameElements.settingsPopup).AsSingle();
    }
}