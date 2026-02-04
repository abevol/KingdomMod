using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap
{
    /// <summary>
    /// 敌人分组信息
    /// </summary>
    public class EnemyGroup
    {
        /// <summary>
        /// 组的中心 X 坐标
        /// </summary>
        public float CenterX { get; set; }

        /// <summary>
        /// 组内成员列表
        /// </summary>
        public List<Component> Members { get; set; }

        /// <summary>
        /// 是否为 Boss 组
        /// </summary>
        public bool IsBossGroup { get; set; }

        /// <summary>
        /// 成员数量
        /// </summary>
        public int Count => Members?.Count ?? 0;

        public EnemyGroup()
        {
            Members = new List<Component>();
        }
    }

    /// <summary>
    /// 敌人分组逻辑
    /// </summary>
    public static class EnemyGrouping
    {
        /// <summary>
        /// 按 X 坐标距离对敌人进行分组
        /// </summary>
        /// <param name="enemies">敌人列表</param>
        /// <param name="maxDistance">分组最大距离</param>
        /// <returns>分组结果列表</returns>
        public static List<EnemyGroup> GroupEnemiesByDistance(List<Component> enemies, float maxDistance)
        {
            if (enemies == null || enemies.Count == 0)
                return new List<EnemyGroup>();

            // 按 X 坐标排序
            var sortedEnemies = enemies
                .Where(e => e != null && e.gameObject != null)
                .OrderBy(e => e.transform.position.x)
                .ToList();

            if (sortedEnemies.Count == 0)
                return new List<EnemyGroup>();

            List<EnemyGroup> groups = new List<EnemyGroup>();
            EnemyGroup currentGroup = null;
            float lastX = 0;

            foreach (var enemy in sortedEnemies)
            {
                float enemyX = enemy.transform.position.x;

                // 如果当前没有组，或者距离上一个敌人超过最大距离，则创建新组
                if (currentGroup == null || Math.Abs(enemyX - lastX) > maxDistance)
                {
                    currentGroup = new EnemyGroup();
                    groups.Add(currentGroup);
                }

                currentGroup.Members.Add(enemy);
                lastX = enemyX;
            }

            // 计算每组的中心 X 坐标
            foreach (var group in groups)
            {
                if (group.Members.Count > 0)
                {
                    float minX = group.Members.Min(e => e.transform.position.x);
                    float maxX = group.Members.Max(e => e.transform.position.x);
                    group.CenterX = (minX + maxX) / 2f;
                }
            }

            return groups;
        }

        /// <summary>
        /// 分别对普通敌人和 Boss 进行分组
        /// </summary>
        /// <param name="enemies">敌人列表</param>
        /// <param name="maxDistance">分组最大距离</param>
        /// <returns>分组结果列表（包含普通敌人组和 Boss 组）</returns>
        public static List<EnemyGroup> GroupEnemiesByType(List<Component> enemies, float maxDistance)
        {
            if (enemies == null || enemies.Count == 0)
                return new List<EnemyGroup>();

            // 过滤掉无效的敌人
            var validEnemies = enemies
                .Where(e => e != null && e.gameObject != null)
                .ToList();

            // 分离普通敌人和 Boss
            var normalEnemies = new List<Component>();
            var bosses = new List<Component>();

            foreach (var enemy in validEnemies)
            {
                // 检查是否是 Boss
                var bossComponent = enemy.GetComponent<Boss>();
                if (bossComponent != null)
                {
                    bosses.Add(enemy);
                }
                else
                {
                    normalEnemies.Add(enemy);
                }
            }

            // 分别对两类敌人进行分组
            var result = new List<EnemyGroup>();

            // 普通敌人分组
            var normalGroups = GroupEnemiesByDistance(normalEnemies, maxDistance);
            foreach (var group in normalGroups)
            {
                group.IsBossGroup = false;
            }
            result.AddRange(normalGroups);

            // Boss 分组
            var bossGroups = GroupEnemiesByDistance(bosses, maxDistance);
            foreach (var group in bossGroups)
            {
                group.IsBossGroup = true;
            }
            result.AddRange(bossGroups);

            return result;
        }
    }
}
