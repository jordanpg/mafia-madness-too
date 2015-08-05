//corpses.cs
//Handles spawning, handling, and investigation of corpses.
//See the script in the roles folder for fingerprint expert functionality.

$MM::LoadedCorpses = true;

$MM::CauseOfDeath[0] = "Murder";
$MM::CauseOfDeath[1] = "Suicide";
$MM::CauseOfDeath[2] = "Falling";

$MM::GPCorpseThrowSpeed = 5;
$MM::GPCorpseGrabRange = 8;
$MM::GPCorpseInvestigationRange = 5;

function AIPlayer::MM_getCorpseName(%this)
{
	if(!%this.isCorpse && !%this.isRisenCorpse)
		return "";

	return %this.name;
}

function AIPlayer::MM_getRole(%this)
{
	if(!%this.isCorpse && !%this.isRisenCorpse)
		return -1;

	return %this.role;
}

function AIPlayer::MM_getRoleName(%this)
{
	if(!%this.isCorpse && !%this.isRisenCorpse)
		return -1;

	if(!isObject(%r = %this.MM_getRole()))
		return -1;

	return %r.getCorpseName();
}

function AIPlayer::MM_onCorpseSpawn(%this, %mini, %client, %killerClient, %damageType)
{
	//mostly here for other modules and whatnot to hook in
}

function AIPlayer::MM_onCorpsePickUp(%this, %obj)
{

}

function AIPlayer::MM_onCorpseThrow(%this, %obj)
{
	
}

function AIPlayer::MM_onCorpseReSpawn(%this, %mini, %client, %killerClient, %oldCorpse, %damageType)
{

}

function AIPlayer::MM_Investigate(%this, %client)
{
	if(!%this.isCorpse && !%this.isRisenCorpse)
		return false;

	messageClient(%client, '', "\c2Name\c3:" SPC %this.MM_getCorpseName());
	messageClient(%client, '', "\c2Job\c3:" SPC %this.MM_getRoleName());
	messageClient(%client, '', "\c2Cause of death\c3:" SPC $MM::CauseOfDeath[%this.causeOfDeath]);
}

function Player::MM_PickUpCorpse(%this, %obj)
{
	if(!%obj.isCorpse && !%obj.isRisenCorpse)
		return false;

	if(!isObject(%this) || !isObject(%cl = %this.getControllingClient()) || %this != %cl.player || %this.isGhost)
		return false;

	if(isObject(%this.heldCorpse))
		%this.MM_ThrowCorpse();

	if(isObject(%obj.holder))
		%obj.holder.heldCorpse = "";

	%this.mountObject(%obj, 0);
	%this.heldCorpse = %obj;
	%obj.holder = %this;

	%obj.MM_onCorpsePickUp(%this);
}

function Player::MM_ThrowCorpse(%this)
{
	if(!isObject(%obj = %this.heldCorpse))
		return;

	%this.mountObject(%obj, 8);
	%obj.dismount();

	%vel = VectorAdd(%this.getVelocity(), VectorScale(%this.getEyeVector(), $MM::GPCorpseThrowSpeed));
	// echo(%vel);
	%obj.schedule(1, setVelocity, %vel);
	%obj.holder = "";

	%this.heldCorpse = 0;

	%obj.MM_onCorpseThrow(%this);
}

package MM_Corpses
{
	function GameConnection::onDeath(%this, %srcObj, %srcClient, %damageType, %loc)
	{
		if(!isObject(%mini = getMiniGameFromObject(%this)))
			return parent::onDeath(%this, %srcObj, %srcClient, %damageType, %loc);

		if(!%mini.running || !%mini.isMM)
			return parent::onDeath(%this, %srcObj, %srcClient, %damageType, %loc);

		if(%this.player.isGhost)
			return parent::onDeath(%this, %srcObj, %srcClient, %damageType, %loc);

		MMDebug("Creating corpse for" SPC %this.getPlayerName(), %this, %mini);

		if(isObject(%this.player.heldCorpse))
		{
			MMDebug("Dropping corpse", %this, %mini);
			%this.player.heldCorpse.dismount();
			%this.player.heldCorpse = "";
		}

		%suicide = 0;
		if(%srcClient == %this)
			%suicide = 1;
		else if(!isObject(%srcClient))
			%suicide = 2;

		if(%this.player.getName() $= "botCorpse")
		{
			MMDebug("Creating new corpse", %this, %mini);

			if(!%this.player.isRisenCorpse)
			{
				%corpse = new AIPlayer("botCorpse")
							{
								datablock = PlayerNoJet;

								originalClient = %this.player.originalClient;
								name = %this.player.name;
								role = %this.player.role;

								timeOfDeath = %this.player.timeOfDeath;
								causeOfDeath = %this.player.causeOfDeath;

								isCorpse = true;
							};

				%notARealDeath = true;
			}
			else
			{
				%corpse = new AIPlayer("botCorpse")
							{
								datablock = PlayerNoJet;

								originalClient = %this;
								name = %this.getSimpleName();
								role = %this.role;

								timeOfDeath = %this.player.timeOfDeath;
								causeOfDeath = %this.player.causeOfDeath;

								isCorpse = true;
							};	
			}

			%corpse.MM_onCorpseReSpawn(%mini, %this, %srcClient, %this.player, %damageType);
		}
		else
		{
			MMDebug("Creating corpse", %this, %mini);
			%corpse = new AIPlayer(botCorpse)
						{
							datablock = PlayerNoJet;

							originalClient = %this;
							name = %this.getSimpleName();
							role = %this.role;

							timeOfDeath = $Sim::Time;
							causeOfDeath = %suicide;

							isCorpse = true;
						};

			%corpse.MM_onCorpseSpawn(%mini, %this, %srcClient, %damageType);
		}

		if(%this.player.doombot)
		{
			%corpse.unHideNode("ALL");
			%corpse.doombot = true;
		}

		if(isObject(%img = %this.player.getMountedImage(0)))
		{
			MMDebug("Mounting image", %this, %mini);
			%corpse.mountImage(%img, 0);
		}

		MMDebug("Warping corpse", %this, %mini);
		%corpse.setTransform(%this.player.getTransform());

		MMDebug("Removing body", %this, %mini);
		%this.player.removeBody();

		MMDebug("Preparing corpse", %this, %mini);
		%corpse.setNodeColor("ALL", "1 0 0 1");
		%corpse.setCrouching(true);
		%corpse.playThread(3, "death1");

		if(!%notARealDeath)
			%this.corpse = %corpse;
		else
			%corpse.originalClient.corpse = %corpse;

		MMDebug("Setting camera orbit", %this, %mini);
		%this.camera.setMode("Corpse", %corpse);
		%this.setControlObject(%this.camera);

		return parent::onDeath(%this, %srcObj, %srcClient, %damageType, %loc);
	}

	function Observer::onTrigger(%this, %obj, %slot, %val)
	{
		if(!isObject(%cl = %obj.getControllingClient()) || !isObject(%mini = getMiniGameFromObject(%cl)) || !$DefaultMinigame.running)
			return parent::onTrigger(%this, %obj, %slot, %val);

		if(!%val)
			return parent::onTrigger(%this, %obj, %slot, %val);

		switch(%slot)
		{
			case 0:
				%start = %obj.getEyePoint();
				%vec = %obj.getEyeVector();
				%end = VectorAdd(%start, VectorScale(%vec, $MM::GPCorpseInvestigationRange));

				%ray = containerRayCast(%start, %end, $Typemasks::PlayerObjectType | $Typemasks::FXbrickObjectType | $Typemasks::TerrainObjectType | $Typemasks::InteriorObjectType | $TypeMasks::VehicleObjectType, %obj);
				%hObj = firstWord(%ray);
				if(!isObject(%hObj) || !%hObj.isCorpse || %hObj.getClassName() !$= "AIPlayer")
					return parent::onTrigger(%this, %obj, %slot, %val);

				%hObj.MM_Investigate(%cl);
		}

		return parent::onTrigger(%this, %obj, %slot, %val);
	}

	function Armor::onTrigger(%this, %obj, %slot, %val)
	{
		if(!isObject(%cl = %obj.getControllingClient()) || !isObject(%mini = getMiniGameFromObject(%cl)) || !$DefaultMinigame.running)
			return parent::onTrigger(%this, %obj, %slot, %val);

		if(!%val)
			return parent::onTrigger(%this, %obj, %slot, %val);

		switch(%slot)
		{
			case 4:
				if(%obj.isGhost)
					return parent::onTrigger(%this, %obj, %slot, %val);
			
				if(!isObject(%obj.heldCorpse))
				{
					%start = %obj.getEyePoint();
					%vec = %obj.getEyeVector();
					%end = VectorAdd(%start, VectorScale(%vec, $MM::GPCorpseGrabRange));

					%ray = containerRayCast(%start, %end, $Typemasks::PlayerObjectType | $Typemasks::FXbrickObjectType | $Typemasks::TerrainObjectType | $Typemasks::InteriorObjectType | $TypeMasks::VehicleObjectType, %obj);
					%hObj = firstWord(%ray);
					if(!isObject(%hObj) || (!%hObj.isCorpse && !%hObj.isRisenCorpse) || %hObj.getClassName() !$= "AIPlayer")
						return parent::onTrigger(%this, %obj, %slot, %val);

					%obj.MM_PickUpCorpse(%hObj);

					return;
				}

				%obj.MM_ThrowCorpse();

				return;
		}

		return parent::onTrigger(%this, %obj, %slot, %val);
	}

	function Player::activateStuff(%obj)
	{
		%r = parent::activateStuff(%obj);

		if(!isObject(%cl = %obj.getControllingClient()) || !isObject(%mini = getMiniGameFromObject(%cl)) || !$DefaultMinigame.running)
			return %r;

		%start = %obj.getEyePoint();
		%vec = %obj.getEyeVector();
		%end = VectorAdd(%start, VectorScale(%vec, $MM::GPCorpseInvestigationRange));

		%ray = containerRayCast(%start, %end, $Typemasks::PlayerObjectType | $Typemasks::FXbrickObjectType | $Typemasks::TerrainObjectType | $Typemasks::InteriorObjectType | $TypeMasks::VehicleObjectType, %obj);
		%hObj = firstWord(%ray);
		if(!isObject(%hObj) || (!%hObj.isCorpse && !%hObj.isRisenCorpse) || %hObj.getClassName() !$= "AIPlayer")
			return %r;

		%hObj.MM_Investigate(%cl);

		return %r;
	}
};
activatePackage(MM_Corpses);