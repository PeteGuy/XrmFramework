﻿using System;
using System.Text;
using XrmFramework.BindingModel;
using XrmFramework.Definitions;

namespace XrmFramework.RemoteDebugger
{
    [CrmEntity(DebugSessionDefinition.EntityName)]
    public class DebugSession : IBindingModel
    {
        [CrmMapping(DebugSessionDefinition.Columns.DebugeeId)] 
        public Guid DebugeeId { get; set; }

        [CrmMapping(DebugSessionDefinition.Columns.CreatedOn)]
        public DateTime SessionStart { get; set; }

        [CrmMapping(DebugSessionDefinition.Columns.SessionEnd)]
        public DateTime SessionEnd { get; set; }

        [CrmMapping(DebugSessionDefinition.Columns.RelayUrl)]
        public string RelayUrl { get; set; }

        [CrmMapping(DebugSessionDefinition.Columns.HybridConnectionName)]
        public string HybridConnectionName { get; set; }

        [CrmMapping(DebugSessionDefinition.Columns.SasKeyName)]
        public string SasKeyName { get; set; }

        [CrmMapping(DebugSessionDefinition.Columns.SasConnectionKey)]
        public string SasConnectionKey { get; set; }

        public Guid Id { get; set; }


        #region Overrides of Object

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"\tDebugeeId = {DebugeeId}");
            sb.AppendLine($"\tSessionStart = {SessionStart}");
            sb.AppendLine($"\tSessionEnd = {SessionEnd}");
            sb.AppendLine();
            sb.AppendLine($"\tRelayUrl = {RelayUrl}");
            sb.AppendLine($"\tHybridConnectionName = {HybridConnectionName}");
            sb.AppendLine($"\tSasKeyName = {SasKeyName}");
            sb.AppendLine($"\tSasConnectionKey = {SasConnectionKey}");
            sb.AppendLine();

            return sb.ToString();
        }

        #endregion
    }
}
