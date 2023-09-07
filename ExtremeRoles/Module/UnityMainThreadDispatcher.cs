using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using BepInEx.Unity.IL2CPP.Utils;

using UnityEngine;

using Il2CppInterop.Runtime.Attributes;

#nullable enable

namespace ExtremeRoles.Module;

// from : https://github.com/PimDeWitte/UnityMainThreadDispatcher

/// Author: Pim de Witte (pimdewitte.com) and contributors, https://github.com/PimDeWitte/UnityMainThreadDispatcher
/// <summary>
/// A thread-safe class which holds a queue with actions to execute on the next Update() method. It can be used to make calls to the main thread for
/// things such as UI Manipulation in Unity. It was developed for use in combination with the Firebase Unity plugin, which uses separate threads for event handling
/// </summary>
public sealed class UnityMainThreadDispatcher : MonoBehaviour
{
	private static readonly Queue<Action> _executionQueue = new Queue<Action>();

	public void Update()
	{
		lock (_executionQueue)
		{
			while (_executionQueue.Count > 0)
			{
				_executionQueue.Dequeue().Invoke();
			}
		}
	}

	/// <summary>
	/// Locks the queue and adds the IEnumerator to the queue
	/// </summary>
	/// <param name="action">IEnumerator function that will be executed from the main thread.</param>
	[HideFromIl2Cpp]
	public void Enqueue(IEnumerator action)
	{
		lock (_executionQueue)
		{
			_executionQueue.Enqueue(() => {
				this.StartCoroutine(action);
			});
		}
	}

	/// <summary>
	/// Locks the queue and adds the Action to the queue
	/// </summary>
	/// <param name="action">function that will be executed from the main thread.</param>
	[HideFromIl2Cpp]
	public void Enqueue(Action action)
	{
		this.Enqueue(this.ActionWrapper(action));
	}

	/// <summary>
	/// Locks the queue and adds the Action to the queue, returning a Task which is completed when the action completes
	/// </summary>
	/// <param name="action">function that will be executed from the main thread.</param>
	/// <returns>A Task that can be awaited until the action completes</returns>
	[HideFromIl2Cpp]
	public Task EnqueueAsync(Action action)
	{
		var tcs = new TaskCompletionSource<bool>();

		void WrappedAction()
		{
			try
			{
				action.Invoke();
				tcs.TrySetResult(true);
			}
			catch (Exception ex)
			{
				tcs.TrySetException(ex);
			}
		}

		this.Enqueue(this.ActionWrapper(WrappedAction));
		return tcs.Task;
	}

	[HideFromIl2Cpp]
	IEnumerator ActionWrapper(Action a)
	{
		a.Invoke();
		yield return null;
	}


	private static UnityMainThreadDispatcher? _instance = null;

	public static bool Exists()
	{
		return _instance != null;
	}

	public static UnityMainThreadDispatcher Instance
	{
		get
		{
			if (!Exists())
			{
				throw new Exception(
					"UnityMainThreadDispatcher could not find the UnityMainThreadDispatcher object. Please ensure you have added the MainThreadExecutor Prefab to your scene.");
			}
			return _instance!;
		}
	}

	void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
	}

	void OnDestroy()
	{
		_instance = null;
	}
}
