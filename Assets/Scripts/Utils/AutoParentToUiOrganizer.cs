using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class AutoParentToUiOrganizer : MonoBehaviour
{
    public int siblingIndexToUse = 0;
    public UIType type;

    [Inject]
    void Init(UiOrganizer uiOrganizer)
    {
        uiOrganizer.ParentThisToCanvas(transform, type);

        transform.SetSiblingIndex(siblingIndexToUse);
    }
}
