using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameMonoBehaviour
{
    void Show();
    void Hide(bool destroy = false);
    void Hide(BacklogType backlogType, bool destroy = false);
    void Show(params BacklogType[] backlogTypes);
    void Show(BacklogType backlogType = BacklogType.DisablePreviousScreen);
}

public class GameMonoBehaviour : MonoBehaviour, IGameMonoBehaviour
{
    public void Show()
    {
        BacklogType backlogType = BacklogType.DisablePreviousScreen;
        Services.BackLogService.OnScreenOpen(gameObject, backlogType);
        View();
    }

    public void Show(BacklogType backlogType = BacklogType.DisablePreviousScreen)
    {
        Services.BackLogService.OnScreenOpen(gameObject, backlogType);
        View();
    }

    public void Show(params BacklogType[] backlogTypes)
    {
        Services.BackLogService.OnScreenOpen(gameObject, backlogTypes);
        View();
    }

    private void View()
    {
        gameObject.transform.SetAsLastSibling();
        gameObject.SetActive(true);
    }

    public void Hide(bool destroy = false)
    {
        gameObject.SetActive(false);

        if (destroy)
            Destroy(gameObject);
    }

    public void Hide(BacklogType backlogType, bool destroy = false)
    {
        if(backlogType == BacklogType.RemovePreviousScreen)
        {
            Services.BackLogService.RemoveLastScreens(gameObject);
        }
        gameObject.SetActive(false);

        if (destroy)
            Destroy(gameObject);
    }
}
