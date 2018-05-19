using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonStrike.Source.Tools
{
    public interface IComponentState
    {
        object[] Description();
    }

    public interface IComponentDiffUtil
    {
        IComponentState CreateState(Component component);

        bool DifferentStates(IComponentState a, IComponentState b);
    }

    public class ComponentTracker
    {
        private readonly Dictionary<GameObject, Dictionary<string, IComponentState>> _componentDictionaries =
            new Dictionary<GameObject, Dictionary<string, IComponentState>>();

        private readonly Dictionary<Type, IComponentDiffUtil> _diffUtils = new Dictionary<Type, IComponentDiffUtil>()
        {
            {typeof(Image), new ImageDiffUtil()},
            {typeof(RectTransform), new RectTransformDiffUtil()}
        };

        private readonly Core.Logger _logger;

        public ComponentTracker(Core.Logger logger)
        {
            _logger = logger;
        }

        public void TrackComponentState(GameObject gameObject, Component component)
        {
            if (!_diffUtils.ContainsKey(component.GetType()))
            {
                return;
            }

            var diffUtil = _diffUtils[component.GetType()];

            if (!_componentDictionaries.ContainsKey(gameObject))
            {
                _componentDictionaries[gameObject] = new Dictionary<string, IComponentState>();
            }

            var components = _componentDictionaries[gameObject];
            var componentName = ComponentName(component);
            var state = diffUtil.CreateState(component);
            if (!components.ContainsKey(componentName) || diffUtil.DifferentStates(components[componentName], state))
            {
                LogUpdate(gameObject.name, componentName, state);
                components[componentName] = state;
            }
        }

        private void LogUpdate(string gameObjectName, string componentName, IComponentState state)
        {
            _logger.Log("Component Update",
                "Game Object", gameObjectName,
                "Component", componentName,
                "State", DescriptionString(state));
        }

        private static string DescriptionString(IComponentState state)
        {
            var result = new StringBuilder("{");
            var description = state.Description();
            for (var i = 0; i < description.Length; ++i)
            {
                result.Append(description[i]);
                var comma = i == description.Length - 1 ? "" : ", ";
                result.Append(i % 2 == 0 ? "=" : comma);
            }

            result.Append("}");

            return result.ToString();
        }

        private static string ComponentName(Component component)
        {
            return component.GetType().ToString();
        }
    }
}