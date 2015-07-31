//amnesiac.cs
//Code for the unaligned Amnesiac role.

$MM::LoadedRole_Amnesiac = true;

if(!isObject(MMRole_Amnesiac))
{
	new ScriptObject(MMRole_Amnesiac)
	{
		class = "MMRole";

		name = "Amnesiac";
		corpseName = "forgetful dummy";
		displayName = "Amnesiac";

		letter = "AM";

		colour = "<color:D0D0D0>";
		nameColour = "0.816 0.816 0.816";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		alignment = 2;

		helpText = 	"\c4You are the <color:D0D0D0>Amnesiac\c4! Starting out, \c3you are neither innocent nor mafia!" NL
					"\c4At some point in the game, \c3you can decide to take the role from a corpse by holding it and typing \c6/takeRole\c4." NL
					"\c4You will adopt that role's alignment and play for that team, while inheriting all the abilities that the role gives." NL
					"\c4When you adopt a new role, all players will receive a notification declaring that the amnesiac (no name is given) has 'remembered' they are the chosen role." NL
					"\c4If you choose to become a member of the mafia, all mafia members will be notified and you will have access to the mafia list.";

		description = 	"\c4Starting out, the <color:D0D0D0>Amnesiac\c3 is neither innocent nor mafia!" NL
						"\c4At some point in the game, \c3you can decide to take the role from a corpse by holding it and typing \c6/takeRole\c4." NL
						"\c4You will adopt that role's alignment and play for that team, while inheriting all the abilities that the role gives." NL
						"\c4When you adopt a new role, all players will receive a notification declaring that the amnesiac (no name is given) has 'remembered' they are the chosen role." NL
						"\c4If you choose to become a member of the mafia, all mafia members will be notified and you will have access to the mafia list.";

		amnesiac = true;
	};
}

//SUPPORT
function GameConnection::MM_isAmnesiac(%this)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return false;

	if(%this.isGhost || %this.lives < 1)
		return false;

	if(!isObject(%this.role))
		return false;

	if(!%this.role.amnesiac)
		return false;

	return true;
}

function GameConnection::MM_RememberRole(%this, %role)
{
	if(!%this.MM_isAmnesiac() || !isObject(%role))
		return false;

	%mini = getMiniGameFromObject(%this);

	%roleStr = %role.getColour(1) @ %role.getRoleName();

	%mini.MM_LogEvent(%this.MM_getName(1) SPC "\c6became the" SPC %roleStr);

	%this.MM_SetRole(%role);

	messageAll('', "<font:impact:24pt>\c4The <color:D0D0D0>Amnesiac \c4has remembered that they are the" SPC %roleStr @ "\c4!");

	%this.knowsFullRole = true;

	%this.MM_UpdateUI();

	%this.MM_DisplayStartText();

	%this.clearInventory();

	%this.MM_GiveEquipment();

	%this.applyBodyParts();
	%this.applyBodyColors();

	if(%this.MM_isMaf())
	{
		for(%i = 0; %i < %mini.numMembers; %i++)
		{
			%mem = %mini.member[%i];

			if(%mem.MM_isMaf())
			{
				messageClient(%mem, '', "<font:impact:24pt>\c3" @ %this.getSimpleName() SPC "\c4has joined the mafia as the" SPC %roleStr @ "\c4!");
				%mem.MM_DisplayMafiaList(2);
			}

			if(isFunction(GameConnection, MM_isCultist) && %mem.MM_isCultist())
			{
				messageClient(%mem, '', "<font:impact:24pt>\c3" @ %this.getSimpleName() SPC "\c4has joined the cult as the" SPC %roleStr @ "\c4!");
				%mem.MM_DisplayCultList(2);
			}
		}
	}

	// if(isFunction(GameConnection, MM_isCultist))
	// {
	// 	if(%this.MM_isCultist())
	// 	{
	// 		for(%i = 0; %i < %mini.numMembers; %i++)
	// 		{
	// 			%mem = %mini.member[%i];

	// 			if(%mem.MM_isCultist())
	// 			{
	// 				messageClient(%mem, '', "<font:impact:24pt>\c3" @ %this.getSimpleName() SPC "\c3has joined the cult as the" SPC %roleStr @ "\c3!");
	// 				%mem.MM_DisplayCultList(2);
	// 			}
	// 		}
	// 	}
	// }
}

function serverCmdTakeRole(%this)
{
	if(!%this.MM_isAmnesiac())
		return;

	if(!isObject(%this.player))
		return;

	if(!isObject(%this.player.heldCorpse))
	{
		messageClient(%this, '', "\c4You cannot take a role because you aren't holding a corpse! Pick up a corpse that contains the role you would like to shift to.");
		return;
	}

	if(%this.player.heldCorpse.disfigured || !isObject(%this.player.heldCorpse.role))
	{
		messageClient(%this, '', "\c4You cannot determine this corpse's role.");
		return;
	}

	%this.MM_RememberRole(%this.player.heldCorpse.role);
}