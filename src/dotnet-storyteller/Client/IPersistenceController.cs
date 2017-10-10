﻿using System;
using StoryTeller.Messages;
using StoryTeller.Model;
using StoryTeller.Model.Persistence;

namespace ST.Client
{
    public interface IPersistenceController : IDisposable
    {
        Hierarchy Hierarchy { get; }
        ResultsCache Results { get; }
        Specification LoadSpecificationById(string id);
        SpecAdded AddSpec(string path, string name);
        void SaveSpecification(string id, Specification specification);
        SpecAdded CloneSpecification(string id, string name);
        SpecData LoadSpecification(string id);
        void AddSuite(string parent, string name);
        void ClearAllResults();
        void ReloadHierarchy();
        void DeleteSpec(string id);
        SpecExecutionCompleted[] AllCachedResults();
        void SetLifecycle(string id, Lifecycle lifecycle);
        void StartWatching(string path);
    }
}
