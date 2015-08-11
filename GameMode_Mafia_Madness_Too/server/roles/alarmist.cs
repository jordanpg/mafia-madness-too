//alarmist.cs
//Code for the Alarmist innocent role.

if(!$MM::LoadedRole_Cultist)
	exec("./cultist.cs");

$MM::LoadedRole_Alarmist = true;

$MM::GPAlarmistFakeDeadStopRecruit = true;

if(!isObject(MMRole_Alarmist))
{
	new ScriptObject(MMRole_Alarmist)
	{
		class = "MMRole";

		name = "Alarmist";
		corpseName = "watchful inquisitor";
		displayName = "Alarmist";

		letter = "AL";

		colour = "<color:B3B300>";
		nameColour = "0.702 0.702 0";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		alignment = 0;

		helpText = 	"\c4You are also the <color:B3B300>Alarmist\c4! Your goal is to stop the <color:400040>cult \c4from recruiting!" NL
					"\c4At night, you can choose to protect another player from recruitment by selecting them with \c3/stopRecruit \c7[player name]\c4." NL
					"\c4If the cult attempts to recruit that player, the recruitment will fail. This effect will remain until the next day." NL 
					"\c4This role is best used to keep special innocents, such as the Fingerprint Expert or the Cop, in play.";

		description = 	"\c4The <color:B3B300>Alarmist\c4's' goal is to stop the <color:400040>cult \c4from recruiting." NL
						"\c4At night, you can choose to protect another player from recruitment by selecting them with \c3/stopRecruit \c7[player name]\c4." NL
						"\c4If the cult attempts to recruit that player, the recruitment will fail. This effect will remain until the next day." NL 
						"\c4This role is best used to keep special innocents, such as the Fingerprint Expert or the Cop, in play.";

		canStopRecruit = true;
	};
}

//SUPPORT
function GameConnection::MM_canStopRecruit(%this)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return false;

	if(%mini.isDay)
		return false;

	if(!isObject(%this.role))
		return false;

	if(%this.isGhost || %this.lives < 1)
		return false;

	if(!%this.role.canStopRecruit && !%mini.allStopRecruit)
		return false;

	if(%this.protected[%mini.day])
		return false;

	return true;
}

function serverCmdStopRecruit(%this, %a0, %a1, %a2, %a3, %a4, %a5)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)))
		return;

	if(!%this.MM_canStopRecruit())
	{
		if(%this.protected[%mini.day])
			messageClient(%this, '', "\c4You can only protect once person per night!");

		return;
	}

	%name = trim(%a0 SPC %a1 SPC %a2 SPC %a3 SPC %a4 SPC %a5);
	%tcl = findClientByName(%name);

	if(!isObject(%tcl))
	{
		%tcl = findClientByBL_ID($Pref::Server::MMNicknames[%name]);

		if(!isObject(%tcl))
		{
			messageClient(%this, '', "\c4Could not find player '\c3" @ %name @ "\c4!'");
			return;
		}
	}

	if(!isObject(%tcl.role))
	{
		messageClient(%this, '', "\c4This player is not in the current game!");
		return;
	}

	if(%tcl.lives < 1 || %tcl.isGhost)
	{
		if(!$MM::GPAlarmistFakeDeadStopRecruit)
		{
			messageClient(%this, '', "\c4This player cannot be protected because they are dead.");
			return;
		}
		else
			%fakeTry = true;
	}

	%this.protected[%mini.day] = true;
	if(!%fakeTry)
		%tcl.stopRecruitment = %this;

	%mini.MM_LogEvent(%this.MM_GetName(1) SPC "\c6chose to protect" SPC %tcl.MM_GetName(1) SPC "\c6from recruitment");

	messageClient(%this, '', "\c4Protecting\c3" SPC %tcl.getSimpleName() SPC "\c4from cult recruitment tonight.");
}

function MinigameSO::MM_ClearCultProtections(%this)
{
	for(%i = 0; %i < %this.numMembers; %i++)
		%this.member[%i].stopRecruitment = "";
}

//HOOKS
package MM_Alarmist
{
	function GameConnection::MM_CultRecruit(%cl, %src)
	{
		if(!isObject(%mini = getMiniGameFromObject(%cl)))
			return parent::MM_CultRecruit(%cl, %src);

		if(isObject(%cl.stopRecruitment))
		{
			if(!%cl.stopRecruitment.role.canStopRecruit)
				return;

			%mini.MM_LogEvent(%cl.stopRecruitment.MM_GetName(1) SPC "\c6protected" SPC %cl.MM_GetName(1) SPC "\c6from recruitment");

			return;
		}

		parent::MM_CultRecruit(%cl, %src);
	}

	function MinigameSO::MM_onDay(%this)
	{
		parent::MM_onDay(%this);

		%this.schedule(0, MM_ClearCultProtections);
	}

	function MMRole::onCleanup(%this, %mini, %client)
	{
		parent::onCleanup(%this, %mini, %client);

		%client.stopRecruitment = "";

		for(%i = 0; %i < %mini.day; %i++)
			%client.protected[%i] = false;
	}
};
activatePackage(MM_Alarmist);