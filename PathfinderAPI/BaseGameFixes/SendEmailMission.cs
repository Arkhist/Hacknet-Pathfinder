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

            c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(Folder), nameof(Folder.folders)));
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.Callvirt, AccessTools.FirstProperty(typeof(List<Folder>), x => x.GetIndexParameters().Length > 0).GetGetMethod());
        }
    }
}
