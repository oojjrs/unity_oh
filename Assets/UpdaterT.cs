namespace oojjrs.oh
{
    public abstract class UpdaterT<T> : Updater where T : Updater.ListenerInterface, new()
    {
        protected override ListenerInterface _Listener => Listener;

        protected T Listener { get; } = new();
    }
}
