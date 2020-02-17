namespace LogicService.Data
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
            Flow<U, V> a = u as Flow<U, V>;
            this.length = a.Length + 1;
        }

        public Flow<Flow<U, V>, T> Append<T>(T t)
        {
            return new Flow<Flow<U, V>, T>(this, t);
        }
    }

    class FlowDemo
    {
        public void demo()
        {
            Flow<string, int> flow = new Flow<string, int>("1", 1);
            flow.Append<double>(1.1);
        }
    }
}
