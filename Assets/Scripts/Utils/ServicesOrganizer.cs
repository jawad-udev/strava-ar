using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.Linq; // using LINQ to GameObject
using Zenject;

public interface IServicesOrganizer
{
    void ParentThisToServices(Transform toBeMadeChild);
    Services GetServices();
}

public class ServicesOrganizer : IServicesOrganizer
{
    private readonly Services services;

    public Services GetServices()
    {
        return services;
    }

    public Transform GetMainCanvasRoot()
    {
        return services.transform;
    }

    public void ParentThisToServices(Transform toBeMadeChild)
    {
        toBeMadeChild.SetParent(GetServices().transform, false);
    }

    public ServicesOrganizer(Services services)
    {
        this.services = services;
    }

    public void Initialize()
    {

    }
}
