namespace Pathfinder.Executable
{
    public class Interface
    {
        private string fileData;

        public string FileData
        {
            get
            {
                return fileData;
            }
            internal set
            {
                this.fileData = value;
            }
        }

        public virtual string GetIdentifer(Instance instance)
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
