using UnityEngine;

namespace Labyrinth.Components
{
    using Labyrinth.Runtime;
    using Labyrinth.Background;

    [RequireComponent(typeof(World))]
    [RequireComponent(typeof(Central))]
    [AddComponentMenu("Labyrinth/Components/Network Manager")]
    public class NetworkManager : MonoBehaviour
    {
        public enum Type
        {
            Server,
            Client
        }

        [SerializeField] private int m_port = 5555;
        [SerializeField] private string m_IP = "localhost";
        [SerializeField] private Type m_type = Type.Server;

        public void Listen()
        {
            NetworkServer.Listen(m_port);
        }

        public void Connect()
        {
            NetworkClient.Connect(Network.Resolve(m_IP, m_port));
        }
    }
}