using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public static class Extensions
{
	// <summary>
	/// Calls a method (no parameters) with delay.
	/// </summary>
	public static void PerformActionWithDelay (this MonoBehaviour mono, float delay, Action action)
	{
		mono.StartCoroutine (mono.ExecuteDelayedAction (delay, action));
	}

	private static IEnumerator ExecuteDelayedAction (this MonoBehaviour ienum, float delay, Action action)
	{
		yield return new WaitForSeconds (delay);

		action ();
	}

	/// <summary>
	/// Gets a random item from a list.
	/// </summary>
	public static T GetRandom <T>(this IList<T> list)
	{
		int randomIndex = UnityEngine.Random.Range(0, list.Count);
		return list[randomIndex];
	}

	/// <summary>
	/// Duplicates a dictionary with empty values.
	/// </summary>
	public static Dictionary<T,U> DuplicateKeys <T,U>(this Dictionary<T,U> source)
	{
		Dictionary<T,U> copy = new Dictionary<T, U>();

		IEnumerator iter = source.Keys.GetEnumerator();

		while(iter.MoveNext())
		{
			T key = (T) iter.Current;
			copy.Add(key, default(U));
		}

		return copy;
	}

	public static DateTime UnixTimeStampToDateTime( double unixTimeStamp )
	{
		unixTimeStamp = unixTimeStamp / 1000;
		// Unix timestamp is seconds past epoch
		System.DateTime dtDateTime = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
		dtDateTime = dtDateTime.AddSeconds( unixTimeStamp ).ToLocalTime();
		return dtDateTime;
	}

	public static float GetRemainingTime(long eventStartTimeStamp)
	{
		DateTime eventTime = UnixTimeStampToDateTime(eventStartTimeStamp).ToLocalTime();
		TimeSpan remainingTime = eventTime - DateTime.Now;
		return (float)remainingTime.TotalSeconds;
	}

	public static string SecondsToHMS(float seconds)
	{
		TimeSpan time = TimeSpan.FromSeconds(seconds);
		
		string str = string.Format("{0:D2}:{1:D2}:{2:D2}", time.Hours, time.Minutes, time.Seconds);

		return str;
	}

	public static Bounds OrthographicBounds(this Camera camera)
	{
		float screenAspect = (float)Screen.width / (float)Screen.height;
		float cameraHeight = camera.orthographicSize * 2;
		Bounds bounds = new Bounds(
			camera.transform.position,
			new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
		return bounds;
	}
}


//Added UniRX Support for TMPro
namespace UniRx
{
	public static class UnityUIExtensions
	{
		public static IDisposable SubscribeToText(this IObservable<string> source, TextMeshProUGUI text)
		{
			return source.SubscribeWithState(text, (x, t) => t.text = x);
		}

		public static IDisposable SubscribeToText<T>(this IObservable<T> source, TextMeshProUGUI text)
		{
			return source.SubscribeWithState(text, (x, t) => t.text = x.ToString());
		}
	}
}