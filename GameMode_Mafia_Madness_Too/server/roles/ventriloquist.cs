//ventriloquist.cs
//Code for the Ventriloquist mafia role

$MM::LoadedRole_Ventriloquist = true;

$MM::ImpError[0] = "";
$MM::ImpError[1] = "";
$MM::ImpError[2] = "";
$MM::ImpError[3] = "";
$MM::ImpError[4] = "";
$MM::ImpError[5] = "";
$MM::ImpError[6] = "\c4You are no longer impersonating anyone.";
$MM::ImpError[7] = "\c4That client is not part of the game!";

$MM::VentGodfatherChat = true;

if(!isObject(MMRole_Ventriloquist))
{
	new ScriptObject(MMRole_Ventriloquist)
	{
		class = "MMRole";

		name = "Ventriloquist";
		corpseName = "scandalous impersonator";
		displayName = "Ventriloquist";

		letter = "V";

		colour = "<color:404040>";
		nameColour = "0.376 0.376 0.376";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = true;
		canCommunicate = false;
		canFingerprint = false;

		alignment = 1;

		helpText = 	"\c4You are also the \c7Ventriloquist\c4!  You have the power to impersonate another's voice!" NL
					"\c4Type \"/imp\" (short for Impersonate) followed by the name of the person you want to impersonate." NL
					"\c4Any messages thereafter will appear to be from that person, but they will still come from your location!" NL
					"\c4Type \"/imp\" followed by nothing to go back to your own voice." NL
					"\c4The Ventriloquist is important for the maf, but has a more stealthy role compared to other maf.  Good luck!" NL
					"\c4NOTE: typing \"/impu\" instead will result in the target not hearing your impersonation, making your impersonation unnoticable.";
	};
}

//SUPPORT
function GameConnection::MM_canImpersonate(%this, %target)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return 0;

	if(!isObject(%this.role))
		return -1;

	if(!%this.MM_isMaf())
		return -2;

	if(!%this.role.getCanImpersonate() && !%mini.allImp)
		return -3;

	if(%this.isGhost || %this.lives < 1)
		return -4;

	if(!isObject(%target))
		return -5;

	if(nameToID(%this) == nameToID(%target))
		return -6;

	if(!isObject(%m2 = getMiniGameFromObject(%target)) || !%m2.isMM || (%target.lives < 1 && !%target.isGhost))
		return -7;

	return 1;
}

function GameConnection::MM_SetImpersonation(%this, %target, %unn)
{
	if(!isObject(%target) || !isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !$DefaultMiniGame.running)
		return;

	if((%e = %this.MM_canImpersonate(%target)) < 0)
	{
		messageClient(%this, '', $MM::ImpError[mAbs(%e)]);

		if(%e == -6)
		{
			%this.MMImpersonate = "";
			%this.MMUnnoticeable = false;
		}

		return;
	}

	%this.MMImpersonate = %target;
	%this.MMUnnoticeable = %unn;

	messageClient(%this, '', "\c4You are now impersonating\c3" SPC %target.getSimpleName() @ "\c4" @ (%unn ? " unnoticeably" : "") @ "!");
}

function serverCmdImp(%this, %v0, %v1, %v2, %v3, %v4)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !$DefaultMiniGame.running)
		return;

	%v = trim(%v0 SPC %v1 SPC %v2 SPC %v3 SPC %v4);
	%target = findClientByName(%v);

	if(%v $= "")
	{
		%this.MMImpersonate = "";
		%this.MMUnnoticeable = false;

		messageClient(%this, '', $MM::ImpError[6]);

		return;
	}

	if(!isObject(%target))
	{
		%target = findClientByBL_ID($Pref::Server::MMNicknames[%v]);

		if(!isObject(%target))
		{
			messageClient(%this, '', "\c4No client by the name of\c3" SPC %v SPC "\c4found.");
			return;
		}
	}

	%this.MM_SetImpersonation(%target, false);
}

function serverCmdImpU(%this, %v0, %v1, %v2, %v3, %v4)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !$DefaultMiniGame.running)
		return;

	%v = trim(%v0 SPC %v1 SPC %v2 SPC %v3 SPC %v4);

	if(%v $= "")
	{
		%this.MMImpersonate = "";
		%this.MMUnnoticeable = false;

		messageClient(%this, '', $MM::ImpError[6]);

		return;
	}

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

	%this.MM_SetImpersonation(%target, true);
}

function MM_ImpersonationCheck(%this, %target, %unn, %original)
{
	if(%original.MM_isMaf() == %target.MM_isMaf() || %target.isGhost || %target.lives < 1)
		return 3;

	if(nameToID(%original.MMImpersonate) == nameToID(%target) && %unn)
		return 2;

	return 0;
}

package MM_Ventriloquist
{
	function MMRole::onChat(%role, %mini, %this, %msg, %type)
	{
		%r = parent::onChat(%role, %mini, %this, %msg, %type);

		if(%type < 1)
			return %r;

		if(!isObject(%this.MMImpersonate))
		{
			if(%this.MMImpersonate !$= "")
			{
				%this.MMImpersonate = "";
				%this.MMUnnoticeable = false;

				messageClient(%this, '', "\c4Your impersonation target left the game, so it has been unset.");
			}

			return %r;
		}

		if(%this.MM_canImpersonate(%this.MMImpersonate) < 1)
		{
			%this.MMImpersonate = "";
			%this.MMUnnoticeable = false;

			messageClient(%this, '', "\c4Your impersonation target is no longer valid, so it has been unset.");

			return %r;
		}

		%pre2 = "\c4[\c6VENT\c3:\c6" @ %this.getSimpleName() @ "\c4]";
		if(%this.MMUnnoticeable)
			%pre2 = "\c4[\c6UNNOTICEABLE\c4]" @ %pre2;

		%mark = getSubStr(%msg, 0, 1);
		if(%mark $= "^")
		{
			if(!$MM::VentGodfatherChat)
			{
				messageClient(%this, '', "\c5You cannot use Godfather Chat because you are not the Godfather!  (^ is Godfather chat.)");
				
				return 1;
			}

			%rMsg = getSubStr(%msg, 1, strLen(%msg) - 1);

			if(!%this.MMImpersonate.MM_canComm())
			{
				messageClient(%this, '', "\c5Your target cannot use Godfather Chat because they are not the Godfather!  (^ is Godfather chat.)");

				return 1;
			}

			return %this.MMImpersonate.MM_GodfatherChat(%rMsg, %pre2);
		}

		%this.MMImpersonate.MM_Chat(%this.player, %type, %msg, (%this.MMUnnoticeable ? %this.MMImpersonate : ""), %pre2, MM_ImpersonationCheck, %this.MMUnnoticeable, %this);

		return 1;
	}

	function MMRole::onTeamChat(%role, %mini, %this, %msg, %type)
	{
		%r = parent::onTeamChat(%role, %mini, %this, %msg, %type);

		if(%type < 1)
			return %r;

		if(!isObject(%this.MMImpersonate))
		{
			if(%this.MMImpersonate !$= "")
			{
				%this.MMImpersonate = "";
				%this.MMUnnoticeable = false;

				messageClient(%this, '', "\c4Your impersonation target left the game, so it has been unset.");
			}

			return %r;
		}

		if(%this.MM_canImpersonate(%this.MMImpersonate) < 1)
		{
			%this.MMImpersonate = "";
			%this.MMUnnoticeable = false;

			messageClient(%this, '', "\c4Your impersonation target is no longer valid, so it has been unset.");

			return %r;
		}

		%pre2 = "\c4[\c6VENT\c3:\c6" @ %this.getSimpleName() @ "\c4]";
		if(%this.MMUnnoticeable)
			%pre2 = "\c4[\c6UNNOTICEABLE\c4]" @ %pre2;

		%mark = getSubStr(%msg, 0, 1);
		if(%mark $= "^")
		{
			if(!$MM::VentGodfatherChat)
			{
				messageClient(%this, '', "\c5You cannot use Godfather Chat because you are not the Godfather!  (^ is Godfather chat.)");
				
				return 1;
			}

			%rMsg = getSubStr(%msg, 1, strLen(%msg) - 1);

			if(!%this.MMImpersonate.MM_canComm())
			{
				messageClient(%this, '', "\c5Your target cannot use Godfather Chat because they are not the Godfather!  (^ is Godfather chat.)");

				return 1;
			}

			return %this.MMImpersonate.MM_GodfatherChat(%rMsg, %pre2);
		}

		%this.MMImpersonate.MM_Chat(%this.player, %type, %msg, (%this.MMUnnoticeable ? %this.MMImpersonate : ""), %pre2, MM_ImpersonationCheck, %this.MMUnnoticeable, %this);

		return 1;
	}

	function MMRole::onCleanup(%this, %mini, %client)
	{
		parent::onCleanup(%this, %mini, %client);

		%client.MMImpersonate = "";
		%client.MMUnnoticeable = false;
	}

	function GameConnection::MM_canComm(%this)
	{
		if(%this.role.getCanImpersonate() && isObject(%this.MMImpersonate))
			return 2;

		return parent::MM_canComm(%this);
	}
};
activatePackage(MM_Ventriloquist);