//peaceMaker.cs

if(!$MM::LoadedGun)
	exec("./MMgun.cs");

$MM::LoadedGun_PeaceMaker = true;

$MM::GunItem[$MM::GunItems] = "PeaceMakerItem";
$MM::GunItems++;

datablock ItemData(PeaceMakerItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	 // Basic Item Properties
	shapeFile = "./PeaceMaker.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Peacemaker";
	iconName = "Add-Ons/Weapon_Gun/icon_gun";
	doColorShift = true;
	colorShiftColor = "0.75 0.75 0.75 1";

	 // Dynamic properties defined by the scripts
	image = PeaceMakerImage;
	canDrop = true;
	
	//Ammo Guns Parameters
	MMmaxAmmo = 6;
	MMcanReload = 1;
};

datablock ShapeBaseImageData(PeaceMakerImage)
{
   // Basic Item properties
   shapeFile = "./PeaceMaker.dts";
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
   item = PeaceMakerItem;
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
   colorShiftColor = "0.75 0.75 0.75 1";

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
	stateTimeoutValue[2]            = 0.5;
	stateFire[2]                    = true;
	stateAllowImageChange[2]        = false;
	stateSequence[2]                = "Fire";
	stateScript[2]                  = "onFire";
	stateWaitForTimeout[2]			= true;
	stateEmitter[2]					= gunFlashEmitter;
	stateEmitterTime[2]				= 0.05;
	stateEmitterNode[2]				= "muzzleNode";
	stateSound[2]					= gunShot2Sound;
	// stateEjectShell[2]       = true;

	stateName[3] = "Smoke";
	stateSequence[3] = "Reload";
	stateEmitter[3]					= gunSmokeEmitter;
	stateEmitterTime[3]				= 1;
	stateEmitterNode[3]				= "muzzleNode";
	stateTimeoutValue[3]            = 0.5;
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
	stateTimeoutValue[6]			= 0.5;
	stateScript[6]				= "onReloadStart";
	stateTransitionOnTimeout[6]		= "EjectShell1";
	stateWaitForTimeout[6]			= true;
	stateSound[6]			= Block_ChangeBrick_Sound;
	
	stateName[7]				= "ReloadMid";
	stateSequence[7] = "Reload";
	stateTimeoutValue[7]			= 1.6;
	stateScript[7]				= "onReloadMid";
	stateTransitionOnTimeout[7]		= "Reloaded";
	stateWaitForTimeout[7]			= true;
	
	stateName[8]				= "Reloaded";
	stateTimeoutValue[8]			= 0.5;
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

function PeaceMakerImage::onFire(%this, %obj, %slot)
{
	MMGunImage::onFire(%this, %obj, %slot);
}

function PeaceMakerImage::onReady(%this,%obj,%slot)
{
	MMGunImage::onReady(%this, %obj, %slot);
}

function PeaceMakerImage::onReloaded(%this, %obj, %slot)
{
	MMGunImage::onReloaded(%this, %obj, %slot);
}

function PeaceMakerImage::onReloadStart(%this, %obj, %slot)
{
	MMGunImage::onReloadStart(%this, %obj, %slot);
}

function PeaceMakerImage::onReloadMid(%this, %obj, %slot)
{
	MMGunImage::onReloadMid(%this, %obj, %slot);
}