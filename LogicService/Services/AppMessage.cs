using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicService.Services
{
    public delegate void EventHandle(object sender);

    /// <summary>
    /// Instance of customized event handle
    /// </summary>
    public class AppMessage
    {
        public enum MessageType
        {
            Inform,
            Warn,
        }

        /// <summary>
        /// Each page should have defined a eventhandle, but they just need one if there is a Public defination.
        /// </summary>
        public static event EventHandle MessageReached;

        /// <summary>
        /// Page uses this to hang on (send) a message, and someone subscribes to event.
        /// Just like Action-Parameter's behavior.
        /// </summary>
        /// <param name="send"></param>
        public static void SendMessage(string send)
        {
            if (MessageReached != null)
            {
                MessageReached(send);
            }
        }

    }
}
