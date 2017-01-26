using System.Collections.Generic;
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

        [SetUp]
        public void SetUpTest()
        {
            _rootObject = new GameObject("Root");
            _rootComponent = _rootObject.AddComponent<Root>();
            _rootComponent.Awake();
            _managedObjects = new List<GameObject>();
            foreach (var component in _rootObject.GetComponents<DungeonStrikeComponent>())
            {
                component.RootObjectForTests = _rootComponent;
            }
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
            result.RootObjectForTests = _rootComponent;
            return result;
        }

        public T CreateTestService<T>() where T : Service
        {
            var service = _rootObject.AddComponent<T>();
            service.RootObjectForTests = _rootComponent;
            return service;
        }

        public T GetService<T>() where T : Service
        {
            return _rootObject.GetComponent<T>();
        }

        public void AwakeAndStartObjects()
        {
            foreach (var component in _rootObject.GetComponents<DungeonStrikeComponent>())
            {
                component.AwakeForTests();
            }
            foreach (var managedObject in _managedObjects)
            {
                foreach (var component in managedObject.GetComponents<DungeonStrikeComponent>())
                {
                    component.AwakeForTests();
                }
            }
            foreach (var component in _rootObject.GetComponents<DungeonStrikeComponent>())
            {
                component.OnEnableForTests();
            }
            foreach (var managedObject in _managedObjects)
            {
                foreach (var component in managedObject.GetComponents<DungeonStrikeComponent>())
                {
                    component.OnEnableForTests();
                }
            }
            foreach (var component in _rootObject.GetComponents<DungeonStrikeComponent>())
            {
                component.StartForTests();
            }
            foreach (var managedObject in _managedObjects)
            {
                foreach (var component in managedObject.GetComponents<DungeonStrikeComponent>())
                {
                    component.StartForTests();
                }
            }
        }
    }
}