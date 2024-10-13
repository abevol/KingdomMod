using System;
using System.Reflection;
using System.Reflection.Emit;

namespace KingdomMod
{
    public static class TypeExtensions
    {
        public static TR GetMethodDelegate<TR>(this Type @this, string method) where TR : Delegate
        {
            if (@this == null)
                throw new NullReferenceException("type is null.");

            var f = @this.GetMethod(method, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (f == null)
                throw new NullReferenceException("method not exist.");

            return (TR)f.CreateDelegate(typeof(TR));
        }

        public static void CallMethod(this Type @this, string method)
        {
            GetMethodDelegate<Action>(@this, method)();
        }
    }
}