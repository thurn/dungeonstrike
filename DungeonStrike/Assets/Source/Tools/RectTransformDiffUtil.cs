using System;
using UnityEngine;

namespace DungeonStrike.Source.Tools
{
    public class RectTransformDiffUtil : IComponentDiffUtil
    {
        class State : IComponentState
        {
            public Vector2 AnchorMin;
            public Vector2 AnchorMax;
            public Vector2 AnchoredPosition;
            public Vector2 SizeDelta;
            public Vector2 Pivot;

            public object[] Description()
            {
                return new object[]
                       {
                           nameof(AnchorMin), AnchorMin,
                           nameof(AnchorMax), AnchorMax,
                           nameof(AnchoredPosition), AnchoredPosition,
                           nameof(SizeDelta), SizeDelta,
                           nameof(Pivot), Pivot
                       };
            }
        }

        public IComponentState CreateState(Component component)
        {
            var transform = (RectTransform) component;
            if (component.GetComponent<Canvas>() != null)
            {
                // Do not track transform for Canvas, changes based on screen size.
                return new State();
            }

            return new State()
                   {
                       AnchorMin = RoundVector(transform.anchorMin),
                       AnchorMax = RoundVector(transform.anchorMax),
                       AnchoredPosition = RoundVector(transform.anchoredPosition),
                       SizeDelta = RoundVector(transform.sizeDelta),
                       Pivot = RoundVector(transform.pivot)
                   };
        }

        public bool DifferentStates(IComponentState a, IComponentState b)
        {
            var previousState = (State) a;
            var newState = (State) b;
            return
                previousState.AnchorMin != newState.AnchorMin ||
                previousState.AnchorMax != newState.AnchorMax ||
                previousState.AnchoredPosition != newState.AnchoredPosition ||
                previousState.SizeDelta != newState.SizeDelta ||
                previousState.Pivot != newState.Pivot;
        }

        private static Vector2 RoundVector(Vector2 vector)
        {
            return new Vector2(Mathf.Round(vector.x), Mathf.Round(vector.y));
        }

        // Normalize rectange as a percentage of the canvas size
        private static Rect RoundRect(Rect rect)
        {
            return new Rect(
                    Mathf.Round(NormalizeWidth(rect.x)),
                    Mathf.Round(NormalizeHeight(rect.y)),
                    Mathf.Round(NormalizeWidth(rect.width)),
                    Mathf.Round(NormalizeHeight(rect.height)));
        }

        private static float NormalizeWidth(float x)
        {
            return (x * 1920.0f) / Screen.width;
        }

        private static float NormalizeHeight(float y)
        {
            return (y * 1080.0f) / Screen.height;
        }
    }
}