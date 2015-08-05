//MM_GameModes.cs
//Sorting functions for various gamemodes.

$MM::LoadedGameModes = true;

$MM::GameMode[-1] = "Custom";
$MM::GameMode[0] = "Original";
$MM::GameMode[1] = "Classic";
$MM::GameMode[2] = "Crazy";
$MM::GameMode[3] = "StandardPlusLaw";
$MM::GameMode[4] = "MafiaMadnessToo";
$MM::GameModes = 5;

$MM::DefaultGameMode = -1;

$MM::BringTheLaw = true;
$MM::LawChance = 0.1;
$MM::HonkHonk = true;
$MM::HonkHonkChance = 0.2;
$MM::AddTraitor = false;
$MM::JohnCena = 1;
$MM::JohnCenaChance = 0.07;
$MM::CultistChance = 0.5;

$MM::MafRatio = 1 / 3.5;
$MM::CopRatio = 1 / 5;
$MM::MiscRatio = 1 / 7;
$MM::TraitorRatio = 1 / 10;

$MM::GameModeDir = $MM::Server @ "gamemodes/";
$MM::CustomFile = "mmtoo.txt";

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
	// %members = $debugVal;

	MMDebug("   +Members:" SPC %members);

	// %mafs = mFloor(%members / 3.5);
	%mafs = mFloor(%members * $MM::MafRatio);
	if(%mafs < 1)
		%mafs = 1;

	MMDebug("   +Mafias:" SPC %mafs);

	// %roles = "A V G C M F O P I";

	%mafRoles = "JOHNCENA A V G C D LAW";
	%innoRoles = "F O P N IC L BB J CLOWN AM S T RC ZC";

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

	if($MM::JohnCena > 0)
		for(%i = 0; %i < $MM::JohnCena; %i++)
			if(getRandom() < $MM::JohnCenaChance)
				%ctJOHNCENA++;

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
	%traitors = "";

	if(getRandom() < $MM::CultistChance)
		%traitors = %traitors SPC "RC";

	if(%ctD > 0)
		%traitors = %traitors SPC "J";

	if($MM::AddTraitor)
		%traitors = %traitors SPC "T";

	%traitors = trim(%traitors);

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

/////////////////////////////////////////
//////CUSTOM GAMEMODE FUNCTIONALITY//////
/////////////////////////////////////////

function MM_LoadGameMode(%filen)
{
	if(!isFile(%filen))
		return false;

	deleteVariables("$MM::GM*");

	%file = new FileObject();
	%file.openForRead(%filen);

	%line = %file.readLine();
	if(firstWord(%line) !$= "MMGAMEMODE")
	{
		%file.close();
		%file.delete();

		return false;
	}

	$MM::GMName = restWords(%line);

	while(!%file.isEOF())
	{
		%line = trim(%file.readLine());

		if(getSubStr(%line, 0, 2) $= "//")
			continue;

		if(getSubStr(%line, 0, 1) $= "[")
		{
			%preVar = getSubStr(%line, 1, strLen(%line) - 2);
			continue;
		}

		if(getSubStr(%line, 0, 1) $= "<")
		{
			%preVal = getSubStr(%line, 1, strLen(%line) - 2);
			continue;
		}

		%ct = getWordCount(%line);
		for(%i = 0; %i < %ct; %i++)
		{
			%w = getWord(%line, %i);

			if(getSubStr(%w, 0, 2) $= "%%")
				%line = setWord(%line, %i, $MMG[getSubStr(%w, 2, strLen(%w) - 2)]);
		}

		%var = firstWord(%line);
		%val = restWords(%line);

		if(%var $= "PREF")
		{
			%var = firstWord(%val);
			%val = restWords(%val);

			%isPref = true;
		}
		else
			%isPref = false;

		if(getSubStr(%var, 0, 1) $= "%")
		{
			%var = getSubStr(%var, 1, strLen(%var) - 1);
			%tempVar = true;
		}
		else
			%tempVar = false;

		if(%preVar !$= "")
			%var = firstWord(%preVar) @ %var;

		if(%preVal !$= "")
			%val = firstWord(%preVal) @ %val;

		if(%isPref)
			$MM::GP[%var] = %val;
		else if(%tempVar)
			$MMG[%var] = %val;
		else
			$MM::GM[%var] = %val;
	}

	%file.close();
	%file.delete();

	return true;
}

function MM_EvaluateCondition(%c)
{
	%status = false;
	%str = %c;
	echo(%c);

	for(%i = 0; %str !$= ""; %i++)
	{
		%str = nextToken(%str, "cond", ",");

		%cond = trim(%cond);

		echo(%i SPC %str);
		echo(%cond);

		if(%i > 0)
		{
			%log = firstWord(%cond);
			%cond = restWords(%cond);
		}
		else
			%log = "";

		%j = -1;

		%c1 = getWord(%cond, %j++);
		if(%c1 $= "?")
		{
			%min = getWord(%cond, %j++);

			if(%min $= "~")
				%c1 = getRandom();
			else
			{
				%max = getWord(%cond, %j++);

				if(getWord(%cond, %j + 1) $= "float")
				{
					%c1 = getRandomFloat(%min, %max);
					%j++;
				}
				else
					%c1 = getRandom(%min, %max);
			}
		}

		%op = getWord(%cond, %j++);
		if(%op $= "not")
		{
			%not = true;
			%op = getWord(%cond, %j++);
			%c2 = getWord(%cond, %j++);
		}
		else
		{
			%not = false;
			%c2 = getWord(%cond, %j++);
		}

		if(%c2 $= "?")
		{
			%min = getWord(%cond, %j++);

			if(%min $= "~")
				%c2 = getRandom();
			else
			{
				%max = getWord(%cond, %j++);

				if(getWord(%cond, %j + 1) $= "float")
				{
					%c2 = getRandomFloat(%min, %max);
					%j++;
				}
				else
					%c2 = getRandom(%min, %max);
			}
		}

		if(getSubStr(%c1, 0, 1) $= "%")
			%c1 = $MMG[getSubStr(%c1, 1, strLen(%c1) - 1)];
		if(getSubStr(%c2, 0, 1) $= "%")
			%c2 = $MMG[getSubStr(%c2, 1, strLen(%c2) - 1)];

		echo(%c1 SPC %op SPC %c2);

		switch$(%op)
		{
			case "==": %r = (%c1 == %c2);
			case "!=" or "<>" or "=/=": %r = (%c1 != %c2);
			case "<": %r = (%c1 < %c2);
			case ">": %r = (%c1 > %c2);
			case "<=" or "=<": %r = (%c1 <= %c2);
			case ">=" or "=>": %r = (%c1 >= %c2);
			case "$=": %r = (%c1 $= %c2);
			case "=":
				if((%c1 * 1) $= %c1 && (%c2 * 1) $= %c2) //not going to touch scientific notation eugh
					%r = (%c1 == %c2);
				else
					%r = (%c1 $= %c2);
			case "has": %r = isInList(%c1, %c2);
			case "contains": %r = (striPos(%c1, %c2) != -1);
			default:
				warn("Could not evaluate condition '" @ %cond @ "' due to an unknown operator '" @ %op @ "'");
				continue;
		}

		%r ^= %not;

		if(%log $= "and" || %log $= "&&" || %log $= "&")
			%status = (%status && %r);
		else
			%status = (%status || %r);

		echo(%r SPC %status);
	}

	return %status;
}

function MM_ModeReadyCustom(%this)
{
	if(!$MM::CustomManual)
	{
		if(!isFile($MM::CustomFile))
		{
			if(isFile($MM::GameModeDir @ $MM::CustomFile))
				$MM::CustomFile = $MM::GameModeDir @ $MM::CustomFile;
			else if(isFile($Pref::Server::MMCustomFile))
				$MM::CustomFile = $Pref::Server::MMCustomFile;
			else if(isFile($MM::GameModeDir @ $Pref::Server::MMCustomFile))
				$MM::CustomFile = $MM::GameModeDir @ $Pref::Server::MMCustomFile;
			else
				return false;
		}

		if(!MM_LoadGameMode($MM::CustomFile))
			return false;
	}

	if($MM::GMSortGroupCt < 1)
		return false;

	return true;
}

function MM_InitModeCustom(%this)
{
	MMDebug("MM_InitModeCustom" SPC %this);
	MMDebug("   +Gamemode Name:" SPC $MM::GMName);

	$MMGmembers = %this.MM_GetNumPlayers();
	// $MMGmembers = $debugVal;

	MMDebug("   +Members:" SPC $MMGmembers);

	// %mafs = mFloor(%members / 3.5);
	$MMGmafs = mFloor($MMGmembers * $MM::GMMafRatio);
	if($MMGmafs < $MM::GMMinMaf)
		$MMGmafs = $MM::GMMinMaf;

	$MMGinnos = $MMGmembers - $MMGmafs;

	MMDebug("   +Mafias:" SPC $MMGmafs);

	// %roles = "A V G C M F O P I";

	// %mafRoles = "JOHNCENA A V G C D LAW";
	// %innoRoles = "F O P N IC L BB J CLOWN AM S T RC ZC";

	$MMGmafRoles = "";
	$MMGinnoRoles = "";

	if(isFunction(%f = "MMGameMode_" @ $MM::GMPreSortCall))
		call(%f, %this);

	for(%i = 0; %i < $MM::GMSortGroupCt; %i++)
	{
		MMDebug("   -Evaluating Sort Group" SPC %i);

		%condition = $MM::GMSortGroup[%i, "Condition"];

		MMDebug("   --+Condition:" SPC %condition);

		if(%condition !$= "" && !MM_EvaluateCondition(%condition))
		{
			MMDebug("   ---Failed condition, skipping group...");
			continue;
		}

		%pot = $MM::GMSortGroup[%i, "RolePot"];
		$MMGmaf = $MM::GMSortGroup[%i, "isMaf"];

		MMDebug("   --+Pot:" SPC %pot);
		MMDebug("   --+isMaf:" SPC $MMGmaf);

		%multiDraw = $MM::GMSortGroup[%i, "AllowMultipleDraw"];
		%noRemoveDrawn = $MM::GMSortGroup[%i, "KeepDrawnRoles"];
		%ratio = $MM::GMSortGroup[%i, "Ratio"];
		%src = $MM::GMSortGroup[%i, "RatioVar"];

		MMDebug("   --+MultiDraw, NoRemoveDrawn:" SPC %multiDraw @ "," SPC %noRemoveDrawn);

		if(getSubStr(%src, 0, 1) $= "%")
			%src = getSubStr(%src, 1, strLen(%src) - 1);

		MMDebug("   --+Ratio Factor:" SPC %ratio);

		$MMGsrcNum = $MMG[%src];

		MMDebug("   --+Ratio Source:" SPC %src @ "," SPC $MMGsrcNum);

		if($MM::GMSortGroup[%i, "RatioAddRandom"])
		{
			%min = ($MM::GMSortGroup[%i, "RatioRandomMin"] >= 0 ? $MM::GMSortGroup[%i, "RatioRandomMin"] : 0);
			%max = ($MM::GMSortGroup[%i, "RatioRandomMax"] >= %min ? $MM::GMSortGroup[%i, "RatioRandomMax"] : 1);

			MMDebug("   --+Random Params:" SPC %min @ "," SPC %max);

			if(%max > 1)
				%max = 1;

			$MMGratioRand = getRandomFloat(%min, %max);

			MMDebug("   --+Random:" SPC $MMGratioRand);
		}
		else
			$MMGratioRand = 0;

		MMDebug("   --+Round:" SPC ($MM::GMSortGroup[%i, "RatioRound"] ? "true" : "false"));

		%mod = mFloor($MM::GMSortGroup[%i, "RatioModCount"]);

		MMDebug("   --+Mod:" SPC %mod);

		$MMGgroupMems = mFloor($MMGsrcNum * %ratio + $MMGratioRand + ($MM::GMSortGroup[%i, "RatioRound"] ? 0.5 : 0));

		MMDebug("   --+Initial Mems:" SPC $MMGgroupMems);

		$MMGgroupMems += %mod;

		MMDebug("   --+Modded Mems:" SPC $MMGgroupMems);

		if($MM::GMSortGroup[%i, "RandomCount"])
		{
			%min = $MM::GMSortGroup[%i, "RandomCountMin"] | 0;

			MMDebug("   --+Random Count Min:" SPC %min);

			if(%min < $MMGgroupMems)
				$MMGgroupMems = getRandom(%min, $MMGgroupMems);
		}

		%e = $MM::GMSortGroup[%i, "ForceMinMembers"] | 0;
		if(%e >= 0 && $MMGgroupMems < %e)
			$MMGgroupMems = %e;

		MMDebug("   --+Min Mems:" SPC %e);

		%e = $MM::GMSortGroup[%i, "ForceMaxMembers"] | 0;
		if(%e > 0 && $MMGgroupMems > %e)
			$MMGgroupMems = %e;

		MMDebug("   --+Max Mems:" SPC %e);

		MMDebug("   --+Members:" SPC $MMGgroupMems);

		$MMGrolePot = "";

		%ct = getWordCount(%pot);
		for(%j = 0; %j < %ct; %j++)
		{
			%r = getWord(%pot, %j);

			MMDebug("   ---Attempting to add role" SPC %r SPC "to the pot...");

			%cond = $MM::GMSortGroup[%i, "RoleCondition", %r];
			MMDebug("   ---+Condition:" SPC %cond);
			if(%cond !$= "" && !MM_EvaluateCondition(%cond))
			{
				MMDebug("   ----Failed");
				continue;
			}

			$MMGrolePot = trim($MMGrolePot SPC %r);

			if(!%has[%r])
			{
				if($MMGmaf)
					$MMGmafRoles = trim($MMGmafRoles SPC %r);
				else
					$MMGinnoRoles = trim($MMGinnoRoles SPC %r);

				%has[%r] = true;
			}
		}

		MMDebug("   --+Curr Maf Roles:" SPC $MMGmafRoles);
		MMDebug("   --+Curr Inno Roles:" SPC $MMGinnoRoles);
		MMDebug("   --+Role Pot:" SPC $MMGrolePot);

		for(%j = 0; %j < $MMGgroupMems; %j++)
		{
			%ct = getWordCount($MMGrolePot) - 1;
			%rand = getRandom(%ct);

			%role = getWord($MMGrolePot, %rand);

			if(!%noRemoveDrawn)
				$MMGrolePot = removeWord($MMGrolePot, %rand);

			if(%multiDraw)
				$MMGct[%role]++;
			else
				$MMGct[%role] = 1;

			MMDebug("   ---Added role" SPC %role SPC "(" @ $MMGct[%role] @ ")");

			if($MMGrolePot $= "")
				break;
		}

		MMDebug("   --+Final Pot:" SPC $MMGrolePot);
	}

	if(isFunction(%f = "MMGameMode_" @ $MM::GMPostSortCall))
		call(%f, %this);

	if($MM::GMForceMafOrder !$= "")
	{
		%newOrder = "";

		%ct = getWordCount($MM::GMForceMafOrder);
		for(%i = 0; %i < %ct; %i++)
		{
			%r = getWord($MM::GMForceMafOrder, %i);

			%newOrder = trim(%newOrder SPC %r);
			%mhas[%r] = true;
		}

		%ct = getWordCount($MMGmafRoles);
		for(%i = 0; %i < %ct; %i++)
		{
			%r = getWord($MMGmafRoles, %i);

			if(!%mhas[%r])
			{
				%newOrder = trim(%newOrder SPC %r);
				%mhas[%r] = true;
			}
		}

		$MMGmafRoles = $MM::GMForceMafOrder;
	}

	if($MM::GMForceInnoOrder !$= "")
	{
		%newOrder = "";

		%ct = getWordCount($MM::GMForceInnoOrder);
		for(%i = 0; %i < %ct; %i++)
		{
			%r = getWord($MM::GMForceInnoOrder, %i);

			%newOrder = trim(%newOrder SPC %r);
			%ihas[%r] = true;
		}

		%ct = getWordCount($MMGinnoRoles);
		for(%i = 0; %i < %ct; %i++)
		{
			%r = getWord($MMGinnoRoles, %i);

			if(!%ihas[%r])
			{
				%newOrder = trim(%newOrder SPC %r);
				%ihas[%r] = true;
			}
		}

		$MMGinnoRoles = $MM::GMForceInnoOrder;
	}

	%ct = getWordCount($MMGmafRoles);
	for(%i = 0; %i < %ct; %i++)
		%mafCts = %mafCts SPC ($MMGct[getWord($MMGmafRoles, %i)] | 0);
	%mafCts = trim(%mafCts);

	MMDebug("   +Maf Roles:" SPC $MMGmafRoles);
	MMDebug("   +Maf Count:" SPC %mafCts);

	%ct = getWordCount($MMGinnoRoles);
	for(%i = 0; %i < %ct; %i++)
		%innoCts = %innoCts SPC ($MMGct[getWord($MMGinnoRoles, %i)] | 0);
	%innoCts = trim(%innoCts);

	MMDebug("   +Inno Roles:" SPC $MMGinnoRoles);
	MMDebug("   +Inno Count:" SPC %innoCts);

	%this.roles = MM_BuildRolesString($MMGmafs, $MMGmembers, $MMGmafRoles, $MMGinnoRoles, %mafCts, %innoCts, $MM::GMMafFill, $MM::GMInnoFill);

	MMDebug("   +Roles:" SPC %this.roles);

	%this.allAbduct = $MM::GMAllAbduct;
	%this.allComm = $MM::GMAllComm;
	%this.allImp = $MM::GMAllImp;
	%this.allInv = $MM::GMAllInv;
	%this.allBubble = $MM:GameModeAllBubble;
	%this.allFingerprint = $MM::GMAllFingerprint;
	%this.allRevive = $MM::GMAllRevive;

	$MM::NoExtraLives = $MM::GMNoExtraLives;

	deleteVariables("$MMG*");
}