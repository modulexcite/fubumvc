namespace FubuPersistence.Reset
{
    public interface IInitialState
    {
        void Load();
    }

    public class NulloInitialState : IInitialState
    {
        public void Load()
        {
            // nothing
        }
    }
}