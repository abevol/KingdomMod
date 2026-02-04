using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

#if IL2CPP
using Il2CppInterop.Runtime.Attributes;
using Il2CppSystem.Collections.Generic;
#endif

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    /// <summary>
    /// 敌人地图标记映射器
    /// 负责将敌人组件转换为地图上的分组标记
    /// 注意：此 Mapper 不通过 ObjectPatcher 触发，而是通过定期刷新 Managers.Inst.enemies._enemies 列表实现
    /// </summary>
    public class EnemyMapper : IComponentMapper
    {
        /// <summary>
        /// 敌人分组的最大距离(游戏单位)
        /// </summary>
        private const float MaxGroupDistance = 5f;

        /// <summary>
        /// 分组更新间隔（秒）
        /// </summary>
        private const float GroupUpdateInterval = 0.5f;

        /// <summary>
        /// 分组匹配的最小重叠度阈值（0.0 - 1.0）
        /// </summary>
        private const float MinOverlapRatio = 0.3f;

        private readonly TopMapView view;

        /// <summary>
        /// 分组标记信息
        /// </summary>
        private class GroupMarkerInfo
        {
            /// <summary>
            /// 虚拟分组 GameObject
            /// </summary>
            public GameObject GameObject { get; set; }

            /// <summary>
            /// 地图标记组件
            /// </summary>
            public MapMarker Marker { get; set; }

            /// <summary>
            /// 当前分组数据（用于 CountUpdater 闭包和成员追踪）
            /// </summary>
            public EnemyGroup CurrentGroup { get; set; }

            /// <summary>
            /// 是否在本次更新中被匹配
            /// </summary>
            public bool MatchedThisUpdate { get; set; }
        }

        /// <summary>
        /// 持久化的分组标记列表
        /// </summary>
        private readonly System.Collections.Generic.List<GroupMarkerInfo> _persistentGroupMarkers 
            = new System.Collections.Generic.List<GroupMarkerInfo>();

        /// <summary>
        /// 上次更新分组的时间
        /// </summary>
        private float _lastGroupUpdateTime = 0f;

        /// <summary>
        /// 自增 ID 计数器（用于 GameObject 命名）
        /// </summary>
        private int _nextGroupId = 0;

        public EnemyMapper(TopMapView view)
        {
            this.view = view;
        }

        /// <summary>
        /// 不使用此方法，因为敌人不通过 ObjectPatcher 触发
        /// </summary>
        /// <param name="component">敌人组件</param>
        public void Map(Component component)
        {
            // 不实现 - 我们直接从 Managers.Inst.enemies._enemies 获取
        }

        /// <summary>
        /// 定期更新敌人分组（由外部调用，如 TopMapView.Update）
        /// </summary>
#if IL2CPP
        [HideFromIl2Cpp]
#endif
        public void UpdateEnemyGroups()
        {
            try
            {
                // 限流：避免频繁更新
                if (Time.time - _lastGroupUpdateTime < GroupUpdateInterval)
                    return;

                _lastGroupUpdateTime = Time.time;

                // 1. 从 Managers 获取所有敌人
                if (Managers.Inst == null || Managers.Inst.enemies == null)
                    return;

                var enemiesSource = Managers.Inst.enemies._enemies;
                if (enemiesSource == null || enemiesSource.Count == 0)
                {
                    // 没有敌人，清空所有标记
                    RemoveAllGroupMarkers();
                    return;
                }

                // 2. 转换为 System.Collections.Generic.List 并过滤有效敌人
                var validEnemies = new System.Collections.Generic.List<Component>();
                
                // 遍历敌人列表（IL2CPP 和 Mono 通用）
                foreach (var enemy in enemiesSource)
                {
                    if (IsEnemyValid(enemy))
                    {
                        validEnemies.Add(enemy);
                    }
                }

                // 3. 进行分组
                var newGroups = EnemyGrouping.GroupEnemiesByType(validEnemies, MaxGroupDistance);

                // 4. 重置所有标记的匹配状态
                foreach (var info in _persistentGroupMarkers)
                {
                    info.MatchedThisUpdate = false;
                }

                // 5. 基于成员重叠度匹配新旧分组
                foreach (var newGroup in newGroups)
                {
                    // 查找最佳匹配的旧分组
                    var bestMatch = FindBestMatchingGroup(newGroup);

                    if (bestMatch != null)
                    {
                        // 复用现有标记，只更新数据
                        UpdateExistingGroupMarker(bestMatch, newGroup);
                        bestMatch.MatchedThisUpdate = true;
                    }
                    else
                    {
                        // 创建新标记
                        var newInfo = CreatePersistentGroupMarker(newGroup);
                        if (newInfo != null)
                        {
                            newInfo.MatchedThisUpdate = true;
                            _persistentGroupMarkers.Add(newInfo);
                        }
                    }
                }

                // 6. 移除未匹配的旧分组（已消失的分组）
                var unmatchedGroups = _persistentGroupMarkers
                    .Where(info => !info.MatchedThisUpdate)
                    .ToList();

                foreach (var info in unmatchedGroups)
                {
                    RemovePersistentGroupMarker(info);
                }
            }
            catch (Exception ex)
            {
                LogError($"EnemyMapper.UpdateEnemyGroups failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 每帧更新所有敌人组的位置(轻量级操作)
        /// </summary>
#if IL2CPP
        [HideFromIl2Cpp]
#endif
        public void UpdateGroupPositions()
        {
            try
            {
                // 遍历所有持久化的分组标记
                foreach (var info in _persistentGroupMarkers)
                {
                    if (info.GameObject == null || info.CurrentGroup == null)
                        continue;

                    // 从当前组成员重新计算中心位置
                    if (info.CurrentGroup.Members != null && info.CurrentGroup.Members.Count > 0)
                    {
                        // 过滤掉已销毁的成员
                        var validMembers = info.CurrentGroup.Members
                            .Where(m => m != null && m.gameObject != null)
                            .ToList();

                        if (validMembers.Count > 0)
                        {
                            // 计算中心 X 坐标
                            float minX = validMembers.Min(m => m.transform.position.x);
                            float maxX = validMembers.Max(m => m.transform.position.x);
                            float centerX = (minX + maxX) / 2f;

                            // 更新虚拟 GameObject 的位置(MapMarker 会自动跟随)
                            info.GameObject.transform.position = new Vector3(centerX, 0, 0);

                            // 更新 CurrentGroup 的 CenterX(用于下次分组计算)
                            info.CurrentGroup.CenterX = centerX;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"EnemyMapper.UpdateGroupPositions failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 查找与新分组最佳匹配的旧分组
        /// </summary>
        /// <param name="newGroup">新分组</param>
        /// <returns>最佳匹配的旧分组，如果没有找到则返回 null</returns>
#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private GroupMarkerInfo FindBestMatchingGroup(EnemyGroup newGroup)
        {
            GroupMarkerInfo bestMatch = null;
            float bestOverlapRatio = 0f;

            foreach (var oldInfo in _persistentGroupMarkers)
            {
                // 跳过已匹配的分组
                if (oldInfo.MatchedThisUpdate)
                    continue;

                var oldGroup = oldInfo.CurrentGroup;

                // 只比较同类型的分组（Boss vs Boss, Enemy vs Enemy）
                if (oldGroup.IsBossGroup != newGroup.IsBossGroup)
                    continue;

                // 计算成员重叠度
                float overlapRatio = CalculateOverlapRatio(oldGroup, newGroup);

                if (overlapRatio > bestOverlapRatio && overlapRatio >= MinOverlapRatio)
                {
                    bestOverlapRatio = overlapRatio;
                    bestMatch = oldInfo;
                }
            }

            return bestMatch;
        }

        /// <summary>
        /// 计算两个分组的成员重叠度
        /// </summary>
        /// <param name="group1">分组1</param>
        /// <param name="group2">分组2</param>
        /// <returns>重叠度（0.0 - 1.0）</returns>
#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private float CalculateOverlapRatio(EnemyGroup group1, EnemyGroup group2)
        {
            if (group1.Members == null || group2.Members == null)
                return 0f;

            if (group1.Members.Count == 0 || group2.Members.Count == 0)
                return 0f;

            // 转换为 HashSet 以提高查找效率
            var set1 = new System.Collections.Generic.HashSet<Component>(group1.Members);
            var set2 = new System.Collections.Generic.HashSet<Component>(group2.Members);

            // 计算交集数量
            int overlapCount = set1.Intersect(set2).Count();

            // 计算重叠度：交集 / 并集
            int unionCount = set1.Union(set2).Count();
            
            return unionCount > 0 ? (float)overlapCount / unionCount : 0f;
        }

        /// <summary>
        /// 更新现有的分组标记
        /// </summary>
        /// <param name="info">分组标记信息</param>
        /// <param name="newGroup">新的分组数据</param>
#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void UpdateExistingGroupMarker(GroupMarkerInfo info, EnemyGroup newGroup)
        {
            // 更新分组数据(CountUpdater 会通过闭包自动读取新的数量)
            info.CurrentGroup = newGroup;

            // 注意: 位置更新已移至 UpdateGroupPositions() 方法,每帧调用
        }

        /// <summary>
        /// 创建持久化的分组标记
        /// </summary>
        /// <param name="group">敌人分组</param>
        /// <returns>分组标记信息</returns>
#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private GroupMarkerInfo CreatePersistentGroupMarker(EnemyGroup group)
        {
            if (group == null || group.Count == 0)
                return null;

            try
            {
                // 创建虚拟的分组 GameObject
                int groupId = _nextGroupId++;
                var groupObject = new GameObject($"EnemyGroup_{groupId}");
                groupObject.transform.SetParent(view.transform, false);
                groupObject.transform.position = new Vector3(group.CenterX, 0, 0);

                // 创建分组标记信息
                var info = new GroupMarkerInfo
                {
                    GameObject = groupObject,
                    CurrentGroup = group
                };

                // 确定样式和标题
                var color = group.IsBossGroup ? MarkerStyle.Boss.Color : MarkerStyle.Enemy.Color;
                var sign = group.IsBossGroup ? MarkerStyle.Boss.Sign : MarkerStyle.Enemy.Sign;
                var title = group.IsBossGroup ? Strings.Boss : Strings.Enemy;

                // 创建地图标记
                // 关键：CountUpdater 使用闭包捕获 info，这样每次更新 info.CurrentGroup 时，计数会自动更新
                var marker = view.TryAddMapMarker(
                    groupObject.transform,
                    color,
                    sign,
                    title,
                    countUpdater: comp => info.CurrentGroup.Count,  // 闭包捕获 info
                    colorUpdater: null,
                    visibleUpdater: comp => true,
                    row: MarkerRow.Movable
                );

                if (marker != null)
                {
                    info.Marker = marker;
                    return info;
                }
                else
                {
                    // 创建失败，清理 GameObject
                    UnityEngine.Object.Destroy(groupObject);
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogError($"EnemyMapper.CreatePersistentGroupMarker failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 移除持久化的分组标记
        /// </summary>
        /// <param name="info">分组标记信息</param>
#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void RemovePersistentGroupMarker(GroupMarkerInfo info)
        {
            if (info == null)
                return;

            // 从 TopMapView 移除标记
            if (info.GameObject != null && info.GameObject.transform != null)
            {
                view.TryRemoveMapMarker(info.GameObject.transform);
            }

            // 销毁 GameObject
            if (info.GameObject != null)
            {
                UnityEngine.Object.Destroy(info.GameObject);
            }

            // 从列表中移除
            _persistentGroupMarkers.Remove(info);
        }

        /// <summary>
        /// 移除所有分组标记
        /// </summary>
#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void RemoveAllGroupMarkers()
        {
            var allMarkers = _persistentGroupMarkers.ToList();
            foreach (var info in allMarkers)
            {
                RemovePersistentGroupMarker(info);
            }
        }

        /// <summary>
        /// 检查敌人是否有效（存在且存活）
        /// </summary>
        /// <param name="enemy">敌人组件</param>
        /// <returns>是否有效</returns>
        private bool IsEnemyValid(Component enemy)
        {
            if (enemy == null || enemy.gameObject == null)
                return false;

            // 检查 Damageable 组件
            var damageable = enemy.GetComponent<Damageable>();
            if (damageable != null && damageable.isDead)
                return false;

            return true;
        }
    }
}
