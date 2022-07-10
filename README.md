# Labyrinth

Network Library For Unity

Dependencies: [Bolt](https://github.com/maelishere/Bolt) and [Lattice](https://github.com/maelishere/Lattice)

## Description

This is my attempt at writing a unity networking library without Mono.Cecil, inspired by [Mirror](https://github.com/vis2k/Mirror).

Dedicated Server or Clients. However, not at the same time.

Allows for networked level streaming

## Getting Started

### Starting Up

    using Labyrith.Background;

    NetworkServer.Listen(int port); or NetworkClient.Connect(IPEndPoint endpoint);

### Shutting Down

    using Labyrith.Background;

    NetworkServer.Close(); or NetworkClient.Disconnect();

### Network Messages (Flag)

These are usualy static function callbacks sent through the network. Use Network.Register(), but id 0, 255, 1 - 12 are taken. There are several channels available to send a message through:

    Channels.Direct - not guaranteed
    
    Channels.Irregular - guaranteed but not in order
    
    Channels.Ordered - guaranteed in order

### Network Scenes

Each scene must have a gameobject with the World component attached (also with their build index). The Server must have either have all possible scenes opened or for performance ensure that every scene any client has loaded or is about to load is always loaded before all clients. When a scene is loaded on a client it sends a message to the server which in turns sends back all the entities within that scene 

The usually process in level streaming is to have one main scene which will remain loaded for the duration of the game; in your this main scene you would include a gameobject with a World and Central component. Central script indicates the scene that will not be unloaded until you disconnect.

### Creating Entities

Create a prefab with an Entity component, then save it in a resources folder. After, create a registry asset in the root of any resources folder, then add the paths to all entities that can be spawn over the network. The registry is intialized in the background on load

To Instantiate, frist you need to find a World component of the scene you want the entity to be in. After you call World.Anchor(), this ensures all newly locally instantiated enitites belong to this scene. Then use the regular unity Instantiate, the entity script does the rest. Alternatively you can call World.Instantiate()

Note: every scene you spawn in an Entity, there needs to be a gameobject with a World component (Anchor()) or else it gets destoryed after instantiate.

Note: to change authority on an Entity (Server Only) use Entity.Advocate().

Note: to move an Entity to another scene (Server Only) over the network after you create it Entity.Scenery().

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
    
### Network Relevancy

Instance messages for Variables and Function calls make use of relevance, in order for the server to save bandwidth when sending data. For it to work an Observer component (***Requirement***) must be placed on represention of a player (Character), which in turn requires an Entity component. You have to sync it's position, from client to server, through your own script. 

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

Relevancy also comes with custom layers (not attached to unity's layers) to allow for more complexity in synchronizing the overall gameworld.

### Network Collections

These objects are independent of runtime network instances. Messages are only sent when the object was changed during a frame.

    Fields

    Lists (Vector)

    Dictionaries (Glossary) 

    Sets/HashSets (Series or Sequence)

To link them over the network call Unit.Create() and call Unit.Destroy() when you're done with them.
    
Note: when the network is running only the server can change them.

Note: ensure you only change these class variables when you need to (not every frame).

Note: not every client has to create an object on the server; only create it when you need the data, then destroy when you're done.

## Roadmap

At the current moment i am just testing and retuning it.

### Bugs

Currently i am not aware of any bugs, if any living soul out there decides to use this library please let me know of any unexpected behaivour; i can point out if it expected behaivour or just a bug.

### Unity Asset Store 

I am working towards the review of this library from unity's team; at the moment i am in the queue. When i publish it on the asset store i will put a link here. The asset will come will a sample project.