//chat.cs
//Handles basic local chat functionality.

$MM::LoadedChat = true;

$MM::ChatRadiusMod = 1;

function GameConnection::MM_Chat(%this, %obj, %type, %msg, %excludeList, %pre2, %condition, %a0, %a1, %a2, %a3, %a4)
{
	// MMDebug("MM_Chat" SPC %this SPC %type);
	// MMDebug(%msg);
	// MMDebug(%excludeList);

	if(%condition !$= "" && !isFunction(%condition))
	{
		%condition = "";
		%pre2 = "";
	}

	%format = '%1\c3%2\c7%3\c6: %4';

	%pre = "\c7";
	%rad = -1;

	switch(%type)
	{
		case 0:
			if(!isObject(%this.player) && %this.lives > 0)
			{
				messageClient(%this, '', "\c5You are dead! Wait until you respawn to say something.");
				return 1;
			}

			%pre = "\c7*\c6" @ (%this.isGhost ? "DEAD" : "SPEC") @ "\c7* ";

		case 1: //Normal round chat
			%rad = 32;

		case 2: //Shout 
			%pre = "\c4[\c6SHOUT\c4]\c7";
			%rad = 64;

		case 3: //Low
			%pre = "\c7[\c4Low\c7]";
			%rad = 8;

		case 4: //Whisper
			%pre = "\c4[\c6Whisper\c4]\c7";
			%rad = 2;

		case -1: //Global
			%rad = -2;

		default:
			return 2;
	}

	// MMDebug(%pre);
	// MMDebug(%rad);

	%ct = ClientGroup.getCount();

	%pre = %pre @ %this.clanPrefix;

	if(%condition $= "")
		%pre = %pre2 @ %pre;

	%pre_2 = %pre2 @ %pre;

	echo(%this.getPlayerName() SPC ":" SPC %msg);

	if(%rad <= 0)
	{
		switch(%rad)
		{
			case -1 or 0:
				for(%i = 0; %i < %ct; %i++)
				{
					%ap = false;

					%cl = ClientGroup.getObject(%i);

					if(isInList(%excludeList, %cl)) continue;

					if(%condition !$= "" && (%cr = call(%condition, %this, %cl, %a0, %a1, %a2, %a3, %a4)))
					{
						if(%cr == 2)
							continue;

						%ap = true;
					}

					if(%cl.lives < 1 || !getMiniGameFromObject(%cl).isMM)
						commandToClient(%cl, 'chatMessage', %this, '', '', %format, (%ap ? %pre_2 : %pre), %this.getSimpleName(), %this.clanSuffix, %msg);
				}

				return 0;

			case -2:
				for(%i = 0; %i < %ct; %i++)
				{
					%ap = false;

					%cl = ClientGroup.getObject(%i);

					if(isInList(%excludeList, %cl)) continue;

					if(%condition !$= "" && (%cr = call(%condition, %this, %cl, %a0, %a1, %a2, %a3, %a4)))
					{
						if(%cr == 2)
							continue;

						%ap = true;
					}

					commandToClient(%cl, 'chatMessage', %this, '', '', %format, (%ap ? %pre_2 : %pre), %this.getSimpleName(), %this.clanSuffix, %msg);
				}

				return 0;

			default:
				return 2;
		}
	}
	else if(!isObject(%p = %obj))
	{
		if(!isObject(%p = %this.player))
			return 1;
	}

	// MMDebug("Checking Radius");

	%rad *= $MM::ChatRadiusMod;

	%pos = %p.getEyePoint();

	for(%i = 0; %i < %ct; %i++)
	{
		%ap = false;

		%cl = ClientGroup.getObject(%i);

		if(isInList(%excludeList, %cl)) continue;

		if(%condition !$= "" && (%cr = call(%condition, %this, %cl, %a0, %a1, %a2, %a3, %a4)))
		{
			if(%cr == 2)
				continue;
				
			%ap = true;
		}

		if(isObject(%clp = %cl.player) && %cl.lives > 0)
		{
			%pos2 = %clp.getEyePoint();
			%dist = VectorDist(%pos2, %pos);

			// MMDebug(%dist);

			if(VectorDist(%pos2, %pos) > %rad && %cr != 3)
				continue;

			commandToClient(%cl, 'chatMessage', %this, '', '', %format, (%ap ? %pre_2 : %pre), %this.getSimpleName(), %this.clanSuffix, %msg);
		}
		else if(%cl.lives < 1 || !getMiniGameFromObject(%cl).isMM)
			commandToClient(%cl, 'chatMessage', %this, '', '', %format, (%ap ? %pre_2 : %pre), %this.getSimpleName(), %this.clanSuffix, %msg);
	}

	return 0;
}

package MM_Chat
{
	function serverCmdMessageSent(%this, %msg)
	{
		if(!$DefaultMinigame.running || !(%mini = getMiniGameFromObject(%this).isMM) || $DefaultMinigame.resolved)
			return parent::serverCmdMessageSent(%this, %msg);

		%msg = stripMLControlChars(%msg);
		%msg = trim(%msg);

		if(%msg $= "")
			return;

		if(isObject(%this.player) && !%this.player.isGhost)
		{
			if(%this.player.gagged)
				return;

			if(%this.player.isCorpse || %this.player.dying)
				return serverCmdTeamMessageSent(%this, %msg);

			%mark = getSubStr(%msg, 0, 1);
			%rMsg = getSubStr(%msg, 1, strLen(%msg) - 1);

			if(%mark $= "!" || (strCmp(strUpr(%msg), %msg) == 0 && strCmp(strLwr(%msg), %msg) != 0))
			{
				%type = 2;
				%msg = strUpr((%mark $= "!" ? %rMsg : %msg));
				if(%msg $= "")
					return;
			}
			else
				%type = 1;
		}
		else
			%type = 0;

		if(isObject(%this.role))
		{
			%e = %this.role.onChat(%mini, %this, %msg, %type);
			if(%e == 1)
				return;
			else if(%e == 2)
				return parent::serverCmdMessageSent(%this, %msg);
		}

		%r = %this.MM_Chat(%this.player, %type, %msg);

		if(%r == 2)
			return parent::serverCmdMessageSent(%this, %msg);
	}

	function serverCmdTeamMessageSent(%this, %msg)
	{
		if(!$DefaultMinigame.running || !(%mini = getMiniGameFromObject(%this).isMM) || $DefaultMinigame.resolved)
			return parent::serverCmdTeamMessageSent(%this, %msg);

		%msg = stripMLControlChars(%msg);
		%msg = trim(%msg);

		if(%msg $= "")
			return;

		if(isObject(%this.player) && !%this.player.isGhost)
		{
			%mark = getSubStr(%msg, 0, 1);
			%rMsg = getSubStr(%msg, 1, strLen(%msg) - 1);

			if(%mark $= "!")
			{
				%type = 4;
				%msg = strLwr(%rMsg);
				if(%msg $= "")
					return;
			}
			else
				%type = 3;
		}
		else
			%type = 0;

		if(isObject(%this.role))
		{
			%e = %this.role.onTeamChat(%mini, %this, %msg, %type);
			if(%e == 1)
				return;
			else if(%e == 2)
				return parent::serverCmdTeamMessageSent(%this, %msg);
		}

		%r = %this.MM_Chat(%this.player, %type, %msg);

		if(%r == 2)
			return parent::serverCmdTeamMessageSent(%this, %msg);
	}

	function serverCmdStartTalking(%this)
	{
		if(getMiniGameFromObject(%this).running)
			return;

		parent::serverCmdStartTalking(%this);
	}


	//below is commented out because it's dumb and not cool. go yell at ottosparks if it's still here in the release version.

	// function serverCmdMessageSent(%this, %msg)
	// {
	// 	if(!$DefaultMiniGame.running)
	// 		return parent::serverCmdMessageSent(%this, %msg);

	// 	%msg = stripMLControlChars(%msg);
	// 	%msg = trim(%msg);

	// 	if(%msg $= "")
	// 		return;

	// 	if(!isObject(%p = %this.player))
	// 	{
	// 		if(%this.lives > 0)
	// 		{
	// 			messageClient(%this,'',"\c5You are dead!  Wait until you respawn to say something.");
	// 			return;
	// 		}

	// 		%pre = %this.isGhost ? "DEAD" : "SPEC";
	// 		%message = "\c7*\c6" @ %pre @ "\c7*" SPC %this.clanPrefix @ "\c3" @ %this.getSimpleName() @ "\c7" @ %this.clanSuffix @ "\c6:" SPC %msg;

	// 		%ct = ClientGroup.getCount();
	// 		for(%i = 0; %i < %ct; %i++)
	// 		{
	// 			%cl = ClientGroup.getObject(%i);

	// 			if(isObject(%cl) && (%cl.lives < 1 || !getMiniGameFromObject(%cl).isMM))
	// 				messageClient(%cl, '', %message);
	// 		}
	// 		return;
	// 	}
	// 	else
	// 	{
	// 		if($Sim::Time - %mini.roundStart < 3)
	// 			return;

	// 		if(%p.gagged)
	// 			return;

	// 		if(isObject(%this.role))
	// 		{
	// 			%exit = %this.role.onChat(%mini, %this, %msg);
	// 			if(%exit)
	// 				return;
	// 		}

	// 		if(%this.MM_canImp() && isObject(%this.MMImpersonate))
	// 		{
	// 			%unn = %this.MMUnNoticeable;
	// 			%realC = %this;
	// 			%this = %this.MMImpersonate;
	// 			%imp = true;
	// 		}

	// 		%mark = getSubStr(%msg, 0, 1);

	// 		if(%mark $= "^")
	// 		{
	// 			if(%imp)
	// 			{
	// 				%imp = false;
	// 				%this = %realC;
	// 				%realC = "";
	// 				%unn = "";
	// 			}

	// 			if(!%this.MM_canComm())
	// 			{
	// 				messageClient(%this, '', "\c5You cannot use Godfather Chat because you are not the Godfather!  (^ is Godfather chat.)");
	// 				return;
	// 			}

	// 			%msg = getSubStr(%msg, 1, strLen(%msg) - 1);
	// 			%message = "\c7[\c6Godfather\c7]" SPC %this.clanPrefix @ "\c3" @ %this.getSimpleName() @ "\c7" @ %this.clanSuffix @ "\c6:" SPC %msg;
	// 			%ct = ClientGroup.getCount();
	// 			for(%i = 0; %i < %ct; %i++)
	// 			{
	// 				%cl = ClientGroup.getObject(%i);

	// 				if((%cl.lives < 1 || !getMiniGameFromObject(%cl).isMM) || (%cl.MM_isMaf() && isObject(%cl.player)))
	// 					messageClient(%cl, '', %message);
	// 			}

	// 			return;
	// 		}
	// 		else if(%mark $= "!" || (strCmp(strUpr(%msg), %msg) == 0 && strCmp(strLwr(%msg), %msg) != 0))
	// 		{
	// 			if(%mark $= "!")
	// 				%msg = strUpr(getSubStr(%msg, 1, strLen(%msg) - 1));

	// 			%rad = 64;
	// 			%message = "\c4[\c6SHOUT\c4]\c7" SPC %this.clanPrefix @ "\c3" @ %this.getSimpleName() @ "\c7" @ %this.clanSuffix @ "\c6:" SPC %msg;
	// 		}
	// 		else
	// 		{
	// 			%rad = 32;
	// 			%message = "\c7" @ %this.clanPrefix @ "\c3" @ %this.getSimpleName() @ "\c7" @ %this.clanSuffix @ "\c6:" SPC %msg;
	// 		}

	// 		// initContainerRadiusSearch(%p.getEyePoint(), %rad, $TypeMasks::PlayerObjectType);
	// 		// for(%i = 0; isObject(%obj = containerSearchNext()); %i++)
	// 		// {
	// 		// 	if(%obj.getName() !$= "botCorpse" && isObject(%cl = %obj.getControllingClient()) && getMiniGameFromObject(%cl) == %mini)
	// 		// 		%heardPlayers[%i] = %cl;
	// 		// }

	// 		// for(%i = 0; isObject(%heardPlayers[%i]); %i++)
	// 		// {
	// 		// 	%cl = %heardPlayers[%i];

	// 		// 	if(%imp)
	// 		// 	{
	// 		// 		if(%cl == %realC || %cl.MM_isMaf())
	// 		// 		{
	// 		// 			messageClient(%cl, '', (%unn ? "\c4[\c6UNNOTICEABLE\c4]" : "\c4") @ "[\c6VENT\c3:\c6" @ %realC.getSimpleName() @ "\c4]" @ %message);
	// 		// 			continue;
	// 		// 		}
	// 		// 		else if(%cl == %this && %unn)
	// 		// 			continue;
	// 		// 	}

	// 		// 	messageClient(%cl, '', %message);
	// 		// }

	// 		%ct = ClientGroup.getCount();
	// 		for(%i = 0; %i < %ct; %i++)
	// 		{
	// 			%cl = ClientGroup.getObject(%i);

	// 			if(isObject(%cl.player) && %cl.lives > 0)
	// 			{
	// 				%dist = VectorDist(%cl.player.getPosition(), %p.getPosition());
	// 				if(%dist > %rad)
	// 					continue;

	// 				if(%imp)
	// 				{
	// 					if(%cl == %realC || %cl.MM_isMaf())
	// 					{
	// 						messageClient(%cl, '', (%unn ? "\c4[\c6UNNOTICEABLE\c4]" : "\c4") @ "[\c6VENT\c3:\c6" @ %realC.getSimpleName() @ "\c4]" @ %message);
	// 						continue;
	// 					}
	// 					else if(%cl == %this && %unn)
	// 						continue;
	// 				}

	// 				messageClient(%cl, '', %message);
	// 			}
	// 			else if(%cl.lives < 1 || !getMiniGameFromObject(%cl).isMM)
	// 			{
	// 				if(%imp)
	// 					messageClient(%cl, '', (%unn ? "\c4[\c6UNNOTICEABLE\c4]" : "\c4") @ "[\c6VENT\c3:\c6" @ %realC.getSimpleName() @ "\c4]" @ %message);
	// 				else
	// 					messageClient(%cl, '', %message);
	// 			}
	// 		}

	// 		return;
	// 	}

	// 	%message = "\c7*\c6SPEC\c7*" SPC %this.clanPrefix @ "\c3" @ %this.getSimpleName() @ "\c7" @ %this.clanSuffix @ "\c6:" SPC %msg;

	// 	%ct = ClientGroup.getCount();
	// 	for(%i = 0; %i < %ct; %i++)
	// 	{
	// 		%cl = ClientGroup.getObject(%i);

	// 		if(isObject(%cl) && (%cl.lives < 1 || !getMiniGameFromObject(%cl).isMM))
	// 			messageClient(%cl, '', %message);
	// 	}
	// }

	// function serverCmdTeamMessageSent(%this, %msg)
	// {
	// 	if(!$DefaultMiniGame.running)
	// 		return parent::serverCmdTeamMessageSent(%this, %msg);

	// 	%msg = stripMLControlChars(%msg);
	// 	%msg = trim(%msg);

	// 	if(%msg $= "")
	// 		return;

	// 	if(!isObject(%p = %this.player))
	// 	{
	// 		if(%this.lives > 0)
	// 		{
	// 			messageClient(%this,'',"\c5You are dead!  Wait until you respawn to say something.");
	// 			return;
	// 		}

	// 		%pre = %this.isGhost ? "DEAD" : "SPEC";
	// 		%message = "\c7*\c6" @ %pre @ "\c7*" SPC %this.clanPrefix @ "\c3" @ %this.getSimpleName() @ "\c7" @ %this.clanSuffix @ "\c6:" SPC %msg;

	// 		%ct = ClientGroup.getCount();
	// 		for(%i = 0; %i < %ct; %i++)
	// 		{
	// 			%cl = ClientGroup.getObject(%i);

	// 			if(isObject(%cl) && (%cl.lives < 1 || !getMiniGameFromObject(%cl).isMM))
	// 				messageClient(%cl, '', %message);
	// 		}
	// 		return;
	// 	}
	// 	else
	// 	{
	// 		if($Sim::Time - %mini.roundStart < 3)
	// 			return;

	// 		if(%p.gagged)
	// 			return;

	// 		if(isObject(%this.role))
	// 		{
	// 			%exit = %this.role.onTeamChat(%mini, %this, %msg);
	// 			if(%exit)
	// 				return;
	// 		}

	// 		if(%this.MM_canImp() && isObject(%this.MMImpersonate))
	// 		{
	// 			%unn = %this.MMUnNoticeable;
	// 			%realC = %this;
	// 			%this = %this.MMImpersonate;
	// 			%imp = true;
	// 		}

	// 		%mark = getSubStr(%msg, 0, 1);

	// 		if(%mark $= "^")
	// 		{
	// 			if(%imp)
	// 			{
	// 				%imp = false;
	// 				%this = %realC;
	// 				%realC = "";
	// 				%unn = "";
	// 			}

	// 			if(!%this.MM_canComm())
	// 			{
	// 				messageClient(%this, '', "\c5You cannot use Godfather Chat because you are not the Godfather!  (^ is Godfather chat.)");
	// 				return;
	// 			}

	// 			%msg = getSubStr(%msg, 1, strLen(%msg) - 1);
	// 			%message = "\c7[\c6Godfather\c7]" SPC %this.clanPrefix @ "\c3" @ %this.getSimpleName() @ "\c7" @ %this.clanSuffix @ "\c6:" SPC %msg;
	// 			%ct = ClientGroup.getCount();
	// 			for(%i = 0; %i < %ct; %i++)
	// 			{
	// 				%cl = ClientGroup.getObject(%i);

	// 				if((%cl.lives < 1 || !getMiniGameFromObject(%cl).isMM) || (%cl.MM_isMaf() && isObject(%cl.player)))
	// 					messageClient(%cl, '', %message);
	// 			}

	// 			return;
	// 		}
	// 		else if(%mark $= "!" || %p.getName() $= "botCorpse" || %p.dying)
	// 		{
	// 			if(%mark $= "!")
	// 				%msg = strLwr(getSubStr(%msg, 1, strLen(%msg) - 1));

	// 			%rad = 1;
	// 			%message = "\c4[\c6Whisper\c4]\c7" SPC %this.clanPrefix @ "\c3" @ %this.getSimpleName() @ "\c7" @ %this.clanSuffix @ "\c6:" SPC %msg;
	// 		}
	// 		else
	// 		{
	// 			%rad = 8;
	// 			%message = "\c7[\c4Low\c7]" SPC @ %this.clanPrefix @ "\c3" @ %this.getSimpleName() @ "\c7" @ %this.clanSuffix @ "\c6:" SPC %msg;
	// 		}

	// 		// initContainerRadiusSearch(%p.getEyePoint(), %rad, $TypeMasks::PlayerObjectType);
	// 		// for(%i = 0; isObject(%obj = containerSearchNext()); %i++)
	// 		// {
	// 		// 	if(%obj.getName() !$= "botCorpse" && isObject(%cl = %obj.getControllingClient()) && getMiniGameFromObject(%cl) == %mini)
	// 		// 		%heardPlayers[%i] = %cl;
	// 		// }

	// 		// for(%i = 0; isObject(%heardPlayers[%i]); %i++)
	// 		// {
	// 		// 	%cl = %heardPlayers[%i];

	// 		// 	if(%imp)
	// 		// 	{
	// 		// 		if(%cl == %realC || %cl.MM_isMaf())
	// 		// 		{
	// 		// 			messageClient(%cl, '', (%unn ? "\c4[\c6UNNOTICEABLE\c4]" : "\c4") @ "[\c6VENT\c3:\c6" @ %realC.getSimpleName() @ "\c4]" @ %message);
	// 		// 			continue;
	// 		// 		}
	// 		// 		else if(%cl == %this && %unn)
	// 		// 			continue;
	// 		// 	}

	// 		// 	messageClient(%cl, '', %message);
	// 		// }

	// 		%ct = ClientGroup.getCount();
	// 		for(%i = 0; %i < %ct; %i++)
	// 		{
	// 			%cl = ClientGroup.getObject(%i);

	// 			if(isObject(%cl.player) && %cl.lives > 0)
	// 			{
	// 				%dist = VectorDist(%cl.player.getPosition(), %p.getPosition());
	// 				if(%dist > %rad)
	// 					continue;

	// 				if(%imp)
	// 				{
	// 					if(%cl == %realC || %cl.MM_isMaf())
	// 					{
	// 						messageClient(%cl, '', (%unn ? "\c4[\c6UNNOTICEABLE\c4]" : "\c4") @ "[\c6VENT\c3:\c6" @ %realC.getSimpleName() @ "\c4]" @ %message);
	// 						continue;
	// 					}
	// 					else if(%cl == %this && %unn)
	// 						continue;
	// 				}

	// 				messageClient(%cl, '', %message);
	// 			}
	// 			else if(%cl.lives < 1 || !getMiniGameFromObject(%cl).isMM)
	// 			{
	// 				if(%imp)
	// 					messageClient(%cl, '', (%unn ? "\c4[\c6UNNOTICEABLE\c4]" : "\c4") @ "[\c6VENT\c3:\c6" @ %realC.getSimpleName() @ "\c4]" @ %message);
	// 				else
	// 					messageClient(%cl, '', %message);
	// 			}
	// 		}

	// 		return;
	// 	}

	// 	//wtf is the point of the rest here? nothing ... that's what buster ...
	// 	%message = "\c7*\c6SPEC\c7*" SPC %this.clanPrefix @ "\c3" @ %this.getSimpleName() @ "\c7" @ %this.clanSuffix @ "\c6:" SPC %msg;

	// 	%ct = ClientGroup.getCount();
	// 	for(%i = 0; %i < %ct; %i++)
	// 	{
	// 		%cl = ClientGroup.getObject(%i);

	// 		if(isObject(%cl) && (%cl.lives < 1 || !getMiniGameFromObject(%cl).isMM))
	// 			messageClient(%cl, '', %message);
	// 	}
	// }
};
activatePackage(MM_Chat);