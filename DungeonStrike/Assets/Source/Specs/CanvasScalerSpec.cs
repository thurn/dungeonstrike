using DungeonStrike.Source.Messaging;
using UnityEngine;

namespace DungeonStrike.Source.Specs
{
    public class CanvasScalerSpec : Spec<CanvasScaler>
    {
        protected override void Update(GameObject gameObject, CanvasScaler scaler)
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
                    component.physicalUnit = UnityEngine.UI.CanvasScaler.Unit.Millimeters;
                    break;
                default:
                    throw ErrorHandler.UnexpectedEnumValue(scaler.ScaleMode);
            }

            if (scaler.ReferenceResolution != null)
            {
                component.referenceResolution = new UnityEngine.Vector2(
                        scaler.ReferenceResolution.X, scaler.ReferenceResolution.Y);
            }
        }
    }
}