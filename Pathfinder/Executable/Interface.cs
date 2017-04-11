using System;

namespace Pathfinder.Executable
{
    public interface IInterface
    {
        string GetIdentifier(Instance instance);
        bool NeedsProxyAccess(Instance instance);
        int GetRamCost(Instance instance);
        void LoadContent(Instance instance);
        void OnComplete(Instance instance);
        void Draw(Instance instance, float time);
        /// <summary>
        /// Draws the outline.
        /// </summary>
        /// <returns><c>true</c>, if vanilla outline draw should continue, <c>false</c> otherwise.</returns>
        bool DrawOutline(Instance instance);
        /// <summary>
        /// Draws the target.
        /// </summary>
        /// <returns><c>true</c>, if vanilla target draw should continue, <c>false</c> otherwise.</returns>
        bool DrawTarget(Instance instance, string typeName = "app:");
        void OnKilled(Instance instance);
        /// <summary>
        /// Runs an update tick for the executable
        /// </summary>
        /// <returns>Whether isExiting should be set to true or not (thus exiting the program), null doesn't set it</returns>
        bool? Update(Instance instance, float time);
        void PreDraw(Instance instance);
        void PostDraw(Instance instance);
    }

    public class Interface : IInterface
    {
        [Obsolete("Rely on instance.ExecutionFile.data instead")]
        public string FileData
        {
            get
            {
                return Handler.GetStandardFileDataBy(this);
            }
        }

        [Obsolete("Use GetIdentifier")]
        public virtual string GetIdentifer(Instance instance)
        {
            return GetIdentifier(instance);
        }

        public virtual string GetIdentifier(Instance instance)
        {
            return "UNKNOWN";
        }

        public virtual bool NeedsProxyAccess(Instance instance)
        {
            return false;
        }

        public virtual int GetRamCost(Instance instance)
        {
            return Hacknet.ExeModule.DEFAULT_RAM_COST;
        }

        public virtual void LoadContent(Instance instance) {}
        public virtual void OnComplete(Instance instance) {}
        public virtual void Draw(Instance instance, float time) {}
        /// <summary>
        /// Draws the outline.
        /// </summary>
        /// <returns><c>true</c>, if vanilla outline draw should continue, <c>false</c> otherwise.</returns>
        public virtual bool DrawOutline(Instance instance) { return true; }
        /// <summary>
        /// Draws the target.
        /// </summary>
        /// <returns><c>true</c>, if vanilla target draw should continue, <c>false</c> otherwise.</returns>
        public virtual bool DrawTarget(Instance instance, string typeName = "app:") { return true; }
        public virtual void OnKilled(Instance instance) {}
        /// <summary>
        /// Runs an update tick for the executable
        /// </summary>
        /// <returns>Whether isExiting should be set to true or not (thus exiting the program), null doesn't set it</returns>
        public virtual bool? Update(Instance instance, float time) { return true; }
        public virtual void PreDraw(Instance instance) {}
        public virtual void PostDraw(Instance instance) {}
    }
}
