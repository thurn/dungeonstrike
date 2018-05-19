using DungeonStrike.Source.Assets;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using UnityEngine;

namespace DungeonStrike.Source.Specs
{
    public class ImageSpec : Spec<Image>
    {
        protected override void Update(GameObject gameObject, Image image)
        {
            var component = GetOrCreateComponent<UnityEngine.UI.Image>(gameObject);
            if (image.SpriteName != SpriteName.Unknown)
            {
                component.sprite = AssetUtil.GetSprite(AssetRefs, image.SpriteName);
            }
            if (image.Color != null)
            {
                component.color = ToUnityColor(image.Color);
            }
            if (image.MaterialName != MaterialName.Unknown)
            {
                component.material = AssetUtil.GetMaterial(AssetRefs, image.MaterialName);
            }
            component.raycastTarget = image.IsRaycastTarget;

            switch (image.ImageType.GetImageSubtype())
            {
                case ImageSubtype.SimpleImageType:
                    UpdateSimpleImage(component, (SimpleImageType) image.ImageType);
                    break;
                default:
                    throw ErrorHandler.UnexpectedEnumValue(image.ImageType.GetImageSubtype());
            }
        }

        private static void UpdateSimpleImage(UnityEngine.UI.Image component, SimpleImageType image)
        {
            component.preserveAspect = image.PreserveAspectRatio;
        }
    }
}