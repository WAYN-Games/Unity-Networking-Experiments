using Unity.Collections;
using Unity.Jobs;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Assertions;



struct ServerUpdateJob : IJobParallelForDefer
{
    public UdpNetworkDriver.Concurrent driver;
    public NativeArray<NetworkConnection> connections;
    public Serializer<CustomType> cts;

    public void Execute(int index)
    {
        DataStreamReader stream;
        if (!connections[index].IsCreated)
            Assert.IsTrue(true);

        NetworkEvent.Type cmd;
        while ((cmd = driver.PopEventForConnection(connections[index], out stream)) !=
        NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Data)
            {
                var readerCtx = default(DataStreamReader.Context);
                byte[] value = stream.ReadBytesAsArray(ref readerCtx, stream.Length);

                CustomType ct = cts.DeSerialize(value);
                Debug.Log("Got " + ct.val + " from the Client adding + 2 to it.");
                ct.val += 2;


                var buff = cts.Serialize(ct);
                using (var writer = new DataStreamWriter(buff.Length, Allocator.Temp))
                {
                    writer.Write(buff);
                    driver.Send(NetworkPipeline.Null, connections[index], writer);
                }
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client disconnected from server");
                connections[index] = default(NetworkConnection);
            }
        }
    }
}