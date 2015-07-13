//cop.cs
//Code for the basic Cop innocent role.

$MM::LoadedRole_Cop = true;

$MM::InvError[0] = "";
$MM::InvError[1] = "\c4You can only investigate someone at night!";
$MM::InvError[2] = "";
$MM::InvError[3] = "";
$MM::InvError[4] = "";
$MM::InvError[5] = "\c4You have already investigated someone tonight!";
$MM::InvError[6] = "\c4You cannot investigate someone while dead!";
$MM::InvError[7] = "\c4You cannot investigate yourself!";
$MM::InvError[8] = "\c4That client is not part of the current game!";
$MM::InvError[9] = ""; //screen this out in /inv

$MM::InvStatus[0] = '\c3%1 \c4is a perfectly \c2upstanding citizen\c4 who bears no cause for suspicion.';
$MM::InvStatus[1] = '\c3%1 \c4is a pretty \c0suspicious fellow\c4 indeed!';

if(!isObject(MMRole_Cop))
{
	new ScriptObject(MMRole_Cop)
	{
		class = "MMRole";

		name = "Cop";
		corpseName = "respectable officer of the law";
		displayName = "Cop";

		letter = "O";

		colour = "<color:1122CC>";
		nameColour = "0.066667 0.13333 0.8";

		canAbduct = false;
		canInvestigate = true;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		alignment = 0;

		helpText = 	"\c4You are also the <color:1122CC>Cop\c4!  Type /inv [name] to find out whether someone is inno or mafia!" NL
					"\c4If there's a P in the roles list however, there's a chance you might be <color:CC4444>Paranoid.\c4  But surely you aren't, right?" NL
					"\c2...Right?";
	};
}

//SUPPORT
function MM_getInvResult(%cop, %target)
{
	if(%target.MM_isMaf())
		return true;

	return false;
}

function GameConnection::MM_canInvestigate(%this, %target)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return 0;

	if(%mini.isDay)
		return -1;

	if(!isObject(%this.role))
		return -2;

	if(%this.MM_isMaf())
		return -3;

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

	if(%target.isGhost || !isObject(%target.role) || %target.lives < 1)
		return -8;

	return 1;
}

function GameConnection::MM_InvestigatePlayer(%this, %target)
{
	if(!isObject(%target) || !isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !$DefaultMiniGame.running)
		return;

	if((%e = %this.MM_canInvestigate(%target)) < 1)
	{
		messageClient(%this, '', $MM::InvError[mAbs(%e)]);
		return;
	}

	%r = MM_getInvResult(%this, %target);

	messageClient(%this, '', $MM::InvStatus[%r], %target.getSimpleName());

	%mini.MM_LogEvent(%this.MM_getName() SPC "\c6investigated" SPC %target.MM_getName());

	%this.investigated[%mini.day] = true;
}

function serverCmdInv(%this, %v0, %v1, %v2, %v3, %v4)
{
	if(!$DefaultMiniGame.running || !isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM)
		return;

	%v = trim(%v0 SPC %v1 SPC %v2 SPC %v3 SPC %v4);
	%target = findClientByName(%v);

	if(!isObject(%target))
	{
		%target = findClientByBL_ID($Pref::Server::MMNicknames[%v]);

		if(!isObject(%target))
		{
			messageClient(%this, '', "\c4No client by the name of\c3" SPC %v SPC "\c4found.");
			return;
		}
	}

	%this.MM_InvestigatePlayer(%target);
}

package MM_Cop
{
	function MMRole::onCleanup(%this, %mini, %client)
	{
		parent::onCleanup(%this, %mini, %client);

		for(%i = 0; %i <= %mini.day; %i++)
			%client.investigated[%i] = "";
	}
};
activatePackage(MM_Cop);