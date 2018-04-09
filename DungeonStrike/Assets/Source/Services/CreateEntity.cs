using System.Collections.Generic;
using System.Threading.Tasks;
using DungeonStrike.Source.Assets;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using DungeonStrike.Source.Utilities;
using UnityEngine;

namespace DungeonStrike.Source.Services
{
    public class CreateEntity : Service
    {
        private AssetLoader _assetLoader;

        protected override string MessageType => CreateEntityMessage.Type;

        protected override async Task<Result> OnEnableService()
        {
            _assetLoader = await GetService<AssetLoader>();

            return Result.Success;
        }

        protected override async Task<Result> HandleMessage(Message receivedMessage)
        {
            var message = (CreateEntityMessage) receivedMessage;
            var result = await CreateSoldier(
                    new Vector3(message.Position.X, 0, message.Position.Y));

            var entity = result.AddComponent<Entity>();
            entity.Initialize(message.EntityType, message.NewEntityId);
            return Result.Success;
        }

        private async Task<GameObject> CreateSoldier(Vector3 position)
        {
            var assetRefs = await _assetLoader.LoadAssets(new List<string>() {
                "Soldier",
                "SoldierForest",
                "SoldierHelmetGreen",
                "SoldierBagsGreen",
                "SoldierVestGreen"
            });

            var prefab = AssetUtil.InstantiateGameObject(assetRefs, "Soldier", position);
            prefab.transform.Find("Body").GetComponent<SkinnedMeshRenderer>().material =
                AssetUtil.GetMaterial(assetRefs, "SoldierForest");
            prefab.transform.Find("Helmet").GetComponent<SkinnedMeshRenderer>().material =
                AssetUtil.GetMaterial(assetRefs, "SoldierHelmetGreen");
            prefab.transform.Find("Bags").GetComponent<SkinnedMeshRenderer>().material =
                AssetUtil.GetMaterial(assetRefs, "SoldierBagsGreen");
            prefab.transform.Find("Vest").GetComponent<SkinnedMeshRenderer>().material =
                AssetUtil.GetMaterial(assetRefs, "SoldierVestGreen");

            return prefab;
        }
    }
}