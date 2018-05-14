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

        protected override string MessageType => UpdateMessage.Type;

        private TransformSpec _transformSpec;
        private Dictionary<ComponentType, ISpec> _specs;

        protected override async Task<Result> OnEnableService()
        {
            _assetLoader = await GetService<AssetLoader>();

            var assetRefs = _assetLoader.GetAssets();
            _transformSpec = new TransformSpec(assetRefs, ErrorHandler);
            _specs = new Dictionary<ComponentType, ISpec>()
            {
                {ComponentType.Image, new ImageSpec(assetRefs, ErrorHandler)},
                {ComponentType.ContentSizeFitter, new ContentSizeFitterSpec(assetRefs, ErrorHandler)}
            };

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

            GameObject newObject;
            if (createObject.PrefabName == PrefabName.Unknown)
            {
                newObject = new GameObject(createObject.ObjectName);
            }
            else
            {
                newObject = AssetUtil.InstantiatePrefab(assetRefs, createObject.PrefabName);
                newObject.name = createObject.ObjectName;
            }

            if (parentObject != null)
            {
                newObject.transform.SetParent(parentObject.transform);
            }

            UpdateComponents(assetRefs, newObject, createObject.Components);

            if (createObject.Transform != null)
            {
                // UpdateTransform should always be last, to handle layout components being added above.
                _transformSpec.UpdateGameObject(newObject, createObject.Transform);
            }
        }

        private void UpdateObject(AssetRefs assetRefs, UpdateObject updateObject)
        {
            var foundObject = GameObject.Find(updateObject.ObjectPath);
            UpdateComponents(assetRefs, foundObject, updateObject.Components);

            if (updateObject.Transform != null)
            {
                _transformSpec.UpdateGameObject(foundObject, updateObject.Transform);
            }
        }

        private void UpdateComponents(AssetRefs assetRefs, GameObject updateObject, List<IComponent> components)
        {
            foreach (var component in components)
            {
                if (_specs.ContainsKey(component.GetComponentType()))
                {
                    _specs[component.GetComponentType()].UpdateGameObject(updateObject, component);
                }
                else
                {
                    switch (component.GetComponentType())
                    {
                        case ComponentType.Renderer:
                            UpdateRenderer(assetRefs, updateObject, (Messaging.Renderer)component);
                            break;
                        case ComponentType.Canvas:
                            UpdateCanvas(assetRefs, updateObject, (Messaging.Canvas)component);
                            break;
                        case ComponentType.CanvasScaler:
                            UpdateCanvasScaler(assetRefs, updateObject, (CanvasScaler) component);
                            break;
                        case ComponentType.GraphicRaycaster:
                            UpdateGraphicRaycaster(assetRefs, updateObject, (GraphicRaycaster) component);
                            break;
                        default:
                            throw ErrorHandler.UnexpectedEnumValue(component.GetComponentType());
                    }
                }
            }
        }

        private void DeleteObject(DeleteObject deleteObject)
        {
        }

        private void UpdateRenderer(AssetRefs assetRefs, GameObject gameObject, Messaging.Renderer renderer)
        {
            var component = GetOrCreateComponent<UnityEngine.Renderer>(gameObject);
            component.material = AssetUtil.GetMaterial(assetRefs, renderer.MaterialName);
        }

        private void UpdateCanvas(AssetRefs assetRefs, GameObject gameObject, Messaging.Canvas canvas)
        {
            var component = GetOrCreateComponent<UnityEngine.Canvas>(gameObject);
            switch (canvas.RenderMode)
            {
                case Messaging.RenderMode.ScreenSpaceCamera:
                    component.renderMode = UnityEngine.RenderMode.ScreenSpaceCamera;
                    break;
                case Messaging.RenderMode.ScreenSpaceOverlay:
                    component.renderMode = UnityEngine.RenderMode.ScreenSpaceOverlay;
                    break;
                case Messaging.RenderMode.WorldSpace:
                    component.renderMode = UnityEngine.RenderMode.WorldSpace;
                    break;
                default:
                    throw ErrorHandler.UnexpectedEnumValue(canvas.RenderMode);
            }
        }

        private void UpdateCanvasScaler(AssetRefs assetRefs, GameObject gameObject, Messaging.CanvasScaler scaler)
        {
            var component = GetOrCreateComponent<UnityEngine.UI.CanvasScaler>(gameObject);
            switch (scaler.ScaleMode)
            {
                case Messaging.ScaleMode.ConstantPixelSize:
                    component.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
                    break;
                case Messaging.ScaleMode.ScaleWithScreenSize:
                    component.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    break;
                case Messaging.ScaleMode.ConstantPhysicalSize:
                    component.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPhysicalSize;
                    break;
                default:
                    throw ErrorHandler.UnexpectedEnumValue(scaler.ScaleMode);
            }
            component.referenceResolution = new UnityEngine.Vector2(
                    scaler.ReferenceResolution.X, scaler.ReferenceResolution.Y);
        }

        private void UpdateGraphicRaycaster(AssetRefs assetRefs, GameObject gameObject,
            Messaging.GraphicRaycaster raycaster)
        {
            GetOrCreateComponent<UnityEngine.UI.GraphicRaycaster>(gameObject);
        }

        private static T GetOrCreateComponent<T>(GameObject gameObject) where T : UnityEngine.Component
        {
            var result = gameObject.GetComponent<T>();
            if (result == null)
            {
                result = gameObject.AddComponent<T>();
            }
            return result;
        }
    }
}