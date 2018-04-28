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

        protected override Task<Result> HandleMessage(Message receivedMessage)
        {
            var message = (CreateEntityMessage) receivedMessage;
            var assetRefs = _assetLoader.GetAssets();
            var prefab = AssetUtil.InstantiatePrefab(assetRefs, message.PrefabName,
                    new Vector3(message.Position.X, 0, message.Position.Y));

            foreach (var materialUpdate in message.MaterialUpdates)
            {
                var renderer = prefab.transform.Find(materialUpdate.EntityChildPath)
                        .GetComponent<SkinnedMeshRenderer>();
                renderer.material = AssetUtil.GetMaterial(assetRefs, materialUpdate.MaterialName);
            }

            var entity = prefab.AddComponent<Entity>();
            entity.Initialize(message.PrefabName, message.NewEntityId);
            return Async.Success;
        }
    }
}