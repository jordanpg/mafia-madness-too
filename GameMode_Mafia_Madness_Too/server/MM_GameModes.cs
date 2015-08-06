//MM_GameModes.cs
//Gamemode system

if(!$MM::LoadedGameModes)
{
	$MM::GameModes = 0;
	$MM::CustomModes = 0;
	$MM::CurrentMode = -1;
}

$MM::LoadedGameModes = true;

$MM::GameMode[-1] = "Custom";

$MM::DefaultGameMode = "Mafia Madness (too)";

$MM::GMDir = $MM::Server @ "gamemodes/";
$MM::CustomFile = "mmtoo.mmgm";
$MM::GamePrefStore = "config/server/mmGPStore.cs";
$MM::ModeSearchPattern = "Add-Ons/*.mmgm";
$MM::AutoFindGameModes = true;
$MM::AdminOnlyGMList = false;

/////////////////////////////////////
///////////CONTROL COMMAND///////////
/////////////////////////////////////
function serverCmdMMGameModes(%this)
{
	if(!%this.isAdmin && $MM::AdminOnlyGMList)
	{
		messageClient(%this, '', "\c4Only admins may access gamemodes.");
		return;
	}

	%pattern = '\c3%1\c4. \c3%2';

	messageClient(%this, '', "\c4====\c3Mafia Madness Gamemodes\c4====");

	for(%i = 0; %i < $MM::GameModes; %i++)
		messageClient(%this, '', %pattern, %i, $MM::GameMode[%i]);

	messageClient(%this, '', "\c6Use \c3Page Up \c6and \c3Page Down \c6to scroll through the list. Use \c3/MMSetGameMode \c7[ID OR NAME] \c6to set the gamemode.");
}

function serverCmdGameModes(%this)
{
	serverCmdMMGameModes(%this);
}

function serverCmdMMSetGameMode(%this, %a0, %a1, %a2, %a3, %a4, %a5)
{
	if(!%this.isSuperAdmin)
		return;

	if(!isObject(%mini = getMinigameFromObject(%this)) || !%mini.isMM)
		return;

	%i = %a0;
	if((%name = $MM::GameMode[%i]) $= "" && %i != -1)
	{
		%name = trim(%a0 SPC %a1 SPC %a2 SPC %a3 SPC %a4 SPC %a5);

		if((%i = MM_FindGameModeByName(%name, true)) != -1)
			%name = $MM::GameMode[%i];
		else
		{
			messageClient(%this, '', "\c4Could not find gamemode '\c3" @ %name @ "\c4,' use \c3/MMGameModes \c4to get a list of gamemodes available.");
			return;
		}
	}

	echo(%this.getSimpleName() SPC "set the gamemode to" SPC %name);

	%mini.MM_SetGameMode(%i);
}

function serverCmdSetGameMode(%this, %a0, %a1, %a2, %a3, %a4, %a5)
{
	serverCmdMMSetGameMode(%this, %a0, %a1, %a2, %a3, %a4, %a5);
}

function serverCmdDescribeGameMode(%this, %a0, %a1, %a2, %a3, %a4, %a5)
{
	if(!%this.isAdmin && $MM::AdminOnlyGMList)
	{
		messageClient(%this, '', "\c4Only admins may access gamemodes.");
		return;
	}

	%name = trim(%a0 SPC %a1 SPC %a2 SPC %a3 SPC %a4 SPC %a5);

	if(%name $= "")
	{
		if(!isObject(%mini = getMinigameFromObject(%this)) || !%mini.isMM)
			return;

		%a0 = %mini.gameMode;

		if($MM::GameMode[%a0] $= "")
			%a0 = MM_FindGameModeByName(%a0);

		if(%a0 == -1 || $MM::GameMode[%a0] $= "")
		{
			messageClient(%this, '', "\c4Input a gamemode ID or name to get its description! (Some gamemodes may not provide a description)");
			return;
		}
	}

	%i = %a0;
	if((%n = $MM::GameMode[%i]) $= "")
	{
		if((%i = MM_FindGameModeByName(%name, true)) != -1)
			%n = $MM::GameMode[%i];
		else
		{
			messageClient(%this, '', "\c4Could not find gamemode '\c3" @ %name @ "\c4,' use \c3/MMGameModes \c4to get a list of gamemodes available.");
			return;
		}
	}

	%this.messageLines($MM::GameModeDesc[%i]);
}

function serverCmdDescribeGM(%this, %a0, %a1, %a2, %a3, %a4, %a5)
{
	serverCmdDescribeGameMode(%this, %a0, %a1, %a2, %a3, %a4, %a5);
}

/////////////////////////////////////
///////////GENERAL SUPPORT///////////
/////////////////////////////////////
function MM_isValidGameMode(%modeName)
{
	if(!isFunction("MM_InitMode" @ %modeName))
		return false;

	return true;
}

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

function MM_RegisterGameMode(%name, %desc, %customID)
{
	if(!MM_isValidGameMode(%name) && %customID $= "")
		return false;

	if($MM::GameModeExists[%name])
	{
		warn("Gamemode by name of '" @ %name @ "' has already been registered!");
		return false;
	}

	$MM::GameMode[$MM::GameModes] = %name;
	$MM::GameModeDesc[$MM::GameModes] = %desc;
	if(%customID !$= "")
	{
		$MM::GameModeIsCustom[$MM::GameModes] = true;
		$MM::GameModeCustomID[$MM::GameModes] = %customID;
	}
	else
		$MM::GameModeIsCustom[$MM::GameModes] = false;

	$MM::GameModeExists[%name] = true;

	$MM::GameModes++;

	return true;
}

function MM_FindGameModeByName(%name, %stripos)
{
	for(%i = 0; %i < $MM::GameModes; %i++)
	{
		if(!%stripos)
		{
			if($MM::GameMode[%i] $= %name)
				return %i;
		}
		else if(striPos($MM::GameMode[%i], %name) != -1)
			return %i;
	}

	return -1;
}

function MM_ClearGameModes()
{
	deleteVariables("$MM::GameMode*");

	$MM::GameMode[-1] = "Custom";
	$MM::GameModes = 0;

	MM_ClearCustomGameModes();
}

function MinigameSO::MM_SetGameMode(%this, %modeID)
{
	if(!%this.isMM)
		return false;

	if((%name = $MM::GameMode[%modeID]) $= "")
		return false;

	if($MM::GameModeIsCustom[%modeID])
	{
		%this.gameMode = %modeID;

		$MM::CurrentMode = $MM::GameModeCustomID[%modeID];
	}
	else
	{
		%this.gameMode = %modeID;

		$MM::CurrentMode = -1;
	}

	messageAll('', "\c4The gamemode has been set to\c3" SPC %name);
}

function MinigameSO::MM_GetGameMode(%this)
{
	if((%this.gameMode | 0) $= %this.gameMode)
	{
		%i = %this.gameMode;

		if($MM::GameModeIsCustom[%i])
			%mode = $MM::GameMode[-1];
		else
			%mode = $MM::GameMode[%i];
	}
	else
	{
		%i = MM_FindGameModeByName(%this.gameMode);

		if(%i != -1)
		{
			if($MM::GameModeIsCustom[%i])
				%mode = $MM::GameMode[-1];
			else
				%mode = $MM::GameMode[%i];
		}
		else
			%mode = %this.gameMode; //set up to fail but w/e
	}

	echo(%mode);

	if(%mode $= "" || !MM_isValidGameMode(%mode))
		return $MM::GameMode[$MM::DefaultGameMode];

	if(isFunction(%r = "MM_ModeReady" @ %mode) && !call(%r, %this))
	{
		warn("MinigameSO::MM_GetGameMode : Mode" SPC %mode SPC "reports that it is not ready to run, using default gamemode.");
		return $MM::GameMode[$MM::DefaultGameMode];
	}

	return %mode;
}

/////////////////////////////////////
///////CUSTOM GAMEMODE SUPPORT///////
/////////////////////////////////////
function MM_ExportGamePrefs(%out)
{
	export("$MM::GP*", %out);
}

function MM_RegisterCustomMode(%name, %file, %description)
{
	if(!isFile(%file))
		return -1;

	if(!MM_RegisterGameMode(%name, %description, $MM::CustomModes))
		return -1;

	$MM::CustomMode[%i = $MM::CustomModes] = %file;
	$MM::CustomModeName[%i] = %name;
	$MM::CustomModeDesc[%i] = %description;
	$MM::CustomModes++;

	warn("--+Registered MM custom mode" SPC %name SPC "from '" @ %file @ "' ...");

	return %i;
}

function MM_ClearCustomGameModes()
{
	deleteVariables("$MM::CustomMode*");

	$MM::CurrentMode = -1;
	$MM::CustomModes = 0;
}

function MM_FindCustomModeByName(%name, %stripos)
{
	for(%i = 0; %i < $MM::CustomModes; %i++)
	{
		if(!%stripos)
			if($MM::CustomModeName[%i] $= %name)
				return %i;
		else if(striPos($MM::CustomModeName[%i], %name) != -1)
			return %i;
	}

	return -1;
}

function MM_GetCustomModeName(%index)
{
	if(%index < 0 || %index >= $MM::CustomModes)
		return -1;

	return $MM::CustomModeName[%index];
}

function MM_GetCustomModeDescription(%index)
{
	if(%index < 0 || %index >= $MM::CustomModes)
		return -1;

	return $MM::CustomModeDesc[%index];
}

function MM_GetCustomModeFile(%index)
{
	if(%index < 0 || %index >= $MM::CustomModes)
		return -1;

	return $MM::CustomMode[%index];
}

function MM_RegisterModeFile(%filen)
{
	if(!isFile(%filen))
		return false;

	%file = new FileObject();
	%file.openForRead(%filen);

	%line = %file.readLine();

	if(firstWord(%line) !$= "MMGAMEMODE")
	{
		%file.close();
		%file.delete();

		return false;
	}

	%name = restWords(%line);

	while(!%file.isEOF())
	{
		%line = %file.readLine();

		if(getSubStr(%line, 0, 3) !$= "///")
			continue;

		%line = getSubStr(%line, 3, strLen(%line) - 3);

		%desc = trim(%desc NL %line);
	}
	%file.close();
	%file.delete();

	return MM_RegisterCustomMode(%name, %filen, collapseEscape(%desc));
}

function MM_RegisterAllModeFiles(%pattern)
{
	%start = $MM::CustomModes;

	warn("Finding Mafia Madness gamemodes...");
	MMDebug("--+Pattern:" SPC %pattern);
	for(%i = findFirstFile(%pattern); isFile(%i); %i = findNextFile(%pattern))
	{
		MMDebug("--+Found file " SPC %i);
		%r = MM_RegisterModeFile(%i);
		MMDebug("--+Returned:" SPC %r);
	}

	%amt = $MM::CustomModes - %start;

	warn("Added" SPC %amt SPC "custom modes.");

	return %amt;
}

function MM_LoadGameMode(%filen)
{
	if(!isFile(%filen))
		return false;

	deleteVariables("$MM::GM*");

	if(isFile($MM::GamePrefStore) && $MM::StoredGamePrefs) //restore default vars
		exec($MM::GamePrefStore);
	else if(!$MM::StoredGamePrefs)
	{
		MM_ExportGamePrefs($MM::GamePrefStore);
		$MM::StoredGamePrefs = true;
	}

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

		%substr = getSubStr(%line, 0, 1);
		if(%substr $= "[")
		{
			%preVar = getSubStr(%line, 1, strLen(%line) - 2);
			continue;
		}

		if(%substr $= "<")
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
	// echo(%c);

	for(%i = 0; %str !$= ""; %i++)
	{
		%str = nextToken(%str, "cond", ",");

		%cond = trim(%cond);

		// echo(%i SPC %str);
		// echo(%cond);

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

		// echo(%c1 SPC %op SPC %c2);

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

		// echo(%r SPC %status);
	}

	return %status;
}

/////////////////////////////////////
//////CUSTOM GAMEMODE FUNCTIONS//////
/////////////////////////////////////
function MM_ModeReadyCustom(%this)
{
	if(!$MM::StoredGamePrefs)
	{
		MM_ExportGamePrefs($MM::GamePrefStore);
		$MM::StoredGamePrefs = true;
	}

	if(!$MM::CustomManual)
	{
		if($MM::CurrentMode >= 0)
		{
			if(isFile(%f = MM_GetCustomModeFile($MM::CurrentMode)))
				$MM::CustomFile = %f;
			else if(isFile(%f = $MM::GMDir @ %f))
				$MM::CustomFile = %f;
		}

		if(!isFile($MM::CustomFile))
		{
			if(isFile(%f = $MM::GMDir @ $MM::CustomFile))
				$MM::CustomFile = %f;
			else if(isFile($Pref::Server::MMCustomFile))
				$MM::CustomFile = $Pref::Server::MMCustomFile;
			else if(isFile(%f = $MM::GMDir @ $Pref::Server::MMCustomFile))
				$MM::CustomFile = %f;
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
	
	deleteVariables("$MMG*");
}