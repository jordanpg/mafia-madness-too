//bubbleBuddy.cs
//Code for the experimental Bubble Buddy innocent role.

$MM::LoadedRole_BubbleBuddy = true;

$MM::BubbleColour = "1 0.5 0.867";
$MM::BubbleOpacity = 0.5;
$MM::BubbleSearchRad = 4;
$MM::BubbleBaseSize = 2.5;
$MM::BubbleFadeInTime = 50;
$MM::BubbleFadeOutTime = 500;
$MM::BubbleSteps = 100;
$MM::BubbleLifeTime = 3100;
$MM::BubbleShooterRad = 2;

if(!isObject(MMRole_BubbleBuddy))
{
	new ScriptObject(MMRole_BubbleBuddy)
	{
		class = "MMRole";

		name = "Bubble Buddy";
		corpseName = "friend steve";
		displayName = "Bubble Buddy";

		letter = "BB";

		colour = "<color:FF80DD>";
		nameColour = "1 0.5 0.867";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		canBubble = true;

		alignment = 0;

		helpText = 	"\c4You are also the <color:FF80DD>Bubble Buddy\c4! When you or someone else near to you is shot, you will create a bubble for protection!" NL
					"\c4Your bubble is best used to protect important people like the cop or fingerprint expert." NL
					"\c4This ability can only activate once per game, however, so make sure you don't squander it!" NL
					"\c3Note that abductions and knives can get around this ability, and it can activate for mafia as well!" NL
					"<font:impact:32pt>\c2This is an experimental role! Report any problems, and be aware that things are subject to change.";

		description = 	"\c4When the <color:FF80DD>Bubble Buddy\c4 or someone else nearby is shot, they will create a bubble for protection!" NL
						"\c4The bubble is best used to protect important people like the cop or fingerprint expert." NL
						"\c4This ability can only activate once per game, however, so make sure you don't squander it!" NL
						"\c3Note that abductions and knives can get around this ability, and it can activate for mafia as well!" NL
						"<font:impact:32pt>\c2This is an experimental role! Report any problems, and be aware that things are subject to change.";
	};
}

//SUPPORT
function GameConnection::MM_canBubble(%this)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return false;

	if(!isObject(%this.player))
		return false;

	if(%this.isGhost || %this.lives < 1)
		return false;

	if(!isObject(%this.role))
		return false;

	if(%this.MM_isMaf())
		return false;

	if(!%this.role.canBubble && !%mini.allBubble)
		return false;

	if(%this.usedBubble)
		return false;

	return true;
}

function StaticShape::bubbleIn(%this, %col, %size, %opacity, %time, %steps, %prog)
{
	cancel(%this.bubbleSched);

	if(%prog < 0)
		%prog = 0;
	else if(%prog > 1)
		%prog = 1;

	%scale = i_Lerp(0, %size, %prog);

	%this.setScale(%scale SPC %scale SPC %scale);
	%this.setNodeColor("ALL", %col SPC i_Lerp(0, %opacity, %prog));

	if(isObject(%this.player))
		%this.setTransform(%this.player.getTransform());

	if(%prog >= 1)
	{
		%this.bubbleSched = "";
		return;
	}

	%s = mFloor(%time / %steps);
	if(%s < 1)
		%s = 1;

	%this.bubbleSched = %this.schedule(%s, bubbleIn, %col, %size, %opacity, %time, %steps, %prog + (%s / %time));
}

function StaticShape::fadeOut(%this, %col, %start, %time, %steps, %del, %prog)
{
	cancel(%this.bubbleSched);

	if(%prog < 0)
		%prog = 0;
	else if(%prog > 1)
		%prog = 1;

	%this.setNodeColor("ALL", %col SPC i_Lerp(%start, 0, %prog));

	if(%prog >= 1)
	{
		%this.bubbleSched = "";

		if(%del)
			%this.delete();

		return;
	}

	%s = mFloor(%time / %steps);
	if(%s < 1)
		%s = 1;

	%this.bubbleSched = %this.schedule(%s, fadeOut, %col, %start, %time, %steps, %del, %prog + (%s / %time));
}

function Player::MM_ActivateBubble(%this)
{
	// talk(bub);

	if(!isObject(%cl = %this.getControllingClient()))
		return false;

	if(!isObject(%mini = getMiniGameFromObject(%cl)) || !%mini.running || !%mini.isMM)
		return false;

	if(!%cl.MM_canBubble())
		return false;

	if(isObject(%this.bubbleShape))
		return false;

	%biggestDist = 0;

	%this.bubbleShape = new StaticShape()
						{
							datablock = SmoothSphereShapeData;
							position = %pos = %this.getHackPosition();

							player = %this;
						};

	initContainerRadiusSearch(%pos, $MM::BubbleSearchRad, $TypeMasks::PlayerObjectType);
	while(isObject(%obj = containerSearchNext()))
	{
		if(%obj == %this)
			continue;

		%pos2 = %obj.getHackPosition();


		//detect obstacles; we don't want bubble buddy to trigger from shots with a floor or wall between the two players
		%ray = containerRayCast(%pos, %pos2, $Typemasks::PlayerObjectType | $Typemasks::FXbrickObjectType | $Typemasks::TerrainObjectType | $Typemasks::InteriorObjectType | $TypeMasks::VehicleObjectType, %obj);
		%o = firstWord(%ray);

		if(isObject(%o) && !(%o.getType() & $TypeMasks::PlayerObjectType))
			continue;

		%dist = VectorDist(%pos2, %pos);
		if(%dist > %biggestDist)
			%biggestDist = %dist;

		%obj.bubbleShape = %this.bubbleShape;

		%players = %players SPC %obj;
	}

	%this.bubbleShape.endSize = %size = $MM::BubbleBaseSize + %biggestDist;

	%this.bubbleShape.bubbleIn($MM::BubbleColour, %size, $MM::BubbleOpacity, $MM::BubbleFadeInTime, $MM::BubbleSteps);
	%this.bubbleShape.kill = %this.bubbleShape.schedule($MM::BubbleLifeTime, fadeOut, $MM::BubbleColour, $MM::BubbleOpacity, $MM::BubbleFadeOutTime, $MM::BubbleSteps, true);

	if(%players $= "")
		%mini.MM_LogEvent(%cl.MM_GetName(1) @ "\c6's bubble activated.");
	else
	{
		%ct = getWordCount(%players);
		%ct1 = %ct - 1;
		for(%i = 0; %i < %ct; %i++)
		{
			%p = getWord(%players, %i);

			if(!isObject(%c = %p.getControllingClient()))
				continue;

			if(%i == %ct1 && %ct > 1)
			{
				if(%ct > 2)
					%str = %str @ "\c6, and";
				else
					%str = %str @ " \c6and";
			}
			else if(%i > 0)
				%str = %str @ "\c6,";

			%str = %str SPC %c.MM_GetName(1);
		}

		%mini.MM_LogEvent(%cl.MM_GetName(1) @ "\c6's bubble activated for" @ %str);
	}

	%cl.usedBubble = true;
}

//HOOKS
package MM_BubbleBuddy
{
	function MMRole::onAssign(%this, %mini, %client)
	{
		parent::onAssign(%this, %mini, %client);

		if(%this.canBubble || %mini.allBubble)
			%mini.bubbleBuddy = true;
	}

	function MMRole::onCleanup(%this, %mini, %client)
	{
		parent::onCleanup(%this, %mini, %client);

		%client.usedBubble = false;
		%mini.bubbleBuddy = false;
	}

	function Player::damage(%this, %obj, %pos, %amt, %type)
	{
		%pos = %this.getHackPosition();

		%db = %this.getDatablock();

		// talk(%this.startedDying);

		if(!isObject(%cl = %this.getControllingClient()))
			return parent::damage(%this, %obj, %pos, %amt, %type);

		if(!$DefaultMinigame.running || !(%mini = getMiniGameFromObject(%cl)).isMM)
			return parent::damage(%this, %obj, %pos, %amt, %type);

		if(!%mini.bubbleBuddy)
			return parent::damage(%this, %obj, %pos, %amt, %type);

		if((%this.dying && %this.startedDying != $Sim::Time) || %this.isGhost || %this.client.lives < 1 || (%this.isCorpse && !isObject(%this.getControllingClient())))
			return parent::damage(%this, %obj, %pos, %amt, %type);

		if(%type $= $DamageType::Impact || %type $= $DamageType::Fall || %type $= $DamageType::Direct || %type $= $DamageType::Suicide || %type $= $DamageType::CombatKnife)
			return parent::damage(%this, %obj, %pos, %amt, %type);

		if(isObject(%obj.sourceClient))
			%scl = %obj.sourceClient;
		else if(isObject(%obj.client))
			%scl = %obj.client;

		if(isObject(%scl.player))
			%spl = %scl.player;

		if(isObject(%spl))
			%spos = %spl.getHackPosition();

		MMDebug("Attempting to activate bubble for" SPC %cl.getSimpleName());
		if(isObject(%scl))
			MMDebug("Source Client:" SPC %scl SPC %scl.getSimpleName());
		if(isObject(%spl))
			MMDebug("Pos:" SPC %spos);

		if(%cl.MM_canBubble())
		{
			if(isObject(%spl))
			{
				%dist = VectorDist(%spos, %pos);

				MMDebug("Dist:" SPC %dist);

				// talk(%dist);
				if(%dist < $MM::BubbleShooterRad)
					return parent::damage(%this, %obj, %pos, %amt, %type);
			}

			if(%this.startedDying == $Sim::Time)
			{
				cancel(%this.deathSched);
				%this.setDamageFlash(0);

				if(isObject(%db.normalVersion))
					%this.setDatablock(%db.normalVersion);
			}

			if(%this.MM_ActivateBubble())
				return;
		}

		for(%i = 0; %i < %mini.numMembers; %i++)
		{
			%mem = %mini.member[%i];

			if(!%mem.MM_canBubble())
				continue;

			%pl = %mem.player;

			%pos2 = %pl.getHackPosition();

			if(isObject(%spl))
			{
				%dist = VectorDist(%spos, %pos2);

				if(%dist < $MM::BubbleShooterRad)
					continue;
			}

			%dist = VectorDist(%pos, %pos2);

			if(%dist < $MM::BubbleSearchRad && %pl.MM_ActivateBubble())
			{
				%found = true;
				break;
			}
		}

		if(!%found)
			return parent::damage(%this, %obj, %pos, %amt, %type);

		if(%this.startedDying == $Sim::Time)
		{
			cancel(%this.deathSched);
			%this.setDamageFlash(0);

			if(isObject(%db.normalVersion))
				%this.setDatablock(%db.normalVersion);
		}
	}

	function MMRole::onDeath(%this, %mini, %client, %srcObj, %srcClient, %damageType, %loc)
	{
		parent::onDeath(%this, %mini, %client, %srcObj, %srcClient, %damageType, %loc);

		if(!isObject(%client.player))
			return;

		if(!isObject(%bub = %client.player.bubbleShape))
			return;

		if(%bub.player != %client.player)
			return;

		if(isEventPending(%bub.kill))
		{
			cancel(%bub.kill);

			%bub.fadeOut($MM::BubbleColour, $MM::BubbleOpacity, $MM::BubbleFadeOutTime, $MM::BubbleSteps, true);
		}
	}
};
activatePackage(MM_BubbleBuddy);