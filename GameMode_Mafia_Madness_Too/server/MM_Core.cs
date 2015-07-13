//MM_Core.cs
//Handles core MM functionality such as day/night cycles, round control, role assignment, etc.
//Does not handle role-related functionality. See MM_Role.cs or the roles folder for that.

$MM::LoadedCore = true;

$MM::DediRoundDelay = 30000;

package disableToolCmds { function serverCmdDuplicator() { } function serverCmdFillcan() { } function serverCmdUsePrintGun() { } };

function MM_isValidGameMode(%modeName)
{
	if(!isFunction("MM_InitMode" @ %modeName))
		return false;

	return true;
}

function MM_clearCorpses()
{
	while(isObject(botCorpse))
		botCorpse.delete();
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

	%this.gameMode = 0;

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

function MinigameSO::MM_GetGameMode(%this)
{
	if((%this.gameMode | 0) $= %this.gameMode)
		%mode = $MM::GameMode[%this.gameMode];
	else
		%mode = %this.gameMode;

	if(%mode $= "" || !MM_isValidGameMode(%mode))
		return $MM::GameMode[0];

	return %mode;
}

function MinigameSO::MM_SetRole(%this, %client, %role)
{
	if(!isObject(%client) || %client.MMIgnore)
		return false;

	if(!isObject(%role))
	{
		if(%role $= "")
		{
			if(isObject(%client.role))
				%client.role.onCleanup(%this, %client);

			%client.role = "";
			%this.role[%client] = "";
			return true;
		}

		if(!isObject(%role = $MM::RoleKey[%role]))
			return false;
	}

	%client.role = %role;
	%this.role[%client] = %role;
	%this.memberCache[%this.memberCacheLen | 0] = %client;
	%this.memberCacheName[%this.memberCacheLen | 0] = %client.getPlayerName();
	%this.memberCacheLen++; 

	%role.onAssign(%this, %client);

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
	%this.allAbduct = false;
	%this.allComm = false;
	%this.allImp = false;
	%this.allInv = false;
	%this.allFingerprint = false;

	%this.roles = "";

	%this.MM_ClearRoles();
}

function MinigameSO::MM_ClearEventLog(%this)
{
	for(%i = 0; %i < %this.eventLogLen; %i++)
		%this.eventLog[%i] = "";

	%this.eventLogLen = 0;
}

function MinigameSO::MM_LogEvent(%this, %str)
{
	%this.eventLog[%this.eventLogLen | 0] = %str;
	%this.eventLogLen++;
}

function MinigameSO::MM_ChatEventLog(%this, %cl)
{
	for(%i = 0; %i < %this.eventLogLen; %i++)
	{
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
				MMDebug("Init client" SPC %client SPC "with role" SPC %role);

				%client.lives = 1;
				%client.isGhost = false;
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

		%client.lives = 1;
		%client.isGhost = false;
	}
}

function MinigameSO::MM_InitRound(%this)
{
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

	%this.MM_AssignRoles();

	%this.running = true;

	%this.time = 0;
	%this.day = 0;
	%this.isDay = false;

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

	%this.reset(0);

	%this.MM_DayCycle(1);

	%this.roundStart = $Sim::Time;
}

function MinigameSO::MM_ClearRoles(%this)
{
	for(%i = 0; %i < %this.numMembers; %i++)
		%this.MM_SetRole(%this.member[%i], "");

	for(%i = 0; %i < %this.memberCacheLen; %i++)
	{
		%this.memberCache[%i] = "";
		%this.memberCacheName[%i] = "";
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
	//TODO: put stuff like dead rising here
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
		%cl.lives = 1;

		if(isObject(%corpse))
		{
			%cl.createPlayer(%corpse.getTransform());
			%corpse.delete();
		}
		else
			%cl.createPlayer(%cl.getControlObject().getTransform());

		if(isObject(%cl.role))
			%cl.role.onSpawn(%this, %cl);
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

function MinigameSO::MM_WinCheck(%this, %cl)
{
	if(%this.resolved)
		return;

	MMDebug("Checking for wins...", %this, %cl);

	for(%i = 0; %i < %this.numMembers; %i++)
	{
		%cl = %this.member[%i];

		if(%cl.lives > 0 && !%cl.player.dying)
		{
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
		MMDebug("Inno win", %this, %cl);

		%this.resolved = 1;
		%this.schedule(3000, MM_Stop);
	}
	else if(%foundMaf && !%foundInno)
	{
		talk("The last Innocent is dead!  The Mafia have won.");
		MMDebug("Maf win", %this, %cl);

		%this.resolved = 1;
		%this.schedule(3000, MM_Stop);
	}
	else if(!%foundMaf && !%foundInno)
	{
		talk("Everyone is dead.  The game has ended in a draw.");
		MMDebug("Tie", %this, %cl);

		%this.resolved = 1;
		%this.schedule(3000, MM_Stop);
	}
}

function MinigameSO::MM_GetMafList(%this)
{
	%list = "";
	for(%i = 0; %i < %this.numMembers; %i++)
	{
		%mem = nameToID(%this.member[%i]);

		if(%mem.MM_isMaf())
			%list = %list SPC %mem;
	}

	return trim(%list);
}

function GameConnection::MM_GetName(%this)
{
	%pre = "";
	if(isObject(%this.role))
		%pre = %this.role.getColour();

	return %pre @ %this.getSimpleName();
}

function GameConnection::MM_isMaf(%this)
{
	if(!isObject(%this.role))
		return false;

	return %this.role.getAlignment() == 1;
}

function GameConnection::MM_canImpersonate(%this)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return false;

	if(!isObject(%this.role))
		return false;

	if(!%this.MM_isMaf())
		return false;

	if(!%this.role.getCanImpersonate() && !%mini.allImp)
		return false;

	return true;
}

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

function GameConnection::MM_UpdateUI(%client)
{
	if(!isObject(%client.role) || %client.isGhost || %client.lives < 1)
	{
		bottomPrint(%client, "", 0);
		return;
	}

	%client.bottomPrint("\c5You are:" SPC %client.role.getColour() @ %client.role.getDisplayName() SPC " " SPC "\c5ROLES\c6:" SPC %client.minigame.roles);
}

function GameConnection::MM_DisplayMafiaList(%this)
{
	%mini = getMiniGameFromObject(%this);
	if(!isObject(%mini) || !%mini.running)
		return;

	%list = %mini.MM_GetMafList();
	%ct = getWordCount(%list);
	for(%i = 0; %i < %ct; %i++)
	{
		%cl = getWord(%list, %i);

		if(!isObject(%r = %cl.role))
			continue;

		messageClient(%this, '', %cl.MM_GetName() SPC "(" @ %r.getRoleName() @ ")");
	}
}

function GameConnection::MM_DisplayStartText(%this)
{
	%mini = getMiniGameFromObject(%this);
	if(!isObject(%mini) || !%mini.running || !isObject(%this.role))
		return;

	if(!%this.MM_isMaf())
		messageClient(%this, '', "\c4You are \c2Innocent\c4!  You don't know who the mafia are, but you must find out and kill them!");
	else
	{
		messageClient(%this, '', "\c4You are the \c0Mafia\c4!  You must kill all the innocents.  Here is a full list of the members of the mafia: ");
		messageClient(%this, '', "\c0--");

		%this.MM_DisplayMafiaList();

		messageClient(%this, '', "\c0--");
		messageClient(%this, '', "\c4If all of the mafia die, you lose.  You can type \c3/mafList\c4 to see it again, and anyone not on this list is innocent.  Good luck!");
	}

	%this.messageLines(%this.role.getHelpText());
}

function GameConnection::MM_GiveEquipment(%this)
{
	if(!isObject(%this.player))
		return;
	
	%this.player.MM_AddGun(%this.gun);

	if(!isObject(%this.role))
		return;

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
		if(!isObject(%mini = getMiniGameFromObject(%this)) || %this.isGhost || %this.lives < 1)
			return parent::applyBodyParts(%this);

		if(%mini.running && !%mini.isDay)
			return;

		parent::applyBodyParts(%this);
	}

	function GameConnection::applyBodyColors(%this)
	{
		if(!isObject(%mini = getMiniGameFromObject(%this)) || %this.isGhost || %this.lives < 1)
			return parent::applyBodyColors(%this);

		if(%mini.running && !%mini.isDay)
			%this.applyMMSilhouette();
		else
			parent::applyBodyColors(%this);

		%p = %this.player;

		if(!isObject(%p))
			return;

		if(%p.getName() $= "botCorpse")
		{
			%this.applyMMSilhouette();
			%p.setNodeColor("ALL", "1 0 0 1");
		}
		else if(%p.doombot)
		{
			%p.unHideNode("ALL");
			%p.setNodeColor("ALL", "0 0 0 1");
			%p.setFaceName("smiley");
			%p.setDecalName("AAA-None");
		}
	}

	function GameConnection::onDeath(%this, %srcObj, %srcClient, %damageType, %loc)
	{
		if(!isObject(%mini = getMiniGameFromObject(%this)))
			return parent::onDeath(%this, %srcObj, %srcClient, %damageType, %loc);

		if(!%mini.running || !%mini.isMM)
			return parent::onDeath(%this, %srcObj, %srcClient, %damageType, %loc);

		if(%this.isGhost || %this.lives < 1)
		{
			%this.schedule(3000, spawnPlayer);
			return;
		}

		MMDebug("Player death of" SPC %this.getPlayerName(), %this, %mini);
		if(%srcClient == %this)
			%mini.MM_LogEvent(%this.MM_GetName() SPC "\c6committed suicide");
		else if(isObject(%srcClient))
		{
			%mini.MM_LogEvent(%srcClient.MM_GetName() SPC "\c6killed" SPC %this.MM_GetName());

			%this.bottomPrint("\c5You were killed by:" SPC (%srcClient.MM_isMaf() ? "\c0" : "\c2") @ %srcClient.getSimpleName());
		}
		else
			%mini.MM_LogEvent(%this.MM_GetName() SPC "\c6fell to their death");

		%this.MMSpecMode = 0;

		if(%this.player.getName() !$= "botCorpse")
			%this.lives--;

		if(%this.lives < 1)
		{
			MMDebug("Checking win status", %this, %mini);
			%this.isGhost = 1;
			%mini.MM_WinCheck(%this);
		}

		MMDebug("Scheduling spawn", %this, %mini);
		%this.schedule(3000, spawnPlayer);
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
			%client.player.kill();

		return parent::removeMember(%this, %client);
	}

	function Armor::onTrigger(%this, %obj, %slot, %val)
	{
		parent::onTrigger(%this, %obj, %slot, %val);

		// MMDebug(%slot SPC %val);

		if(!isObject(%cl = %obj.getControllingClient()) || !isObject(%mini = getMiniGameFromObject(%cl)) || !$DefaultMinigame.running)
			return;

		if(isObject(%cl.role) && isObject(%cl.player) && %cl.player == %cl.getControlObject() && !%cl.player.isGhost)
			%cl.role.onTrigger(%mini, %cl, %obj, %slot, %val);
	}

	function serverCmdDropTool(%this, %slot)
	{
		if($DefaultMinigame.running && getMiniGameFromObject(%this).isMM)
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

	function Armor::damage(%db, %this, %obj, %pos, %amt, %type)
	{
		parent::damage(%db, %this, %obj, %pos, %amt, %type);
	}

	function Player::damage(%this, %obj, %pos, %amt, %type)
	{
		%db = %this.getDatablock();
		%techAmt = %this.isCrouched() ? %amt * 2.1 : %amt;

		if(%this.getName() $= "botCorpse") return;

		if(!isObject(%cl = %this.client))
			return parent::damage(%this, %obj, %pos, %amt, %type);

		if(%this.getDamageLevel() + %techAmt < %db.maxDamage)
			return parent::damage(%this, %obj, %pos, %amt, %type);

		if(!isObject(%mini = getMiniGameFromObject(%cl)))
			return parent::damage(%this, %obj, %pos, %amt, %type);

		if(!$DefaultMinigame.running || !%mini.isMM)
			return parent::damage(%this, %obj, %pos, %amt, %type);

		if(%this.dying)
			return parent::damage(%this, %obj, %pos, %amt, %type);

		if(%type $= $DamageType::Impact || %type $= $DamageType::Fall || %type $= $DamageType::Direct || %type $= $DamageType::Suicide || %type $= $DamageType::CombatKnife)
			return parent::damage(%this, %obj, %pos, %amt, %type);

		MMDebug("Player" SPC %cl.getPlayerName() SPC "is now dying", %cl);
		%this.setDatablock(bracketsHatesTGE(%db));

		%this.dying = true;
		%this.schedule(1000, damage, %obj, %pos, %amt, %type);
		%this.setDamageFlash(0.75);
		%this.emote(PainMidImage);

		%cl.applyBodyParts();
		%cl.applyBodyColors();
	
		return;
	}

	function GameConnection::spawnPlayer(%this)
	{
		%r = parent::spawnPlayer(%this);

		if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !$DefaultMinigame.running)
			return %r;

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
};
activatePackage(MM_Core);