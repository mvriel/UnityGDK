using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if UNITY_ANDROID
using Improbable.Gdk.Android;
#endif
using Improbable.Gdk.Core;
using Improbable.Worker.Core;
using Unity.Entities;
using UnityEngine;

namespace Playground
{
    public class WorkerConnectorBase : MonoBehaviour, IDisposable
    {
        private delegate Task<Worker> ConnectionDelegate();

        public GameObject LevelPrefab;
        public int MaxConnectionAttempts = 3;
        public bool UseExternalIp = false;

        public Worker Worker;

        private GameObject levelInstance;

        private static readonly SemaphoreSlim workerConnectionSemaphore = new SemaphoreSlim(1, 1);

        // Important run in this step as otherwise it can interfere with the the domain unloading logic
        private void OnApplicationQuit()
        {
            Dispose();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        public async Task Connect(string workerType, ILogDispatcher logger)
        {
            // Check that other workers have finished trying to connect before this one starts
            // This prevents races on the workers starting and races on when we start ticking systems
            await workerConnectionSemaphore.WaitAsync();
            try
            {
                var origin = transform.position;
                ConnectionDelegate connectionDelegate;
                if (ShouldUseLocator())
                {
                    connectionDelegate = async () =>
                        await Worker
                            .CreateWorkerAsync(GetLocatorConfig(workerType), SelectDeploymentName, logger, origin)
                            .ConfigureAwait(false);
                }
                else
                {
                    connectionDelegate = async () =>
                        await Worker.CreateWorkerAsync(GetReceptionistConfig(workerType), logger, origin)
                            .ConfigureAwait(false);
                }

                var worker = await ConnectWithRetries(connectionDelegate, MaxConnectionAttempts, logger, workerType);
                InitializeWorker(worker);
            }
            catch (Exception e)
            {
                logger.HandleLog(LogType.Error, new LogEvent("Failed to create worker")
                    .WithException(e)
                    .WithField("WorkerType", workerType)
                    .WithField("Message", e.Message));
#if UNITY_EDITOR
                // Temporary warning to be replaced when we can reliably detect if a local runtime is running, or not.
                logger.HandleLog(LogType.Warning,
                    new LogEvent(
                            "Is a local runtime running? If not, you can start one from 'SpatialOS -> Local launch' or by pressing Cmd/Ctrl-L")
                        .WithField("Reason", "A worker running in the editor failing to connect was observed"));
#endif
                Dispose();
                return;
            }
            finally
            {
                workerConnectionSemaphore.Release();
            }
        }

        protected virtual void AddWorkerSystems()
        {
        }

        protected virtual string SelectDeploymentName(DeploymentList deployments)
        {
            return null;
        }

        protected virtual ReceptionistConfig GetReceptionistConfig(string workerType)
        {
            ReceptionistConfig config;
            if (Application.isEditor)
            {
                config = new ReceptionistConfig
                {
                    WorkerType = workerType,
                    WorkerId = $"{workerType}-{Guid.NewGuid()}",
                    UseExternalIp = UseExternalIp
                };
            }
            else
            {
#if UNITY_ANDROID
                if (Application.isMobilePlatform)
                {
                    if (DeviceInfo.IsAndroidStudioEmulator())
                    {
                        config = ReceptionistConfig.CreateConnectionConfigForAndroidEmulator();
                        config.WorkerType = workerType;
                        config.WorkerId = $"{workerType}-{Guid.NewGuid()}";
                    }
                    else
                    {
                        // TODO need to stop hardcoding ip
                        config = ReceptionistConfig.CreateConnectionConfigForPhysicalAndroid("172.16.2.52");
                        config.WorkerType = workerType;
                        config.WorkerId = $"{workerType}-{Guid.NewGuid()}";
                    }
                }
                else
                {
#endif
                var commandLineArguments = Environment.GetCommandLineArgs();
                    var commandLineArgs = CommandLineUtility.ParseCommandLineArgs(commandLineArguments);
                    config = ReceptionistConfig.CreateConnectionConfigFromCommandLine(commandLineArgs);
                    config.WorkerType = workerType;
                    config.UseExternalIp = UseExternalIp;
                    if (!commandLineArgs.ContainsKey(RuntimeConfigNames.WorkerId))
                    {
                        config.WorkerId = $"{workerType}-{Guid.NewGuid()}";
                    }
                }
#if UNITY_ANDROID
            }
#endif
            return config;
        }

        protected virtual LocatorConfig GetLocatorConfig(string workerType)
        {
            var commandLineArguments = Environment.GetCommandLineArgs();
            var commandLineArgs = CommandLineUtility.ParseCommandLineArgs(commandLineArguments);
            var config = LocatorConfig.CreateConnectionConfigFromCommandLine(commandLineArgs);
            config.WorkerType = workerType;
            config.WorkerId = $"{workerType}-{Guid.NewGuid()}";
            return config;
        }

        private static async Task<Worker> ConnectWithRetries(ConnectionDelegate connectionDelegate, int attempts,
            ILogDispatcher logger, string workerType)
        {
            while (attempts > 0)
            {
                try
                {
                    return await connectionDelegate();
                }
                catch (ConnectionFailedException e)
                {
                    logger.HandleLog(LogType.Error,
                        new LogEvent($"Failed attempt to create worker")
                            .WithField("WorkerType", workerType)
                            .WithField("Message", e.Message));
                    attempts--;
                }
            }

            throw new ConnectionFailedException(
                $"Exceeded maximum connection attempts ({attempts})",
                ConnectionErrorReason.ExceededMaximumRetries);
        }

        private bool ShouldUseLocator()
        {
            if (Application.isEditor)
            {
                return false;
            }

            var commandLineArguments = Environment.GetCommandLineArgs();
            var commandLineArgs = CommandLineUtility.ParseCommandLineArgs(commandLineArguments);
            return commandLineArgs.ContainsKey(RuntimeConfigNames.LoginToken);
        }

        private void InitializeWorker(Worker worker)
        {
            Worker = worker;
            AddWorkerSystems();
            InstantiateLevel();
            Worker.OnDisconnect += OnDisconnected;
            World.Active = World.Active ?? Worker.World;

            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.AllWorlds.ToArray());
        }

        private void InstantiateLevel()
        {
            levelInstance = Instantiate(LevelPrefab, transform);
            levelInstance.transform.SetParent(null);
        }

        private void OnDisconnected(string reason)
        {
            Worker.LogDispatcher.HandleLog(LogType.Log, new LogEvent($"Worker disconnected")
                .WithField("WorkerId", Worker.WorkerId)
                .WithField("Reason", reason));
            StartCoroutine(DeferredDisposeWorker());
        }

        private IEnumerator DeferredDisposeWorker()
        {
            yield return null;
            Dispose();
        }

        public void Dispose()
        {
            Worker?.Dispose();
            Worker = null;
            // Check needed for the case that play mode is exited before the connection can complete
            if (Application.isPlaying)
            {
                Destroy(levelInstance);
                Destroy(this);
            }
        }
    }
}
