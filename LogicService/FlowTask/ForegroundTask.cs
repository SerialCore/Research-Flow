using System;

namespace LogicService.FlowTask
{
    public abstract class ForegroundTask
    {

        public ForegroundTask()
        {
            this.ID = "";
            this.IsRunning = false;
            this.IsAvailable = false;
        }

        public string ID { get; protected set; }

        public bool IsRunning { get; protected set; }

        public bool IsAvailable { get; protected set; }

        public abstract void Run();

    }

    public class TaskCompletedEventArgs : EventArgs
    {
        public Type Task { get; set; }

        public string Log { get; set; }
    }
}
