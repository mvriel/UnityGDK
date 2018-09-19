using Improbable.Gdk.Core;
using Playground;
using UnityEngine;

public class FakeClientCoordinatorConnector : WorkerConnectorBase
{
    public GameObject FakeClientConnector;

    private async void Start()
    {
        await Connect("FakeClientCoordinator", new ForwardingDispatcher());
        Instantiate(FakeClientConnector, Vector3.zero, Quaternion.identity);
        Instantiate(FakeClientConnector, Vector3.zero, Quaternion.identity);
        Instantiate(FakeClientConnector, Vector3.zero, Quaternion.identity);
        Instantiate(FakeClientConnector, Vector3.zero, Quaternion.identity);
    }

    protected override void AddWorkerSystems()
    {
    }
}
