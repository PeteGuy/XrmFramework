﻿using System.Runtime.CompilerServices;
using VerifyTests;

namespace XrmFramework.Analyzers.Tests
{
    public static class ModuleInitializer
    {
        [ModuleInitializer]
        public static void Init()
        {
            VerifySourceGenerators.Enable();
        }
    }
}
