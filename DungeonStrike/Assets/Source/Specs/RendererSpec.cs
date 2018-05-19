using DungeonStrike.Source.Assets;
using DungeonStrike.Source.Core;
using UnityEngine;
using Renderer = DungeonStrike.Source.Messaging.Renderer;

namespace DungeonStrike.Source.Specs
{
    public class RendererSpec : Spec<Renderer>
    {
        protected override void Update(GameObject gameObject, Renderer renderer)
        {
            var component = GetOrCreateComponent<UnityEngine.Renderer>(gameObject);
            component.material = AssetUtil.GetMaterial(AssetRefs, renderer.MaterialName);
        }
    }
}