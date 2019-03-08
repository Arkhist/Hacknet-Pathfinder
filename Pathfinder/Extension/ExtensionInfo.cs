using System;
using System.Collections.Generic;
using System.IO;
using Pathfinder.Event;

namespace Pathfinder.Extension
{
    public abstract class Info
    {
        internal Dictionary<string, Tuple<Command.Handler.CommandFunc, string, bool>> commands =
            new Dictionary<string, Tuple<Command.Handler.CommandFunc, string, bool>>();
        internal Dictionary<string, Daemon.Interface> daemons = new Dictionary<string, Daemon.Interface>();
        internal Dictionary<string, Executable.Interface> executables = new Dictionary<string, Executable.Interface>();
        internal Dictionary<string, Mission.Interface> missions = new Dictionary<string, Mission.Interface>();
        internal Dictionary<string, Mission.IGoal> goals = new Dictionary<string, Mission.IGoal>();
        internal Dictionary<string, Port.Type> ports = new Dictionary<string, Port.Type>();
        internal Dictionary<Type, List<Tuple<Action<PathfinderEvent>, string, string, int>>> eventListeners =
            new Dictionary<Type, List<Tuple<Action<PathfinderEvent>, string, string, int>>>();

        /// <summary>
        /// Gets the extension identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; internal set; }
        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Pathfinder.Extension.Info"/> is active.
        /// </summary>
        /// <value><c>true</c> if is active; otherwise, <c>false</c>.</value>
        public bool IsActive => this == Handler.ActiveInfo;
        /// <summary>
        /// Gets the extension's name.
        /// </summary>
        /// <value>The name.</value>
        public abstract string Name { get; }
        /// <summary>
        /// Gets the extension's description.
        /// </summary>
        /// <value>The description.</value>
        public abstract string Description { get; }
        /// <summary>
        /// Gets the extension's logo path. (if it does not exist or is null, chooses the default logo)
        /// </summary>
        /// <value>The logo path.</value>
        public abstract string LogoPath { get; }
        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Pathfinder.Extension.Info"/> allows saves.
        /// </summary>
        /// <value><c>true</c> if allows saves; otherwise, <c>false</c>.</value>
        public abstract bool AllowSaves { get; }
        /// <summary>
        /// Executes on extension construction.
        /// </summary>
        /// <param name="os">The Os.</param>
        public abstract void OnConstruct(Hacknet.OS os);
        /// <summary>
        /// Executes on extension loading.
        /// </summary>
        /// <param name="os">The Os.</param>
        /// <param name="loadingStream">The Loading stream.</param>
        public virtual void OnLoad(Hacknet.OS os, Stream loadingStream) {}
        public virtual void OnUnload(Hacknet.OS os) {}
    }
}
