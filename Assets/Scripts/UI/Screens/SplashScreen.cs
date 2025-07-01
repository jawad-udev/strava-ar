using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashScreen : GameMonoBehaviour
{
    private void Start()
    {
        Extensions.PerformActionWithDelay(this, 2f, () =>
        {
            this.Hide(destroy: true);
            Services.GameService.SetState<MenuState>();
        });
    }
}