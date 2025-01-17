﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XrmFramework
{
    public abstract class DefaultServiceWithSettings<T> : DefaultService where T : CrmSettings, new()
    {
        protected T Settings => _settings.Value;

        private readonly Lazy<T> _settings;

        protected virtual IEnumerable<(string settingName, object settingValue)> InitSettings(IEnumerable<(string settingName, Type settingType)> settingDefinitions)
        {
            foreach (var settingDefinition in settingDefinitions)
            {
                yield return (settingDefinition.settingName,
                    GetEnvironmentVariable(settingDefinition.settingType, settingDefinition.settingName)
                );
            }
        }

        protected DefaultServiceWithSettings(IServiceContext context) : base(context)
        {
            _settings = new Lazy<T>(() =>
                {
                    var propertySettings = typeof(T).GetProperties()
                        .Where(p => p.GetCustomAttribute<SettingNameAttribute>() != null)
                        .Select(p => new {Name = p.GetCustomAttribute<SettingNameAttribute>().Name, Property = p})
                        .ToList();

                    var returnSettings = new T();

                    var settingsValues = InitSettings(
                        propertySettings
                            .Select(p => (p.Name, p.Property.PropertyType)
                            )
                    );

                    foreach (var value in settingsValues)
                    {
                        var property = propertySettings.FirstOrDefault(p => p.Name == value.settingName);

                        property?.Property.SetValue(returnSettings, value.settingValue);
                    }

                    return returnSettings;
                });
        }
    }
}