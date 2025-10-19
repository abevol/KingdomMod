#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingdomMod.SharedLib
{
    public static class ContainerExtensions
    {
        /// <summary>
        /// 过滤掉数组中的 null 元素。
        /// </summary>
        public static T[] WithoutNulls<T>(this T?[] source) where T : class
        {
            return source.Where(x => x != null).ToArray()!;
        }

        public static IEnumerable<T> WithoutNulls<T>(this IEnumerable<T?> source) where T : class
        {
            return source.Where(x => x != null)!;
        }
    }
}
