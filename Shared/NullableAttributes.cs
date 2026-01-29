// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Nullable attributes polyfill
// These attributes are required by C# 8+ nullable reference types feature
// but may not be available in certain target frameworks

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Reserved to be used by the compiler for tracking metadata.
    /// This class should not be used by developers in source code.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Property |
        AttributeTargets.Field |
        AttributeTargets.Event |
        AttributeTargets.Parameter |
        AttributeTargets.ReturnValue |
        AttributeTargets.GenericParameter,
        AllowMultiple = false,
        Inherited = false)]
    internal sealed class NullableAttribute : Attribute
    {
        /// <summary>Initializes the attribute.</summary>
        public NullableAttribute(byte value) => NullableFlags = new[] { value };

        /// <summary>Initializes the attribute.</summary>
        public NullableAttribute(byte[] value) => NullableFlags = value;

        /// <summary>Flags specifying metadata related to nullable reference types.</summary>
        public readonly byte[] NullableFlags;
    }

    /// <summary>
    /// Reserved to be used by the compiler for tracking metadata.
    /// This class should not be used by developers in source code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    internal sealed class NullableContextAttribute : Attribute
    {
        /// <summary>Initializes the attribute.</summary>
        public NullableContextAttribute(byte value) => Flag = value;

        /// <summary>Flag specifying metadata related to nullable reference types.</summary>
        public readonly byte Flag;
    }
}
