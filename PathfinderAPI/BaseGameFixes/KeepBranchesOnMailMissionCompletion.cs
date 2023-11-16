using Hacknet;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Pathfinder.BaseGameFixes;

[HarmonyPatch]
public static class KeepBranchesOnMailMissionCompletion {
	[HarmonyPatch(typeof(MailServer), nameof(MailServer.doRespondDisplay))]
	[HarmonyILManipulator]
	public static void DoRespondDisplayManipulator(ILContext context) {
		ILCursor cursor = new(context);

		cursor.GotoNext(MoveType.After,
			x => x.MatchLdarg(0),
			x => x.MatchLdarg(0),
			x => x.MatchLdfld<Hacknet.Daemon>("os"),
			x => x.MatchLdfld<OS>("branchMissions"),
			x => x.MatchLdloc(4),
			x => x.MatchCallvirt(out _),
			x => x.MatchCall<MailServer>("attemptCompleteMission"),
			x => x.MatchStloc(11)
		);
		Instruction startOfRange = cursor.Next;

		cursor.GotoNext(MoveType.After,
			x => x.MatchLdarg(0),
			x => x.MatchLdfld<Hacknet.Daemon>("os"),
			x => x.MatchLdfld<OS>("branchMissions"),
			x => x.MatchCallvirt(out _)
		);
		Instruction endOfRange = cursor.Prev;

		cursor.Goto(startOfRange);
		int count = cursor.Instrs.IndexOf(endOfRange) - cursor.Instrs.IndexOf(startOfRange) + 1;
		cursor.RemoveRange(count);
	}
}
