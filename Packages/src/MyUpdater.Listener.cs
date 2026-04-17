namespace oojjrs.oh
{
    public abstract partial class MyUpdater
    {
        public abstract class ListenerInterface
        {
            public bool Flag { get; set; }

            protected void On()
            {
                Flag = true;
            }
        }
    }
}
