//ahole.cs
//Ripped from the original. Revised implementation to be a bit more modular.
//These are actually disabled by default in the release version of MM, but they should still work.

$MM::LoadedAHole = true;

$MM::GPEnableAholeCmds = false;

function aholeCmdSayCheck(%client,%msg) {
	switch$(strUpr(%msg)) {
		case "EVERY CASE IS GONNA BE AIRTIGHT":
			aholeAirtight(%client);
		// case "I AM BECOME DEATH, DESTROYER OF WORLDS":
			// aholeDoombot(%client);
	}
}

function aholeCmdShoutCheck(%client,%msg) {
	switch$(strUpr(%msg)) {
		case "FOR FREEDOM WE RISE":
			aholeGravity(%client);
		case "FUS RO DAH":
			aholeShout(%client);
		case "I TRUSTED YOU TITA":
			aholeBetrayed(%client);
	}
}

function aholeCmdWhisperCheck(%client,%msg) {
	return;
}

function aholeCmdLowCheck(%client,%msg) {
	switch$(strUpr(%msg)) {
		case "I AM BECOME DEATH, DESTROYER OF WORLDS":
			aholeDoombot(%client);
	}
}

function aholeAirtight(%client) {
	if(isObject(%p = %client.player)) {
		if(%p.airtight)
			return;
		%p.playThread(0,look);
		messageAll('',"\c3" @ %client.getSimpleName() SPC "\c6is looking for clues!");
		%p.airtight = 1;
	}
}

function aholeGravity(%client) {
	if(isObject(%p = %client.player)) {
		if(%p.gravity)
			return;
		if(%client.BL_ID == 10104) {
			%p.setVelocity("0 0 20");
			%p.schedule(100,setVelocity,"0 0 -200");
			messageAll('',"\c3" @ %client.getSimpleName() SPC "\c6sucks!");
			%p.gravity = 1;
			return;
		}
		%p.gravity = 1;

		if(isObject(MusicData_Gravity_Hurts))
		{
			%p.playAudio(0,"MusicData_Gravity_Hurts");
			%a = new AudioEmitter() { position = %p.getPosition(); coneVector = "0 1 0"; maxDistance = 50; profile = "MusicData_Gravity_Hurts";};
			%a.schedule(7000,delete);
		}
		
		%p.setVelocity("0 0 200");
		messageAll('',"\c3" @ %client.getSimpleName() SPC "\c6learned to fly!");
	}
}

function aholeShout(%client) {
	if(isObject(%p = %client.player)) {
		if(%mini = %client.minigame) {
			if(($MMLastShout+1200000) > getRealTime()) {
				return;
			}

			getMinigameFromObject(%client).MM_LogEvent(%client.MM_getName(1) SPC "\c6activated their Thu'um");

			$MMLastShout = getRealTime();
			%len = 30;
			%rad = 10;
			for(%i = 0;%i<%mini.numMembers;%i++) {
				%cl = %mini.member[%i];
				if(!isObject(%cl))
					continue;
				if(%cl == %client)
					continue;
				if(!isObject(%cl.player))
					continue;
				//talk("found a player, name is" SPC %cl.getSimpleName());
				%bPos = %cl.player.getEyePoint();
				%aPos = %p.getEyePoint();
				%aVec = %p.getEyeVector();
				//dest, then start
				%bVec = VectorSub(%bPos, %aPos);
				%cDist = (getWord(%aVec,0) * getWord(%bVec,0)) + (getWord(%aVec,1) * getWord(%bVec,1)) + (getWord(%aVec,2) * getWord(%bVec,2));
				if(%cDist > %len) {
					//talk("further away than length of cone, lol");
					continue;
				}
				%cPos = VectorAdd(%aPos, VectorScale(%aVec,%cDist));
				%cRad = %rad * (%cDist / %len);
				%cbDist = VectorDist(%cPos, %bPos);
				//talk(%cbDist);
				//talk(%cRad);
				//talk(%cR
				if(%cbDist <= %cRad) {
					//talk(VectorScale(VectorNormalize(%bVec),50));
					%cl.player.addVelocity(VectorScale(VectorNormalize(VectorSub(%bPos, VectorAdd(%aPos,"0 0 -2"))),50));
				}
			}
		}
	}
}

function aholeBetrayed(%client) {
	%player = %client.player;
	%titaplayer = findclientbyname("tita").player;
	if(isObject(%titaplayer) && isObject(%player)) {
		%player.damage(%titaplayer,"0 0 0",100,$DamageType::Gun);
		if(isObject(%client.corpse)) {
			%client.corpse.fingerprints[0] = "The Titanium";
			%client.corpse.fingerprintcount++;
		}
		messageAll('',"\c3" @ %client.getSimpleName() SPC "\c6was betrayed!");
	}
}

function aholeDoombot(%client) {
	if(isObject(%p = %client.player)) {
		if(%mini = %client.minigame) {
			if(%mini.doombot || %p.doombot)
				return;
			%p.doombotTries++;
			%lastdigits = getSubStr(getRealTime(),strLen(getRealTime())-2,2);
			echo(%lastDigits);
			if(%lastDigits == 13) {
				%mini.doombot = 1;
				%p.doombot = 1;
				%client.applyBodyParts();
				%client.applyBodyColors();
				messageAll('',"\c8He lives.");
			}
			else {
				if(%p.doombotTries == 3) {
					%p.kill();
					messageAll('',"\c6He sleeps.");
				}
			}
			// %mini.doombot = 1;
			// %p.doombot = 1;
			// %client.applyBodyParts();
			// %client.applyBodyColors();
			// messageAll('',"\c8He lives.");
		}
	}
}

package MM_AHole
{
	function GameConnection::MM_Chat(%this, %obj, %type, %msg, %excludeList, %pre2, %condition, %a0, %a1, %a2, %a3, %a4)
	{
		%r = parent::MM_Chat(%this, %obj, %type, %msg, %excludeList, %pre2, %condition, %a0, %a1, %a2, %a3, %a4);

		if(!$MM::GPEnableAholeCmds || !$DefaultMinigame.running || %this.lives < 1 || %this.isGhost || !isObject(%this.player))
			return %r;

		%rCl = (isObject(%obj.getControllingClient()) ? %obj.client : %this);

		if(%type == 1)
			aholeCmdSayCheck(%rCl, %msg);
		else if(%type == 2)
			aholeCmdShoutCheck(%rCl, %msg);
		else if(%type == 3)
			aholeCmdLowCheck(%rCl, %msg);
		else if(%type == 4)
			aholeCmdWhisperCheck(%rCl, %msg);

		return %r;
	}

	// function serverCmdMessageSent(%this, %msg)
	// {
	// 	if(!$MM::GPEnableAholeCmds || !$DefaultMinigame.running || %this.lives < 1 || %this.isGhost || !isObject(%this.player))
	// 		return parent::serverCmdMessageSent(%this, %msg);

	// 	%mark = getSubStr(%msg, 0, 1);
	// 	%rMsg = getSubStr(%msg, 1, strLen(%msg) - 1);

	// 	if(%mark $= "!" || (strCmp(strUpr(%msg), %msg) == 0 && strCmp(strLwr(%msg), %msg) != 0))
	// 		aholeCmdShoutCheck(%this, %rmsg);
	// 	else
	// 		aholeCmdSayCheck(%this, %msg);

	// 	parent::serverCmdMessageSent(%this, %msg);
	// }

	// function serverCmdTeamMessageSent(%this, %msg)
	// {
	// 	if(!$MM::GPEnableAholeCmds || !$DefaultMinigame.running || %this.lives < 1 || %this.isGhost || !isObject(%this.player))
	// 		return parent::serverCmdTeamMessageSent(%this, %msg);

	// 	%mark = getSubStr(%msg, 0, 1);
	// 	%rMsg = getSubStr(%msg, 1, strLen(%msg) - 1);

	// 	if(%mark $= "!")
	// 		aholeCmdWhisperCheck(%this, %rmsg);
	// 	else
	// 		aholeCmdLowCheck(%this, %msg);

	// 	parent::serverCmdMessageSent(%this, %msg);
	// }
};
activatePackage(MM_AHole);