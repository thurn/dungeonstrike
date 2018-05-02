using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// =============================================================================
// WARNING: Do not modify this file by hand! This file is automatically
// generated by 'code_generator.clj' on driver startup from the message
// specifications found in 'messages.clj'. Refer to the documentation in those
// files for more information.
// =============================================================================

namespace DungeonStrike.Source.Messaging
{
    public enum ComponentType
    {
        Unknown,
        Canvas,
        Renderer,
    }

    public enum PrefabName
    {
        Unknown,
        Soldier,
    }

    public enum SceneName
    {
        Unknown,
        Empty,
        Flat,
    }

    public enum SpriteName
    {
        Unknown,
        AirCardRarityRank5Big,
        FireCardElFiHeadingSeparate,
        AirCardElementalAirCardArtMask,
        FireCardCostSymbol,
        WaterCardEcWaterFrontNoHeading,
        EarthCardEcEarthFrontBig,
        WaterCardEcWaterFrontBigShadowed,
        EarthCardRarityRank3,
        LightCardRarityRank3Big,
        AirCardRarityRank4Big,
        LightCardRarityRank2Big,
        AirCardRarityRank3Big,
        WaterCardEcWaterMaskBig,
        AirCardCostSymbolAirBig,
        ShadowCardPowerSymbolShadowBig,
        WaterCardRarityRank5,
        LightCardRarityRank4Big,
        WaterCardEcWaterBackBig,
        EarthCardEcEarthFrontHeading,
        EarthCardCostBigEarthShadowed,
        FireCardElFiCardBodyFront,
        EarthCardEcEarthMaskBig,
        ShadowCardRarityRank3Big,
        ShadowCardShadowHeadingSeparated,
        AirCardPowerSymbolAirBigShadowed,
        FireCardElementalFireCardArtFrame,
        LightCardLightCardBigBack,
        FireCardPowerSymbolFireShadowed,
        FireCardElementalFireCardFrontShadowed,
        FireCardRarityRank5Big,
        EarthCardHealthBigEarth,
        WaterCardRarityRank4,
        LightCardLightCardBigBackShadowed,
        ShadowCardCostSymbolShadowBig,
        ShadowCardRarityRank2Big,
        EarthCardHealthBigEarthShadowed,
        ShadowCardRarityRank5Big,
        EarthCardEcEarthFrontBigShadowed,
        AirCardCostSymbolAirBigShadowed,
        AirCardArtworkAirBig,
        FireCardElementalFireCardBackShadowed,
        LightCardRarityRank5Big,
        FireCardHealthSymbolFireShadowed,
        ShadowCardShadowArtFrameBig,
        EarthCardRarityRank1,
        FireCardHealthSymbol,
        WaterCardEcWaterArtFrameBig,
        ShadowCardShadowBodyFront,
        FireCardRarityRank3Big,
        ShadowCardRarityRank1Big,
        WaterCardHealthBigSymbol,
        ShadowCardHealthSymbolShadowBig,
        ShadowCardPowerSymbolShadowBigOutlined,
        LightCardLightCardBigFront,
        AirCardHealthSymbolAirBigShadowed,
        AirCardElAirHeadingSeparate,
        EarthCardEcEarthBackBigShadowed,
        EarthCardEcEarthBackBig,
        AirCardRarityRank1Big,
        ShadowCardHealthSymbolShadowBigOutlined,
        ShadowCardShadowBigArtMask,
        EarthCardPowerBigEarthShadowed,
        WaterCardRarityRank3,
        LightCardArtMaskBigLight,
        WaterCardEcWaterFrontBig,
        LightCardLightCardBigFrontShadowed,
        AirCardPowerSymbolAir,
        AirCardElementalAirCardBackShadowed,
        EarthCardEcEarthFrontNoHeading,
        WaterCardPowerBigSymbol,
        FireCardRarityRank1Big,
        ShadowCardShadowBackBigOutlined,
        LightCardHealthSymbolBigLightShadowed,
        LightCardPowerSymbolBigLightShadowed,
        AirCardElAirCardBodyFront,
        FireCardScorchedEarthArtBig,
        AirCardRarityRank2Big,
        LightCardLightCardBody,
        FireCardRarityRank2Big,
        AirCardElementalAirCardFrontShadowed,
        ShadowCardShadowArtBig,
        EarthCardEcEarthArtFrameBig,
        WaterCardEcWaterBackBigShadowed,
        FireCardElementalFireCardArtMask,
        EarthCardRarityRank2,
        EarthCardRarityRank5,
        AirCardElementalAirCardArtFrame,
        LightCardPowerSymbolBigLight,
        WaterCardRarityRank2,
        EarthCardCostBigEarth,
        EarthCardEarthArtworkBig,
        WaterCardEcWaterFrontHeading,
        LightCardCostSymbolBigLightShadowed,
        LightCardCostSymbolBigLight,
        LightCardRarityRank1Big,
        ShadowCardShadowFrontBig,
        ShadowCardRarityRank4Big,
        FireCardRarityRank4Big,
        FireCardElementalFireCardFront,
        FireCardCostSymbolFireShadowed,
        LightCardHealthSymbolBigLight,
        LightCardLightArtBig,
        LightCardLightCardHeading,
        ShadowCardCostSymbolShadowBigOutlined,
        EarthCardPowerBigEarth,
        WaterCardRarityRank1,
        EarthCardRarityRank4,
        AirCardHealthSymbolAir,
        AirCardElementalAirCardBack,
        WaterCardCostBigSymbol,
        LightCardPortraitFrameBigLight,
        FireCardElementalFireCardBack,
        ShadowCardShadowFontBigOutlined,
        ShadowCardShadowBackBig,
        AirCardElementalAirCardFront,
        WaterCardWaterArtworkBig,
        FireCardPowerSymbol,
    }

    public enum MaterialName
    {
        Unknown,
        SoldierHelmetGreen,
        Soldier02HelmetKhaki,
        SoldierForest,
        SoldierBagsKhaki,
        SoldierGorka,
        SoldierSurpat,
        SoldierVestBlack,
        SoldierDesert,
        Soldier02BagsKhaki,
        SoldierWinter,
        SoldierVestGreen,
        Soldier02VestKhaki,
        SoldierHelmetKhaki,
        Soldier02BodyGorka,
        SoldierJungle,
        SoldierHelmetBlack,
        SoldierBagsGreen,
        SoldierVestKhaki,
        SoldierBagsBlack,
        SoldierBlack,
    }

    public interface IComponent
    {
      ComponentType GetComponentType();
    }

    public sealed class Canvas : IComponent
    {
      public ComponentType ComponentType;
      public ComponentType GetComponentType()
      {
        return ComponentType;
      }
    }

    public sealed class Renderer : IComponent
    {
      public ComponentType ComponentType;
      public MaterialName MaterialName;
      public ComponentType GetComponentType()
      {
        return ComponentType;
      }
    }

    public sealed class ComponentJsonConverter
        : UnionJsonConverter<IComponent>
    {
        public override string GetTypeIdentifier()
        {
            return "ComponentType";
        }

        public override object GetEmptyObjectForType(string type)
        {
            switch (type) {
                case "Canvas":
                    return new Canvas();
                case "Renderer":
                    return new Renderer();
                default:
                    throw new InvalidOperationException(
                        "Unrecognized type: " + type);
            }
        }
    }

    public sealed class DeleteObject
    {
        public string ObjectPath;
    }

    public sealed class MaterialUpdate
    {
        public string EntityChildPath;
        public MaterialName MaterialName;
    }

    public sealed class UpdateObject
    {
        public string ObjectPath;
        public Transform Transform;
        public List<IComponent> Components;
    }

    public sealed class Transform
    {
        public Position Position;
    }

    public sealed class CreateObject
    {
        public string ObjectName;
        public string ParentPath;
        public PrefabName PrefabName;
        public Transform Transform;
        public List<IComponent> Components;
    }

    public sealed class Position
    {
        public int X;
        public int Y;
    }

    public sealed class TestMessage : Message
    {
        public static readonly string Type = "Test";

        public TestMessage() : base("Test")
        {
        }

        public SceneName SceneName { get; set; }
    }

    public sealed class LoadSceneMessage : Message
    {
        public static readonly string Type = "LoadScene";

        public LoadSceneMessage() : base("LoadScene")
        {
        }

        public SceneName SceneName { get; set; }
    }

    public sealed class QuitGameMessage : Message
    {
        public static readonly string Type = "QuitGame";

        public QuitGameMessage() : base("QuitGame")
        {
        }

    }

    public sealed class CreateEntityMessage : Message
    {
        public static readonly string Type = "CreateEntity";

        public CreateEntityMessage() : base("CreateEntity")
        {
        }

        public string NewEntityId { get; set; }
        public PrefabName PrefabName { get; set; }
        public Position Position { get; set; }
        public List<MaterialUpdate> MaterialUpdates { get; set; }
    }

    public sealed class UpdateMessage : Message
    {
        public static readonly string Type = "Update";

        public UpdateMessage() : base("Update")
        {
        }

        public List<CreateObject> CreateObjects { get; set; }
        public List<UpdateObject> UpdateObjects { get; set; }
        public List<DeleteObject> DeleteObjects { get; set; }
    }

    public sealed class ClientConnectedAction : UserAction
    {
        public static readonly string Type = "ClientConnected";

        public ClientConnectedAction() : base("ClientConnected")
        {
        }

        public string ClientLogFilePath { get; set; }
        public string ClientId { get; set; }
    }

    public sealed class Messages
    {
        public static Message EmptyMessageForType(string messageType)
        {
            switch (messageType)
            {
                case "Test":
                    return new TestMessage();
                case "LoadScene":
                    return new LoadSceneMessage();
                case "QuitGame":
                    return new QuitGameMessage();
                case "CreateEntity":
                    return new CreateEntityMessage();
                case "Update":
                    return new UpdateMessage();
                default:
                    throw new InvalidOperationException(
                        "Unrecognized message type: " + messageType);
            }
        }

        public static JsonConverter[] GetJsonConverters()
        {
            return new JsonConverter[] {
                new MessageConverter(),
                new StringEnumConverter(),
                new ComponentJsonConverter(),
            };
        }
    }
}