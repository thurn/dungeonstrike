using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DungeonStrike.Source.Assets;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using DungeonStrike.Source.Utilities;
using UnityEngine;
using Vectrosity;

namespace DungeonStrike.Source.EntityComponents
{
    public class ShowMoveSelector : EntityComponent
    {
        protected override string MessageType => ShowMoveSelectorMessage.Type;
        private AssetLoader _assetLoader;

        protected override async Task<Result> OnEnableEntityComponent()
        {
            _assetLoader = await GetService<AssetLoader>();

            return Result.Success;
        }

        protected override async Task<Result> HandleMessage(Message receivedMessage)
        {
            var message = (ShowMoveSelectorMessage) receivedMessage;
            var material = await _assetLoader.LoadAsset<Material>(Materials.MoveSelectorMaterial);

            DrawGrid(message.Positions, material);
            return Result.Success;
        }

        private void DrawGrid(List<Position> positions, Material material)
        {
            var lines = new HashSet<Tuple<int, int, int, int>>();
            foreach (var position in positions)
            {
                lines.Add(Tuple.Create(position.X - 1, position.Y - 1, position.X - 1, position.Y));
                lines.Add(Tuple.Create(position.X - 1, position.Y, position.X, position.Y));
                lines.Add(Tuple.Create(position.X, position.Y, position.X, position.Y - 1));
                lines.Add(Tuple.Create(position.X, position.Y - 1, position.X - 1, position.Y - 1));
            }
            var points = new List<Vector3>();
            foreach (var line in lines)
            {
                points.Add(new Vector3(line.Item1 + 0.5f, 0.11f, line.Item2 + 0.5f));
                points.Add(new Vector3(line.Item3 + 0.5f, 0.11f, line.Item4 + 0.5f));
            }
            var vectorLine = new VectorLine("MoveSelector", points, 5.0f)
            {
                material = material
            };
            vectorLine.Draw3DAuto();
        }
    }
}