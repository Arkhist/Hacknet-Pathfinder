using System.Runtime.InteropServices;
using System;
using System.Globalization;
using Hacknet;
using Microsoft.Xna.Framework;

namespace Pathfinder.Executable
{
    public enum CompletionResult
    {
        Error = -1,
        Running,
        Failure,
        Killed,
        Success,
    }
    public class GameExecutable : BaseExecutable
    {
        /// <summary>
        /// Lifetime value of the executable, added to upon being added to the OS on every Update
        /// </summary>
        /// <value></value>
        public float Lifetime { get; set; }
        private CompletionResult _Result = CompletionResult.Running;
        /// <summary>
        /// Status of the executable, sets isExiting if not set to CompletionResult.Running
        /// </summary>
        /// <value></value>
        public CompletionResult Result
        {
            get => _Result;
            set
            {
                _Result = value;
                isExiting = Result != CompletionResult.Running;
            }
        }
        /// <summary>
        /// Determines whether the executable is added to the OS when executed
        /// </summary>
        public virtual bool CanAddToSystem { get; set; } = true;
        /// <summary>
        /// Written to the terminal if failed or errored when not null
        /// </summary>
        /// <value></value>
        public virtual string ErrorReturn { get; set; }
        /// <summary>
        /// Determines whether the vanilla proxy fail behavior should also happen
        /// </summary>
        /// <value></value>
        public virtual bool IgnoreProxyFailPrint { get; set; }
        /// <summary>
        /// Determines whether the vanilla no available memory fail behavior should also happen
        /// </summary>
        /// <value></value>
        public virtual bool IgnoreMemoryBehaviorPrint { get; set; }

        public GameExecutable() : base(default, null, null)
        {
        }

        public void Assign(Rectangle location, OS os, string[] args)
        {
            this.os = os;
            bounds = location;
            Args = args;
        }

        /// <summary>
        /// Called immediately before being added to OS.exes.
        /// </summary>
        public virtual void OnInitialize() {}

        /// <summary>
        /// Called immediately before being removed from OS.exes.
        /// Called when Result is CompletionResult.Error.
        /// </summary>
        public virtual void OnCompleteError() {}
        /// <summary>
        /// Called immediately before being removed from OS.exes.
        /// Called when Result is CompletionResult.Failure.
        /// </summary>
        public virtual void OnCompleteFailure() {}
        /// <summary>
        /// Called immediately before being removed from OS.exes.
        /// Called when Result is CompletionResult.Killed.
        ///
        /// </summary>
        public virtual void OnCompleteKilled() {}
        /// <summary>
        /// Called immediately before being removed from OS.exes.
        /// Called when Result is CompletionResult.Success.
        /// </summary>
        public virtual void OnCompleteSuccess() {}
        /// <summary>
        /// Called immediately before being removed from OS.exes.
        /// Called after any dedicated completion method.
        /// </summary>
        public virtual void OnComplete() {}

        /// <summary>
        /// Called when not enough ram is available to run the executable
        /// </summary>
        public virtual void OnNoAvailableRam() {}
        /// <summary>
        /// Called when the computer has an active proxy and the executable could not bypass it
        /// </summary>
        public virtual void OnProxyBypassFailure() {}
        /// <summary>
        /// Called every Update when Result is CompletitionResult.Running
        /// </summary>
        /// <param name="delta">The time since the last OS.Update call</param>
        public virtual void OnUpdate(float delta) {}

        /// <summary>
        /// Called when an exception in thrown for this executable
        /// </summary>
        /// <param name="exception">The thrown exception</param>
        /// <returns>True if the exception is caught, false if it propagates</returns>
        public virtual bool CatchException(Exception exception) { return false; }

        public sealed override void LoadContent() {}

        public sealed override void Completed()
        {
            try
            {
                switch(Result)
                {
                    case CompletionResult.Success:
                        OnCompleteSuccess();
                        break;
                    case CompletionResult.Failure:
                        OnCompleteFailure();
                        break;
                    case CompletionResult.Killed:
                        OnCompleteKilled();
                        break;
                    case CompletionResult.Error:
                        OnCompleteError();
                        break;
                }
                OnComplete();

                if(Result == CompletionResult.Failure
                    || Result == CompletionResult.Error
                    && ErrorReturn != null)
                    os.write($"{Args[0]}: {(Result == CompletionResult.Failure ? $"failed" : "errored")} with '{ErrorReturn}'");
            }
            catch(Exception e)
            {
                if(CatchException(e))
                    throw e;
            }
        }

        public sealed override void Killed()
        {
            Result = CompletionResult.Killed;
            needsRemoval = true;
            os.exes.Remove(this);
        }

        public override void Update(float t)
        {
            if(Result != CompletionResult.Running) return;
            Lifetime += t;
            try
            {
                OnUpdate(t);
            }
            catch(Exception e)
            {
                if(CatchException(e))
                    throw e;
            }
            base.Update(t);
        }

        [Obsolete]
        public sealed override string GetIdentifier()
        {
            throw new NotImplementedException();
        }
    }
}