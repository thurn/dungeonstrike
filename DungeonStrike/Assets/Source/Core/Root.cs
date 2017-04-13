using System;
using System.IO;
using DungeonStrike.Source.Messaging;
using DungeonStrike.Source.Services;
using UnityEngine;

namespace DungeonStrike.Source.Core
{
    /// <summary>
    /// Central registration system for <see cref="Service"/> components.
    /// </summary>
    /// <remarks>
    /// The Root component should be added to exactly one GameObject in each scene. It is responsible for registering
    /// all <see cref="Service"/> components in its Awake() method. These components can retrieved by calling
    /// <see cref="DungeonStrikeComponent.GetService{T}"/>.
    /// </remarks>
    public sealed class Root : MonoBehaviour
    {
        public void Awake()
        {
            LogWriter.Initialize();
            Application.logMessageReceivedThreaded += LogWriter.HandleUnityLog;
            Debug.Log("Awake");
            RegisterServices();
        }

        /// <summary>
        /// Central registration point for services. Add all service components here. This method should only be
        /// invoked from test code.
        /// </summary>
        public void RegisterServices()
        {
            gameObject.AddComponent<MessageRouter>();
            gameObject.AddComponent<WebsocketManager>();
            gameObject.AddComponent<SceneLoader>();
        }
    }
}