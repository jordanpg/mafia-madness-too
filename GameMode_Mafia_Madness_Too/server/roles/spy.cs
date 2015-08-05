//spy.cs
//Code for the Spy innocent role.

if(!$MM::LoadedRole_Godfather)
	exec("./godfather.cs");

$MM::LoadedRole_Spy = true;

$MM::GPSpyObfuscateMaxLenMod = 4;
$MM::GPSpyObfuscateTable = "??abcdefghijklmnopqrstuvwxyz??ABCDEFGHIJKLMNOPQRSTUVWXYZ??0123456789??!@#$%^&*-+_??";
$MM::GPSpyObfuscateMinLen = 3;

if(!isObject(MMRole_Spy))
{
	new ScriptObject(MMRole_Spy)
	{
		class = "MMRole";

		name = "Spy";
		corpseName = "impeccable sleuth";
		displayName = "Spy";

		letter = "S";

		colour = "<color:808080>";
		nameColour = "0.5 0.5 0.5";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;
		canSleuth = true;

		alignment = 0;

		helpText = 	"\c4You are also the <color:808080>Spy\c4! As the spy, you are able to listen in on the mafia." NL
					"\c4You will be able to monitor all activity in the Godfather chat, but will not know the names of those speaking through it." NL
					"\c4Use this ability to help stop the mafia, \c3but be careful! \c4As such a powerful innocent, you may be a high-priority target if you reveal yourself!";

		description = 	"\c4The <color:808080>Spy\c4 is able to listen in on the mafia." NL
						"\c4You will be able to monitor all activity in the Godfather chat, but will not know the names of those speaking through it." NL
						"\c4Use this ability to help stop the mafia, \c3but be careful! \c4As such a powerful innocent, you may be a high-priority target if you reveal yourself!";
	};
}
//SUPPORT
function GameConnection::MM_canSleuth(%this)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return false;

	if(%this.isGhost || %this.lives < 1)
		return false;

	if(!isObject(%this.role))
		return false;

	if(!%this.role.canSleuth)
		return false;

	return true;
}

function GameConnection::getObfuscatedName(%this, %table, %maxLenMod, %minLen)
{
	%mini = getMiniGameFromObject(%this);

	if(%this.obfuscatedName !$= "" && %this.lastObfuscation >= %mini.startTime) //only return the last obfuscation if it was after the last round start
		return %this.obfuscatedName;

	%name = %this.getSimpleName();
	%len = strLen(%name);

	%oldSeed = getRandomSeed();

	%a = 0;
	if(%mini.running)
		%a = mFloatLength(%mini.startTime, 0); //we want to keep the gibberish the same on a per-round basis, but different across rounds, so this is a nice solution!

	%newSeed = %this.getBLID() | %a | %len;

	setRandomSeed(%newSeed);

	%lenMod = getRandom(%maxLenMod) * (getRandom(1) ? 1 : -1);
	%len += %lenMod;
	if(%len < %minLen)
		%len = %minLen;

	if(%len <= 0)
		%len = (%minLen > 0 ? %minLen : 1); 

	%tableLen = strLen(%table) - 1;

	%str = "";
	for(%i = 0; %i < %len; %i++)
		%str = %str @ getSubStr(%table, getRandom(%tableLen), 1);

	setRandomSeed(%oldSeed); //don't wanna disrupt other RNG going on

	%this.obfuscatedName = %str; //cache this stuff so we don't have to go through this mess every time
	%this.lastObfuscation = $Sim::Time;

	return %str;
}

//HOOKS
package MM_Spy
{
	function GameConnection::MM_GodfatherChat(%this, %msg, %pre2)
	{
		%r = parent::MM_GodfatherChat(%this, %msg, %pre2);

		if(!(%c = %this.MM_canComm()))
			return %r;

		if(%c == 2)
			return %r;

		%pre = "\c7[\c6Godfather\c7]";

		%name = %this.getObfuscatedName($MM::GPSpyObfuscateTable, $MM:SpyObfuscateMaxLenMod, $MM::GPSpyObfuscateMinLen);

		%format = '%1\c5%2\c6: %3';

		%mini = getMiniGameFromObject(%this);
		for(%i = 0; %i < %mini.numMembers; %i++)
		{
			%mem = %mini.member[%i];
			if(%mem.MM_canSleuth())
				commandToClient(%mem, 'chatMessage', %this, '', '', %format, %pre, %name, %msg);
		}

		return %r;
	}
};
activatePackage(MM_Spy);