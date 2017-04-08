using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathfinder.Daemon
{
    public class Interface
    {
        public virtual void OnCreate(Instance instance) {}
        public virtual void Draw(Instance instance, Rectangle bounds, SpriteBatch sb) {}
        public virtual string GetSaveString(Instance instance) { return ""; }
        public virtual void InitFiles(Instance instance) {}
        public virtual void LoadInit(Instance instance) {}
        public virtual void OnNavigatedTo(Instance instance) {}
        public virtual void OnUserAdded(Instance instance, string name, string pass, byte type) {}
    }
}
