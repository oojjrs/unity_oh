namespace oojjrs.oh
{
    public abstract class MyUpdaterT<T> : MyUpdater where T : MyUpdater.ListenerInterface, new()
    {
        protected override ListenerInterface _Listener => Listener;

        protected T Listener { get; } = new();
    }
}
