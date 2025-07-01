using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonMonobehaviour <T> : MonoBehaviour where T : MonoBehaviour
{
	protected static SingletonMonobehaviour<T> _monoInstance;

	protected T _singletonType;

	protected virtual void Awake()
	{
		if (_monoInstance == null)
		{
			_monoInstance = this;
			_monoInstance._singletonType = this as T;
			DontDestroyOnLoad (gameObject);
		} 
		else
			Destroy (gameObject);
	}

	public static T instance
	{
		get
		{
			if (_monoInstance == null)
				return null;

			return _monoInstance._singletonType;
		}
	}
}
