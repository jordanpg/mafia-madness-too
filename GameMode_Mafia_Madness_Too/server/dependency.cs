//dependency.cs
//Random support functions used throughout the code.

function calculateDaycycleFraction() {
	%len = DayCycle.DayLength;
	return ($Sim::Time % %len) / %len;
}

function setDayCycleTime(%frac) {
	%time = calculateDayCycleFraction();
	%off = %frac - %time;
	if(%off < 0) {
		%off = 1 + %off;
	}
	$EnvGuiServer::DayOffset = %off;
	DayCycle.setDayOffset(%off);
}

function getDaySuffix(%day)
{
	if(%day > 3 && %day < 21)
		%suffix = "th";
	else
	{
		switch(%day % 10)
		{
			case 1: %suffix = "st";
			case 2: %suffix = "nd";
			case 3: %suffix = "rd";
			default: %suffix = "th";
		}
	}

	return %suffix;
}

function getRandomFloat( %min, %max )
{
	return %min + getRandom() * ( %max - %min );
}

function pointBetween(%point, %vectA, %vectB, %xy)
{
	%xA = getWord(%vectA, 0);
	%yA = getWord(%vectA, 1);
	%zA = getWord(%vectA, 2);
	%xB = getWord(%vectB, 0);
	%yB = getWord(%vectB, 1);
	%zB = getWord(%vectB, 2);

	if(%xA > %xB)
	{
		%t = %xA;
		%xA = %xB;
		%xB = %t;
	}
	if(%yA > %yB)
	{
		%t = %yA;
		%yA = %yB;
		%yB = %t;
	}
	if(%zA > %zB)
	{
		%t = %zA;
		%zA = %zB;
		%zB = %t;
	}

	%x = getWord(%point, 0);
	%y = getWord(%point, 1);
	%z = getWord(%point, 2);

	if(%x < %xA || %x > %xB)
		return false;
	if(%y < %yA || %y > %yB)
		return false;
	if((%z < %zA || %z > %zB) && !%xy)
		return false;
	return true;
}

function VectorRandom(%vectA, %vectB)
{
	%xA = getWord(%vectA, 0);
	%yA = getWord(%vectA, 1);
	%zA = getWord(%vectA, 2);
	%xB = getWord(%vectB, 0);
	%yB = getWord(%vectB, 1);
	%zB = getWord(%vectB, 2);

	if(%xA != %xB)
		%x = getRandomFloat(%xA, %xB);
	else
		%x = %xA;
		
	if(%yA != %yB)
		%y = getRandomFloat(%yA, %yB);
	else
		%y = %xA;
		
	if(%zA != %zB)
		%z = getRandomFloat(%zA, %zB);
	else
		%z = %zA;

	return %x SPC %y SPC %z;
}

function isInList(%list, %search)
{
	%ct = getWordCount(%list);
	for(%i = 0; %i < %ct; %i++)
		if(getWord(%list, %i) $= %search) return true;

	return false;
}

function bracketsHatesTGE(%data) //i super-hate that this is necessary but we need the jump and air control modifications :(
{
	if(!isObject(%data))
		return -1;
		
	if(isObject(%data.normalVersion))
		return %data;

	if(isObject(%data.slowVersion))
		return %data.slowVersion;
	else 
	{
		//so just declaring the datablock normally doesn't work... time to pley HARDBALL leoaleolaeole
		// datablock PlayerData((%data.getName() @ "SlowReload") : (%data.getName())) {
			// maxForwardSpeed = %data.maxForwardSpeed/4;
			// maxSideSpeed = %data.maxSideSpeed/4;
			// maxBackwardSpeed = %data.maxSideSpeed/4;
			// normalVersion = %data;
		// };
		//i've always wanted to do this
		eval("datablock PlayerData(" @ %data.getName() @ "SlowReload" @ ":" @ %data.getName() @ ") { maxForwardSpeed =" SPC %data.maxForwardSpeed/4 @ "; maxSideSpeed =" SPC %data.maxSideSpeed/4 @ "; maxBackwardSpeed =" SPC %data.maxBackwardSpeed/4 @ "; maxForwardCrouchSpeed =" SPC %data.maxForwardCrouchSpeed / 4 @ "; maxSideCrouchSpeed =" SPC %data.maxSideCrouchSpeed / 4 @ "; maxBackwardCrouchSpeed =" SPC %data.maxBackwardCrouchSpeed / 4 @ "; normalVersion =" SPC %data @ "; uiName = \"\";jumpForce = 0; airControl = 0;};");
		//take THAT, TGE!!!1
		%data.slowVersion = %data.getName() @ "SlowReload";
		updateClientDatablocks();

		echo("Made Slow Version for" SPC %data.getName());

		return %data.slowVersion;
	}
}

function stripToBasicString(%str)
{
	%table = "abcdefghijklmnopqrstuvwxyz0123456789_";

	%i = 0;
	while(%i < strLen(%str))
	{
		%c = getSubStr(%str, %i, 1);

		if(striPos(%table, %c) != -1)
		{
			%i++;
			continue;
		}

		%str = strReplace(%str, %c, "");
	}

	return %str;
}

function i_Lerp(%y1, %y2, %mu)
{
	return (%y1 * (1 - %mu) + %y2 * %mu);
}

function Player::setSpeedMod(%this, %mod)
{
	%db = %this.getDatablock();

	%this.setMaxForwardSpeed(%db.maxForwardSpeed * %mod);
	%this.setMaxBackwardSpeed(%db.maxBackwardSpeed * %mod);
	%this.setMaxSideSpeed(%db.maxSideSpeed * %mod);
	%this.setMaxCrouchForwardSpeed(%db.maxForwardCrouchSpeed * %mod);
	%this.setMaxCrouchBackwardSpeed(%db.maxBackwardCrouchSpeed * %mod);
	%this.setMaxCrouchSideSpeed(%db.maxSideCrouchSpeed * %mod);
}

function GameConnection::messageLines(%this, %str)
{
	while(%str !$= "")
	{
		%str = nextToken(%str, "line", "\n");
		messageClient(%this, '', %line);
	}
}

function GameConnection::clearInventory(%this)
{
	if(!isObject(%p = %this.player))
		return;

	%db = %p.getDatablock();
	for(%i = 0; %i < %db.maxTools; %i++)
	{
		%p.tool[%i] = 0;
		messageClient(%this, 'MsgItemPickup', '', %i, 0);
	}
}