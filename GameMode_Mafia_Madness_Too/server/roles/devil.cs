//devil.cs
//Code for the Devil mafia role.

if(!$MM::LoadedRole_Cop)
	exec("./cop.cs");

$MM::LoadedRole_Devil = true;

$MM::InvError[10] = "\c4That player is a fellow mafia, why are you investigating them?  Type \c3/maflist\c4 to see mafia roles.";

$MM::DevilInvPattern = '\c3%1 \c4is the %2\c4!';

if(!isObject(MMRole_Devil))
{
	new ScriptObject(MMRole_Devil)
	{
		class = "MMRole";

		name = "Devil";
		corpseName = "villainy incarnate";
		displayName = "Devil";

		letter = "D";

		colour = "<color:B40450>";
		nameColour = "0.71 0.02 0.02";

		canAbduct = false;
		canInvestigate = true;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		alignment = 1;

		helpText = 	"\c4You are also the <color:B40450>Devil\c4!  Type /inv [name] once per night to find out an Innocent's role." NL
					"\c4Use your supernatural powers to find out the identities of the Innocent Special Roles, and tell the other mafia so they can eliminate them.";
	};
}

package MM_Devil
{
	function GameConnection::MM_InvestigatePlayer(%this, %target)
	{
		if(!isObject(%target) || !isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !$DefaultMiniGame.running)
			return;

		if(nameToID(%this.role) != nameToID(MMRole_Devil))
			return parent::MM_InvestigatePlayer(%this, %target);

		if((%e = %this.MM_canInvestigate(%target)) < 1)
		{
			messageClient(%this, '', $MM::InvError[mAbs(%e)]);
			return;
		}

		messageClient(%this, '', $MM::DevilInvPattern, %target.getSimpleName(), %target.role.getColour(1) @ %target.role.getRoleName());

		%mini.MM_LogEvent(%this.MM_getName(1) SPC "\c6investigated" SPC %target.MM_getName(1));

		%this.investigated[%mini.day] = true;
	}

	function GameConnection::MM_canInvestigate(%this, %target)
	{
		%r = parent::MM_canInvestigate(%this, %target);

		if(nameToID(%this.role) == nameToID(MMRole_Devil) || (%this.MM_isMaf() && %mini.allInv))
		{
			if(%target.MM_isMaf())
				return -10;

			if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
				return 0;

			if(%mini.isDay)
				return -1;

			if(!isObject(%this.role))
				return -2;

			if(!%this.role.getCanInvestigate() && !%mini.allInv)
				return -4;

			if(%this.investigated[%mini.day])
				return -5;

			if(%this.isGhost || %this.lives < 1)
				return -6;

			if(!isObject(%target))
				return -9;

			if(nameToID(%this) == nameToID(%target))
				return -7;

			if(!isObject(%target.role))
				return -8;

			return 1;
		}

		return %r;
	}
};
activatePackage(MM_Devil);