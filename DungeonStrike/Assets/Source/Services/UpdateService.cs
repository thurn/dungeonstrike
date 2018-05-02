using System.Collections.Generic;
using System.Threading.Tasks;
using DungeonStrike.Source.Assets;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using DungeonStrike.Source.Utilities;
using UnityEngine;

namespace DungeonStrike.Source.Services
{
    public class UpdateService : Service
    {
        private AssetLoader _assetLoader;

        protected override string MessageType => UpdateMessage.Type;

        protected override async Task<Result> OnEnableService()
        {
            _assetLoader = await GetService<AssetLoader>();

            return Result.Success;
        }

        protected override Task<Result> HandleMessage(Message receivedMessage)
        {
            var message = (UpdateMessage) receivedMessage;
            var assetRefs = _assetLoader.GetAssets();

            foreach (var createObject in message.CreateObjects)
            {
                CreateObject(assetRefs, createObject);
            }

            foreach (var updatObject in message.UpdateObjects)
            {
                UpdateObject(assetRefs, updatObject);
            }

            foreach (var deleteObject in message.DeleteObjects)
            {
                DeleteObject(deleteObject);
            }

            return Async.Success;
        }

        private void CreateObject(AssetRefs assetRefs, CreateObject createObject)
        {
            GameObject parentObject = null;
            if (createObject.ParentPath != null)
            {
                parentObject = GameObject.Find(createObject.ParentPath);
                if (parentObject == null)
                {
                    ErrorHandler.ReportError("Parent object not found.", createObject.ParentPath);
                }
            }

            GameObject gameObject;
            if (createObject.PrefabName == PrefabName.Unknown)
            {
                gameObject = new GameObject(createObject.ObjectName);
            }
            else
            {
                gameObject = AssetUtil.InstantiatePrefab(assetRefs, createObject.PrefabName);
                gameObject.name = createObject.ObjectName;
            }

            if (parentObject != null)
            {
                gameObject.transform.SetParent(parentObject.transform);
            }

            UpdateTransform(gameObject, createObject.Transform);
        }

        private void UpdateObject(AssetRefs assetRefs, UpdateObject updateObject)
        {
            var gameObject = GameObject.Find(updateObject.ObjectPath);
            foreach (var component in updateObject.Components)
            {
                switch (component.GetComponentType())
                {
                    case ComponentType.Renderer:
                        UpdateRenderer(assetRefs, gameObject, (Messaging.Renderer)component);
                        break;
                    default:
                        ErrorHandler.ReportError("Unsupported component type", component.GetComponentType());
                        break;
                }
            }
        }

        private void DeleteObject(DeleteObject deleteObject)
        {
        }

        private void UpdateTransform(GameObject gameObject, Messaging.Transform transform)
        {
            gameObject.transform.position = new Vector3(transform.Position.X, 0, transform.Position.Y);
        }

        private void UpdateRenderer(AssetRefs assetRefs, GameObject gameObject, Messaging.Renderer renderer)
        {
            var component = gameObject.GetComponent<UnityEngine.Renderer>();
            component.material = AssetUtil.GetMaterial(assetRefs, renderer.MaterialName);
        }
    }
}