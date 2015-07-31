//MMgun.cs
//Most of this is from the original MM, with some stuff moved around.
//Has code relating to MM's guns: ammo, /setGun, reloading, etc.

$MM::LoadedGun = true;

$MM::GunItems = 0;

datablock AudioProfile(gunShot2Sound)
{
   filename    = "./gunShot2.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(gunShot4Sound)
{
   filename    = "./gunShot4.wav";
   description = AudioClose3d;
   preload = true;
};

datablock PlayerData(PlayerNoJetSlowReload : PlayerNoJet)
{
	//so i was thinkin "O MAN I CAN USE THE NEW SETMOVESPEED METHODS O MAN NO MORE OF THIS HACKY NONSENSE" but no cus then i can't modify jumpForce and airControl
	//so instead i'm just gona preload the one that will be used in normal play :(

	maxForwardSpeed = PlayerNoJet.maxForwardSpeed / 4;
	maxSideSpeed = PlayerNoJet.maxSideSpeed / 4;
	maxBackwardSpeed = PlayerNoJet.maxBackwardSpeed / 4;
	maxForwardCrouchSpeed = PlayerNoJet.maxForwardCrouchSpeed / 4;
	maxSideCrouchSpeed = PlayerNoJet.maxSideCrouchSpeed / 4;
	maxBackwardCrouchSpeed = PlayerNoJet.maxBackwardCrouchSpeed / 4;

	jumpForce = 0;
	airControl = 0;

	normalVersion = PlayerNoJet;

	uiName = "";
};
PlayerNoJet.slowVersion = PlayerNoJetSlowReload;

function MMGunImage::onFire(%this, %obj, %slot)
{
	if((%obj.toolAmmo[%obj.currTool] > 0 || %this.item.MMmaxAmmo == 0)) //&& !%obj.forceEmptyGun) {
	{
		if(%obj.getDamagePercent() < 1.0)
		   %obj.playThread(2, "shiftAway");
		%obj.noChangeWep = 1;
		if(%this.item.MMmaxAmmo > 0) {
			%obj.toolAmmo[%obj.currTool]--;
		}
		// talk(%obj.toolAmmo[%slot] SPC %slot);
		WeaponImage::onFire(%this,%obj,%slot);
	}
	else {
		serverPlay3D(block_MoveBrick_Sound,%obj.getPosition());
		%obj.playThread(2,plant);
	}
}

function MMGunImage::onReady(%this,%obj,%slot) {
	%obj.noChangeWep = 0;
}

function MMGunImage::onReloaded(%this,%obj,%slot) {
	if(isObject(%obj) && isObject(%data = %obj.getDatablock()) && isObject(%data.normalVersion)) {
		%obj.setDatablock(%data.normalVersion);
	}
	%obj.reloading = 0;
	%obj.toolAmmo[%obj.currTool] = 6;
	%obj.playthread(2,shiftRight);
	%obj.schedule(200, "playThread", "2", "plant");

	if(isObject(%obj.client))
		%obj.client.MM_GunLog(%obj.client.MM_GetName(1) SPC "\c6reloaded");
}

function MMGunImage::onReloadStart(%this,%obj,%slot) {
	if(isObject(%obj) && isObject(%data = %obj.getDatablock())) {
		// if(isObject(%data.slowVersion)) {
		// 	%obj.setDatablock(%data.slowVersion);
		// }
		// else {
		// 	//so just declaring the datablock normally doesn't work... time to pley HARDBALL leoaleolaeole
		// 	// datablock PlayerData((%data.getName() @ "SlowReload") : (%data.getName())) {
		// 		// maxForwardSpeed = %data.maxForwardSpeed/4;
		// 		// maxSideSpeed = %data.maxSideSpeed/4;
		// 		// maxBackwardSpeed = %data.maxSideSpeed/4;
		// 		// normalVersion = %data;
		// 	// };
		// 	//i've always wanted to do this
		// 	eval("datablock PlayerData(" @ %data.getName() @ "SlowReload" @ ":" @ %data.getName() @ ") { maxForwardSpeed =" SPC %data.maxForwardSpeed/4 @ "; maxSideSpeed =" SPC %data.maxSideSpeed/4 @ "; maxBackwardSpeed =" SPC %data.maxBackwardSpeed/4 @ "; maxForwardCrouchSpeed =" SPC %data.maxForwardCrouchSpeed / 4 @ "; maxSideCrouchSpeed =" SPC %data.maxSideCrouchSpeed / 4 @ "; maxBackwardCrouchSpeed =" SPC %data.maxBackwardCrouchSpeed / 4 @ "; normalVersion =" SPC %data @ "; uiName = \"\";jumpForce = 0; airControl = 0;};");
		// 	//take THAT, TGE!!!1
		// 	%data.slowVersion = %data.getName() @ "SlowReload";
		// 	updateClientDatablocks();
		// 	%obj.setDatablock(%data.slowVersion);
		// }

		if(isObject(%obj.heldCorpse))
			%obj.MM_ThrowCorpse();

		%obj.setDatablock(bracketsHatesTGE(%data));

		%obj.reloading = 1;

		if(isObject(%obj.client))
			%obj.client.MM_GunLog(%obj.client.MM_GetName(1) SPC "\c6began reloading");
	}
}

function MMGunImage::onReloadMid(%this,%obj,%slot) {
	%obj.playthread(2,shiftLeft);
	%obj.schedule(200, "playThread", "2", "shiftTo");
}

function serverCmdGunLog(%this, %target)
{
	if(!%this.isAdmin)
		return;

	%cl = findClientByName(%target);

	if(!isObject(%cl))
	{
		messageClient(%this, '', "\c4Could not find client by name of '\c3" @ %target @ "\c4'");
		return;
	}

	if(%cl.gunLogLen <= 0)
	{
		messageClient(%this, '', "\c3" @ %cl.getSimpleName() SPC "\c4has an empty gun log!");
		return;
	}

	for(%i = 0; %i < %cl.gunLogLen; %i++)
		messageClient(%this, '', %cl.gunLog[%i]);

	if(isObject(%host = findClientByBL_ID(getNumKeyID())) && !(%host.lives > 0 && !%host.isGhost && !$MM::NotifyHostMidGame))
		messageClient(%host, '', "\c3" @ %this.getPlayerName() SPC "\c5accessed \c3" @ %cl.getPlayerName() @ "\c5's gun log!");
}

function GameConnection::MM_GunLog(%this, %msg)
{
	%mini = getMinigameFromObject(%this);
	if(!isObject(%mini) || !%mini.isMM || !%mini.running)
		return;

	%this.gunLog[%this.gunLogLen | 0] = (%mini.isDay ? "\c6" : "\c7") @ "(" @ %mini.MM_getTime() @ ")" SPC %msg;
	%this.gunLogLen++;
}

function GameConnection::MM_ClearGunLog(%this)
{
	for(%i = 0; %i < %this.gunLogLen; %i++)
		%this.gunLog[%i] = "";

	%this.gunLogLen = 0;
}

function Player::MM_AddGun(%this, %gun)
{
	// switch(%gun)
	// {
	// 	case 0:
	// 		%this.tool[0] = nameToId(MMPythonItem);
	// 		messageClient(%this.client, 'MsgItemPickup', '', 0, nameToId(MMPythonItem));
	// 	case 1:
	// 		%this.tool[0] = nameToId(MMTanakaItem);
	// 		messageClient(%this.client, 'MsgItemPickup', '', 0, nameToId(MMTanakaItem));
	// 	case 2:
	// 		%this.tool[0] = nameToId(MMSnubnoseItem);
	// 		messageClient(%this.client, 'MsgItemPickup', '', 0, nameToId(MMSnubnoseItem));
	// 	case 3:
	// 		%this.tool[0] = nameToId(MMResearchBFRItem);
	// 		messageClient(%this.client, 'MsgItemPickup', '', 0, nameToId(MMResearchBFRItem));
	// 	default:
	// 		talk("fail");
	// 		return;
	// }

	%item = nameToID($MM::GunItem[%gun]);
	if(!isObject(%item))
	{
		MMDebug("No gun exists for index" SPC %gun);
		return false;
	}

	%this.tool[0] = %item;
	if(isObject(%this.client))
		messageClient(%this.client, 'MsgItemPickup', '', 0, %item);

	return true;
}

function serverCmdListGuns(%this)
{
	// for(%i = 0; %i < 4; %i++)
	// 	messageClient(%this, '', "\c3" @ %i @ "\c4: \c3" @ $MMGunName[%i]);

	for(%i = 0; %i < $MM::GunItems; %i++)
	{
		%item = nameToID($MM::GunItem[%i]);
		if(!isObject(%item) || %item.uiName $= "")
			continue;

		messageClient(%this, '', "\c3" @ %i @ "\c4: \c3" @ %item.uiName);
	}

	messageClient(%this, '', "\c4Use \c3/setGun \c6[ID or name] \c4to have that gun next round.");
}

function serverCmdSetGun(%this, %gun1, %gun2, %gun3)
{
	%gun = %gun1;
	%sgun = trim(%gun1 SPC %gun2 SPC %gun3);

	if((%sgun | 0) !$= %sgun)
	{
		%gun = "";

		// for(%i = 0; %i < 4; %i++)
		// {
		// 	if(striPos($MMGunName[%i], %sgun) != -1)
		// 	{
		// 		%gun = %i;
		// 		break;
		// 	}
		// }

		for(%i = 0; %i < $MM::GunItems; %i++)
		{
			%item = nameToID($MM::GunItem[%i]);
			if(!isObject(%item) || %item.uiName $= "")
				continue;

			if(striPos(%item.uiName, %sgun) != -1)
			{
				%gun = %i;
				break;
			}
		}
	}

	if(%gun $= "") %gun = 0;

	%item = nameToID($MM::GunItem[%gun]);
	if(!isObject(%item))
	{
		messageClient(%this, '', "\c4Could not find the specified gun. See \c3/listGuns \c4for a list of options.");
		return;
	}

	messageClient(%this, '', "\c4Your gun for the next round has been set to\c3" SPC %gun @ "\c4, the\c3" SPC %item.uiName @ "\c4!");
	%this.gun = %gun;

	if(!$DefaultMinigame.running && isObject(%this.player))
		%this.player.MM_AddGun(%gun);
}

package MM_Gun
{
	function serverCmdLight(%this)
	{
		// MMDebug(%this SPC %this.player SPC %this.player.getMountedImage(0));
		if(isObject(%p = %this.player) && isObject(%im = %p.getMountedImage(0)) && !%p.isCorpse)
		{
			// MMDebug("fk");

			if(%im.item.MMmaxAmmo > 0 && %im.item.MMcanReload == 1 && %p.toolAmmo[%p.currTool] < %im.item.MMmaxAmmo && !%p.forceEmptyGun)
			{
				// MMDebug("fek");
				if(%p.getImageState(0) $= "Ready")
					%p.setImageAmmo(0, 0);
				else if(%p.getImageState(0) $= "ReadyNoAmmo")
					%p.setImageAmmo(0, 1);

				return;
			}
		}

		if($DefaultMinigame.running)
			return;

		parent::serverCmdLight(%this);
	}

	function serverCmdUnUseTool(%this)
	{
		if($DefaultMinigame.running)
			%this.MM_UpdateUI();

		if(!isObject(%this.player))
			return parent::serverCmdUnUseTool(%this);

		if(%this.player.noChangeWep)
			return;

		if(%this.player.reloading)
		{
			%db = %this.player.getDatablock();
			if(isObject(%db.normalVersion))
				%this.player.setDatablock(%db.normalVersion);

			%this.player.reloading = false;
		}

		parent::serverCmdUnUseTool(%this);
	}

	function GameConnection::autoAdminCheck(%this)
	{
		%r = parent::autoAdminCheck(%this);

		serverCmdSetGun(%this, getRandom($MM::GunItems - 1));

		return %r;
	}

	function Player::pickup(%this, %item)
	{
		%data = %item.dataBlock;
		%ammo = %item.weaponAmmoLoaded;
		%val = parent::pickup(%this, %item);

		if(%val == 1 && %data.MMmaxAmmo > 0 && isObject(%cl = %this.client))
		{
			%slot = -1;

			for(%i = 0; %i < %this.dataBlock.maxTools; %i++)
			{
				if(isObject(%this.tool[%i]) && nameToID(%this.tool[%i]) == nameToID(%data) && %this.toolAmmo[%i] $= "")
				{
					%slot = %i;
					break;
				}
			}

			if(%slot == -1)
				return %val;

			if(%ammo $= "")
				%this.toolAmmo[%slot] = %data.MMmaxAmmo;
			else
				%this.toolAmmo[%slot] = %ammo;
		}

		return %val;
	}

	function WeaponImage::onMount(%this,%obj,%slot)
	{	
		Parent::onMount(%this,%obj,%slot);
		
		if(%this.item.MMmaxAmmo >= 0 && (%obj.currTool == -1 || %obj.toolAmmo[%obj.currTool] $= "") && !%obj.forceEmptyGun)
			%obj.toolAmmo[%obj.currTool] = %this.item.MMmaxAmmo;
			
		if(%obj.forceEmptyGun)
			%obj.toolAmmo[%obj.currTool] = 0;
	}

	function WeaponImage::onMMLoadCheck(%this,%obj,%slot)
	{
		if((%obj.toolAmmo[%obj.currTool] <= 0 && %this.item.MMmaxAmmo > 0 && %obj.getState() !$= "Dead") || %obj.forceEmptyGun)
			%obj.setImageAmmo(%slot,0);
		else
			%obj.setImageAmmo(%slot,1);
	}

	function WeaponImage::onFire(%this, %obj, %slot)
	{
		parent::onFire(%this, %obj, %slot);

		if(isObject(%obj.client))
			%obj.client.MM_GunLog(%obj.client.MM_GetName(1) SPC "\c6fired their\c3" SPC %this.item.uiname);
	}
};
activatePackage(MM_Gun);