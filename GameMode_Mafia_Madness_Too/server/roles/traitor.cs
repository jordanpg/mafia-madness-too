//traitor.cs
//Code for the Traitor "innocent" role.

if(!$MM::LoadedRole_Cop)
	exec("./cop.cs");

$MM::Alignment[3] = "Traitor";
$MM::AlignmentColour[3] = "<color:FF0080>";

$MM::InvStatus[3] = $MM::InvStatus[0];

$MM::LoadedRole_Traitor = true;

if(!isObject(MMRole_Traitor))
{
	new ScriptObject(MMRole_Traitor)
	{
		class = "MMRole";

		name = "Traitor";
		corpseName = "benedict arnold";
		displayName = "Traitor";

		letter = "T";

		colour = "<color:FF0080>";
		nameColour = "1 0 0.5";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		alignment = 3;
		isEvil = 1;

		helpText = "";

		description = 	"\c4The <color:FF0080>Traitor \c4is a role that is amongst the \c2Innocent\c4, but works for the \c0Mafia\c4." NL
						"\c4The Traitor will come up as \c2upstanding \c4in investigations, and can see godfather chat as well as impersonations." NL
						"\c4The Traitor only wins with a mafia victory, and its goal is to deflect suspicion and cause confusion." NL
						"\c4The Traitor knows who the mafia is, but the mafia does not know who the traitor is.";
	};
}

//SUPPORT
function GameConnection::MM_isTraitor(%this)
{
	if(!isObject(%this.role))
		return false;

	return %this.role.getAlignment() == 3;
}

//HOOK
function MMRole_Traitor::SpecialWinCheck(%this, %mini, %client, %killed, %killer)
{
	parent::SpecialWinCheck(%this, %mini, %client, %killed, %killer);

	if(%client.lives < 1)
		return 3;

	for(%i = 0; %i < %mini.numMembers; %i++)
	{
		%mem = %mini.member[%i];

		if(%mem.MM_isMaf() && %mem.lives > 0)
			return 2;
	}

	return 1;
}

package MM_Traitor
{
	function MM_GodfatherCheck(%this, %target)
	{
		%r = parent::MM_GodfatherCheck(%this, %target);

		if(%target.MM_isTraitor() && !(%target.isGhost || %target.lives < 1))
			return 1;

		return %r;
	}

	function MM_ImpersonationCheck(%this, %target, %unn, %original)
	{
		%r = parent::MM_ImpersonationCheck(%this, %target, %unn, %original);

		if(%target.MM_isTraitor())
			return 3;

		return %r;
	}

	function serverCmdMafList(%this)
	{
		parent::serverCmdMafList(%this);

		if(%this.MM_isTraitor())
			%this.MM_DisplayMafiaList();
	}

	function GameConnection::MM_DisplayAlignmentDetails(%this, %alignment)
	{
		%r = parent::MM_DisplayAlignmentDetails(%this, %alignment);

		if(%r >= 0)
			return %r;

		if(%alignment == 3)
		{
			messageClient(%this, '', "\c4You are a <color:FF0080>Traitor\c4! Your goal is to help the mafia win, \c3but you're still considered an \c2Innocent\c3!");
			messageClient(%this, '', "\c4However, the Mafia doesn't know who you are. Your goal is to deflect suspicion and cause confusion. Type \c3/mafList \c4to see the list of mafia again.");

			%this.schedule(0, MM_DisplayMafiaList);

			return 3;
		}

		return %r;
	}
};
activatePackage(MM_Traitor);