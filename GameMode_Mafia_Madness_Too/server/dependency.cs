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