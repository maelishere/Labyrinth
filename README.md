# Labyrinth
Network Library For Unity
    ***Experimental***

Dependencies: [Bolt](https://github.com/maelishere/Bolt) and [Lattice](https://github.com/maelishere/Lattice)

## Description

Dedicated Server or Clients. However, not at the same time.

Allows for networked level streaming 

### Example

Starting Up

    using Labyrith.Components;

    NetworkManager.Listen(); or NetworkManager.Connect();

    or

    using Labyrith.Background;

    NetworkServer.Listen(int port); or NetworkClient.Connect(IPEndPoint endpoint);

Shutting Down

    using Labyrith.Components;

    NetworkManager.Terminate();

    or

    using Labyrith.Background;

    NetworkServer.Destroy(); or NetworkClient.Disconnect();

Creating Entities

    Create a prefab with an Entity component, then save it in a resources folder

    Create a registry asset in the root of any resources folder, then add the path to all entities that can be spawn over the network

    The registry is intialized in the background on load

Instantiating Entities

    Use the regular unity Instantiate, the entity script does the rest

Network Scenes

    Each scene must have a gameobject with the World component attached (also with their build index)

    The Server must have either have all possible scenes opened or for performance ensure that every scene any client has loaded or is about to load is loaded before all clients

    When a scene is loaded on a client it sends a message to the server which in turns sends back all the entities within that scene 

Network Behaviour Scripts

    using Laybrith.Runtime;

    Inhert from Appendix

    The gameobject must include an Entity or World

    Variables - use the Appendix.Var<T>() function to synchronize between server and client

    Function calls - use Appendix.Method() to register and Appendix.RPC() to call over network, with a max of three generic parameters. However, only primitives and some built-in unity struts are supported.
