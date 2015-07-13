//MM_GameModes.cs
//Sorting functions for various gamemodes.

$MM::LoadedGameModes = true;

$MM::GameMode[0] = "Standard";
$MM::GameMode[1] = "Classic";
$MM::GameMode[2] = "Crazy";
$MM::GameModes = 3;

function MM_BuildRolesString(%numMaf, %numPlayers, %mafRoles, %innoRoles, %mafCts, %innoCts, %mafFill, %innoFill)
{
	if(%mafFill $= "")
		%mafFill = "M";

	if(%innoFill $= "")
		%innoFill = "I";

	%numInno = %numPlayers - %numMaf;

	%str = "";

	%mCt = 0;

	%num = getWordCount(%mafRoles);
	for(%i = 0; %i < %num; %i++)
	{
		%ct = getWord(%mafCts, %i);
		%r = getWord(%mafRoles, %i);

		if(!isObject($MM::RoleKey[%r]))
		{
			warn("No role exists for letter" SPC %r SPC ", using M instead.");
			%r = "M";
		}

		for(%j = 0; %j < %ct; %j++)
		{
			%str = %str SPC %r;
			%mCt++;

			if(%mCt >= %numMaf)
				break;
		}

		if(%mCt >= %numMaf)
			break;
	}

	for(%i = %mCt; %i < %numMaf; %i++)
		%str = %str SPC %mafFill;

	%iCt = 0;

	%num = getWordCount(%innoRoles);
	for(%i = 0; %i < %num; %i++)
	{
		%ct = getWord(%innoCts, %i);
		%r = getWord(%innoRoles, %i);

		if(!isObject($MM::RoleKey[%r]))
		{
			warn("No role exists for letter" SPC %r SPC ", using I instead.");
			%r = "I";
		}

		for(%j = 0; %j < %ct; %j++)
		{
			%str = %str SPC %r;
			%iCt++;

			if(%iCt >= %numInno)
				break;
		}

		if(%iCt >= %numMaf)
			break;
	}

	for(%i = %iCt; %i < %numInno; %i++)
		%str = %str SPC %innoFill;

	return trim(%str);
}

function MM_InitModeStandard(%this)
{
	MMDebug("MM_InitModeStandard" SPC %this);

	%members = %this.MM_GetNumPlayers();

	MMDebug("   +Members:" SPC %members);

	%mafs = mFloor(%members / 3.5);
	if(%mafs < 1)
		%mafs = 1;

	MMDebug("   +Mafias:" SPC %mafs);

	%roles = "A V G C M F O P I";

	%mafRoles = "A V G C";
	%innoRoles = "F O P L";

	%ctA = 1;
	%ctL = %this.numMillers;

	if(%members > 3)
	{
		if(%mafs > 1)
		{
			%ctO = 1;

			if(%mafs < 3)
				%ctV = getRandom(1);
			else
			{
				%ctV = 1;

				if(%mafs > 3)
					%ctC = 1;
			}

			if(%members > 11)
				%ctF = 1;

			if(%members > 9)
				%ctP = 1;

			%ctG = 1;
		}
		else
			%ctF = 1;
	}

	for(%i = 0; %i < 4; %i++)
		%mafCts = %mafCts SPC (%ct[getWord(%mafRoles, %i)] | 0);
	%mafCts = trim(%mafCts);

	MMDebug("   +Maf Roles:" SPC %mafRoles);
	MMDebug("   +Maf Count:" SPC %mafCts);

	for(%i = 0; %i < 4; %i++)
		%innoCts = %innoCts SPC (%ct[getWord(%innoRoles, %i)] | 0);
	%innoCts = trim(%innoCts);

	MMDebug("   +Inno Roles:" SPC %innoRoles);
	MMDebug("   +Inno Count:" SPC %innoCts);

	%this.roles = MM_BuildRolesString(%mafs, %members, %mafRoles, %innoRoles, %mafCts, %innoCts);

	MMDebug("   +Roles:" SPC %this.roles);

	%this.allAbduct = false;
	%this.allComm = false;
	%this.allImp = false;
	%this.allInv = false;
}

function MM_InitModeClassic(%this)
{
	MMDebug("MM_InitModeClassic" SPC %this);

	%members = %this.MM_GetNumPlayers();

	MMDebug("   +Members:" SPC %members);

	%mafs = mFloor(%members / 3.5);
	if(%mafs < 1)
		%mafs = 1;

	MMDebug("   +Mafias:" SPC %mafs);

	%this.roles = MM_BuildRolesString(%mafs, %members);

	MMDebug("   +Roles:" SPC %this.roles);

	%this.allAbduct = false;
	%this.allComm = false;
	%this.allImp = false;
	%this.allInv = false;
}

function MM_InitModeCrazy(%this)
{
	MMDebug("MM_InitModeCrazy" SPC %this);

	%members = %this.MM_GetNumPlayers();

	MMDebug("   +Members:" SPC %members);

	%mafs = mFloor(%members / 1.75);
	if(%mafs < 1)
		%mafs = 1;

	MMDebug("   +Mafias:" SPC %mafs);

	%mafRoles = "C";
	%mafCts = %mafs;

	MMDebug("   +Maf Roles:" SPC %mafRoles);
	MMDebug("   +Maf Count:" SPC %mafCts);

	%this.roles = MM_BuildRolesString(%mafs, %members, %mafRoles, "", %mafCts, "");

	MMDebug("   +Roles:" SPC %this.roles);

	%this.allAbduct = false;
	%this.allComm = false;
	%this.allImp = false;
	%this.allInv = false;
}