using UnityEngine;
using Canvas = DungeonStrike.Source.Messaging.Canvas;

namespace DungeonStrike.Source.Specs
{
    public class CanvasSpec : Spec<Canvas>
    {
        protected override void Update(GameObject gameObject, Canvas canvas)
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
    }
}