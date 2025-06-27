using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace ExtremeRoles.Module;

#nullable enable

public class LruCache<TKey, TValue>
	where TKey : IEquatable<TKey>
	where TValue : UnityEngine.Object
{
	public static void Add(TKey key, TValue value)
	{
		if (instance == null)
		{
			instance = new LruCache<TKey, TValue>(DEFAULT_CAPACITY);
		}
		instance.add(key, value);
	}
	public static bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
	{
		if (instance == null)
		{
			instance = new LruCache<TKey, TValue>(DEFAULT_CAPACITY);
		}
		return instance.tryGetValue(key, out value);
	}


	private static LruCache<TKey, TValue>? instance;
	private const int DEFAULT_CAPACITY = 2048;

	private readonly int capacity;
	private readonly Dictionary<TKey, LinkedListNode<CacheItem>> cacheMap;
	private readonly LinkedList<CacheItem> lruList;
	private readonly ReaderWriterLockSlim @lock = new ReaderWriterLockSlim();

	public LruCache(int capacity)
	{
		this.capacity = capacity;
		cacheMap = new Dictionary<TKey, LinkedListNode<CacheItem>>(this.capacity);
		lruList = new LinkedList<CacheItem>();
	}

	~LruCache()
	{
		@lock.Dispose();
	}

	private bool tryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
	{
		@lock.EnterReadLock();  // 読み取りロック
		try
		{
			if (cacheMap.TryGetValue(key, out var node) &&
				node != null &&
				node.Value.Value != null)
			{
				// 読み取りロックを解除してから、書き込みロックを取得して更新操作を行う
				@lock.ExitReadLock();
				@lock.EnterWriteLock();

				try
				{
					// Accessed node becomes most recently used, so move it to the front of the list
					lruList.Remove(node);
					lruList.AddFirst(node);
				}
				finally
				{
					@lock.ExitWriteLock();
				}

				value = node.Value.Value;
				return true;
			}

			value = default;
			return false;
		}
		finally
		{
			if (@lock.IsReadLockHeld)
			{
				@lock.ExitReadLock();  // 読み取りロック解除
			}
		}
	}

	private void add(TKey key, TValue value)
	{
		@lock.EnterWriteLock();
		try
		{
			if (cacheMap.TryGetValue(key, out var checkNode) &&
				(checkNode == null || checkNode.Value.Value == null))
			{
				cacheMap.Remove(key);
				lruList.RemoveLast();
			}
			else if (cacheMap.ContainsKey(key))
			{
				throw new ArgumentException();
			}
			else if (cacheMap.Count >= capacity)
			{
				// If the cache is full, remove the least recently used item (last in the list)
				var lruNode = lruList.Last;
				if (lruNode != null)
				{
					if (cacheMap.TryGetValue(lruNode.Value.Key, out var node) &&
						node != null &&
						node.Value.Value != null)
					{
						UnityEngine.Object.Destroy(node.Value.Value);
					}
					cacheMap.Remove(lruNode.Value.Key);
					lruList.RemoveLast();
				}
			}

			// Add new item to the cache and mark it as most recently used
			var cacheItem = new CacheItem(key, value);
			var newNode = new LinkedListNode<CacheItem>(cacheItem);
			lruList.AddFirst(newNode);
			Helper.Logging.Debug($"Add CacheItem({typeof(TKey).Name}):{key}");
			cacheMap[key] = newNode;
		}
		finally
		{
			if (@lock.IsWriteLockHeld)
			{
				@lock.ExitWriteLock();
			}
		}
	}

	private sealed record CacheItem(TKey Key, TValue Value);
}
