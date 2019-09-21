using Unity.Collections;
using Unity.Jobs;
using Unity.Networking.Transport;
using UnityEngine;

struct ClientUpdateJob : IJob
{
    public UdpNetworkDriver driver;
    public NativeArray<NetworkConnection> connection;
    public NativeArray<byte> done;
    public Serializer<CustomType> cts;

    public void Execute() {


        if (!connection[0].IsCreated)
        {
            if (done[0] != 1)
                Debug.Log("Something went wrong during connect");
            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;

        while ((cmd = connection[0].PopEvent(driver, out stream)) !=
               NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("We are now connected to the server");

                CustomType customType = new CustomType() { val = 1 };
                var buff = cts.Serialize(customType);
                using (var writer = new DataStreamWriter(buff.Length, Allocator.Temp))
                {
                    writer.Write(buff);
                    connection[0].Send(driver, writer);
                }
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                var readerCtx = default(DataStreamReader.Context);
                var value = stream.ReadBytesAsArray(ref readerCtx, stream.Length);
                CustomType ct = cts.DeSerialize(value);
                Debug.Log("Got the value = " + ct.val + " back from the server");
                done[0] = 1;
                connection[0].Disconnect(driver);
                connection[0] = default(NetworkConnection);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                connection[0] = default(NetworkConnection);
            }
        }

    }
}