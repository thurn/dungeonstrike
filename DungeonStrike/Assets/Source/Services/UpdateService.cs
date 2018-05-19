using System.Collections.Generic;
using System.Threading.Tasks;
using DungeonStrike.Source.Assets;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using DungeonStrike.Source.Specs;
using DungeonStrike.Source.Utilities;
using UnityEngine;

namespace DungeonStrike.Source.Services
{
    public class UpdateService : Service
    {
        private AssetLoader _assetLoader;
        private AssetRefs _assetRefs;

        protected override string MessageType => UpdateMessage.Type;

        private TransformSpec _transformSpec;
        private Dictionary<ComponentType, ISpec> _specs;

        protected override async Task<Result> OnEnableService()
        {
            _assetLoader = await GetService<AssetLoader>();

            _assetRefs = _assetLoader.GetAssets();
            _transformSpec = new TransformSpec
            {
                ErrorHandler = ErrorHandler,
                AssetRefs = _assetRefs
            };

            _specs = new Dictionary<ComponentType, ISpec>()
            {
                {ComponentType.Image, new ImageSpec()},
                {ComponentType.ContentSizeFitter, new ContentSizeFitterSpec()},
                {ComponentType.Renderer, new RendererSpec()},
                {ComponentType.CanvasScaler, new CanvasScalerSpec()},
                {ComponentType.Canvas, new CanvasSpec()},
                {ComponentType.GraphicRaycaster, new GraphicRaycasterSpec()}
            };

            foreach (var pair in _specs)
            {
                pair.Value.AssetRefs = _assetRefs;
                pair.Value.ErrorHandler = ErrorHandler;
            }

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

            foreach (var updateObject in message.UpdateObjects)
            {
                UpdateObject(updateObject);
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

            GameObject newObject;
            if (createObject.PrefabName == PrefabName.Unknown)
            {
                newObject = new GameObject(createObject.ObjectName);
            }
            else
            {
                newObject = AssetUtil.InstantiatePrefab(assetRefs, createObject.PrefabName);
                newObject.name = createObject.ObjectName;
                newObject.tag = "Prefab";
            }

            if (parentObject != null)
            {
                newObject.transform.SetParent(parentObject.transform);
            }

            UpdateComponents(newObject, createObject.Components);

            if (createObject.Transform != null)
            {
                // UpdateTransform should always be last, to handle layout components being added above.
                _transformSpec.UpdateGameObject(newObject, createObject.Transform);
            }
        }

        private void UpdateObject(UpdateObject updateObject)
        {
            var foundObject = GameObject.Find(updateObject.ObjectPath);
            UpdateComponents(foundObject, updateObject.Components);

            if (updateObject.Transform != null)
            {
                _transformSpec.UpdateGameObject(foundObject, updateObject.Transform);
            }
        }

        private void UpdateComponents(GameObject updateObject, List<IComponent> components)
        {
            foreach (var component in components)
            {
                if (_specs.ContainsKey(component.GetComponentType()))
                {
                    _specs[component.GetComponentType()].UpdateGameObject(updateObject, component);
                }
                else
                {
                    throw ErrorHandler.UnexpectedEnumValue(component.GetComponentType());
                }
            }
        }

        private void DeleteObject(DeleteObject deleteObject)
        {
        }
    }
}