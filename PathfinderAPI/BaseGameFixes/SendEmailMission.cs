using System;
using System.Collections.Generic;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Hacknet;

namespace Pathfinder.BaseGameFixes
{
    [HarmonyPatch]
    internal static class SendEmailMission
    {
        [HarmonyILManipulator]
        [HarmonyPatch(typeof(MailServer), nameof(MailServer.MailWithSubjectExists))]
        public static void IndexIntoInboxFolderIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchStloc(1)
            );

            c.EmitDelegate<Func<Folder, Folder>>(folder => folder.folders[0]);
        }
    }
}
