using DungeonStrike.Source.Messaging;
using UnityEngine;

namespace DungeonStrike.Source.Specs
{
    public class GraphicRaycasterSpec : Spec<GraphicRaycaster>
    {
        protected override void Update(GameObject gameObject, GraphicRaycaster value)
        {
            GetOrCreateComponent<UnityEngine.UI.GraphicRaycaster>(gameObject);
        }
    }
}