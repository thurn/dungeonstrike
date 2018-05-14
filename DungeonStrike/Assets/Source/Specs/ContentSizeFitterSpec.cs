using System;
using DungeonStrike.Source.Assets;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using UnityEngine;

namespace DungeonStrike.Source.Specs
{
    public class ContentSizeFitterSpec : Spec<ContentSizeFitter>
    {
        public ContentSizeFitterSpec(AssetRefs refs, ErrorHandler errorHandler) : base(refs, errorHandler)
        {
        }

        protected override void Update(GameObject gameObject, ContentSizeFitter fitter)
        {
            var component = GetOrCreateComponent<UnityEngine.UI.ContentSizeFitter>(gameObject);
            switch (fitter.HorizontalFitMode)
            {
                case HorizontalFitMode.Unconstrained:
                    component.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
                    break;
                case HorizontalFitMode.MinSize:
                    component.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.MinSize;
                    break;
                case HorizontalFitMode.PreferredSize:
                    component.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                    break;
                default:
                    throw ErrorHandler.UnexpectedEnumValue(fitter.HorizontalFitMode);
            }

            switch (fitter.VerticalFitMode)
            {
                case VerticalFitMode.Unconstrained:
                    component.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
                    break;
                case VerticalFitMode.MinSize:
                    component.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.MinSize;
                    break;
                case VerticalFitMode.PreferredSize:
                    component.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                    break;
                default:
                    throw ErrorHandler.UnexpectedEnumValue(fitter.VerticalFitMode);
            }
        }
    }
}