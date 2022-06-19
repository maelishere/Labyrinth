using System;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Labyrinth.Background
{
    public static class NetworkLoop
    {
        static bool AddToPlayerLoop(PlayerLoopSystem.UpdateFunction function, Type ownerType, ref PlayerLoopSystem playerLoop, Type playerLoopSystemType, bool beginning)
        {
            // did we find the type? e.g. EarlyUpdate/PreLateUpdate/etc.
            if (playerLoop.type == playerLoopSystemType)
            {
                // resize & expand subSystemList to fit one more entry
                int oldListLength = (playerLoop.subSystemList != null) ? playerLoop.subSystemList.Length : 0;
                Array.Resize(ref playerLoop.subSystemList, oldListLength + 1);

                // IMPORTANT: always insert a FRESH PlayerLoopSystem!
                // We CAN NOT resize and then OVERWRITE an entry's type/loop.
                PlayerLoopSystem system = new PlayerLoopSystem
                {
                    type = ownerType,
                    updateDelegate = function
                };

                // prepend our custom loop to the beginning
                if (beginning)
                {
                    // shift to the right, write into first array element
                    Array.Copy(playerLoop.subSystemList, 0, playerLoop.subSystemList, 1, playerLoop.subSystemList.Length - 1);
                    playerLoop.subSystemList[0] = system;

                }
                else
                {
                    // simply write into last array element
                    playerLoop.subSystemList[oldListLength] = system;
                }
                return true;
            }

            // recursively keep looking
            if (playerLoop.subSystemList != null)
            {
                for (int i = 0; i < playerLoop.subSystemList.Length; ++i)
                {
                    if (AddToPlayerLoop(function, ownerType, ref playerLoop.subSystemList[i], playerLoopSystemType, beginning))
                        return true;
                }
            }
            return false;
        }
        
        // hook into Unity runtime to actually add our custom functions
        [RuntimeInitializeOnLoadMethod]
        static void RuntimeInitializeOnLoad()
        {
            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            AddToPlayerLoop(NetworkEarlyUpdate, typeof(NetworkLoop), ref playerLoop, typeof(EarlyUpdate), false);
            AddToPlayerLoop(NetworkLateUpdate, typeof(NetworkLoop), ref playerLoop, typeof(PreLateUpdate), false);

            //// set the new loop
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        static void NetworkEarlyUpdate()
        {
            NetworkServer.Update();
            NetworkClient.Update();
        }

        static void NetworkLateUpdate()
        {
        }
    }
}