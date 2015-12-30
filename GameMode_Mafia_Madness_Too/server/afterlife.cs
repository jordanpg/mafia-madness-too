//afterlife.cs
//Handles stuff that happens related to spectating or the afterlife.
//Some of this is moved over from MM_Core.cs rather than developed separately, so bits and pieces from the two may overlap.

$MM::LoadedAfterlife = true;

//backwards compatibility! and other stuff from before i thought "oh hey spawn points are a nice way to indicate where to spawn stuff"
function MinigameSO::MM_GetAfterLifeTransform(%this)
{
	switch(%this.afterLifeMode)
	{
		case 0:
			if(%this.afterLifeLoc !$= "")
				return %this.afterLifeLoc;

			if($Pref::Server::MMAfterLifeLoc !$= "")
				return $Pref::Server::MMAfterLifeLoc;

		case 1:
			if(%this.afterLifeBox !$= "")
				%box = %this.afterLifeBox;
			else if($Pref::Server::MMAfterLifeBox !$= "")
				%box = $Pref::Server::MMAfterLifeBox;

			%cornerA = getWords(%box, 0, 2);
			%cornerB = getWords(%box, 3, 5);

			return VectorRandom(%cornerA, %cornerB) SPC "0 0 0 0";

		case 2:
			if(isObject(%this.afterLifeBrick))
				return %this.afterLifeBrick.getTransform();

			if(isObject($Pref::Server::MMAfterLifeBrick))
				return $Pref::Server::MMAfterLifeBrick.getTransform();
	}

	if(isObject(MMAfterLifeSpawns) && (%ct = MMAfterLifeSpawns.getCount()) > 0)
		return MMAfterLifeSpawns.getObject(getRandom(%ct - 1)).getSpawnPoint();

	return -1;
}

function MinigameSO::MM_GetInArena(%this, %point)
{
	switch(%this.arenaMode)
	{
		case -1:
			return false;

		case 1:
			if(%this.arenaBox !$= "")
				%box = %this.arenaBox;
			else if($Pref::Server::MMArenaBox !$= "")
				%box = $Pref::Server::MMArenaBox;

			%cornerA = getWords(%box, 0, 2);
			%cornerB = getWords(%box, 3, 5);

			return pointBetween(%point, %cornerA, %cornerB, %this.arenaBoxXY);

		case 2:
			if(isObject(%this.arenaBrick))
				%cornerA = %this.arenaBrick.getPosition();

			if(isObject($Pref::Server::MMArenaBrick))
				%cornerA = $Pref::Server::MMArenaBrick.getPosition();

			%cornerB = VectorAdd(%cornerA, "12 12 0");

			return pointBetween(%point, %cornerA, %cornerB, true);

		default:
			if(%this.afterLifeLoc !$= "")
				%cornerA = %this.arenaLoc;

			if($Pref::Server::MMArenaLoc !$= "")
				%cornerA = $Pref::Server::MMArenaLoc;

			%cornerB = VectorAdd(%cornerA, "12 12 0");

			// talk(%cornerA SPC %cornerB);

			return pointBetween(%point, %cornerA, %cornerB, true);
	}

	return -1;
}

function Camera::setSpecPlayer(%this, %cl, %specCl)
{
	if(!isObject(%pl = %specCl.player))
		return;

	%this.setMode("Corpse", %pl);
	%cl.setControlObject(%this);

	if(isObject(%pl.claimRole))
		%str = "<just:left>\c5Claim:" SPC %pl.claimRole.getColour(1) @ %pl.claimRole.getRoleName();
	else
		%str = "<just:left>\c5Claim: \c6NONE";

	%cl.bottomPrint("\c5Spectating:\c6" SPC %specCl.getSimpleName() @ "<just:right>\c5ROLES\c6:" SPC MM_ColourCodeRoles(%cl.minigame.MM_getRolesList()) @ " " NL %str);
}

function Camera::MM_activateSpectator(%obj, %cl, %backward)
{
	if(!isObject(%mini = getMiniGameFromObject(%cl)))
		return;

	if(%cl.specMode)
	{
		%cl.specIndex |= 0;

		if(isObject(%pl = %mini.member[%cl.specIndex].player) && !%pl.isGhost)
		{
			%obj.setSpecPlayer(%cl, %mini.member[%cl.specIndex]);

			return;
		}

		if(!%backward)
		{
			%start = %cl.specIndex + 1;
			if(%start >= %mini.numMembers)
				%start -= %mini.numMembers;

			for(%i = %start; %i != %cl.specIndex; %i++)
			{
				if(%i >= %mini.numMembers)
					%i -= %mini.numMembers;

				%cl2 = %mini.member[%i];

				if(isObject(%pl = %cl2.player) && !%pl.isGhost)
				{
					// %obj.setMode("Corpse", %pl);
					// %cl.setControlObject(%obj);
					%cl.specIndex = %i;

					%obj.setSpecPlayer(%cl, %mini.member[%cl.specIndex]);

					return;
				}
			}
		}
		else
		{
			%start = %cl.specIndex - 1;
			if(%start < 0)
				%start += %mini.numMembers;

			for(%i = %start; %i != %cl.specIndex; %i--)
			{
				if(%i < 0)
					%i += %mini.numMembers;

				%cl2 = %mini.member[%i];

				if(isObject(%pl = %cl2.player) && !%pl.isGhost)
				{
					// %obj.setMode("Corpse", %pl);
					// %cl.setControlObject(%obj);
					%cl.specIndex = %i;
					%obj.setSpecPlayer(%cl, %mini.member[%cl.specIndex]);

					return;
				}
			}
		}
	}
	else
	{
		%obj.setMode("Observer");
		%cl.setControlObject(%obj);
		bottomPrint(%cl, "<just:center>\c5Press the space bar to enter observer mode.");
	}
}

package MM_AfterLife
{
	function MinigameSO::pickSpawnPoint(%this, %client)
	{
		if(!%this.isMM || !%this.running)
			return parent::pickSpawnPoint(%this, %client);

		if(%client.lives < 1 || %client.MMIgnore)
		{
			%point = %this.MM_GetAfterLifeTransform();

			return %point;
		}

		return parent::pickSpawnPoint(%this, %client);
	}

	function GameConnection::spawnPlayer(%this)
	{
		if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !$DefaultMinigame.running)
			return parent::spawnPlayer(%this);

		%point = %mini.pickSpawnPoint(%this);

		if(%point == -1 && !isObject(%this.getControlObject()))
		{
			%this.camera.setMode("Observer");
			%this.setControlObject(%this.camera);

			if(%this.showMessage)
			{
				%this.centerPrint("<color:604060>You are now spectating.  Wait until the next round to respawn.<br>\c3Use the light key to spectate.", 3);
				%this.bottomPrint("\c5Use the light key to spectate.");
				%this.showMessage = false;
			}

			return;
		}

		if(%this.lives < 1 || %this.MMIgnore)
		{
			%this.centerPrint("");

			if(isObject(%this.player))
				%this.player.delete();

			%this.createPlayer(%point);

			if(isObject(%this.player))
			{
				%this.player.setShapeNameDistance(13.5);
				%this.clearInventory();
				%this.player.weaponcount--;
				%this.player.isGhost = true;
			}

			%this.applyBodyParts();
			%this.applyBodyColors();

			if(%this.showMessage)
			{
				%this.centerPrint("<color:604060>You are now in the afterlife.  Wait until the next round to respawn.<br>\c3Use the light key to spectate.", 3);
				%this.bottomPrint("\c5Use the light key to spectate.");
				%this.showMessage = false;
			}

			return;
		}

		%r = parent::spawnPlayer(%this);

		// // MMDebug("fuck" SPC %this SPC %this.player);

		// if(isObject(%this.player))
		// {
		// 	// MMDebug("butt" SPC %this.player);

		// 	%this.player.setShapeNameDistance(13.5);
		// 	%this.player.MM_AddGun(%this.gun);
		// }

		// if(isObject(%this.role))
		// {
		// 	%this.role.onSpawn(%mini, %this);
		// 	%this.MM_UpdateUI();
		// }

		return %r;
	}

	function MinigameSO::addMember(%this, %client)
	{
		%r = parent::addMember(%this, %client);

		if(%this.isMM)
		{
			if(isObject(%p = %client.player))
				%p.delete();

			%client.schedule(33, spawnPlayer);
			%client.showMessage = true;
		}

		return %r;
	}

	function Observer::onTrigger(%this, %obj, %slot, %val)
	{
		if(!$DefaultMiniGame.running)
			return parent::onTrigger(%this, %obj, %slot, %val);

		if(!isObject(%cl = %obj.getControllingClient()) || !getMiniGameFromObject(%cl).isMM)
			return parent::onTrigger(%this, %obj, %slot, %val);

		if(%cl.lives > 0)
			return parent::onTrigger(%this, %obj, %slot, %val);

		if(isObject(%cl.player) && !%cl.player.isGhost)
			return parent::onTrigger(%this, %obj, %slot, %val);

		switch(%slot)
		{
			case 0:
				if(!%val || !%cl.specMode)
					return;

				%cl.centerPrint("");

				%cl.specIndex++;
				if(%cl.specIndex >= %mini.numMembers)
					%cl.specIndex -= %mini.numMembers;

				// if(isObject(%pl = %mini.member[%cl.specIndex].player) && !%pl.isGhost)
				// {
				// 	%obj.setMode("Corpse", %pl);
				// 	%cl.setControlObject(%obj);

				// 	return;
				// }

				// for(%i = ((%cl.spexIndex + 1) % %mini.numMembers); %i != %cl.specIndex; %i++)
				// {
				// 	%i %= %mini.numMembers;

				// 	%cl2 = %mini.member[%i];

				// 	if(isObject(%pl = %cl2.player) && !%pl.isGhost)
				// 	{
				// 		%obj.setMode("Corpse", %pl);
				// 		%cl.setControlObject(%obj);
				// 		%cl.specIndex = %i;

				// 		return;
				// 	}
				// }

				%obj.MM_activateSpectator(%cl);

			case 4:
				if(!%val || !%cl.specMode)
					return;

				%cl.centerPrint("");

				%cl.specIndex--;
				if(%cl.specIndex < 0)
					%cl.specIndex += %mini.numMembers;

				// if(isObject(%pl = %mini.member[%cl.specIndex].player) && !%pl.isGhost)
				// {
				// 	%obj.setMode("Corpse", %pl);
				// 	%cl.setControlObject(%obj);

				// 	return;
				// }

				// %start = %cl.specIndex - 1;
				// if(%start < 0)
				// 	%start += %mini.numMembers;

				// for(%i = %start; %i != %cl.specIndex; %i--)
				// {
				// 	if(%i < 0)
				// 		%i += %mini.numMembers;

				// 	%cl2 = %mini.member[%i];

				// 	if(isObject(%pl = %cl2.player) && !%pl.isGhost)
				// 	{
				// 		%obj.setMode("Corpse", %pl);
				// 		%cl.setControlObject(%obj);
				// 		%cl.specIndex = %i;

				// 		return;
				// 	}
				// }

				%obj.MM_activateSpectator(%cl, true);

			case 2:
				if(!%val)
					return;

				%cl.specMode ^= 1;

				%obj.MM_activateSpectator(%cl);
		}
	}

	function serverCmdLight(%this)
	{
		if($DefaultMinigame.running && (%mini = getMiniGameFromObject(%this)).isMM)
		{
			if(!isObject(%p = %this.player))
			{
				if(%this.lives < 1 && %mini.MM_GetAfterLifeTransform() !$= -1)
					%this.spawnPlayer();

				return;
			}

			if(!%p.isGhost)
				return parent::serverCmdLight(%this);

			if(%this.getControlObject() != %p)
			{
				%this.setControlObject(%p);

				bottomPrint(%this, "");

				return;
			}

			%this.camera.MM_activateSpectator(%this);
		}

		parent::serverCmdLight(%this);
	}

	function Player::damage(%this, %obj, %pos, %amt, %type)
	{
		if(!isObject(%mini = getMiniGameFromObject(%this)) || !$DefaultMinigame.running)
			return parent::damage(%this, %obj, %pos, %amt, %type);

		%techAmt = %this.isCrouched() ? %amt * 2.1 : %amt;

		%cl = %this.client;

		if(%this.isGhost || (isObject(%cl) && %cl.lives < 1 && !%this.isCorpse))
		{
			%pos = %this.getPosition();

			if(isObject(%obj.client) && !%mini.MM_GetInArena(%obj.client.player.getPosition()))
				return;

			if(!%mini.MM_GetInArena(%pos) || !%mini.MM_GetInArena(%obj.getPosition()))
				return;

			if(%this.getDamageLevel() + %techAmt >= %this.getDatablock().maxDamage)
			{
				%this.schedule(0, delete);
				%cl.schedule(1, spawnPlayer);

				return;
			}
		}

		parent::damage(%this, %obj, %pos, %amt, %type);
	}
};
activatePackage(MM_AfterLife);
