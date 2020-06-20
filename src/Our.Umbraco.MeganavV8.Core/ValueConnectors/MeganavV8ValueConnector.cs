﻿using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;

namespace Our.Umbraco.MeganavV8.Core.ValueConnectors
{
    /// <summary>
    /// Represents a value connector for the Our.Umbraco.MeganavV8 property editor.
    /// </summary>
    /// <seealso cref="IValueConnector" />
    public class MeganavV8ValueConnector : IValueConnector
    {

        public string ToArtifact(object value, PropertyType propertyType, ICollection<ArtifactDependency> dependencies)
        {
            var svalue = value as string;
            if (string.IsNullOrWhiteSpace(svalue) || !svalue.DetectIsJson())
            {
                return null;
            }

            var rootLinks = ParseLinks(JArray.Parse(svalue), dependencies, Direction.ToArtifact);

            return rootLinks.ToString(Formatting.None);
        }

        public object FromArtifact(string value, PropertyType propertyType, object currentValue)
        {
            var rootLinks = ParseLinks(JArray.Parse(value), null, Direction.FromArtifact);

            value = rootLinks.ToString(Formatting.None);

            return value;
        }

        public IEnumerable<string> PropertyEditorAliases => new[] { "Our.Umbraco.MeganavV8" };


        private static JArray ParseLinks(JArray links, ICollection<ArtifactDependency> dependencies, Direction direction)
        {
            foreach (var link in links)
            {
                if (direction == Direction.ToArtifact)
                {
                    GuidUdi.TryParse(link.Value<string>("udi"), out var guidUdi);
                    if (guidUdi != default)
                    {
                        dependencies.Add(new ArtifactDependency(guidUdi, false, ArtifactDependencyMode.Exist));
                    }
                }

                var children = link.Value<JArray>("children");
                if (children != null)
                {
                    link["children"] = ParseLinks(children, dependencies, direction);
                }
            }

            return links;
        }
    }
}