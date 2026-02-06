using System;
using System.Collections.Generic;
using UnityEngine;
using KingdomMod.Shared.Attributes;

#if IL2CPP
using Il2CppInterop.Runtime.Attributes;
#endif

namespace KingdomMod.OverlayMap.Gui.TopMap;

/// <summary>
/// 墙体连接线控制器。
/// 负责管理墙体标记的列表、排序和连接线渲染。
/// </summary>
public class WallLineController
{
    private readonly TopMapView _view;

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public LinkedList<MapMarker> LeftWalls { get; } = new();

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public LinkedList<MapMarker> RightWalls { get; } = new();

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public List<WallLine> WallLines { get; } = new();

    public WallLineController(TopMapView view)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
    }

    /// <summary>
    /// 将墙体 marker 添加到 LeftWalls 或 RightWalls 列表，并创建连接线
    /// </summary>
#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public void AddWallToList(MapMarker wallMarker)
    {
        if (wallMarker?.Data?.Target == null)
        {
            OverlayMapHolder.LogError("AddWallToList: wallMarker or its data is null");
            return;
        }

        // 判断墙在城堡的左侧还是右侧
        var kingdom = Managers.Inst.kingdom;
        var worldPosX = wallMarker.Data.Target.transform.position.x;
        var isLeftSide = kingdom.GetBorderSideForPosition(worldPosX) == Side.Left;
        var wallList = isLeftSide ? LeftWalls : RightWalls;

        // 根据 worldPosX 的绝对值插入到有序列表中
        var absX = Math.Abs(worldPosX);
        LinkedListNode<MapMarker> insertNode = null;
        var current = wallList.First;

        while (current != null)
        {
            var currentAbsX = Math.Abs(current.Value.Data.Target.transform.position.x);
            if (absX < currentAbsX)
            {
                insertNode = current;
                break;
            }
            current = current.Next;
        }

        // 插入到列表
        LinkedListNode<MapMarker> wallNode;
        if (insertNode != null)
            wallNode = wallList.AddBefore(insertNode, wallMarker);
        else
            wallNode = wallList.AddLast(wallMarker);

        // 创建从前一个墙（或城堡）到当前墙的连接线
        CreateWallLineForNode(wallNode);

        // 如果当前墙后面还有墙，需要更新下一个墙的连接线（指向当前墙而不是前一个）
        if (wallNode.Next != null)
        {
            UpdateWallLineForNode(wallNode.Next);
        }
    }

    /// <summary>
    /// 从列表中移除墙体 marker，并清理相关连接线
    /// </summary>
#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public void RemoveWallFromList(MapMarker wallMarker)
    {
        if (wallMarker == null)
            return;

        // 先确定墙体在哪个列表中
        bool isInLeftWalls = LeftWalls.Contains(wallMarker);
        bool isInRightWalls = RightWalls.Contains(wallMarker);

        if (!isInLeftWalls && !isInRightWalls)
        {
            OverlayMapHolder.LogWarning($"RemoveWallFromList: wallMarker not found in LeftWalls or RightWalls");
            return;
        }

        var wallList = isInLeftWalls ? LeftWalls : RightWalls;

        // 重要：在移除之前找到下一个节点
        LinkedListNode<MapMarker> nextNode = null;
        var currentNode = wallList.First;
        while (currentNode != null)
        {
            if (currentNode.Value == wallMarker)
            {
                nextNode = currentNode.Next;
                break;
            }
            currentNode = currentNode.Next;
        }

        // 从列表中移除
        wallList.Remove(wallMarker);

        // 更新下一个墙的连接线（如果存在）
        if (nextNode != null)
        {
            UpdateWallLineForNode(nextNode);
        }
    }

    /// <summary>
    /// 为指定节点创建连接线
    /// </summary>
#if IL2CPP
    [HideFromIl2Cpp]
#endif
    private void CreateWallLineForNode(LinkedListNode<MapMarker> wallNode)
    {
        if (wallNode?.Value == null)
            return;

        var currentWall = wallNode.Value;

        // 获取连接目标（前一个墙或城堡）
        MapMarker targetMarker;
        if (wallNode.Previous?.Value != null)
            targetMarker = wallNode.Previous.Value;
        else
            targetMarker = _view.CastleMarker; // 第一个墙连接到城堡

        if (targetMarker == null)
        {
            OverlayMapHolder.LogError("CreateWallLineForNode: target marker (castle or previous wall) is null");
            return;
        }

        // 获取线条颜色（与当前墙的颜色一致）
        Color lineColor = currentWall.Data.Color != null
            ? (Color)currentWall.Data.Color
            : Color.green;

        // 创建 WallLine 组件
        var wallLine = WallLine.Create(_view, currentWall, targetMarker, lineColor);

        // 添加到 WallLines 列表
        WallLines.Add(wallLine);

        OverlayMapHolder.LogDebug($"Created WallLine for wall at {currentWall.Data.Target.transform.position.x}, connecting to {targetMarker.Data.Target.transform.position.x}, color: {lineColor}");
    }

    /// <summary>
    /// 更新指定节点的连接线（删除旧线条并创建新线条）
    /// </summary>
#if IL2CPP
    [HideFromIl2Cpp]
#endif
    private void UpdateWallLineForNode(LinkedListNode<MapMarker> wallNode)
    {
        if (wallNode?.Value == null)
            return;

        var wallMarker = wallNode.Value;

        // 从 WallLines 列表中查找并销毁旧的 WallLine
        for (int i = WallLines.Count - 1; i >= 0; i--)
        {
            var wallLine = WallLines[i];
            if (wallLine != null && wallLine.OwnerMarker == wallMarker)
            {
                WallLines.RemoveAt(i);
                UnityEngine.Object.Destroy(wallLine.gameObject);
                break; // 每个墙体只有一条自己的 WallLine
            }
        }

        // 创建新的 WallLine
        CreateWallLineForNode(wallNode);
    }

    /// <summary>
    /// 清空墙体列表和所有 WallLine（游戏退出时调用）
    /// </summary>
    public void ClearWallNodes()
    {
        // 销毁所有 WallLine
        foreach (var wallLine in WallLines)
        {
            if (wallLine != null)
            {
                UnityEngine.Object.Destroy(wallLine.gameObject);
            }
        }
        WallLines.Clear();

        LeftWalls.Clear();
        RightWalls.Clear();
    }
}
