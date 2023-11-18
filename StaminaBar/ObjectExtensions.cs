using System;
using System.Reflection;
using System.Reflection.Emit;

namespace KingdomMod
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

        public static TR GetMethodDelegate<TR>(this object @this, string method) where TR : Delegate
        {
            if (@this == null)
                throw new NullReferenceException("inst is null.");

            var f = @this.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (f == null)
                throw new NullReferenceException("method not exist.");

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
    }
}
