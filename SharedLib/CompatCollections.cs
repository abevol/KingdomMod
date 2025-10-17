using System;
#if IL2CPP
using Il2CppSystem.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
#else
using System.Collections.Generic;
#endif

namespace KingdomMod.SharedLib;

public static class CompatCollections
{
    // ✅ 快速创建 Il2Cpp HashSet
    public static HashSet<T> CreateHashSet<T>(params T[] items)
    {
        var set = new HashSet<T>();
        foreach (var item in items)
            set.Add(item);
        return set;
    }

    // ✅ 快速创建 Il2Cpp List
    public static List<T> CreateList<T>(params T[] items)
    {
        var list = new List<T>();
        foreach (var item in items)
            list.Add(item);
        return list;
    }

    // ✅ 快速创建 Il2Cpp Dictionary
    public static Dictionary<TKey, TValue> CreateDictionary<TKey, TValue>(System.Collections.Generic.IEnumerable<KeyValuePair<TKey, TValue>> pairs)
    {
        var dict = new Dictionary<TKey, TValue>();
        foreach (var pair in pairs)
            dict.Add(pair.Key, pair.Value);
        return dict;
    }

    public static Dictionary<TKey, TValue> CreateDictionary<TKey, TValue>(params (TKey Key, TValue Value)[] pairs)
    {
        var dict = new Dictionary<TKey, TValue>();
        foreach (var (k, v) in pairs)
            dict.Add(k, v);
        return dict;
    }

    // ✅ 从托管 List 转 Il2Cpp List
    public static List<T> ToIl2CppList<T>(this System.Collections.Generic.IEnumerable<T> src)
    {
        var list = new List<T>();
        foreach (var item in src)
            list.Add(item);
        return list;
    }

    // ✅ 从 Il2Cpp List 转托管 List
    public static System.Collections.Generic.List<T> ToManagedList<T>(this List<T> il2CppList)
    {
        var list = new System.Collections.Generic.List<T>();
        foreach (var item in il2CppList)
            list.Add(item);
        return list;
    }

    // ✅ 从 Il2Cpp Dictionary 转托管 Dictionary
    public static System.Collections.Generic.Dictionary<TKey, TValue> ToManagedDictionary<TKey, TValue>(this Dictionary<TKey, TValue> il2CppDict)
    {
        var dict = new System.Collections.Generic.Dictionary<TKey, TValue>();
        foreach (var kvp in il2CppDict)
            dict[kvp.Key] = kvp.Value;
        return dict;
    }

#if IL2CPP
    public static LinkedListNode<T> AddBefore<T>(this LinkedList<T> @this, LinkedListNode<T> node, T value)
    {
        @this.ValidateNode(node);
        LinkedListNode<T> linkedListNode = new LinkedListNode<T>(node.list, value);
        @this.InternalInsertNodeBefore(node, linkedListNode);
        if (node == @this.head)
        {
            @this.head = linkedListNode;
        }

        return linkedListNode;
    }

    public static T[] ToArray<T>(this HashSet<T> source) where T : unmanaged
    {
        var result = new T[source.Count];
        // 创建一个 Il2CppStructArray 包裹托管数组指针
        var il2CppArray = new Il2CppStructArray<T>(result);
        source.CopyTo(il2CppArray);
        return result;
    }

    public static void UnionWith<T>(this HashSet<T> source, System.Collections.Generic.IEnumerable<T> items)
    {
        foreach (var item in items)
            source.Add(item);
    }

#endif
}
