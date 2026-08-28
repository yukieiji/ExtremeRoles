using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using BepInEx.Configuration;
using Moq;
using ExtremeRoles.Module;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module;

[Collection(nameof(MockSetupHelper.SetupLogger))]
public class LruCacheTests
{
    public LruCacheTests()
    {
		MockSetupHelper.SetupLogger();

        var debugModeProperty = typeof(ExtremeRolesPlugin).GetProperty("DebugMode", BindingFlags.Public | BindingFlags.Static);
        if (debugModeProperty != null && debugModeProperty.GetValue(null) == null)
        {
			string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var config = new ConfigFile(tempPath, true);
            var entry = config.Bind("DeBug", "DebugMode", false);
            debugModeProperty.SetValue(null, entry);
        }
    }

    private DummyScriptableObject CreateDummyObject()
    {
        return new DummyScriptableObject();
    }

    private sealed class DummyScriptableObject : ScriptableObject
    {
    }

    private static bool InvokeTryGetValue<TKey, TValue>(LruCache<TKey, TValue> cache, TKey key, out TValue? value)
        where TKey : IEquatable<TKey>
        where TValue : UnityEngine.Object
    {
        var method = typeof(LruCache<TKey, TValue>).GetMethod("tryGetValue", BindingFlags.NonPublic | BindingFlags.Instance);
        var args = new object?[] { key, null };
        var result = (bool)method!.Invoke(cache, args)!;
        value = (TValue?)args[1];
        return result;
    }

    private static void InvokeAdd<TKey, TValue>(LruCache<TKey, TValue> cache, TKey key, TValue value)
        where TKey : IEquatable<TKey>
        where TValue : UnityEngine.Object
    {
        var method = typeof(LruCache<TKey, TValue>).GetMethod("add", BindingFlags.NonPublic | BindingFlags.Instance);
        try
        {
            method!.Invoke(cache, new object?[] { key, value });
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException ?? ex;
        }
    }

    [Fact]
    public void TryGetValue_WhenKeyDoesNotExist_ReturnsFalse()
    {
        var cache = new LruCache<string, DummyScriptableObject>(5);
        bool result = InvokeTryGetValue(cache, "non_existent", out var value);

        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void Add_And_TryGetValue_ReturnsValueSuccessfully()
    {
        var cache = new LruCache<string, DummyScriptableObject>(5);
        var obj = CreateDummyObject();

        InvokeAdd(cache, "key1", obj);

        bool result = InvokeTryGetValue(cache, "key1", out var fetched);

        Assert.True(result);
        Assert.Same(obj, fetched);
    }

    [Fact]
    public void Add_DuplicateKey_ThrowsArgumentException()
    {
        var cache = new LruCache<string, DummyScriptableObject>(5);
        var obj1 = CreateDummyObject();
        var obj2 = CreateDummyObject();

        InvokeAdd(cache, "key1", obj1);

        Assert.Throws<ArgumentException>(() => InvokeAdd(cache, "key1", obj2));
    }

    [Fact]
    public void Add_WhenCapacityReached_EvictsLeastRecentlyUsed()
    {
        var cache = new LruCache<string, DummyScriptableObject>(2);
        var obj1 = CreateDummyObject();
        var obj2 = CreateDummyObject();
        var obj3 = CreateDummyObject();

        InvokeAdd(cache, "key1", obj1);
        InvokeAdd(cache, "key2", obj2);

        // Access key1 so key2 becomes LRU
        InvokeTryGetValue(cache, "key1", out _);

        // Add key3, should evict key2
        InvokeAdd(cache, "key3", obj3);

        Assert.True(InvokeTryGetValue(cache, "key1", out _));
        Assert.False(InvokeTryGetValue(cache, "key2", out _));
        Assert.True(InvokeTryGetValue(cache, "key3", out _));
    }

    [Fact]
    public void Add_WhenCapacityReached_AndLRUNodeValueIsNull_EvictsWithoutDestroying()
    {
        var cache = new LruCache<string, DummyScriptableObject>(2);
        var obj1 = CreateDummyObject();
        var obj2 = CreateDummyObject();
        var obj3 = CreateDummyObject();

        InvokeAdd(cache, "key1", obj1);
        InvokeAdd(cache, "key2", obj2);

        // Access key2 so key1 becomes LRU
        InvokeTryGetValue(cache, "key2", out _);

        // Make key1 node's value null to hit the null check during eviction
        var cacheMapField = typeof(LruCache<string, DummyScriptableObject>).GetField("cacheMap", BindingFlags.NonPublic | BindingFlags.Instance);
        var cacheMap = (System.Collections.IDictionary)cacheMapField!.GetValue(cache)!;
        var node = cacheMap["key1"]!;
        var cacheItemType = node.GetType().GetGenericArguments()[0];
        var newCacheItem = Activator.CreateInstance(cacheItemType, "key1", null);
        node.GetType().GetProperty("Value")!.SetValue(node, newCacheItem);

        // Adding key3 should evict key1 even when its Value is null
        InvokeAdd(cache, "key3", obj3);

        Assert.False(InvokeTryGetValue(cache, "key1", out _));
        Assert.True(InvokeTryGetValue(cache, "key2", out _));
        Assert.True(InvokeTryGetValue(cache, "key3", out _));
    }

    [Fact]
    public void Add_OverwritesKey_WhenExistingNodeValueIsNull()
    {
        var cache = new LruCache<string, DummyScriptableObject>(5);
        var obj1 = CreateDummyObject();
        var obj2 = CreateDummyObject();

        InvokeAdd(cache, "key1", obj1);

        // Force node.Value.Value in cacheMap to be null
        var cacheMapField = typeof(LruCache<string, DummyScriptableObject>).GetField("cacheMap", BindingFlags.NonPublic | BindingFlags.Instance);
        var cacheMap = (System.Collections.IDictionary)cacheMapField!.GetValue(cache)!;
        var node = cacheMap["key1"]!;
        var cacheItemType = node.GetType().GetGenericArguments()[0];
        var newCacheItem = Activator.CreateInstance(cacheItemType, "key1", null);
        node.GetType().GetProperty("Value")!.SetValue(node, newCacheItem);

        // Adding key1 again should remove old node and succeed
        InvokeAdd(cache, "key1", obj2);

        bool result = InvokeTryGetValue(cache, "key1", out var fetched);

        Assert.True(result);
        Assert.Same(obj2, fetched);
    }

    [Fact]
    public void StaticAdd_And_StaticTryGetValue_WorksWithDefaultCapacity()
    {
        var obj = CreateDummyObject();
        string key = "static_key_" + Guid.NewGuid();

        LruCache<string, DummyScriptableObject>.Add(key, obj);

        bool result = LruCache<string, DummyScriptableObject>.TryGetValue(key, out var fetched);

        Assert.True(result);
        Assert.Same(obj, fetched);
    }

    [Fact]
    public void ConcurrentAccess_AddAndTryGetValue_IsThreadSafe()
    {
        var cache = new LruCache<int, DummyScriptableObject>(100);

        Parallel.For(0, 50, i =>
        {
            var obj = CreateDummyObject();
            InvokeAdd(cache, i, obj);
            InvokeTryGetValue(cache, i, out _);
        });

        int count = 0;
        for (int i = 0; i < 50; i++)
        {
            if (InvokeTryGetValue(cache, i, out _))
            {
                count++;
            }
        }

        Assert.Equal(50, count);
    }
}
