//MM_Core.cs
//Handles core MM functionality such as day/night cycles, round control, role assignment, etc.
//Does not handle role-related functionality. See MM_Role.cs or the roles folder for that.

$MM::LoadedCore = true;

$MM::DediRoundDelay = 30000;

$MM::GPDeadRising = 4;

$MM::DMEquipment[0] = nameToID(TrenchKnifeItem);
// $MM::DMEquipment[1] = nameToID(TommyGunItem);

$MM::MafListSetting = 1;

package disableToolCmds { function serverCmdDuplicator() { } function serverCmdFillcan() { } function serverCmdUsePrintGun() { } };

function MM_clearCorpses()
{
	while(isObject(botCorpse))
		botCorpse.delete();
}

function MM_clearForceRoles()
{
	%ct = ClientGroup.getCount();
	for(%i = 0; %i < %ct; %i++)
		ClientGroup.getObject(%i).forceRole = "";
}

function MM_onDestroy()
{
	echo("Clearing MMT roles...");
	MMRoles.delete();
	deleteVariables("$MM::RoleKey*");
}

function MinigameSO::MM_Init(%this)
{
	if(isObject(MMRoles))
		%this.rolesGrop = MMRoles;
	else
		%this.rolesGroup = new SimGroup(MMRoles);

	%this.allAbduct = false;
	%this.allComm = false;
	%this.allImp = false;
	%this.allInv = false;
	%this.allFingerprint = false;

	%this.gameMode = $MM::DefaultGameMode;

	%this.isMM = true;

	deactivatePackage(advanceLight);
	deactivatePackage(action);
	deactivatepackage(grenadedroppackage);
	deactivatepackage(grenadebagpackage);
	activatepackage(disableToolCmds);
	gunProjectile.directDamage = 100;
	gunProjectile.muzzleVelocity = 200;

	%this.afterLifeMode = -1;

	if($Pref::Server::MMAfterLifeMode $= "")
	{
		if($Pref::Server::MMAfterLifeLoc !$= "")
		{
			%this.afterLifeLoc = $Pref::Server::MMAfterLifeLoc;
			%this.afterLifeMode = 0;
		}

		if($Pref::Server::MMAfterLifeBox !$= "")
		{
			%this.afterLifeBox = $Pref::Server::MMAfterLifeLoc;
			%this.afterLifeMode = 1;
		}

		if($Pref::Server::MMAfterLifeBrick !$= "" && isObject($Pref::Server::MMAfterLifeBrick))
		{
			%this.afterLifeBrick = $Pref::Server::MMAfterLifeBrick;
			%this.afterLifeMode = 2;
		}
	}
	else
		%this.afterLifeMode = $Pref::Server::MMAfterLifeMode;
}

function MinigameSO::MM_GetNumPlayers(%this)
{
	%ct = 0;
	for(%i = 0; %i < %this.numMembers; %i++)
	{
		if(!%this.member[%i].MMIgnore)
			%ct++;
	}

	return %ct;
}

function MinigameSO::MM_SetRole(%this, %client, %role)
{
	if(!isObject(%client))
		return false;

	if(isObject(%client.role))
		%client.role.onCleanup(%this, %client);

	if(!isObject(%role) || %client.MMIgnore)
	{
		if(%role $= "" || %client.MMIgnore)
		{
			%client.role = "";
			%this.role[%client] = "";
			%client.knowsFullRole = false;
			return true;
		}

		if(!isObject(%role = $MM::RoleKey[%role]))
			return false;
	}

	%client.role = %role;
	%this.role[%client] = %role;
	%this.memberCache[%this.memberCacheLen | 0] = %client;
	%this.memberCacheName[%this.memberCacheLen | 0] = %client.getPlayerName();
	%this.memberCacheRole[%this.memberCacheLen | 0] = %role;
	%this.memberCacheKey[%client] = %this.memberCacheLen | 0;
	%this.memberCacheLen++; 

	%client.knowsFullRole = false;

	%role.onAssign(%this, %client);

	if(%this.running)
		%this.MM_WinCheck();

	return true;
}

function GameConnection::MM_SetRole(%this, %role) //just a nice lil shortcut
{
	if(!isObject(%mini = getMiniGameFromObject(%this)))
		return false;

	return %mini.MM_SetRole(%this, %role);
}

function MinigameSO::MM_ResetVals(%this)
{
	%this.allAbduct = false | $MM::AllAbduct;
	%this.allComm = false | $MM::AllComm;
	%this.allImp = false | $MM::AllImp;
	%this.allInv = false | $MM::AllInv;
	%this.allFingerprint = false | $MM::AllFingerprint;

	%this.roles = "";

	%this.forcedARole = false;
	%this.forcedRoleStr = "";

	%this.MM_ClearRoles();
}

function MinigameSO::MM_ClearEventLog(%this)
{
	for(%i = 0; %i < %this.eventLogLen; %i++)
		%this.eventLog[%i] = "";

	%this.eventLogLen = 0;

	for(%i = 0; %i < %this.numMembers; %i++)
		%this.member[%i].MM_ClearGunLog();
}

function MinigameSO::MM_LogEvent(%this, %str)
{
	%this.eventLog[%this.eventLogLen | 0] = (%this.isDay ? "\c6" : "\c7") @ "(" @ %this.MM_getTime() @ ")" SPC %str;
	%this.eventLogLen++;
}

function MinigameSO::MM_ChatEventLog(%this, %cl, %search)
{
	for(%i = 0; %i < %this.eventLogLen; %i++)
	{
		if(%search !$= "" && striPos(%this.eventLog[%i], %search) == -1)
			continue;

		if(isObject(%cl))
			messageClient(%cl, '', %this.eventLog[%i]);
		else
			messageAll('', %this.eventLog[%i]);
	}
}

function MinigameSO::MM_AssignRoles(%this)
{
	if(%this.roles $= "")
	{
		MMDebug("Attempt to call MM_AssignRoles with empty roles list!", %this);
		return;
	}

	%roles = %this.roles;

	for(%i = 0; %i < %this.numMembers; %i++)
	{
		%client = %this.member[%i];

		if(!isObject(%client))
			continue;

		if(%client.MMIgnore)
		{
			%client.lives = 0;

			continue;
		}

		if(%client.forceRole !$= "")
		{
			if(%this.MM_SetRole(%client, %client.forceRole))
			{
				%rl = %client.forceRole;
				if(!isObject(%rl))
					%rl = $MM::RoleKey[%client.forceRole];

				MMDebug("Init client" SPC %client SPC "with role" SPC %role);

				%client.lives = 1 + (!$MM::GPNoExtraLives ? %rl.additionalLives : 0);
				%client.isGhost = false;

				%this.forcedARole = true;

				continue;
			}
		}

		%r = getRandom(getWordCount(%roles) - 1);

		%role = getWord(%roles, %r);
		%roles = removeWord(%roles, %r);

		%s = %this.MM_SetRole(%client, %role);

		if(!%s)
			MMDebug("Client" SPC %client SPC "was unable to receive role" SPC %role @ "! A bug, maybe?", %this, %client);

		MMDebug("Init client" SPC %client SPC "with role" SPC %role);

		%rl = %role;
		if(!isObject(%rl))
			%rl = $MM::RoleKey[%rl];

		%client.lives = 1 + (!$MM::GPNoExtraLives ? %rl.additionalLives : 0);
		%client.isGhost = false;
	}
}

function MinigameSO::MM_InitRound(%this)
{
	cancel(%this.MMNextGame);

	if(%this.running)
		return;

	if(!%this.isMM)
		%this.MM_Init();

	%this.MM_ResetVals(); //doing this here for administration between rounds
	%this.MM_ClearEventLog();

	if(%this.MM_GetNumPlayers() < 1)
	{
		if(%this.MMDedi)
			%this.MMNextGame = %this.schedule($MM::DediRoundDelay, MM_InitRound);

		return;
	}

	%mode = %this.MM_GetGameMode();

	// echo(%mode);

	MMDebug("MM_InitRound" SPC %this SPC %mode, %this);

	if(isFunction(%f = "MM_InitMode" @ %mode))
		call(%f, %this);
	else
	{
		MMDebug("Gamemode" SPC %this.gameMode SPC "did not resolve to a valid mode! (" @ %mode @ ")", %this);
		return;
	}

	if(%this.roles $= "")
	{
		MMDebug("Gamemode" SPC %mode SPC "didn\'t build a proper roles list!", %this);
		return;
	}

	%this.roundStart = $Sim::Time;

	%this.allAbduct |= $MM::AllAbduct;
	%this.allComm |= $MM::AllComm;
	%this.allImp |= $MM::AllImp;
	%this.allInv |= $MM::AllInv;
	%this.allFingerprint |= $MM::AllFingerprint;

	%this.MM_AssignRoles();

	%this.running = true;

	%this.time = 0;
	%this.day = 0;
	%this.isDay = false;

	%this.reset(0);

	%this.MM_DayCycle(1);

	if(%this.allAbduct)
		messageAll('', "<color:FF0000><font:impact:32pt>All mafia can abduct this round.  Just try to survive.");
	if(%this.allComm)
		messageAll('', "<color:FF0000><font:impact:32pt>All mafia can use the Godfather chat this round.  Good luck with that.");
	if(%this.allInv)
		messageAll('', "<color:00FF00><font:impact:32pt>All innocent can investigate this round. Try hiding now.");
	if(%this.allImp)
		messageAll('', "<color:FF0000><font:impact:32pt>All mafia can impersonate this round. That\'ll be fun.");
	if(%this.allFingerprint)
		messageAll('', "<color:00FF00><font:impact:32pt>All innocent can examine fingerprints this round. Hope you have gloves.");
}

function MinigameSO::MM_ClearRoles(%this)
{
	for(%i = 0; %i < %this.numMembers; %i++)
		%this.MM_SetRole(%this.member[%i], "");

	for(%i = 0; %i < %this.memberCacheLen; %i++)
	{
		%c = %this.memberCache[%i];
		%this.memberCacheKey[%c] = "";
		%this.memberCache[%i] = "";
		%this.memberCacheName[%i] = "";
		%this.memberCacheRole[%i] = "";
	}

	%this.memberCacheLen = 0;
}

function MinigameSO::MM_Stop(%this)
{
	MMDebug("Ending MM round", %this);

	%this.running = false;

	%this.doombot = false;
	%this.resolved = false;

	cancel(%this.timeLoop);
	$EnvGuiServer::DayCycleEnabled = 0;
	DayCycle.setEnabled(false);
	

	MMDebug("Clearing corpses", %this);
	MM_clearCorpses();

	talk("The Mafia Madness game is now over.");

	MMDebug("Destroying event log", %this);
	%this.MM_ChatEventLog();

	talk("DM until the next round starts!");

	MMDebug("Resetting minigame", %this);
	%this.reset(0);

	MMDebug("Clearing bottomprints", %this);
	for(%i = 0; %i < %this.numMembers; %i++)
		bottomPrint(%this.member[%i], "", 0);

	if(%this.MMDedi)
	{
		MMDebug("Scheduling next game", %this);
		%this.MMNextGame = %this.schedule($MM::DediRoundDelay, MM_InitRound);
	}
}

function MinigameSO::MM_Time(%this, %day)
{
	if(%this.isDay == %day)
		return;

	if(%day)
		%this.day++;

	if(%day)
	{
		%this.isDay = true;

		%this.MM_onDay();
	}
	else
	{
		%this.isDay = false;

		%this.MM_onNight();
	}

	for(%i = 0; %i < %this.numMembers; %i++)
	{
		%cl = %this.member[%i];

		if(%day)
		{
			%cl.applyBodyParts();
			%cl.applyBodyColors();

			if(isObject(%cl.role))
				%cl.role.onDay(%this, %cl);
		}
		else
		{
			%cl.applyMMSilhouette();

			if(isObject(%cl.role))
				%cl.role.onNight(%this, %cl);
		}
	}

	%this.MM_onTimeTransition();
}

function MinigameSO::MM_DayCycle(%this, %day)
{
	if(%this.isDay == %day)
	{
		MMDebug("MM_DayCycle: you broke something.", %this);
		return;
	}

	cancel(%this.timeLoop);

	%this.MM_Time(%day);

	$EnvGuiServer::DayCycleEnabled = true;
	DayCycle.setEnabled(true);
	if(%day)
	{
		$EnvGuiServer::DayCycleFile = "Add-Ons/DayCycle_MafiaMadness/MMDay.daycycle";
		loadDayCycle($EnvGuiServer::DayCycleFile);
		$EnvGuiServer::DayLength = 315;
		DayCycle.setDayLength($EnvGuiServer::DayLength);
		setDayCycleTime(0);

		%this.timeLoop = %this.schedule(DayCycle.DayLength * 1000 * 0.6, MM_DayCycle, 0);
	}
	else
	{
		$EnvGuiServer::DayCycleFile = "Add-Ons/DayCycle_MafiaMadness/MMNight.daycycle";
		loadDayCycle($EnvGuiServer::DayCycleFile);
		// talk(calculateDayCycleTime());
		$EnvGuiServer::DayLength = 210;
		DayCycle.setDayLength($EnvGuiServer::DayLength);
		setDayCycleTime(0);

		%this.timeLoop = %this.schedule(DayCycle.DayLength * 1000 * 0.6, MM_DayCycle, 1);
	}
}

function MinigameSO::MM_onDay(%this)
{
	%suffix = getDaySuffix(%this.day);

	messageAll('', "\c2It is now \c3Dawn\c2 of the\c3" SPC %this.day @ %suffix SPC "\c2day.");
	%this.MM_LogEvent("\c4Dawn\c6 of the\c4" SPC %this.day @ %suffix SPC "\c6day.");
}

function MinigameSO::MM_onNight(%this)
{
	%suffix = getDaySuffix(%this.day);

	messageAll('', "\c2It is now the \c3Night\c2 of the\c3" SPC %this.day @ %suffix SPC "\c2day.");
	%this.MM_LogEvent("\c4Night\c6 of the\c4" SPC %this.day @ %suffix SPC "\c6day.");
}

function MinigameSO::MM_onTimeTransition(%this)
{
	if(%this.day >= $MM::GPDeadRising && $MM::GPDeadRising > 0)
		%this.MM_RaiseDead();
	else if((%this.day + 1) == $MM::GPDeadRising && $MM::GPDeadRising > 0 && !%this.isDay)
		messageAll('',"<color:CC2222>The dead rise at dawn...");
}

function MinigameSO::MM_Res(%this, %cl, %tCl)
{
	if(isObject(%tCl))
		%corpse = %tCl.corpse;
	else
		%corpse = %cl.corpse;

	if(isObject(%cl) && (isObject(%corpse) || (%cl.bl_id == getNumKeyID() && !isObject(%targClient))) && %cl.lives < 1)
	{
		if(isObject(%cl.player))
		{
			if(%cl.player.getName() $= "botCorpse")
				return;

			%cl.player.delete();
		}

		%cl.isGhost = false;
		%cl.lives = 1 + %cl.role.additionalLives;

		if(isObject(%corpse))
		{
			%cl.createPlayer(%corpse.getTransform());
			%corpse.delete();
		}
		else
			%cl.createPlayer(%cl.getControlObject().getTransform());

		if(isObject(%cl.player))
		{
			%cl.player.setShapeNameDistance(13.5);
			%cl.MM_GiveEquipment();
		}

		if(isObject(%cl.role))
		{
			%cl.role.onSpawn(%this, %cl);
			%cl.MM_UpdateUI();
			%cl.MM_DisplayStartText();
		}
	}
}

function MinigameSO::MM_Rise(%this, %cl, %tCl)
{
	if(isObject(%tCl))
		%corpse = %tCl.corpse;
	else
		%corpse = %cl.corpse;

	if(isObject(%cl) && isObject(%corpse))
	{
		if(isObject(%cl.player) && %cl.player.isGhost)
			%cl.player.delete();

		%cl.player = %corpse;
		%corpse.client = %cl;
		%cl.setControlObject(%corpse);
	}
}

function MinigameSO::MM_RaiseDead(%this)
{
	messageAll('',"<color:333333>The dead rise again...");
	%this.MM_LogEvent("<color:333333>The dead rise again...");

	for(%i = 0; %i < %this.numMembers; %i++)
		%this.MM_Rise(%this.member[%i]);
}

function MinigameSO::MM_WinCheck(%this, %killed, %killer)
{
	if(%this.resolved)
		return;

	MMDebug("Checking for wins...", %this, %killed, %killer);

	for(%i = 0; %i < %this.numMembers; %i++)
	{
		%cl = %this.member[%i];

		if(isObject(%cl.role))
		{
			MMDebug("Calling special win check for" SPC %cl.role.getRoleName());
			%r = %cl.role.SpecialWinCheck(%this, %cl, %killed, %killer);

			switch(%r)
			{
				case 1: %foundInno = true;
				case 2: %foundMaf = true;
				case 3: continue;
				case 4: return;
			}
		}
	}

	for(%i = 0; %i < %this.numMembers; %i++)
	{
		%cl = %this.member[%i];

		if(!isObject(%cl.role))
			continue;

		if(%cl.role.getAlignment() < 0 || %cl.role.getAlignment() > 1)
			continue;

		if(%cl.lives > 0 && !%cl.player.dying)
		{
			// if(isObject(%cl.role))
			// {
			// 	%r = %cl.role.SpecialWinCheck(%this, %cl, %killed, %killer);

			// 	switch(%r)
			// 	{
			// 		case 1:
			// 			%foundInno = true;
			// 			if(%foundMaf)
			// 				break;
			// 		case 2:
			// 			%foundMaf = true;
			// 			if(%foundInno)
			// 				break;
			// 		case 3:
			// 			continue;
			// 		case 4:
			// 			return;
			// 	}
			// }

			if(%cl.MM_isMaf())
			{
				%foundMaf = true;
				if(%foundInno)
					break;
			}
			else
			{
				%foundInno = true;
				if(%foundMaf)
					break;
			}
		}
	}

	if(!%foundMaf && %foundInno)
	{
		talk("The last Mafia is dead!  The Innocents have won.");
		MMDebug("Inno win", %this, %killed, %killer);

		%this.resolved = 1;
		%this.schedule(3000, MM_Stop);
	}
	else if(%foundMaf && !%foundInno)
	{
		talk("The last Innocent is dead!  The Mafia have won.");
		MMDebug("Maf win", %this, %killed, %killer);

		%this.resolved = 1;
		%this.schedule(3000, MM_Stop);
	}
	else if(!%foundMaf && !%foundInno)
	{
		talk("Everyone is dead.  The game has ended in a draw.");
		MMDebug("Tie", %this, %killed, %killer);

		%this.resolved = 1;
		%this.schedule(3000, MM_Stop);
	}
}

function MinigameSO::MM_GetMafList(%this)
{
	%list = "";
	for(%i = 0; %i < %this.memberCacheLen; %i++)
	{
		%mem = %this.memberCache[%i];

		%r = %this.memberCacheRole[%i];

		if(%has[%mem])
			continue;

		%has[%mem] = true;

		if(!isObject(%mem) && %r.getAlignment() == 1)
			%list = %list SPC %mem;
		else if(isObject(%mem) && %mem.MM_isMaf())
			%list = %list SPC %mem;
	}

	return trim(%list);
}

function MinigameSO::MM_getRolesList(%this)
{
	if(!%this.forcedARole)
		return %this.roles;


	if(%this.forcedRoleStr $= "")
	{
		%str = "";

		for(%i = 0; %i < %this.memberCacheLen; %i++)
			%str = %str SPC %this.memberCacheRole[%i];

		%str = trim(%str);

		//sort 'em for consistency

		%mStr = "";
		%iStr = "";

		%ct = getWordCount(%str);
		for(%i = 0; %i < %ct; %i++)
		{
			%r = getWord(%str, %i);


			%alg = %r.getAlignment();
			if(%alg == 1)
				%mStr = %mStr SPC %r.getLetter();
			else
			{
				// if(%currAlg $= "" || %currAlg != %alg)
				// {
				// 	%iStr = %iStr @ $MM::AlignmentColour[%alg];
				// 	%currAlg = %alg;
				// }

				%iStr = %iStr SPC %r.getLetter();
			}
		}

		return (%this.forcedRoleStr = trim(%mStr) SPC trim(%iStr));
	}

	return %this.forcedRoleStr;
}

function MinigameSO::MM_getTime(%this)
{
	if(!%this.running)
		return -1;

	return getTimeString(mFloatLength($Sim::Time - %this.roundStart, 0));
}

function MinigameSO::MM_getPeriodIndex(%this)
{
	return (%this.day - 1) * 2 + (!%this.isDay | 0);
}

function GameConnection::MM_GetName(%this, %forceNorm, %noLetter)
{
	%pre = "";
	if(isObject(%this.role))
		%pre = %this.role.getColour(%forceNorm);

	return %pre @ %this.getSimpleName() @ ((!%noLetter && isObject(%this.role)) ? " \c6(\c3" @ %this.role.getLetter() @ "\c6)" : "");
}

function GameConnection::MM_isMaf(%this)
{
	if(!isObject(%this.role))
		return false;

	return %this.role.getAlignment() == 1;
}

function GameConnection::MM_UpdateUI(%client)
{
	if(!isObject(%client.role) || %client.isGhost || %client.lives < 1)
	{
		bottomPrint(%client, "", 0);
		return;
	}

	%role = %client.role.getColour(%client.knowsFullRole) @ (%client.knowsFullRole ? %client.role.getRoleName() : %client.role.getDisplayName());

	%client.bottomPrint("\c5You are:" SPC %role SPC "<just:right>\c5ROLES\c6 (" @ getMiniGameFromObject(%client).MM_GetNumPlayers() @ "):" SPC MM_ColourCodeRoles(%client.minigame.MM_getRolesList()) @ " ");
}

function GameConnection::MM_DisplayMafiaList(%this, %centrePrint)
{
	%mini = getMiniGameFromObject(%this);
	if(!isObject(%mini) || !%mini.running)
		return;

	if(%centrePrint $= "")
		%centrePrint = $MM::MafListSetting | 0;

	if(%centrePrint != 2)
		messageClient(%this, '', "\c0--");

	%cStr = "";

	%list = %mini.MM_GetMafList();
	%ct = getWordCount(%list);
	for(%i = 0; %i < %ct; %i++)
	{
		%cl = getWord(%list, %i);

		if(!isObject(%r = %mini.role[%cl]))
			continue;

		%str = (isObject(%cl) ? %cl.MM_GetName(false, true) : %mini.memberCacheName[%mini.memberCacheKey[%cl]]) SPC "(" @ %r.getLetter() @ ")";

		if(%centrePrint != 2)
			messageClient(%this, '', %str);

		if(%centrePrint)
			%cStr = %cStr NL %str @ " ";
	}

	if(%centrePrint != 2)
		messageClient(%this, '', "\c0--");

	if(%centrePrint)
		%this.centerPrint("<just:right><font:verdana:18>\c0Mafia List\c6:\n<font:verdana:16>" @ trim(%cStr) @ " ");
	else
		%this.centerPrint("");
}

function GameConnection::MM_DisplayAlignmentDetails(%this, %alignment)
{
	if(%alignment == 0)
	{
		messageClient(%this, '', "\c4You are \c2Innocent\c4!  You don't know who the mafia are, but you must find out and kill them!");

		return 0;
	}
	else if(%alignment == 1)
	{
		messageClient(%this, '', "\c4You are the \c0Mafia\c4!  You must kill all the innocents.");
		messageClient(%this, '', "\c4If all of the mafia die, you lose.  You can type \c3/mafList\c4 to see the mafia again, and anyone not on the list is innocent.  Good luck!");

		%this.schedule(0, MM_DisplayMafiaList);

		if($MM::MafListSetting > 0)
			%this.schedule(10000, MM_DisplayMafiaList, 2);

		return 1;
	}

	return -1;
}

function GameConnection::MM_DisplayStartText(%this)
{
	%mini = getMiniGameFromObject(%this);
	if(!isObject(%mini) || !%mini.running || !isObject(%this.role))
		return;

	// if(!%this.MM_isMaf())
	// 	messageClient(%this, '', "\c4You are \c2Innocent\c4!  You don't know who the mafia are, but you must find out and kill them!");
	// else
	// {
	// 	messageClient(%this, '', "\c4You are the \c0Mafia\c4!  You must kill all the innocents.  Here is a full list of the members of the mafia: ");
	// 	messageClient(%this, '', "\c0--");

	// 	messageClient(%this, '', "\c0--");
	// 	messageClient(%this, '', "\c4If all of the mafia die, you lose.  You can type \c3/mafList\c4 to see it again, and anyone not on this list is innocent.  Good luck!");
	// }

	%this.MM_DisplayAlignmentDetails(%this.role.getAlignment());

	%this.messageLines(%this.role.getHelpText());

	%this.centerprint("<font:impact:32pt><color:00FF00>Your role has been delivered.  Look at the chat for a description.", 10);
	%this.schedule(500, centerprint, "<font:impact:32pt><color:00FFFF>Your role has been delivered.  Look at the chat for a description.", 9.5);
	%this.schedule(1000, centerprint, "<font:impact:32pt><color:00FF00>Your role has been delivered.  Look at the chat for a description.", 9);
	%this.schedule(1500, centerprint, "<font:impact:32pt><color:00FFFF>Your role has been delivered.  Look at the chat for a description.", 8.5);
	%this.player.setWhiteOut(0.75);

	// if(%this.MM_isMaf())
	// 	%this.MM_DisplayMafiaList();
}

function GameConnection::MM_GiveDMEquipment(%this)
{
	if(!isObject(%this.player) || $DefaultMinigame.running)
		return;

	%this.player.MM_AddGun(%this.gun | 0);

	%ct = %this.player.getDatablock().maxTools - 1;

	for(%i = 0; %i < %ct; %i++)
	{
		if(!isObject($MM::DMEquipment[%i]))
			continue;

		%this.player.tool[%i + 1] = $MM::DMEquipment[%i];
		messageClient(%this, 'MsgItemPickup', '', %i + 1, $MM::DMEquipment[%i]);
	}
}

function GameConnection::MM_GiveEquipment(%this)
{
	if(!$DefaultMinigame.running)
	{
		%this.MM_GiveDMEquipment();
		return;
	}

	if(!isObject(%this.player))
		return;

	if(!isObject(%this.role))
		return;	

	if(!isObject(%this.role.gun) && %this.role.gun != -1)
		%this.player.MM_AddGun(%this.gun);
	else if(%this.role.gun != -1)
	{
		%this.player.tool[0] = %this.role.gun;
		messageClient(%this, 'MsgItemPickup', '', 0, %this.role.gun);
	}
	else
	{
		%this.player.tool[0] = 0;
		messageClient(%this, 'MsgItemPickup', '', 0, 0);
	}

	%ct = %this.player.getDatablock().maxTools - 1;

	for(%i = 0; %i < %ct; %i++)
	{
		if(!isObject(%this.role.equipment[%i]))
			continue;

		%this.player.tool[%i + 1] = %this.role.equipment[%i];
		messageClient(%this, 'MsgItemPickup', '', %i + 1, %this.role.equipment[%i]);
	}
}

function GameConnection::applyMMSilhouette(%this)
{
	if(%this.isGhost || %this.lives < 1)
		return;

	%player = %this.player;

	if(!isObject(%player)) {
		return;
	}
	if(%player.getName() $= "botCorpse") {
		%player.setNodeColor("ALL","1 0 0 1");
	}
	else {
		%player.setNodeColor("ALL","0.0 0.0 0.0 1.0");
	}
	if(%player.doombot) {
		%player.unHideNode("ALL");
		%player.setFaceName("smiley");
		%player.setDecalName("AAA-None");
		return;
	}
	if(fileName(%player.getDatablock().shapeFile) !$= "m.dts") {
		return;
	}
	%player.hideNode("ALL");
	
	if(isObject((%o = %player.getControlObject())) && %o.getDatablock().getName() $= "SkiVehicle") {
		%player.unHideNode("lski");
		%player.unHideNode("rski");
	}
	%player.unHideNode("headSkin");
	%player.unHideNode("chest");
	%player.unHideNode("pants");
	%player.unHideNode("LShoe");
	%player.unHideNode("RShoe");
	%player.unHideNode("LArm");
	%player.unHideNode("RArm");
	%player.unHideNode("LHand");
	%player.unHideNode("RHand");
	%player.setFaceName("smiley");
	%player.setDecalName("AAA-None");
}

function MM_ColourCodeRoles(%str)
{
	%nStr = "";
	%currAlg = "";
	%ct = getWordCount(%str);
	for(%i = 0; %i < %ct; %i++)
	{
		%rw = getWord(%str, %i);

		%r = $MM::RoleKey[%rw];
		if(!isObject(%r))
		{
			%nStr = %nStr SPC "\c6" @ %rw;
			continue;
		}

		%alg = %r.getAlignment();
		if(%currAlg $= "" || %currAlg !$= %alg)
		{
			%nStr = %nStr @ $MM::AlignmentColour[%alg];
			%currAlg = %alg;
		}

		%nStr = %nStr SPC %rw;
	}

	return %nStr;
}

package MM_Core
{
	// function MinigameSO::onAdd(%this)
	// {
	// 	parent::onAdd(%this);

	// 	if(%this == $DefaultMiniGame)
	// 	{
	// 		%this.MM_Init();
	// 	}
	// }

	function GameConnection::applyBodyParts(%this)
	{
		if(!isObject(%mini = getMiniGameFromObject(%this)) || !$DefaultMinigame.running)
			return parent::applyBodyParts(%this);

		%p = %this.player;

		if(isObject(%p))
		{
			if(%p.getName() $= "botCorpse")
			{
				%this.applyMMSilhouette();
				%p.setNodeColor("ALL", "1 0 0 1");

				return;
			}
			else if(%p.doombot)
			{
				%p.unHideNode("ALL");
				%p.setNodeColor("ALL", "0 0 0 1");
				%p.setFaceName("smiley");
				%p.setDecalName("AAA-None");

				return;
			}

			if(isObject(%this.role))
			{
				%r = %this.role.applyOutfit(%mini, %this, %mini.isDay);

				if(%r)
					return;
			}
		}

		if(%this.isGhost || %this.lives < 1)
			return parent::applyBodyParts(%this);

		if(%mini.running && !%mini.isDay)
			return;

		parent::applyBodyParts(%this);
	}

	function GameConnection::applyBodyColors(%this)
	{
		if(!isObject(%mini = getMiniGameFromObject(%this)) || !$DefaultMinigame.running)
			return parent::applyBodyColors(%this);

		%p = %this.player;

		if(isObject(%p))
		{
			if(%p.getName() $= "botCorpse")
			{
				%this.applyMMSilhouette();
				%p.setNodeColor("ALL", "1 0 0 1");

				return;
			}
			else if(%p.doombot)
			{
				%p.unHideNode("ALL");
				%p.setNodeColor("ALL", "0 0 0 1");
				%p.setFaceName("smiley");
				%p.setDecalName("AAA-None");

				return;
			}

			if(isObject(%this.role))
			{
				%r = %this.role.applyOutfit(%mini, %this, %mini.isDay);

				if(%r)
					return;
			}
		}

		if(%this.isGhost || %this.lives < 1)
			return parent::applyBodyColors(%this);

		if(%mini.running && !%mini.isDay)
			%this.applyMMSilhouette();
		else
			parent::applyBodyColors(%this);
	}

	function GameConnection::onDeath(%this, %srcObj, %srcClient, %damageType, %loc)
	{
		if(!isObject(%mini = getMiniGameFromObject(%this)))
			return parent::onDeath(%this, %srcObj, %srcClient, %damageType, %loc);

		if(!%mini.running || !%mini.isMM)
			return parent::onDeath(%this, %srcObj, %srcClient, %damageType, %loc);

		%p = %this.player;

		// if(%p.isGhost)
		// {
		// 	%this.schedule(3000, spawnPlayer);
		// 	return;
		// }

		MMDebug("Player death of" SPC %this.getPlayerName(), %this, %mini);
		if(%srcClient == %this)
			%s = %this.MM_GetName(1) SPC "\c6committed suicide";
		else if(isObject(%srcClient))
		{
			if(isObject(%srcClient.player))
				%d = " \c6from" SPC mFloatLength(VectorDist(%p.getHackPosition(), %srcClient.player.getHackPosition()) / 2, 1) SPC "studs away";
			else
				%d = "";

			%s = %srcClient.MM_GetName(1) SPC "\c6killed" SPC %this.MM_GetName(1) @ %d;

			%alg = %srcClient.role.getAlignment();

			%str = "\c5You were killed by:" SPC $MM::AlignmentColour[%alg] @ %srcClient.getSimpleName() SPC "\c5(" @ $MM::AlignmentColour[%alg] @ $MM::Alignment[%alg] @ "\c5)";

			%this.bottomPrint(%str);
			messageClient(%this, '', %str);
		}
		else
			%s = %this.MM_GetName(1) SPC "\c6fell to their death";

		%mini.MM_LogEvent(%s);
		if(isObject(%srcClient))
			%srcClient.MM_GunLog(%s);
		if(%this != %srcClient)
			%this.MM_GunLog(%s);

		%this.MMSpecMode = 0;

		if(!%this.player.isCorpse)
			%this.lives--;

		if(isObject(%this.role))
			%this.role.onDeath(%mini, %this, %srcObj, %srcClient, %damageType, %loc);

		if(%this.lives < 1)
		{
			MMDebug("Checking win status", %this, %mini);
			%this.isGhost = 1;
			%mini.MM_WinCheck(%this, %srcClient);
		}

		MMDebug("Scheduling spawn", %this, %mini);
		%this.spawnSched = %this.schedule(3000, spawnPlayer);
	}

	function MinigameSO::addMember(%this, %client)
	{
		if($DefaultMinigame == %this && !%this.isMM)
			%this.MM_Init();

		return parent::addMember(%this, %client);
	}

	function MinigameSO::removeMember(%this, %client)
	{
		if(%this.isMM && %client.lives > 0 && isObject(%client.player))
		{
			%client.lives = 1;
			%client.player.kill();
		}

		return parent::removeMember(%this, %client);
	}

	function Armor::onTrigger(%this, %obj, %slot, %val)
	{
		// parent::onTrigger(%this, %obj, %slot, %val);

		// MMDebug(%slot SPC %val);

		// echo("HHH");

		if(!isObject(%cl = %obj.getControllingClient()) || !isObject(%mini = getMiniGameFromObject(%cl)) || !$DefaultMinigame.running)
			return parent::onTrigger(%this, %obj, %slot, %val);

		// echo("BBB");

		if(isObject(%cl.role) && isObject(%cl.player) && !%cl.player.isGhost)
			%cl.role.onTrigger(%mini, %cl, %obj, %slot, %val);

		parent::onTrigger(%this, %obj, %slot, %val);
	}

	function serverCmdDropTool(%this, %slot)
	{
		if(getMiniGameFromObject(%this).isMM)
			return;

		parent::serverCmdDropTool(%this, %slot);
	}

	//does anyone even use this mod anymore lol
	function MinigameSO::displayScoresList(%mini)
	{
		if(%mini.isMM && %mini.running)
			return;

		return parent::displayScoresList(%mini);
	}

	function SimObject::onCameraEnterOrbit(%this, %orbit)
	{
		if($DefaultMinigame.running)
			return;

		parent::onCameraEnterOrbit(%this, %orbit);
	}

	function serverCmdSit(%this)
	{
		if(isObject(%this.player))
		{
			if(%this.player.getName() $= "botCorpse")
			{
				%this.player.playThread(3, "death1");
				return;
			}
		}

		return parent::serverCmdSit(%this);
	}

	function Player::damage(%this, %obj, %pos, %amt, %type)
	{
		%db = %this.getDatablock();
		%techAmt = %this.isCrouched() ? %amt * 2.1 : %amt;

		if(%this.getName() $= "botCorpse" && !isObject(%this.getControllingClient())) return;

		if(!isObject(%cl = %this.client))
			return parent::damage(%this, %obj, %pos, %amt, %type);

		if(%this.getDamageLevel() + %techAmt < %db.maxDamage)
			return parent::damage(%this, %obj, %pos, %amt, %type);

		if(!isObject(%mini = getMiniGameFromObject(%cl)))
			return parent::damage(%this, %obj, %pos, %amt, %type);

		if(!$DefaultMinigame.running || !%mini.isMM)
			return parent::damage(%this, %obj, %pos, %amt, %type);

		if(%this.dying || %this.isGhost || %this.client.lives < 1 || (%this.isCorpse && isObject(%this.getControllingClient())))
			return parent::damage(%this, %obj, %pos, %amt, %type);

		if(%type $= $DamageType::Impact || %type $= $DamageType::Fall || %type $= $DamageType::Direct || %type $= $DamageType::Suicide || %type $= $DamageType::CombatKnife)
			return parent::damage(%this, %obj, %pos, %amt, %type);

		MMDebug("Player" SPC %cl.getPlayerName() SPC "is now dying", %cl);
		%this.setDatablock(bracketsHatesTGE(%db));

		%this.dying = true;
		%this.startedDying = $Sim::Time;
		%this.deathSched = %this.schedule(1000, damage, %obj, %pos, %db.maxDamage, %type);
		%this.setDamageFlash(0.75);
		%this.emote(PainMidImage);

		%cl.applyBodyParts();
		%cl.applyBodyColors();
	
		return;
	}

	function GameConnection::spawnPlayer(%this)
	{
		%r = parent::spawnPlayer(%this);

		cancel(%this.spawnSched);

		if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !$DefaultMinigame.running)
		{
			%this.MM_GiveDMEquipment();
			%this.centerPrint("");

			return %r;
		}

		if(%this.lives < 1 || %this.isGhost || %this.MMIgnore)
			return %r;

		if(isObject(%this.player))
		{
			// MMDebug("butt" SPC %this.player);

			%this.player.setShapeNameDistance(13.5);
			%this.MM_GiveEquipment();
		}

		if(isObject(%this.role))
		{
			%this.role.onSpawn(%mini, %this);
			%this.MM_UpdateUI();
			%this.MM_DisplayStartText();
		}

		return %r;
	}

	function destroyServer()
	{
		MM_onDestroy();

		return parent::destroyServer();
	}
};
activatePackage(MM_Core);