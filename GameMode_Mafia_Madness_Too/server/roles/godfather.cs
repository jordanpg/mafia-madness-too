//godfather.cs
//Code for the Godfather mafia role

$MM::LoadedRole_Godfather = true;

if(!isObject(MMRole_Godfather))
{
	new ScriptObject(MMRole_Godfather)
	{
		class = "MMRole";

		name = "Godfather";
		corpseName = "professor moriarty";
		displayName = "Godfather";

		letter = "G";

		colour = "<color:FFFFFF>";
		nameColour = "1 1 1";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = true;
		canFingerprint = false;

		alignment = 1;

		helpText = 	"\c4You are also the \c6Godfather\c4!  As the leader of the mafia, your hands are clean in all affairs!" NL
					"\c4If the <color:1122CC>Cop\c4 investigates you, he won't be able to find anything on you - you will appear \c2\Innocent\c4!" NL
					"<font:impact:32pt>\c4You are still a member of the \c0Mafia\c4 though, so don't forget it!" NL
					"\c4You can also talk to other Mafia by starting a chat message with ^" NL
					"\c4E.G. if you type \"^abduct The Titanium tonight\" all the other mafia (and noone else!) will receive the message directly." NL
					"\c4The other mafia can't respond, though, so try to use this to establish communications via secret signals.  Good luck!";

		forceInvAlignment = 0;
	};
}

//SUPPORT
function GameConnection::MM_canComm(%this)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return false;

	if(!isObject(%this.role))
		return false;

	if(!%this.MM_isMaf())
		return false;

	if(!%this.role.getCanCommunicate() && !%mini.allComm)
		return false;

	return true;
}

function MM_GodfatherCheck(%this, %target)
{
	if(!%target.MM_isMaf() && !(%target.isGhost || %target.lives < 1))
		return 2;

	return 1;
}

function GameConnection::MM_GodfatherChat(%this, %msg, %pre2)
{
	if(!(%c = %this.MM_canComm()))
	{
		if(%c == 2)
			return 1;

		messageClient(%this, '', "\c5You cannot use Godfather Chat because you are not the Godfather!  (^ is Godfather chat.)");
		
		return 1;
	}

	%pre2 = %pre2 @ "\c7[\c6Godfather\c7]";

	%this.MM_Chat(%this.player, -1, %msg, "", %pre2, MM_GodfatherCheck);

	return 1;
}

package MM_Godfather
{
	function MMRole::onChat(%role, %mini, %this, %msg, %type)
	{
		%r = parent::onChat(%role, %mini, %this, %msg, %type);

		%mark = getSubStr(%msg, 0, 1);
		if(%mark !$= "^")
			return %r;

		if(%type < 1)
			return 1;

		%rMsg = getSubStr(%msg, 1, strLen(%msg) - 1);

		return %this.MM_GodfatherChat(%rMsg);
	}
};
activatePackage(MM_Godfather);