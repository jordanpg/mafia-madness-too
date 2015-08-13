//cmd.cs
//ServerCmds for general use and administration here.
//Role-related commands will be in their respective role scripts.

$MM::LoadedCmd = true;

$MM::SpectatorMafList = true;

// $MMGameModeName[0] = "Standard";
// $MMGameModeName[1] = "Classic";
// $MMGameModeName[2] = "I Hate You All";
// $MMGameModeName[3] = "hahahahahahahahahahahaha";
// $MMGameModeName[4] = "Just Try to Survive";
// $MMGameModeName[5] = "Abduct Titanium Tonight";
// $MMGameModeName[6] = "brackets, think of the name -ottosparks (6)"; //wtf is this one
// $MMGameModes = 6;

function serverCmdUpdateUI(%this)
{
	%this.MM_UpdateUI();
}

function serverCmdRoles(%this)
{
	if(!(%mini = getMinigameFromObject(%this)).running)
		return;

	messageClient(%this, '', "\c5Roles\c6:" SPC %mini.MM_GetRolesList());
}

function serverCmdMMDay(%this)
{
	if(!$DefaultMinigame.running)
		return;

	messageClient(%this, '', "\c6It is \c4" @ $DefaultMinigame.MM_getTime() @ "\c6 into the round on the \c4" @ $DefaultMinigame.day @ getDaySuffix($DefaultMinigame.day) SPC ($DefaultMinigame.isDay ? "\c6day." : "\c6night."));
}

function serverCmdMafList(%this, %cp)
{
	if(!%this.MM_isMaf() && !(%this.lives < 1 && $MM::SpectatorMafList))
		return;

	%this.MM_DisplayMafiaList(%cp);
}

function serverCmdMMReqInfo(%client, %targ, %type, %flag)
{
	if(!%client.isAdmin && !%client.isSuperAdmin)
		return;
	%targ = findClientByName(%targ);
	switch(%type)
	{
		case 0: commandToClient(%client, 'MMRecInfo', %type, %targ.MMIgnore, %flag);
		case 1: commandToClient(%client, 'MMRecInfo', %type, %targ.manualRole, %flag);
		case 2: commandToClient(%client, 'MMRecInfo', %type, %targ.role, %flag);
		case 3: commandToClient(%client, 'MMRecInfo', %type, $MMManualGame, %flag);
	}
}

function serverCmdGetNickname(%this, %v0, %v1, %v2, %v3, %v4)
{
	%v = trim(%v0 SPC %v1 SPC %v2 SPC %v3 SPC %v4);

	%blid = $Pref::Server::MMNicknames[%v];
	if(%blid $= "")
	{
		messageClient(%this, '', "\c3The nickname of\c6" SPC %v SPC "\c3could not be found.");
		return;
	}

	%cl = findClientByBL_ID(%blid);

	if(isObject(%cl))
		messageClient(%this,'',"\c3The nickname of\c6" SPC %v SPC "\c3belongs to\c6" SPC %cl.getPlayerName() SPC "\c3.");
	else
		messageClient(%this,'',"\c3The nickname of\c6" SPC %v SPC "\c3belongs to BLID\c6" SPC %blid SPC "\c3.");
}

function serverCmdClaim(%this, %claim)
{
	if(!$DefaultMinigame.running)
	{
		messageClient(%this, '', "\c4MM is currently not running.");
		return;
	}

	if(!isObject(%this.player))
		return;

	if(%this.lives < 1 || %this.isGhost)
	{
		messageClient(%this, '', "\c4You are dead and cannot claim a role!");
		return;
	}

	if(!isObject(%role = $MM::RoleKey[%claim]))
	{
		messageClient(%this, '', "\c4That role doesn\'t exist!");
		return;
	}

	if($Sim::Time - %this.MMClaimTimeout < 10)
	{
		messageClient(%this, '', "\c4You have already shifted your claim too recently! Wait ten seconds.");
		return;
	}

	%this.MMClaimTimeout = $Sim::Time;
	%this.player.setShapeNameColor(%role.getNameColour());
	%this.player.claimRole = %role;
	messageClient(%this, '', "\c4Your claimed role has been set to the" SPC %role.getColour(1) @ %role.getRoleName());
}