using System.Collections.Generic;
using System.Threading.Tasks;
using DungeonStrike.Source.Assets;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.EntityComponents;
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
            var result = await CreateSoldier();

            var entity = result.AddComponent<Entity>();
            entity.Initialize(message.EntityType, message.NewEntityId);
            await AddAndEnableComponent<ShowMoveSelector>(result);

            result.transform.position = new Vector3(message.Position.X, 0, message.Position.Y);
            return Result.Success;
        }

        private async Task<GameObject> CreateSoldier()
        {
            var prefab = await _assetLoader.LoadAsset<GameObject>(Units.Soldier);

            await Task.WhenAll(new List<Task>
            {
                LoadAndSetMaterial(prefab, "Body", SoldierRu.SoldierRuForest),
                LoadAndSetMaterial(prefab, "Helmet", SoldierRu.HelmetGreen),
                LoadAndSetMaterial(prefab, "Bags", SoldierRu.BagsGreen),
                LoadAndSetMaterial(prefab, "Vest", SoldierRu.VestGreen)
            });

            return prefab;
        }

        /// <summary>
        /// Loads the material in "asset" from disk and then sets it as the renderer material for "prefab".
        /// </summary>
        private async Task LoadAndSetMaterial(GameObject prefab, string childName, AssetReference asset)
        {
            var material = await _assetLoader.LoadAsset<Material>(asset);

            var meshRenderer = prefab.transform.Find(childName).GetComponent<SkinnedMeshRenderer>();
            meshRenderer.material = material;
        }
    }
}