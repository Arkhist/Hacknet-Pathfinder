using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathfinder.Daemon
{
    public interface IInterface
    {
        string InitialServiceName { get; }

        void OnCreate(Instance instance);
        void Draw(Instance instance, Rectangle bounds, SpriteBatch sb);
        void LoadInstance(Instance instance, Dictionary<string, string> objects);
        void InitFiles(Instance instance);
        void LoadInit(Instance instance);
        void OnNavigatedTo(Instance instance);
        void OnUserAdded(Instance instance, string name, string pass, byte type);
    }

    public class Interface : IInterface
    {
        public virtual string InitialServiceName => GetType().FullName;
        public virtual void OnCreate(Instance instance) {}
        public virtual void Draw(Instance instance, Rectangle bounds, SpriteBatch sb) {}
        public virtual void LoadInstance(Instance instance, Dictionary<string, string> objects) {}
        public virtual void InitFiles(Instance instance) {}
        public virtual void LoadInit(Instance instance) {}
        public virtual void OnNavigatedTo(Instance instance) {}
        public virtual void OnUserAdded(Instance instance, string name, string pass, byte type) {}
    }
}
