using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoParentToServices : MonoBehaviour
{
    public int siblingIndexToUse = 99;

    void Start()
    {
        Services.ServicesOrganizer.ParentThisToServices(transform);
        transform.SetSiblingIndex(siblingIndexToUse);
    }
}
