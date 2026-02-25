using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if IL2CPP
using Il2CppInterop.Runtime.Injection;
#endif

namespace KingdomMod.SharedLib.Attributes
{
#if IL2CPP
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterTypeInIl2Cpp : Attribute
    {
        internal static List<Assembly> registrationQueue = new();
        internal static bool ready;
        internal bool LogSuccess = true;

        public RegisterTypeInIl2Cpp() { }

        public RegisterTypeInIl2Cpp(bool logSuccess)
        {
            LogSuccess = logSuccess;
        }

        public static void RegisterAssembly(Assembly asm)
        {
            IEnumerable<Type> types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null); }

            foreach (var type in types)
            {
                if (!type.IsClass)
                    continue;

                var attr = type.GetCustomAttribute<RegisterTypeInIl2Cpp>(false);
                if (attr == null)
                    continue;

                try
                {
                    ClassInjector.RegisterTypeInIl2Cpp(type);
                    if (attr.LogSuccess)
                        UnityEngine.Debug.Log($"[RegisterTypeInIl2Cpp] Registered: {type.FullName}");
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[RegisterTypeInIl2Cpp] Failed to register {type.FullName}: {ex}");
                }
            }
        }

        public static void SetReady()
        {
            ready = true;

            foreach (var asm in registrationQueue)
                RegisterAssembly(asm);

            registrationQueue.Clear();
        }

        /// <summary>
        /// 用于在插件加载阶段注册
        /// </summary>
        public static void InitRegisterHook()
        {
            AppDomain.CurrentDomain.AssemblyLoad += (_, args) =>
            {
                if (!ready)
                {
                    registrationQueue.Add(args.LoadedAssembly);
                }
                else
                {
                    RegisterAssembly(args.LoadedAssembly);
                }
            };

            // 主动注册当前已加载的程序集
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (ready)
                    RegisterAssembly(asm);
                else
                    registrationQueue.Add(asm);
            }
        }
    }
#endif
}
