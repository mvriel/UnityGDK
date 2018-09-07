using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Improbable.Gdk.Tools;
using Improbable.Worker.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

namespace Improbable.Gdk.Core.PlaymodeTests
{
    public class WorkerConnectionFlowTests
    {
        // Given that we are not polling for the runtime to start - if we have failed to connect 6 times in a row
        // (120 secs), something has gone wrong (either runtime failed to start - bad! or workers are broken - also bad!).
        // At that point - this test fails and allow an engineer to manually investigate.
        private const int NumAttempts = 6;
        private const int TimeBetweenAttemptsSecs = 20;
        private const int TestTimeoutMillis = (NumAttempts + 1) * TimeBetweenAttemptsSecs * 1000;

        private const string WorkerType = "UnityGameLogic";
        private static readonly Vector3 WorkerOrigin = Vector3.one;

        private static readonly Type[] coreManagerTypes = new[]
        {
            typeof(SpatialOSReceiveSystem),
            typeof(SpatialOSSendSystem),
            typeof(CleanReactiveComponentsSystem),
            typeof(WorldCommandsCleanSystem),
            typeof(WorldCommandsSendSystem),
            typeof(CommandRequestTrackerSystem)
        };

        private Process runtimeProcess;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            runtimeProcess = LocalLaunch.Launch();

            if (runtimeProcess == null)
            {
                throw new Exception("SpatialOS Runtime failed to start properly.");
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            runtimeProcess?.CloseMainWindow();
        }

        [UnityTest]
        [Timeout(TestTimeoutMillis)]
        public IEnumerator ConnectWorkerAsync_should_connect_properly_and_instantiate_worker()
        {
            int i;
            for (i = 0; i < NumAttempts; i++)
            {
                // TODO: Replace LoggingDispatcher with TestLogDispatcher once other PR is merged.
                var config = GetConfig(i);
                var workerTask = Worker.CreateWorkerAsync(config, new StupidLogger(), WorkerOrigin);

                while (Task.WaitAny(new[] { workerTask }, 0) == -1)
                {
                    yield return null;
                }

                if (workerTask.IsFaulted)
                {
                    yield return new WaitForSecondsRealtime(TimeBetweenAttemptsSecs);
                    continue;
                }

                if (workerTask.IsCompleted)
                {
                    var worker = workerTask.Result;
                    Assert.IsNotNull(worker.Connection);
                    Assert.IsTrue(worker.Connection.IsConnected);
                    Assert.AreEqual(config.WorkerId, worker.Connection.GetWorkerId());

                    Assert.IsNotNull(worker.LogDispatcher);
                    Assert.AreEqual(WorkerOrigin, worker.Origin);

                    Assert.IsNotNull(worker.World);

                    var managerTypes = worker.World.BehaviourManagers.Select(manager => manager.GetType()).ToList();

                    foreach (var managerType in coreManagerTypes)
                    {
                        Assert.IsTrue(managerTypes.Contains(managerType),
                            $"Worker does not contain core required system: {managerType.FullName}");
                    }

                    break;
                }
            }

            // We went through the entire for loop without breaking.
            if (i == NumAttempts)
            {
                Assert.Fail($"Failed to connect to SpatialOS runtime after {NumAttempts} attempts");
            }
        }

        private ReceptionistConfig GetConfig(int attemptNum)
        {
            var config = new ReceptionistConfig();
            config.WorkerType = WorkerType;
            config.WorkerId = $"{WorkerType}-{attemptNum}";

            return config;
        }
    }

    public class StupidLogger : ILogDispatcher
    {
        public void Dispose()
        {
        }

        public Connection Connection { get; set; }
        public void HandleLog(LogType type, LogEvent logEvent)
        {
        }
    }
}

