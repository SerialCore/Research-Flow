namespace LogicService.FlowTask
{
    public class Flow<U, V>
    {
        private U ou;
        private V ov;
        private int length = 1;

        public U ObjectU
        {
            get { return ou; }
            set { ou = value; }
        }

        public V ObjectV
        {
            get { return ov; }
            set { ov = value; }
        }

        public int Length
        {
            get { return length; }
        }

        public Flow(U u, V v)
        {
            this.ou = u;
            this.ov = v;
        }

        public Flow<Flow<U, V>, T> AppendRight<T>(T t)
        {
            return new Flow<Flow<U, V>, T>(this, t);
        }

        public Flow<T, Flow<U, V>> AppendLeft<T>(T t)
        {
            return new Flow<T, Flow<U, V>>(t, this);
        }
    }

    //class FlowDemo
    //{
    //    public void demo()
    //    {
    //        Flow<string, int> flow = new Flow<string, int>("1", 1);
    //        flow.AppendRight<double>(1.1);
    //    }
    //}
}
