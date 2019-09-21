using Unity.Collections;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Jobs;

public class JobifiedClientBehaviour : MonoBehaviour
{
    public UdpNetworkDriver m_Driver;
    public NativeArray<NetworkConnection> m_Connection;
    public NativeArray<byte> m_Done;
    public JobHandle ClientJobHandle;
    public int bufferSize = 0;
    private Serializer<CustomType> cts;
    void Start()
    {
        m_Driver = new UdpNetworkDriver(new INetworkParameter[0]);
        m_Connection = new NativeArray<NetworkConnection>(1, Allocator.Persistent);
        m_Done = new NativeArray<byte>(1, Allocator.Persistent);
        m_Done[0] = 0;

        m_Connection[0] = default(NetworkConnection);

        NetworkEndPoint client_endpoint = NetworkEndPoint.LoopbackIpv4;
        client_endpoint.Port = 9000;
        m_Connection[0] = m_Driver.Connect(client_endpoint);
        cts = new Serializer<CustomType>();
        cts.Initialize();
    }

    public void OnDestroy()
    {
        ClientJobHandle.Complete();

        m_Connection.Dispose();
        m_Driver.Dispose();
        m_Done.Dispose();
    }

    void Update()
    {
        ClientJobHandle.Complete();


        var job = new ClientUpdateJob
        {
            driver = m_Driver,
            connection = m_Connection,
            done = m_Done,
            cts = cts
        };

        ClientJobHandle = m_Driver.ScheduleUpdate();
        ClientJobHandle = job.Schedule(ClientJobHandle);

    }
}
