# Labyrinth

Network Library For Unity

***Experimental***

Dependencies: [Bolt](https://github.com/maelishere/Bolt) and [Lattice](https://github.com/maelishere/Lattice)

## Description

This is my attempt at writing a unity networking library without Mono.Cecil (magic) behind Mirror, Unet or any other unity networking library that unity's team will eventually give up on.

Dedicated Server or Clients. However, not at the same time.

Allows for networked level streaming

## Getting Started

### Starting Up

    using Labyrith.Components;

    NetworkManager.Listen(); or NetworkManager.Connect();

    or

    using Labyrith.Background;

    NetworkServer.Listen(int port); or NetworkClient.Connect(IPEndPoint endpoint);

### Shutting Down

    using Labyrith.Components;

    NetworkManager.Terminate();

    or

    using Labyrith.Background;

    NetworkServer.Destroy(); or NetworkClient.Disconnect();

### Creating Entities

Create a prefab with an Entity component, then save it in a resources folder. After, create a registry asset in the root of any resources folder, then add the path to all entities that can be spawn over the network. The registry is intialized in the background on load

### Instantiating Entities

Use the regular unity Instantiate, the entity script does the rest

### Network Scenes

Each scene must have a gameobject with the World component attached (also with their build index). The Server must have either have all possible scenes opened or for performance ensure that every scene any client has loaded or is about to load is loaded before all clients. When a scene is loaded on a client it sends a message to the server which in turns sends back all the entities within that scene 

### Network Behaviour Scripts

The gameobject must include an Entity or World. 

    using Laybrith.Runtime;

    Inhert from Appendix

Variables - use the Appendix.Var<T>() function to synchronize between server and client.

Function calls - use Appendix.Method() to register and Appendix.RPC() to call over network, with a max of three generic parameters. However, only primitives and some built-in unity structs are supported.

### Network Relevance

Instance messages for Variables and Function call make use of relevance, in order for the server to save bandwidth when sending data. For it to work an observer component (***Requirement***) must be placed on represention of a player (Character), which in turn requires an Entity component. You have to sync it's position, from client to server, through your own script; or NetworkTranform or NetworkRigidbody. 

Note: you can have multiple observers for each client.

Note: your custom network behaviours on the gameobject(s) with an observer; variables and functions will always be sent back to the client with authority (if that's how your behaviour is defined).

Optionally you can add a Sector script to an empty gameobject in any network scene (doesn't require an Entity or World, the process happens on the server). This provides additionally details for relevance calculation.

Note: A Sector (on the server) is required if you set relevancy to Relevance.Sectors for any variables or functions.
