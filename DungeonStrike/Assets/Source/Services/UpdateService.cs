using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DungeonStrike.Source.Assets;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using DungeonStrike.Source.Utilities;
using UnityEngine;
using UnityEngine.UI;

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

            UpdateComponents(assetRefs, gameObject, createObject.Components);
            UpdateTransform(gameObject, createObject.Transform);
        }

        private void UpdateObject(AssetRefs assetRefs, UpdateObject updateObject)
        {
            var gameObject = GameObject.Find(updateObject.ObjectPath);
            UpdateComponents(assetRefs, gameObject, updateObject.Components);
        }

        private void UpdateComponents(AssetRefs assetRefs, GameObject gameObject, List<IComponent> components)
        {
            foreach (var component in components)
            {
                switch (component.GetComponentType())
                {
                    case ComponentType.Renderer:
                        UpdateRenderer(assetRefs, gameObject, (Messaging.Renderer)component);
                        break;
                    case ComponentType.Canvas:
                        UpdateCanvas(assetRefs, gameObject, (Messaging.Canvas)component);
                        break;
                    case ComponentType.CanvasScaler:
                        UpdateCanvasScaler(assetRefs, gameObject, (Messaging.CanvasScaler)component);
                        break;
                    case ComponentType.GraphicRaycaster:
                        UpdateGraphicRaycaster(assetRefs, gameObject, (Messaging.GraphicRaycaster)component);
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

        private static T GetOrCreateComponent<T>(GameObject gameObject) where T : UnityEngine.Component
        {
            var result = gameObject.GetComponent<T>();
            if (result == null)
            {
                result = gameObject.AddComponent<T>();
            }
            return result;
        }

        private void UpdateTransform(GameObject gameObject, Messaging.Transform transform)
        {
            gameObject.transform.position = new Vector3(transform.Position.X, 0, transform.Position.Y);
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
                    ErrorHandler.ReportError("Unsupported render mode", canvas.RenderMode);
                    break;
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
                    ErrorHandler.ReportError("Unsupported scale mode", scaler.ScaleMode);
                    break;
            }
            component.referenceResolution = new Vector2(scaler.ReferenceResolution.X, scaler.ReferenceResolution.Y);
        }

        private void UpdateGraphicRaycaster(AssetRefs assetRefs, GameObject gameObject,
            Messaging.GraphicRaycaster raycaster)
        {
            GetOrCreateComponent<UnityEngine.UI.GraphicRaycaster>(gameObject);
        }
    }
}