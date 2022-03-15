﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xrm.Sdk;

namespace Deploy
{
    partial class CustomApi
    {
        public List<CustomApiRequestParameter> InArguments { get; } = new List<CustomApiRequestParameter>();

        public List<CustomApiResponseProperty> OutArguments { get; } = new List<CustomApiResponseProperty>();

        public string FullName
        {
            get;
            set;
        }

        public string Prefix
        {
            set => UniqueName = $"{value}{Name}";
        }
    }
}
