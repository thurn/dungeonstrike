using System;
using System.Collections.Generic;
using DungeonStrike.Assets.Source.Core;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DungeonStrike.Assets.Tests.Editor
{
    public class DungeonStrikeTest
    {
        private GameObject _rootObject;
        private Root _rootComponent;
        private List<GameObject> _managedObjects = new List<GameObject>();

        [SetUp]
        public void SetUpTests()
        {
            _rootObject = new GameObject("Root");
            _rootComponent = _rootObject.AddComponent<Root>();
            _rootComponent.Awake();
        }

        [TearDown]
        public void TearDownTests()
        {
            Object.DestroyImmediate(_rootObject);
            foreach (var obj in _managedObjects)
            {
                Object.DestroyImmediate(obj);
            }
        }

        public T PopulateComponent<T>(Action<T> beforeStart = null) where T : DungeonStrikeBehavior
        {
            var gameObject = new GameObject(typeof(T) + "Container");
            _managedObjects.Add(gameObject);
            var result = gameObject.AddComponent<T>();
            result.RootObjectForTests = _rootComponent;
            if (beforeStart != null)
            {
                beforeStart(result);
            }
            result.Awake();
            result.Start();
            return result;
        }

        public T GetSingleton<T>() where T : Component
        {
            return _rootObject.GetComponent<T>();
        }
    }
}