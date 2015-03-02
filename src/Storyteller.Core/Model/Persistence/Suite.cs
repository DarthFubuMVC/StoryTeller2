using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Storyteller.Core.Model.Persistence
{
    public class Suite
    {
        public static string JoinPath(string parent, string name)
        {
            return (parent + '/' + name).Trim('/');
        }

        public Suite[] suites;
        public SpecNode[] specs;
        public string name;
        public string path;

        [JsonIgnore]
        public string Folder;

        public void WritePath(string parentFolder)
        {
            path = JoinPath(parentFolder, name);

            if (suites != null) suites.Each(x => x.WritePath(path));
            if (specs != null) specs.Each(x => x.WritePath(path));
        }

        public Hierarchy ToHierarchy()
        {
            var hierarchy = new Hierarchy
            {
                Top = this
            };

            organizeIntoHierarchy(hierarchy);

            return hierarchy;
        }

        private void organizeIntoHierarchy(Hierarchy hierarchy)
        {
            hierarchy.Suites[path] = this;
            suites.Each(x => x.organizeIntoHierarchy(hierarchy));

            specs.Each(x => hierarchy.Nodes[x.id] = x);
        }

        public IEnumerable<SpecNode> GetAllSpecs()
        {
            return specs.Union(suites.SelectMany(x => x.GetAllSpecs()));
        }

        public static string SuitePathOf(string path)
        {
            return path.Split('/').Reverse().Skip(1).Reverse().Join("/");
        }

        public void ReplaceNode(SpecNode node)
        {
            var index = Array.IndexOf(specs, node);
            if (index > -1)
            {
                specs[index] = node;
            }
        }

        public void AddSpec(SpecNode node)
        {
            specs = specs.Union(new[] {node}).ToArray();
        }
    }
}