using System;
using System.Reflection;
using System.Reflection.Emit;

namespace KingdomMod.SharedLib
{
    public static class ObjectExtensions
    {
        public static TR GetFieldOrPropertyValue<T, TR>(this object @this, string field)
        {
            if (@this == null)
                throw new NullReferenceException("inst is null.");

            var f = typeof(T).GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null)
                return (TR)f.GetValue(@this);

            var p = typeof(T).GetProperty(field, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null)
                return (TR)p.GetValue(@this);

            throw new NullReferenceException("field not exist.");
        }

        public static TR GetFieldOrPropertyValue<TR>(this object @this, string field)
        {
            if (@this == null)
                throw new NullReferenceException("inst is null.");

            var f = @this.GetType().GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null)
                return (TR)f.GetValue(@this);

            var p = @this.GetType().GetProperty(field, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null)
                return (TR)p.GetValue(@this);

            throw new NullReferenceException("field not exist.");
        }

        public static void SetFieldOrPropertyValue<T>(this object @this, string field, object value)
        {
            if (@this == null)
                throw new NullReferenceException("inst is null.");
            var f = typeof(T).GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null)
            {
                f.SetValue(@this, value);
                return;
            }

            var p = typeof(T).GetProperty(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null)
            {
                p.SetValue(@this, value);
                return;
            }

            throw new NullReferenceException("field not exist.");
        }

        public static void SetFieldOrPropertyValue(this object @this, string field, object value)
        {
            if (@this == null)
                throw new NullReferenceException("inst is null.");
            var f = @this.GetType().GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null)
            {
                f.SetValue(@this, value);
                return;
            }

            var p = @this.GetType().GetProperty(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null)
            {
                p.SetValue(@this, value);
                return;
            }

            throw new NullReferenceException("field not exist.");
        }

        public static TR GetFieldValue<T, TR>(this object @this, string field)
        {
            if (@this == null)
                throw new NullReferenceException("inst is null.");

            var f = typeof(T).GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f == null)
                throw new NullReferenceException("field not exist.");

            return (TR)f.GetValue(@this);
        }

        public static TR GetFieldValue<TR>(this object @this, string field)
        {
            if (@this == null)
                throw new NullReferenceException("inst is null.");

            var f = @this.GetType().GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f == null)
                throw new NullReferenceException("field not exist.");

            return (TR)f.GetValue(@this);
        }

        public static void SetFieldValue<T>(this object @this, string field, object value)
        {
            if (@this == null)
                throw new NullReferenceException("inst is null.");
            var f = typeof(T).GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f == null)
                throw new NullReferenceException("field not exist.");

            f.SetValue(@this, value);
        }

        public static void SetFieldValue(this object @this, string field, object value)
        {
            if (@this == null)
                throw new NullReferenceException("inst is null.");
            var f = @this.GetType().GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f == null)
                throw new NullReferenceException("field not exist.");

            f.SetValue(@this, value);
        }

        public static void SetPropertyValue<T>(this object @this, string prop, object value)
        {
            if (@this == null)
                throw new NullReferenceException("inst is null.");
            var f = typeof(T).GetProperty(prop, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f == null)
                throw new NullReferenceException("property not exist.");

            f.SetValue(@this, value);
        }

        public static void SetPropertyValue(this object @this, string prop, object value)
        {
            if (@this == null)
                throw new NullReferenceException("inst is null.");
            var f = @this.GetType().GetProperty(prop, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f == null)
                throw new NullReferenceException("property not exist.");

            f.SetValue(@this, value);
        }

        public static TR GetPropertyValue<T, TR>(this object @this, string prop)
        {
            if (@this == null)
                throw new NullReferenceException("inst is null.");

            var f = typeof(T).GetProperty(prop, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f == null)
                throw new NullReferenceException("Property not exist.");

            return (TR)f.GetValue(@this);
        }

        public static TR GetPropertyValue<TR>(this object @this, string prop)
        {
            if (@this == null)
                throw new NullReferenceException("inst is null.");

            var f = @this.GetType().GetProperty(prop, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f == null)
                throw new NullReferenceException("Property not exist.");

            return (TR)f.GetValue(@this);
        }

        public static TR GetMethodDelegate<T, TR>(this object @this, string method) where TR : Delegate
        {
            if (@this == null)
                throw new NullReferenceException("inst is null.");

            var f = typeof(T).GetMethod(method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (f == null)
                throw new NullReferenceException("method not exist.");

            return (TR)f.CreateDelegate(typeof(TR), @this);
        }

        public static TR GetMethodDelegate<TR>(this Type @this, string method) where TR : Delegate
        {
            if (@this == null)
                throw new NullReferenceException("inst is null.");

            var f = @this.GetMethod(method, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (f == null)
                throw new NullReferenceException("method not exist.");

            return (TR)f.CreateDelegate(typeof(TR));
        }

        public static TR GetMethodDelegate<TR>(this Type @this, object caller, string method) where TR : Delegate
        {
            if (@this == null)
                throw new NullReferenceException("inst is null.");

            var f = @this.GetMethod(method, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (f == null)
                throw new NullReferenceException("method not exist.");

            return (TR)f.CreateDelegate(typeof(TR), caller);
        }

        public static TR GetMethodDelegate<TR>(this object @this, string method) where TR : Delegate
        {
            if (@this == null)
                throw new NullReferenceException("inst is null.");

            var f = @this.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (f == null)
                throw new NullReferenceException("method not exist.");

            if (f.IsStatic)
                return (TR)f.CreateDelegate(typeof(TR));
            return (TR)f.CreateDelegate(typeof(TR), @this);
        }

        public static void CallMethod(this object @this, string method)
        {
            GetMethodDelegate<Action>(@this, method)();
        }

        public static TR GetOverrideMethodDelegate<T, TR>(this object @this, string method) where TR : Delegate
        {
            if (@this == null)
                throw new NullReferenceException("Inst is null.");

            var f = typeof(T).GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f == null)
                throw new NullReferenceException("Method not exist.");

            var result = Activator.CreateInstance(typeof(TR), @this, f.MethodHandle.GetFunctionPointer());
            if (result == null)
                throw new NullReferenceException("Create delegate failed.");

            return (TR)result;
        }

        public static TR GetOverrideMethodDelegate<TR>(this object @this, string method) where TR : Delegate
        {
            if (@this == null)
                throw new NullReferenceException("Inst is null.");

            var baseType = @this.GetType().BaseType;
            if (baseType == null)
                throw new NullReferenceException("baseType is null.");

            var f = baseType.GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f == null)
                throw new NullReferenceException("Method not exist.");

            var result = Activator.CreateInstance(typeof(TR), @this, f.MethodHandle.GetFunctionPointer());
            if (result == null)
                throw new NullReferenceException("Create delegate failed.");

            return (TR)result;
        }

        public static void CallOverrideMethod(this object @this, string method)
        {
            GetOverrideMethodDelegate<Action>(@this, method)();
        }

        public static void CallOverrideMethod<T>(this object @this, string method)
        {
            GetOverrideMethodDelegate<T, Action>(@this, method)();
        }

        public static void CallNonVirtual<T>(this object @this, string method)
        {
            if (@this == null)
                throw new NullReferenceException("Inst is null.");

            MethodInfo m = typeof(T).GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (m == null)
                throw new NullReferenceException("Method not exist.");

            DynamicMethod dm = new DynamicMethod("Base_" + method, null, new Type[] { typeof(object) }, @this.GetType());
            ILGenerator gen = dm.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, m);
            gen.Emit(OpCodes.Ret);

            dm.Invoke(null, new[] { @this });
            // var BaseFoo = (Action<object>)dm.CreateDelegate(typeof(Action<object>));
            // BaseFoo(@this);
        }

#if MONO
#nullable enable
        /// <summary>
        /// Mono 兼容性替代方法，用于替代 IL2CPP 中 <c>Il2CppObjectBase.Cast&lt;T&gt;()</c>。
        /// 使用 <c>as</c> 运算符将对象转换为指定类型，转换失败时抛出 <see cref="InvalidCastException"/>。
        /// </summary>
        /// <typeparam name="T">目标类型，必须为引用类型。</typeparam>
        /// <param name="this">要转换的对象。</param>
        /// <returns>转换后的目标类型实例。</returns>
        /// <exception cref="InvalidCastException">对象无法转换为目标类型时抛出。</exception>
        public static T Cast<T>(this object @this) where T : class
        {
            return @this as T ?? throw new InvalidCastException(
                $"Can't cast object of type {@this?.GetType()} to type {typeof(T)}");
        }

        /// <summary>
        /// Mono 兼容性替代方法，用于替代 IL2CPP 中 <c>Il2CppObjectBase.TryCast&lt;T&gt;()</c>。
        /// 使用 <c>as</c> 运算符尝试将对象转换为指定类型，转换失败时返回 <c>null</c>，不抛异常。
        /// </summary>
        /// <typeparam name="T">目标类型，必须为引用类型。</typeparam>
        /// <param name="this">要转换的对象（可为 null）。</param>
        /// <returns>转换成功时返回目标类型实例；输入为 null 或转换失败时返回 <c>null</c>。</returns>
        public static T? TryCast<T>(this object? @this) where T : class
        {
            return @this as T;
        }
#endif
    }
}
