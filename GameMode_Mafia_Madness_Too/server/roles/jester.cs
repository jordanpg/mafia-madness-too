//jester.cs
//Code for the unaligned Jester role.

$MM::GPJesterNoMafTrigger = true;

if(!isObject(MMRole_Jester))
{
	new ScriptObject(MMRole_Jester)
	{
		class = "MMRole";

		name = "Jester";
		corpseName = "suicidal gagster";
		displayName = "Jester";

		letter = "J";

		colour = "<color:80FF80>";
		nameColour = "0.5 1 0.5";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		alignment = 2;

		helpText = 	"\c4You are the <color:80FF80>Jester\c4! You aren't part of the mafia or innocent." NL
					"\c4If you get killed by any other player, you will win the round." NL
					"\c4But your gun is empty! You must get players to kill you by bringing suspicion onto yourself." NL
					"\c4Suicide and falling won't count, though! The only death that counts is a kill.";

		description = 	"\c4The <color:80FF80>Jester\c4 isnt't part of the mafia or the innocents." NL
						"\c4If you get killed by any other player, you will win the round." NL
						"\c4But your gun is empty! You must get players to kill you by bringing suspicion onto yourself." NL
						"\c4Suicide and falling won't count, though! The only death that counts is a kill.";

		// gun = -1;
	};
}

//HOOKS
function MMRole_Jester::SpecialWinCheck(%this, %mini, %client, %killed, %killer)
{
	%r = parent::SpecialWinCheck(%this, %mini, %client, %killed, %killer);

	if(%killed == %killer || !isObject(%killer))
		return %r;

	if(%client == %killed && %client.lives < 1 && !(%killer.MM_isMaf() && $MM::GPJesterNoMafTrigger))
	{
		// talk(%client.lives);
		%mini.MM_LogEvent("<color:80FF80>HONK HONK");
		talk("The Jester was killed! Everybody loses!");
		MMDebug("Jester win", %mini, %killed, %killer, %client);

		%mini.resolved = 1;
		%mini.schedule(3000, MM_Stop);

		return 4;
	}

	return %r;
}

function MMRole_Jester::onSpawn(%this, %mini, %client)
{
	parent::onSpawn(%this, %mini, %client);

	if(isObject(%client.player))
	{
		%client.player.forceEmptyGun = true;
		%client.player.toolAmmo[0] = 0; //shouldn't be necessary but i'll have the line here just in case
	}
}