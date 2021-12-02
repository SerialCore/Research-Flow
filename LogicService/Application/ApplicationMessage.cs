using System;

namespace LogicService.Application
{
    public enum MessageType
    {
        /// <summary>
        /// Real-time Message in App
        /// </summary>
        Banner,
        /// <summary>
        /// Exception
        /// </summary>
        InApp,
        /// <summary>
        /// Background Notification
        /// </summary>
        Toast
    }

    /// <summary>
    /// Instance of customized event handle
    /// </summary>
    public class ApplicationMessage
    {

        public delegate void MessageHandle(MessageEventArgs args);
        // public static event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// Each page should have defined a eventhandle, but they just need one if there is a Public defination.
        /// </summary>
        public static event MessageHandle MessageReceived;

        /// <summary>
        /// Page uses this to hang on (send) a message, and someone subscribes to event.
        /// Just like Action-Parameter's behavior.
        /// </summary>
        /// <param name="text">message</param>
        /// <param name="span">time span for message in second</param>
        public static void SendMessage(MessageEventArgs args)
        {
            if (MessageReceived != null)
            {
                MessageReceived(args);
            }
        }

    }

    public class MessageEventArgs : EventArgs
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public MessageType Type { get; set; }

        public DateTimeOffset Time { get; set; }
    }
}
