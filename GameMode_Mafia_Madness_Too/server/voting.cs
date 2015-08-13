//voting.cs
//Code for the gamemode voting system

$MM::LoadedVoting = true;

$MM::VoteAllowed = true;
$MM::VoteStartRequirement = 0.5;
$MM::VoteNotifyForStart = true;
$MM::VoteNotifyForUnvote = true;
$MM::VoteNotifyForVote = true;
$MM::VotePeriod = 30000;
$MM::VoteAutoStartAfter = 15 * 60000;
$MM::VoteOnePlayerSets = true;
$MM::VoteDediPeriod = true;

function serverCmdVoteGameMode(%this, %a0, %a1, %a2, %a3, %a4, %a5)
{
	if(!isObject(%mini = getMinigameFromObject(%this)) || !%mini.isMM || !$MM::VoteAllowed)
		return;

	%pl = %mini.MM_GetNumPlayers();

	if(!%mini.voteActive && (%pl > 1 && $MM::VoteOnePlayerSets))
	{
		if(%mini.runVote)
		{
			messageClient(%this, '', "\c4A vote will occur in the next DM period. Wait until then!");
			return;
		}

		%needed = mFloor(%pl * $MM::VoteStartRequirement);

		if(!$MMvoteStart[%this])
		{
			%mini.voteCount++;
			$MMvoteStart[%this] = true;

			if($MM::VoteNotifyForStart)
				messageAll('', "\c3" @ %this.getSimpleName() SPC "\c4voted to change the gamemode (\c3" @ %mini.voteCount SPC "\c4/" SPC %needed @ ")");

			echo(%this.getSimpleName() SPC "voted to change the gamemode (" @ %mini.voteCount SPC "/" SPC %needed @ ")");
		}
		else
		{
			%mini.voteCount--;
			$MMvoteStart[%this] = false;

			if($MM::VoteNotifyForUnvote)
				messageAll('', "\c3" @ %this.getSimpleName() SPC "\c4un-voted to change the gamemode (\c3" @ %mini.voteCount SPC "\c4/" SPC %needed @ ")");

			echo(%this.getSimpleName() SPC "un-voted to change the gamemode (" @ %mini.voteCount SPC "/" SPC %needed @ ")");
		}

		if(%mini.voteCount >= %needed)
			%mini.MM_BeginGameModeVote();

		return;
	}

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

	if(%pl == 1 && $MM::VoteOnePlayerSets)
	{
		echo(%this.getSimpleName() SPC "set the gamemode to" SPC %name SPC "(only player)");
		%mini.MM_SetGameMode(%i, $MM::SilentSetGamemode);

		return;
	}

	if($MMvoteChoice[%this] == %i)
	{
		messageClient(%this, '', "\c4You already voted for\c3" SPC %name @ "\c4!");
		return;
	}

	$MMvote[%i]++;

	if($MMvoteChoice[%this] !$= "")
	{
		$MMvote[$MMvoteChoice[%this]]--;
		%change = true;
	}

	if($MM::VoteNotifyForVote)
		messageAll('', "\c3" @ %this.getSimpleName() SPC (%change ? "\c4changed their vote from\c3" SPC $MM::GameMode[$MMvoteChoice[%this]] SPC "\c4(\c3" @ $MMvote[$MMvoteChoice[%this]] @ "\c4) to\c3" : "\c4voted to change the gamemode to\c3") SPC %name SPC "\c4(\c3" @ $MMvote[%i] @ "\c4)");
	echo(%this.getSimpleName() SPC "voted to change the gamemode to" SPC %name SPC "(" @ $MMvote[%i] @ ")");

	$MMvoteChoice[%this] = %i;
}

function serverCmdStartGameModeVote(%this)
{
	if(!%this.isAdmin)
		return;

	if(!isObject(%mini = getMinigameFromObject(%this)) || !%mini.isMM)
		return;

	%mini.MM_BeginGameModeVote();
}

function serverCmdStartGMVote(%this)
{
	serverCmdStartGameModeVote(%this);
}

function serverCmdCancelGameModeVote(%this)
{
	if(!%this.isAdmin)
		return;

	if(!isObject(%mini = getMinigameFromObject(%this)) || !%mini.isMM)
		return;

	%mini.MM_CancelGameModeVote();

	echo(%this.getPlayerName() SPC "cancelled the gamemode vote.");
}

function serverCmdCancelGMVote(%this)
{
	serverCmdCancelGameModeVote(%this);
}

function serverCmdFinishGameModeVote(%this)
{
	if(!%this.isAdmin)
		return;

	if(!isObject(%mini = getMinigameFromObject(%this)) || !%mini.isMM)
		return;

	%mini.MM_EndGameModeVote();

	echo(%this.getPlayerName() SPC "force-finished the gamemode vote.");
}

function serverCmdFinishGMVote(%this)
{
	serverCmdFinishGameModeVote(%this);
}

function serverCmdEndGMVote(%this)
{
	serverCmdFinishGameModeVote(%this);
}

function MinigameSO::MM_BeginGameModeVote(%this)
{
	if(%this.running)
	{
		%this.runVote = true;
		messageAll('', "\c2A gamemode vote will take place during the next DM period.");
	}
	else
		%this.MM_StartGameModeVote();
}

function MinigameSO::MM_CancelGameModeVote(%this)
{
	cancel(%this.voteEnd);

	%this.runVote = false;
	%this.voteCount = 0;
	%this.voteActive = false;
	deleteVariables("$MMvote*");

	messageAll('', "\c2The gamemode vote has been cancelled.");
}

function MinigameSO::MM_StartGameModeVote(%this)
{
	cancel(%this.nextVote);

	%this.runVote = false;
	%this.voteCount = 0;
	%this.voteActive = true;
	deleteVariables("$MMvote*");

	messageAll('', "<font:verdana:28>\c2A vote has begun to change the gamemode! Use \c3/voteGameMode \c7[ID OR NAME] \c2to place your votes!");
	messageAll('', "\c4You can access the list of gamemodes by doing \c3/MMGameModes\c4, and a description by doing \c3/describeGameMode \c7[ID OR NAME]\c4.");

	cancel(%this.MMNextGame);

	if($MM::VotePeriod > 0)
		%this.voteEnd = %this.schedule($MM::VotePeriod, MM_EndGameModeVote);
}

function MinigameSO::MM_EndGameModeVote(%this)
{
	cancel(%this.voteEnd);

	%highest = 0;
	%highestID = -1;
	for(%i = 0; %i < $MM::GameModes; %i++)
	{
		if($MMvote[%i] > %highest)
		{
			%highest = $MMvote[%i];
			%highestID = %i;
		}
		else if($MMvote[%i] == %highest)
			%highestID = %highestID SPC %i;
	}

	if(%highestID == -1)
	{
		messageAll('', "\c2No vote winner could be resolved!");

		return;
	}

	%ct = getWordCount(%highestID);
	if(%ct > 1)
	{
		%ct1 = %ct - 1;

		%str = "";
		for(%i = 0; %i < %ct; %i++)
		{
			%w = getWord(%highestID, %i);

			%n = $MM::GameMode[%w];

			%m = "";

			if(%i == %ct1 && %ct > 1)
			{
				if(%ct > 2)
					%m = "\c2, and";
				else
					%m = "\c2 and";
			}
			else if(%i > 0 && %ct > 2)
				%m = "\c2,";

			%str = %str @ %m SPC "\c3" @ %n;
		}

		messageAll('', "\c2The vote is tied between" SPC trim(%str) @ "\c2, and will be randomly resolved...");

		%highestID = getWord(%highestID, getRandom(%ct1));
	}

	messageAll('', "<font:verdana:28>\c2The vote to change the gamemode is now over! The winner is\c3" SPC $MM::GameMode[%highestID] @ "\c2!");
	echo("Gamemode vote resolved; winner is" SPC $MM::GameMode[%highestID]);

	%this.MM_SetGameMode(%highestID, 1);

	if($MM::VoteAutoStartAfter > 0)
		%this.nextVote = %this.schedule($MM::VoteAutoStartAfter, MM_BeginGameModeVote);

	if(%this.MMDedi)
	{
		if($MM::VoteDediPeriod)
		{
			messageAll('', "\c4The round will start in a few moments!");

			MMDebug("Scheduling next game (vote ended)", %this);
			%this.MMNextGame = %this.schedule($MM::DediRoundDelay, MM_InitRound);
		}
		else
			%this.MM_InitRound();
	}

	%this.runVote = false;
	%this.voteCount = 0;
	%this.voteActive = false;
	deleteVariables("$MMvote*");
}

package MM_Voting
{
	function MinigameSO::MM_Stop(%this)
	{
		parent::MM_Stop(%this);

		if(%this.runVote)
			%this.schedule(0, MM_StartGameModeVote);
	}

	function GameConnection::onClientLeaveGame(%this)
	{
		parent::onClientLeaveGame(%this);

		if($MMvoteChoice[%this] !$= "")
			$MMvote[$MMvoteChoice[%this]]--;

		if($MMvoteStart[%this] && isObject(%mini = getMinigameFromObject(%this)))
			%mini.voteCount--;
	}
};
activatePackage(MM_Voting);