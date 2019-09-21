using UnityEngine;

using Unity.Networking.Transport;
using Unity.Collections;
using Unity.Jobs;

public class JobifiedServerBehaviour : MonoBehaviour
{
    public UdpNetworkDriver m_Driver;
    public NativeList<NetworkConnection> m_Connections;
    private JobHandle ServerJobHandle;

    private int bufferSize = 0;
    private Serializer<CustomType> cts;

    void Start()
    {
        m_Driver = new UdpNetworkDriver(new NetworkDataStreamParameter { size = bufferSize });
        NetworkEndPoint server_endpoint = NetworkEndPoint.AnyIpv4;
        server_endpoint.Port = 9000;

        if (m_Driver.Bind(server_endpoint) != 0)
            Debug.Log("Failed to bind to port 9000");
        else
            m_Driver.Listen();

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        cts = new Serializer<CustomType>();
        cts.Initialize();
    }

    void OnDestroy()
    {
        ServerJobHandle.Complete();
        m_Connections.Dispose();
        m_Driver.Dispose();
    }

    void Update()
    {
        ServerJobHandle.Complete();

        var connectionJob = new ServerUpdateConnectionsJob
        {
            driver = m_Driver,
            connections = m_Connections
        };

        var serverUpdateJob = new ServerUpdateJob
        {
            driver = m_Driver.ToConcurrent(),
            connections = m_Connections.AsDeferredJobArray(),
            cts = cts
        };

        ServerJobHandle = m_Driver.ScheduleUpdate();
        ServerJobHandle = connectionJob.Schedule(ServerJobHandle);
        ServerJobHandle = serverUpdateJob.Schedule(m_Connections, 1, ServerJobHandle);
    }


}
