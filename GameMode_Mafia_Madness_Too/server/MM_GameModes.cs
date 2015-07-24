//MM_GameModes.cs
//Sorting functions for various gamemodes.

$MM::LoadedGameModes = true;

$MM::GameMode[0] = "Standard";
$MM::GameMode[1] = "Classic";
$MM::GameMode[2] = "Crazy";
$MM::GameMode[3] = "StandardPlusLaw";
$MM::GameMode[4] = "MafiaMadnessToo";
$MM::GameModes = 5;

$MM::DefaultGameMode = 4;

$MM::BringTheLaw = true;
$MM::LawChance = 0.1;
$MM::HonkHonk = true;
$MM::HonkHonkChance = 0.2;
$MM::AddTraitor = false;

$MM::MafRatio = 1 / 3.5;
$MM::CopRatio = 1 / 5;
$MM::MiscRatio = 1 / 7;
$MM::TraitorRatio = 1 / 10;

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

		// MMDebug(%r SPC %ct);

		if(!isObject($MM::RoleKey[%r]))
		{
			warn("No role exists for letter" SPC %r SPC ", using M instead.");
			%r = "M";
		}

		for(%j = 0; %j < %ct; %j++)
		{
			%str = %str SPC %r;
			%mCt++;

			// MMDebug(%str);
			// MMDebug(%mCt);

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

		// MMDebug(%r SPC %ct);

		if(!isObject($MM::RoleKey[%r]))
		{
			warn("No role exists for letter" SPC %r SPC ", using I instead.");
			%r = "I";
		}

		for(%j = 0; %j < %ct; %j++)
		{
			%str = %str SPC %r;
			%iCt++;

			// MMDebug(%str);
			// MMDebug(%iCt);

			if(%iCt >= %numInno)
				break;
		}

		if(%iCt >= %numInno)
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

	// %roles = "A V G C M F O P I";

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

function MM_InitModeStandardPlusLaw(%this)
{
	MMDebug("MM_InitModeStandardPlusLaw" SPC %this);

	%members = %this.MM_GetNumPlayers();

	MMDebug("   +Members:" SPC %members);

	%mafs = mFloor(%members / 3.5);
	if(%mafs < 1)
		%mafs = 1;

	MMDebug("   +Mafias:" SPC %mafs);

	%roles = "LAW A V G C M F O P I";

	%mafRoles = "LAW A V G C";
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

	for(%i = 0; %i < $MM::LawCt; %i++)
		%ctLAW += getRandom() < $MM::LawChance;

	for(%i = 0; %i < 5; %i++)
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

function MM_InitModeMafiaMadnessToo(%this)
{
	MMDebug("MM_InitModeMafiaMadnessToo" SPC %this);

	%members = %this.MM_GetNumPlayers();

	MMDebug("   +Members:" SPC %members);

	// %mafs = mFloor(%members / 3.5);
	%mafs = mFloor(%members * $MM::MafRatio);
	if(%mafs < 1)
		%mafs = 1;

	MMDebug("   +Mafias:" SPC %mafs);

	// %roles = "A V G C M F O P I";

	%mafRoles = "A V G C D LAW";
	%innoRoles = "F O P N IC L BB J CLOWN AM S T";

	///////////////////////////////
	/////ROLE ASSIGNMENT LOGIC/////
	///////////////////////////////

	%ctA = 1; //always have at least one abductor.

	if(%members > 3)
		%ctF = 1;

	if(%mafs > 1)
	{
		// %cops = mFloor(%members / 5);
		%cops = mFloor(%members * $MM::CopRatio);
		if(%cops >= 4) //max we want is one of each cop type
			%cops = 4;

		if(%cops > 0)
			%ctO = 1;

		if(%cops > 1)
		{
			%goofCops = "P P N N IC BB";
			%goofCt = %cops - 1;

			for(%i = 0; %i < %goofCt; %i++)
			{
				%rand = getRandom(getWordCount(%goofCops) - 1);
				%r = getWord(%goofCops, %rand);
				%goofCops = removeWord(%goofCops, %rand);

				%ct[%r] = 1; //may not necessarily get as many cops as we said we would!
			}
		}

		%ctG = getRandom(2) > 0;


		%goofMafs = "V V V C C D";
		%goofCt = %mafs - 1;

		for(%i = 0; %i < %goofCt; %i++)
		{
			%rand = getRandom(getWordCount(%goofMafs) - 1);
			%r = getWord(%goofMafs, %rand);

			// echo(%goofMafs);
			// echo(%r SPC %rand);

			%goofMafs = removeWord(%goofMafs, %rand);

			%ct[%r] = 1;
		}
	}

	if(%ctC > 0 && $MM::BringTheLaw && getRandom() < $MM::LawChance) //I Am The Law
	{
		%ctC--;
		%ctLAW++;
	}

	// if($MM::HonkHonkCt > 0)
	// 	for(%i = 0; %i < $MM::HonkHonkCt; %i++)
	// 		%ctCLOWN += (getRandom() < $MM::HonkHonkRate);

	%oddballs = mFloor(%members * $MM::MiscRatio + getRandom());
	%oddCt = getRandom(%oddballs);
	%odds = "L AM";
	if(%ctG > 0)
		%odds = %odds SPC "S";

	for(%i = 0; %i < %oddCt; %i++)
	{
		%rand = getRandom(getWordCount(%odds) - 1);
		%r = getWord(%odds, %rand);

		%odds = removeWord(%odds, %rand);

		%ct[%r] = 1;
	}

	%maxTraitors = mFloor(%members * $MM::TraitorRatio + getRandom());
	%traitorCt = getRandom(%maxTraitors);
	%traitors = "J";

	if($MM::AddTraitor)
		%traitors = %traitors SPC "T";

	for(%i = 0; %i < %traitorCt; %i++)
	{
		%rand = getRandom(getWordCount(%traitors) - 1);
		%r = getWord(%traitors, %rand);

		%traitors = removeWord(%traitors, %rand);

		%ct[%r] = 1;
	}

	if($MM::HonkHonk && %ctJ > 0 && getRandom() < $MM::HonkHonkChance)
	{
		%ctJ--;
		%ctCLOWN++;
	}

	///////////////////////////////
	///////////////////////////////
	///////////////////////////////


	%ct = getWordCount(%mafRoles);
	for(%i = 0; %i < %ct; %i++)
		%mafCts = %mafCts SPC (%ct[getWord(%mafRoles, %i)] | 0);
	%mafCts = trim(%mafCts);

	MMDebug("   +Maf Roles:" SPC %mafRoles);
	MMDebug("   +Maf Count:" SPC %mafCts);

	%ct = getWordCount(%innoRoles);
	for(%i = 0; %i < %ct; %i++)
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