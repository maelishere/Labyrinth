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

    NetworkServer.Close(); or NetworkClient.Disconnect();

### Network Flags

These are usualy static function callbacks sent through the network. Use Network.Register, but id 0 is not allowed and 1 - 11 is taken. You can use this for messages that need to be called without the need of an instance within the networked game world. There are server channels avaiable to send a messgae through:

    Channels.Direct - not guaranted
    
    Channels.Irregular - not guaranteed but not in order
    
    Channels.Ordered - not guaranteed in order

### Network Scenes

Each scene must have a gameobject with the World component attached (also with their build index). The Server must have either have all possible scenes opened or for performance ensure that every scene any client has loaded or is about to load is loaded before all clients. When a scene is loaded on a client it sends a message to the server which in turns sends back all the entities within that scene 

The usually process in level streaming is to have one main scene which will remain loaded for the duration of the game; in your this main scene you would include a gameobject with a World and Central component. Central script indicates the scene that will not be unloaded until you disconnect.

### Creating Entities

Create a prefab with an Entity component, then save it in a resources folder. After, create a registry asset in the root of any resources folder, then add the path to all entities that can be spawn over the network. The registry is intialized in the background on load

To Instantiate, frist you need to find a World component of the scene you want the entity to be in. After you call World.Anchor(), this ensures new enitites belong to this scene. Then use the regular unity Instantiate, the entity script does the rest. Alternatively you can call World.Instantiate() 

### Network Behaviour Scripts

The gameobject must include an Entity or World.

Your custom script

    using Laybrith.Runtime;

    public class Custom : Appendix // inherit from appendix

Variables - use the Appendix.Var<T>() function to synchronize between server and client. Restrictions:
    
    Signature.Rule.Round - client to server then server to all clients including authority
    
    Signature.Rule.Server - server to all clients including authority; client with authority doesn't send any message
    
    Signature.Rule.Authority -  client to server then server to all clients excluding authority

Function calls - use Appendix.Method() to register and Appendix.RPC() to call over network, with a max of three generic parameters. However, only primitives and some built-in unity structs are supported. Functions can use any channel like flags. Precautions:
    
    Procedure.Rule.Any - if received on any host it will always run
    
    Procedure.Rule.Client - if received on any host it will only run if the host in question is a client
    
    Procedure.Rule.Server - if received on any host it will  only run if the host in question is a server
    
    

### Network Relevance

Instance messages for Variables and Function call make use of relevance, in order for the server to save bandwidth when sending data. For it to work an observer component (***Requirement***) must be placed on represention of a player (Character), which in turn requires an Entity component. You have to sync it's position, from client to server, through your own script; or NetworkTranform or NetworkRigidbody. 

Note: you can have multiple observers for each client.

Note: your custom network behaviours on the gameobject(s) with an observer; variables and functions will always be sent back to the client with authority (if that's how your behaviour is defined).

Optionally you can add a Sector script to an empty gameobject in any network scene (doesn't require an Entity or World, the process happens on the server). This provides additionally details for relevance calculation.

Note: A Sector requires an observer to be functional.
    
Relevance Options
    
    Relevance.None - always sends 
    
    Relevance.Observers - only sends if an observer within range of the instance
    
    Relevance.Sectors - only sends if an observer and instance are in the same cell
    
    Relevance.General - either satisfies Relevance.Observers or Relevance.Sectors
    
Note: A Sector (on the server) is required if you set relevancy to Relevance.Sectors for any variables or functions.
