﻿using System;
using XrmFramework.DeployUtils.Context;
using XrmFramework.DeployUtils.Service;

namespace XrmFramework.DeployUtils.Utils
{
    public interface IAssemblyFactory
    {
        IAssemblyContext CreateFromLocalAssemblyContext(Type TPlugin);
        IAssemblyContext CreateFromRemoteAssemblyContext(IRegistrationService service, string assemblyName);
        IFlatAssemblyContext CreateFlatAssemblyContextFromAssemblyContext(IAssemblyContext from);
    }
}
