using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicService.Application
{
    /// <summary>
    /// Instance of customized event handle
    /// </summary>
    public class ApplicationMessage
    {

        public enum MessageType
        {
            TopBanner,
            InAppNotification,
        }

        public delegate void MessageHandle(string message, MessageType type);

        /// <summary>
        /// Each page should have defined a eventhandle, but they just need one if there is a Public defination.
        /// </summary>
        public static event MessageHandle MessageReached;

        /// <summary>
        /// Page uses this to hang on (send) a message, and someone subscribes to event.
        /// Just like Action-Parameter's behavior.
        /// </summary>
        /// <param name="text">message</param>
        /// <param name="span">time span for message in second</param>
        public static void SendMessage(string text, MessageType type)
        {
            if (MessageReached != null)
            {
                MessageReached(text, type);
            }
        }

    }
}
