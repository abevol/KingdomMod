#if IL2CPP
using Il2CppSystem.Collections.Generic;
#else
using System.Collections.Generic;
#endif
using UnityEngine;

namespace KingdomMod
{
    public class GameExtensions
    {
        public static T GetPayableOfType<T>() where T : Component
        {
            var payables = Managers.Inst.payables;
            if (!payables) return null;

            foreach (var obj in payables.AllPayables
                    )
            {
                if (obj == null) continue;
                var comp = obj.GetComponent<T>();
                if (comp != null)
                    return comp;
            }

            return null;
        }

        public static List<T> GetPayablesOfType<T>() where T : Component
        {
            var result = new List<T>();
            var payables = Managers.Inst.payables;
            if (!payables) return result;

            foreach (var obj in payables.AllPayables
                    )
            {
                if (obj == null) continue;
                var comp = obj.GetComponent<T>();
                if (comp != null)
                    result.Add(comp);
            }

            return result;
        }

        public static T GetPayableBlockerOfType<T>() where T : Component
        {
            var payables = Managers.Inst.payables;
            if (!payables) return null;

            foreach (var obj in payables._allBlockers)
            {
                if (obj == null) continue;
                var comp = obj.GetComponent<T>();
                if (comp != null)
                    return comp;
            }

            return null;
        }

        public static List<T> FindObjectsWithTagOfType<T>(string tagName)
        {
            var list = new List<T>();
            foreach (var obj in GameObject.FindGameObjectsWithTag(tagName))
            {
                if (obj == null) continue;
                var comp = obj.GetComponent<T>();
                if (comp != null)
                    list.Add(comp);
            }

            return list;
        }

        public static List<Character> FindCharactersOfType<T>()
        {
            var list = new List<Character>();
            var kingdom = Managers.Inst.kingdom;
            if (kingdom == null) return list;
            foreach (var character in kingdom._characters)
            {
                if (character == null) continue;
                if (character.GetComponent<T>() != null)
                    list.Add(character);
            }

            return list;
        }

        public static int GetArcherCount(ArcherType archerType)
        {
            var result = 0;
            foreach (var obj in Managers.Inst.kingdom._archers)
            {
                if (archerType == ArcherType.Free)
                {
                    if (!obj.inGuardSlot && !obj.isKnightSoldier)
                        result++;
                }
                else if (archerType == ArcherType.GuardSlot)
                {
                    if (obj.inGuardSlot)
                        result++;
                }
                else if (archerType == ArcherType.KnightSoldier)
                {
                    if (obj.isKnightSoldier)
                        result++;
                }
            }

            return result;
        }

        public static int GetKnightCount(bool needsArmor)
        {
            var knightCount = 0;
            foreach (var knight in Managers.Inst.kingdom._knights)
            {
                if (knight._needsArmor == needsArmor)
                    knightCount++;
            }
            return knightCount;
        }

        public enum ArcherType
        {
            Free,
            GuardSlot,
            KnightSoldier
        }
    }
}