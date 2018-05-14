using System;
using DungeonStrike.Source.Assets;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace DungeonStrike.Source.Specs
{
    public class TransformSpec : Spec<ITransform>
    {
        public TransformSpec(AssetRefs refs, ErrorHandler errorHandler) : base(refs, errorHandler)
        {
        }

        protected override void Update(GameObject gameObject, ITransform transform)
        {
            switch (transform.GetTransformType())
            {
                case TransformType.CubeTransform:
                    UpdateCubeTransform(gameObject, (Messaging.CubeTransform)transform);
                    break;
                case TransformType.RectTransform:
                    UpdateRectTransform(gameObject, (Messaging.RectTransform)transform);
                    break;
                default:
                    throw ErrorHandler.UnexpectedEnumValue(transform.GetTransformType());
            }
        }

        private void UpdateCubeTransform(GameObject gameObject, Messaging.CubeTransform transform)
        {
            gameObject.transform.position = ToUnityVector(transform.Position3d);
        }

        private void UpdateRectTransform(GameObject gameObject, Messaging.RectTransform transform)
        {
            var rectTransform = (UnityEngine.RectTransform)gameObject.transform;
            rectTransform.sizeDelta = ToUnityVector(transform.Size);
            rectTransform.position = ToUnityVector(transform.Position2d);

            rectTransform.pivot = PivotValue(transform.Pivot == Pivot.Unknown ?
                    Pivot.MiddleCenter : transform.Pivot);

            var anchors = AnchorValues(
                    transform.VerticalAnchor == VerticalAnchor.Unknown ?
                    VerticalAnchor.Top : transform.VerticalAnchor,
                    transform.HorizontalAnchor == HorizontalAnchor.Unknown ?
                    HorizontalAnchor.Left : transform.HorizontalAnchor);
            rectTransform.anchorMin = anchors.Item1;
            rectTransform.anchorMax = anchors.Item2;
        }

        private UnityEngine.Vector2 PivotValue(Pivot pivot)
        {
            switch (pivot)
            {
                case Pivot.UpperLeft:
                    return new UnityEngine.Vector2(x: 0f, y: 1f);

                case Pivot.UpperCenter:
                    return new UnityEngine.Vector2(x: 0.5f, y: 1f);

                case Pivot.UpperRight:
                    return new UnityEngine.Vector2(x: 1f, y: 1f);

                case Pivot.MiddleLeft:
                    return new UnityEngine.Vector2(x: 0f, y: 0.5f);

                case Pivot.MiddleCenter:
                    return new UnityEngine.Vector2(x: 0.5f, y: 0.5f);

                case Pivot.MiddleRight:
                    return new UnityEngine.Vector2(x: 1f, y: 0.5f);

                case Pivot.LowerLeft:
                    return new UnityEngine.Vector2(x: 0f, y: 0f);

                case Pivot.LowerCenter:
                    return new UnityEngine.Vector2(x: 0.5f, y: 1f);

                case Pivot.LowerRight:
                    return new UnityEngine.Vector2(x: 1f, y: 0f);

                default:
                    throw ErrorHandler.UnexpectedEnumValue(pivot);
            }
        }

        private Tuple<UnityEngine.Vector2, UnityEngine.Vector2> AnchorValues(
            VerticalAnchor verticalAnchor,
            HorizontalAnchor horizontalAnchor)
        {
            float minX;
            float minY;
            float maxX;
            float maxY;

            switch (horizontalAnchor)
            {
                case HorizontalAnchor.Right:
                    minX = 1f;
                    maxX = 1f;
                    break;

                case HorizontalAnchor.Center:
                    minX = 0.5f;
                    maxX = 0.5f;
                    break;

                case HorizontalAnchor.Left:
                    minX = 0f;
                    maxX = 0f;
                    break;

                case HorizontalAnchor.Stretch:
                    minX = 0f;
                    maxX = 1f;
                    break;

                default:
                    throw ErrorHandler.UnexpectedEnumValue(horizontalAnchor);
            }

            switch (verticalAnchor)
            {
                case VerticalAnchor.Top:
                    minY = 1f;
                    maxY = 1f;
                    break;

                case VerticalAnchor.Middle:
                    minY = 0.5f;
                    maxY = 0.5f;
                    break;

                case VerticalAnchor.Bottom:
                    minY = 0f;
                    maxY = 0f;
                    break;

                case VerticalAnchor.Stretch:
                    minY = 0f;
                    maxY = 1f;
                    break;

                default:
                    throw ErrorHandler.UnexpectedEnumValue(verticalAnchor);
            }

            return Tuple.Create(new UnityEngine.Vector2(minX, minY), new UnityEngine.Vector2(maxX, maxY));
        }
    }
}