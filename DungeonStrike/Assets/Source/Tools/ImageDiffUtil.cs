using UnityEngine;
using UnityEngine.UI;

namespace DungeonStrike.Source.Tools
{
    public class ImageDiffUtil : IComponentDiffUtil
    {
        class State : IComponentState
        {
            public string SpriteName;

            public object[] Description()
            {
                return new object[] {nameof(SpriteName), SpriteName};
            }
        }

        public IComponentState CreateState(Component component)
        {
            return new State()
                   {
                       SpriteName = ((Image) component).sprite.name
                   };
        }

        public bool DifferentStates(IComponentState a, IComponentState b)
        {
            var previousState = (State) a;
            var newState = (State) b;
            return previousState.SpriteName != newState.SpriteName;
        }
    }
}