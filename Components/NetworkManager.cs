using UnityEngine;

namespace Labyrinth.Components
{
    using Labyrinth.Runtime;
    using Labyrinth.Background;

    [AddComponentMenu("Labyrinth/Components/Network Manager")]
    public class NetworkManager : MonoBehaviour
    {
        [SerializeField] private int m_port = 5555;
        [SerializeField] private string m_IP = "localhost";

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Listen()
        {
            NetworkServer.Listen(m_port);
        }

        public void Connect()
        {
            NetworkClient.Connect(Network.Resolve(m_IP, m_port));
        }

        public void Termintate()
        {
            if (NetworkServer.Running)
            {
                NetworkServer.Destroy();
            }

            if (NetworkClient.Running)
            {
                NetworkClient.Disconnect();
            }
        }
    }
}