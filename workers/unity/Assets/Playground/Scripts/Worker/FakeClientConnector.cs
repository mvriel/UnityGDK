using System;
using Improbable.Gdk.Core;
using Playground;
using UnityEngine;

public class FakeClientConnector : WorkerConnectorBase
{
    private async void Start()
    {
        await Connect("FakeClient", new ForwardingDispatcher()).ConfigureAwait(false);
    }

    protected override void AddWorkerSystems()
    {
    }

    protected override ReceptionistConfig GetReceptionistConfig(string workerType)
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
            var commandLineArguments = Environment.GetCommandLineArgs();
            var commandLineArgs = CommandLineUtility.ParseCommandLineArgs(commandLineArguments);
            config = ReceptionistConfig.CreateConnectionConfigFromCommandLine(commandLineArgs);
            config.WorkerType = "FakeClient";
            config.UseExternalIp = UseExternalIp;
            config.WorkerId = $"{workerType}-{Guid.NewGuid()}";
        }

        return config;
    }
}
