using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if IL2CPP
using Il2CppInterop.Runtime.Injection;
#endif

// TODO: 将 RegisterTypeInIl2Cpp 属性应用到所有需要注册的类型上，确保它们在 IL2CPP 中正确注册

// TODO: 用 Cursor 检测 Unity 事件函数 Awake 和 Start 中的代码是否需要调整位置，确保它们在正确的时机执行

namespace KingdomMod.Shared.Attributes
{
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
}
