using System.Linq;
using Unity.Entities;
using Unity.Networking.Transport;

public class FallingCubeSystem : ComponentSystem
{
    protected override void OnCreateManager()
    {
        // If this system is created in the server world, start listening
        // If this system is created in a client world, connect
        var ep = NetworkEndPoint.LoopbackIpv4;
        ep.Port = 51234;
        if (World == ClientServerBootstrap.serverWorld)
            World.GetOrCreateSystem<NetworkStreamReceiveSystem>().Listen(ep);
        else if (ClientServerBootstrap.clientWorld.Contains(World))
            World.GetOrCreateSystem<NetworkStreamReceiveSystem>().Connect(ep);
    }

    protected override void OnUpdate()
    {
        // If there are any connection entities which are not yet in game, mark them as in-game right away
        Entities.WithNone<NetworkStreamInGame>().ForEach((Entity entity, ref NetworkStreamConnection connection) =>
        {
            PostUpdateCommands.AddComponent(entity, new NetworkStreamInGame());
        });
    }
}