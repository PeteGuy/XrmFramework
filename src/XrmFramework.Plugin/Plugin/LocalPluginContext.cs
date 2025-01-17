﻿// Copyright (c) Christophe Gondouin (CGO Conseils). All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;

namespace XrmFramework
{
    internal partial class LocalPluginContext : LocalContext, IPluginContext
    {
        public LocalPluginContext(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            if (PluginExecutionContext.ParentContext != null)
            {
                ParentLocalContext = new LocalPluginContext(this, PluginExecutionContext.ParentContext);
            }

            ObjectContainer.RegisterInstanceAs(this, typeof(IPluginContext));
            ObjectContainer.RegisterInstanceAs(this, typeof(ICustomApiContext));
        }

        private LocalPluginContext(LocalPluginContext context, IPluginExecutionContext parentContext)
            : base(context, parentContext)
        {
            if (PluginExecutionContext.ParentContext != null)
            {
                ParentLocalContext = new LocalPluginContext(this, PluginExecutionContext.ParentContext);
            }
        }

        private IPluginExecutionContext PluginExecutionContext => (IPluginExecutionContext)ExecutionContext;

        public bool IsPreOperation()
        {
            return IsStage(Stages.PreOperation);
        }

        public bool IsPreValidation()
        {
            return IsStage(Stages.PreValidation);
        }

        public bool IsPostOperation()
        {
            return IsStage(Stages.PostOperation);
        }

        public bool IsStage(Stages stage)
        {
            return PluginExecutionContext.Stage == (int)stage;
        }

        public string PrimaryEntityName => PluginExecutionContext.PrimaryEntityName;

        public Guid PrimaryEntityId => PluginExecutionContext.PrimaryEntityId;

        public int Stage => PluginExecutionContext.Stage;

        public Guid OrganizationId => PluginExecutionContext.OrganizationId;

        public int Depth => PluginExecutionContext.Depth;

        public IPluginContext ParentContext => (IPluginContext)ParentLocalContext;

        public bool IsMultiplePrePostOperation
        {
            get
            {
                if (ParentContext != null && (IsCreate() || IsUpdate()))
                {
                    var currentTarget = GetInputParameter<Entity>(InputParameters.Target);
                    var parentTarget = ParentContext.GetInputParameter<Entity>(InputParameters.Target);

                    return parentTarget.Attributes.Count != currentTarget.Attributes.Count;
                }
                return false;
            }
        }

        public bool ShouldExecuteStep(Step step)
        {
            var isValid = 
                IsStage(step.Stage) 
                && Mode == step.Mode 
                && IsMessage(step.Message);

            if (isValid && step.EntityName != PrimaryEntityName)
            {
                isValid = false;

                if ((step.Message == Messages.Associate || step.Message == Messages.Disassociate)
                    && !string.IsNullOrWhiteSpace(step.UnsecureConfig))
                {
                    try
                    {
                        var stepConfig = JsonConvert.DeserializeObject<StepConfiguration>(step.UnsecureConfig);
                        isValid = step.EntityName == stepConfig.RelationshipName;
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            return isValid;
        }
    }

}
