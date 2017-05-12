﻿using System.Collections.Generic;
using DungeonStrike.Source.Core;
using NUnit.Framework;
using UnityEngine;

namespace DungeonStrike.Tests.Editor
{
    public class DungeonStrikeTest
    {
        private GameObject _rootObject;
        private Root _rootComponent;
        private List<GameObject> _managedObjects;
        private List<Service> _testServices;
        private LogContext _rootLogContext;

        [SetUp]
        public void SetUpTest()
        {
            LogWriter.DisableForTests();
            _rootLogContext = LogContext.NewRootContext(GetType());
            _rootObject = new GameObject("Root");
            _rootComponent = _rootObject.AddComponent<Root>();
            _rootComponent.IsUnitTest = true;
            _rootComponent.Awake();
            _rootComponent.OnEnable();
            _managedObjects = new List<GameObject>();
            _testServices = new List<Service>();
        }

        [TearDown]
        public void TearDownTest()
        {
            foreach (var managedObject in _managedObjects)
            {
                foreach (var component in managedObject.GetComponents<DungeonStrikeComponent>())
                {
                    component.OnDisableForTests();
                }
            }
            foreach (var component in _rootObject.GetComponents<DungeonStrikeComponent>())
            {
                component.OnDisableForTests();
            }
            foreach (var obj in _managedObjects)
            {
                Object.DestroyImmediate(obj);
            }
            Object.DestroyImmediate(_rootObject);
        }

        public GameObject NewTestGameObject(string name)
        {
            var result = new GameObject(name);
            _managedObjects.Add(result);
            return result;
        }

        public GameObject NewTestEntityObject(string name, string entityType, string entityId)
        {
            var result = NewTestGameObject(name);
            var entity = result.AddComponent<Entity>();
            entity.Initialize(entityType, entityId);
            return result;
        }

        public T AddTestEntityComponent<T>(GameObject gameObject) where T : EntityComponent
        {
            var result = gameObject.AddComponent<T>();
            result.Root = _rootComponent;
            return result;
        }

        public T CreateTestService<T>() where T : Service
        {
            var service = _rootObject.AddComponent<T>();
            service.Root = _rootComponent;
            _testServices.Add(service);
            return service;
        }

        public T GetService<T>() where T : Service
        {
            return _rootObject.GetComponent<T>();
        }

        public void EnableObjects()
        {
            foreach (var service in _testServices)
            {
                service.Enable(_rootLogContext);
            }
            foreach (var managedObject in _managedObjects)
            {
                foreach (var component in managedObject.GetComponents<EntityComponent>())
                {
                    component.Enable(_rootLogContext);
                }
            }
        }
    }
}