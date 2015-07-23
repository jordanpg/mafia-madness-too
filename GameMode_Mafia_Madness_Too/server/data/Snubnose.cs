datablock ProjectileData(MMSnubnoseProjectile)
{
   projectileShapeName = "Add-Ons/Weapon_Gun/bullet.dts";
   directDamage        = 100;
   directDamageType    = $DamageType::Gun;
   radiusDamageType    = $DamageType::Gun;

   brickExplosionRadius = 0;
   brickExplosionImpact = true;          //destroy a brick if we hit it directly?
   brickExplosionForce  = 10;
   brickExplosionMaxVolume = 1;          //max volume of bricks that we can destroy
   brickExplosionMaxVolumeFloating = 2;  //max volume of bricks that we can destroy if they aren't connected to the ground

   impactImpulse	     = 400;
   verticalImpulse	  = 400;
   explosion           = gunExplosion;
   particleEmitter     = ""; //bulletTrailEmitter;

   muzzleVelocity      = 200;
   velInheritFactor    = 1;

   armingDelay         = 00;
   lifetime            = 4000;
   fadeDelay           = 3500;
   bounceElasticity    = 0.5;
   bounceFriction      = 0.20;
   isBallistic         = false;
   gravityMod = 0.0;

   hasLight    = false;
   lightRadius = 3.0;
   lightColor  = "0 0 0.5";

   uiName = "";
};

datablock ItemData(MMSnubnoseItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	 // Basic Item Properties
	shapeFile = "./Snubnose.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Snubnose";
	iconName = "Add-Ons/Weapon_Gun/icon_gun";
	doColorShift = true;
	colorShiftColor = "0.350 0.350 0.350 1.000";

	 // Dynamic properties defined by the scripts
	image = MMSnubnoseImage;
	canDrop = true;
	
	//Ammo Guns Parameters
	MMmaxAmmo = 6;
	MMcanReload = 1;
};
datablock ShapeBaseImageData(MMSnubnoseImage)
{
   // Basic Item properties
   shapeFile = "./Snubnose.dts";
   emap = true;

   // Specify mount point & offset for 3rd person, and eye offset
   // for first person rendering.
   mountPoint = 0;
   offset = "0 0 0";
   eyeOffset = 0; //"0.7 1.2 -0.5";
   rotation = eulerToMatrix( "0 0 0" );

   // When firing from a point offset from the eye, muzzle correction
   // will adjust the muzzle vector to point to the eye LOS point.
   // Since this weapon doesn't actually fire from the muzzle point,
   // we need to turn this off.  
   correctMuzzleVector = true;

   // Add the WeaponImage namespace as a parent, WeaponImage namespace
   // provides some hooks into the inventory system.
   className = "WeaponImage";

   // Projectile && Ammo.
   item = MMGunItem;
   ammo = " ";
   projectile = gunProjectile;
   projectileType = Projectile;

	casing = gunShellDebris;
	shellExitDir        = "1.0 -1.3 1.0";
	shellExitOffset     = "0 0 0";
	shellExitVariance   = 15.0;	
	shellVelocity       = 7.0;

   //melee particles shoot from eye node for consistancy
   melee = false;
   //raise your arm up or not
   armReady = true;

   doColorShift = true;
   // colorShiftColor = gunItem.colorShiftColor;//"0.400 0.196 0 1.000";
   colorShiftColor = "0.350 0.350 0.350 1.000";

   //casing = " ";

   // Images have a state system which controls how the animations
   // are run, which sounds are played, script callbacks, etc. This
   // state system is downloaded to the client so that clients can
   // predict state changes and animate accordingly.  The following
   // system supports basic ready->fire->reload transitions as
   // well as a no-ammo->dryfire idle state.

   // Initial start up state
	stateName[0]                     = "Activate";
	stateTimeoutValue[0]             = 0.5;
	stateTransitionOnTimeout[0]       = "LoadCheckA";
	stateSound[0]					= weaponSwitchSound;

	stateName[1]                     = "Ready";
	stateTransitionOnTriggerDown[1]  = "Fire";
	stateAllowImageChange[1]         = true;
	stateScript[1] = "onReady";
	stateTransitionOnNoAmmo[1] = "ReloadWait";
	// stateSequence[1]	= "Ready";

	stateName[2]                    = "Fire";
	stateTransitionOnTimeout[2]     = "Smoke";
	stateTimeoutValue[2]            = 0.14;
	stateFire[2]                    = true;
	stateAllowImageChange[2]        = false;
	// stateSequence[2]                = "Fire";
	stateScript[2]                  = "onFire";
	stateWaitForTimeout[2]			= true;
	stateEmitter[2]					= gunFlashEmitter;
	stateEmitterTime[2]				= 0.05;
	stateEmitterNode[2]				= "muzzleNode";
	stateSound[2]					= gunShot1Sound;
	// stateEjectShell[2]       = true;

	stateName[3] = "Smoke";
	stateSequence[3] = "Reload";
	stateEmitter[3]					= gunSmokeEmitter;
	stateEmitterTime[3]				= 1;
	stateEmitterNode[3]				= "muzzleNode";
	stateTimeoutValue[3]            = 1;
	stateAllowImageChange[3]        = false;
	stateTransitionOnTimeout[3]     = "LoadCheckA";

	// stateName[4]			= "Reload";
	// stateSequence[4]                = "Reload";
	// stateScript[4] = "onReload";
	// stateTransitionOnTriggerUp[4]     = "Ready";
	// stateSequence[4]	= "Ready";
	
	//Torque switches states instantly if there is an ammo/noammo state, regardless of stateWaitForTimeout
	stateName[4]				= "LoadCheckA";
	stateScript[4]				= "onMMLoadCheck";
	stateTimeoutValue[4]			= 0.01;
	stateTransitionOnTimeout[4]		= "LoadCheckB";
	
	stateName[5]				= "LoadCheckB";
	stateTransitionOnAmmo[5]		= "Ready";
	stateTransitionOnNoAmmo[5]		= "ReadyNoAmmo";
	
	stateName[6]				= "ReloadWait";
	stateTimeoutValue[6]			= 0.3;
	stateScript[6]				= "onReloadStart";
	stateTransitionOnTimeout[6]		= "EjectShell1";
	stateWaitForTimeout[6]			= true;
	
	stateName[7]				= "ReloadMid";
	stateSequence[7] = "Reload";
	stateTimeoutValue[7]			= 2.0;
	stateScript[7]				= "onReloadMid";
	stateTransitionOnTimeout[7]		= "Reloaded";
	stateWaitForTimeout[7]			= true;
	
	stateName[8]				= "Reloaded";
	stateTimeoutValue[8]			= 0.3;
	stateScript[8]				= "onReloaded";
	stateTransitionOnTimeout[8]		= "LoadCheckA";
	stateSequence[8]		= "Fire";
	stateSound[8]			= Block_PlantBrick_Sound;
	
	stateName[9]                     = "ReadyNoAmmo";
	stateTransitionOnTriggerDown[9]  = "FireNoAmmo";
	stateAllowImageChange[9]         = true;
	stateScript[9] = "onReady";
	stateTransitionOnAmmo[9] = "ReloadWait";
	
	stateName[10]                    = "FireNoAmmo";
	stateTransitionOnTimeout[10]     = "FireNoAmmoRelease";
	stateTimeoutValue[10]            = 0.14;
	stateFire[10]                    = true;
	stateAllowImageChange[10]        = false;
	stateScript[10]                  = "onFire";
	stateWaitForTimeout[10]			= true;
	
	stateName[11]					= "FireNoAmmoRelease";
	stateTransitionOnTriggerUp[11]	= "ReadyNoAmmo";
	
	stateName[12] = "EjectShell1";
	stateEjectShell[12] = true;
	stateTransitionOnTimeout[12] = "EjectShell2";
	stateTimeoutValue[12] = 0.05;
	stateWaitForTimeout[12] = true;
	stateScript[12] = "onReloadMid";
	
	stateName[13] = "EjectShell2";
	stateEjectShell[13] = true;
	stateTransitionOnTimeout[13] = "EjectShell3";
	stateTimeoutValue[13] = 0.05;
	stateWaitForTimeout[13] = true;
	
	stateName[14] = "EjectShell3";
	stateEjectShell[14] = true;
	stateTransitionOnTimeout[14] = "EjectShell4";
	stateTimeoutValue[14] = 0.05;
	stateWaitForTimeout[14] = true;
	
	stateName[15] = "EjectShell4";
	stateEjectShell[15] = true;
	stateTransitionOnTimeout[15] = "EjectShell5";
	stateTimeoutValue[15] = 0.05;
	stateWaitForTimeout[15] = true;
	
	stateName[16] = "EjectShell5";
	stateEjectShell[16] = true;
	stateTransitionOnTimeout[16] = "EjectShell6";
	stateTimeoutValue[16] = 0.05;
	stateWaitForTimeout[16] = true;
	
	stateName[17] = "EjectShell6";
	stateEjectShell[17] = true;
	stateTransitionOnTimeout[17] = "ReloadMid";
	stateTimeoutValue[17] = 0.05;
	stateWaitForTimeout[17] = true;

};

function MMSnubnoseImage::onFire(%this,%obj,%slot)
{
	if(%obj.toolAmmo[%obj.currTool] > 0 || %this.item.MMmaxAmmo == 0) {
		if(%obj.getDamagePercent() < 1.0)
			%obj.playThread(2, shiftAway);
		%obj.noChangeWep = 1;
		if(%this.item.MMmaxAmmo > 0) {
			%obj.toolAmmo[%obj.currTool]--;
		}
		// talk(%obj.toolAmmo[%slot] SPC %slot);
		Parent::onFire(%this,%obj,%slot);
	}
	else {
		serverPlay3D(block_MoveBrick_Sound,%obj.getPosition());
		%obj.playThread(2,shiftRight);
	}
}

function MMSnubnoseImage::onReady(%this,%obj,%slot) {
	%obj.noChangeWep = 0;
}

function MMSnubnoseImage::onReloaded(%this,%obj,%slot) {
	if(isObject(%obj) && isObject(%data = %obj.getDatablock()) && isObject(%data.normalVersion)) {
		%obj.setDatablock(%data.normalVersion);
	}
	%obj.reloading = 0;
	%obj.toolAmmo[%obj.currTool] = 6;
	%obj.playthread(2,shiftDown);
}


function MMSnubnoseImage::onReloadStart(%this,%obj,%slot) {
	if(isObject(%obj) && isObject(%data = %obj.getDatablock())) {
		// if(isObject(%data.slowVersion)) {
		// 	%obj.setDatablock(%data.slowVersion);
		// }
		// else {
		// 	//so just declaring the datablock normally doesn't work... time to pley HARDBALL leoaleolaeole
		// 	// datablock PlayerData((%data.getName() @ "SlowReload") : (%data.getName())) {
		// 		// maxForwardSpeed = %data.maxForwardSpeed/4;
		// 		// maxSideSpeed = %data.maxSideSpeed/4;
		// 		// maxBackwardSpeed = %data.maxSideSpeed/4;
		// 		// normalVersion = %data;
		// 	// };
		// 	//i've always wanted to do this
		// 	eval("datablock PlayerData(" @ %data.getName() @ "SlowReload" @ ":" @ %data.getName() @ ") { maxForwardSpeed =" SPC %data.maxForwardSpeed/4 @ "; maxSideSpeed =" SPC %data.maxSideSpeed/4 @ "; maxBackwardSpeed =" SPC %data.maxBackwardSpeed/4 @ "; maxForwardCrouchSpeed =" SPC %data.maxForwardCrouchSpeed / 4 @ "; maxSideCrouchSpeed =" SPC %data.maxSideCrouchSpeed / 4 @ "; maxBackwardCrouchSpeed =" SPC %data.maxBackwardCrouchSpeed / 4 @ "; normalVersion =" SPC %data @ "; uiName = \"\";jumpForce = 0; airControl = 0;};");
		// 	//take THAT, TGE!!!1
		// 	%data.slowVersion = %data.getName() @ "SlowReload";
		// 	updateClientDatablocks();
		// 	%obj.setDatablock(%data.slowVersion);
		// }

		%obj.setDatablock(bracketsHatesTGE(%data));

		%obj.reloading = 1;
	}
}

function MMSnubnoseImage::onReloadMid(%this,%obj,%slot) {
	%obj.playthread(2,shiftLeft);
}