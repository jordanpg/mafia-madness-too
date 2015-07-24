//tommyGun.cs

$MM::LoadedTommyGun = true;

datablock ItemData(TommyGunItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	 // Basic Item Properties
	shapeFile = "./tommyGun.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Tommy Gun";
	iconName = "Add-Ons/Weapon_Package_Tier2/Assaultrifle";
	doColorShift = true;
	colorShiftColor = "0.75 0.75 0.75 1";

	 // Dynamic properties defined by the scripts
	image = TommyGunImage;
	canDrop = true;
	
	maxAmmo = 18;
	canReload = 1;
};

////////////////
//weapon image//
////////////////
datablock ShapeBaseImageData(TommyGunImage)
{
   // Basic Item properties
   shapeFile = "./tommyGun.dts";
   emap = true;

   // Specify mount point & offset for 3rd person, and eye offset
   // for first person rendering.
   mountPoint = 0;
   offset = "0 0 0.03";
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
   item = TommyGunItem;
   ammo = " ";
   projectile = TAssaultRifleProjectile1;
   projectileType = Projectile;

   casing = GunShellDebris;
   shellExitDir        = "1.0 0.1 1.0";
   shellExitOffset     = "0 0 0";
   shellExitVariance   = 10.0;	
   shellVelocity       = 5.0;

   //melee particles shoot from eye node for consistancy
   melee = false;
   //raise your arm up or not
   armReady = true;

   doColorShift = true;
   colorShiftColor = TommyGunItem.colorShiftColor;

   // Images have a state system which controls how the animations
   // are run, which sounds are played, script callbacks, etc. This
   // state system is downloaded to the client so that clients can
   // predict state changes and animate accordingly.  The following
   // system supports basic ready->fire->reload transitions as
   // well as a no-ammo->dryfire idle state.

   // Initial start up state
	stateName[0]                     = "Activate";
	stateTimeoutValue[0]             = 0.05;
	stateTransitionOnTimeout[0]       = "LoadCheckA";
	stateSound[0]					= weaponSwitchSound;

	stateName[1]                     = "Ready";
	stateTransitionOnNoAmmo[1]       = "Reload";
	stateTransitionOnTriggerDown[1]  = "Fire";
	stateAllowImageChange[1]         = true;

	stateName[2]                    = "Fire";
	stateTransitionOnTimeout[2]     = "Delay";
	stateTimeoutValue[2]            = 0.01;
	stateFire[2]                    = true;
	stateAllowImageChange[2]        = false;
	stateSequence[2]                = "Fire";
	stateScript[2]                  = "onFire";
	stateEjectShell[2]       	  = true;
	stateEmitter[2]					= gunFlashEmitter;
	stateEmitterTime[2]				= 0.05;
	stateEmitterNode[2]				= "muzzleNode";
	stateWaitForTimeout[2]			= true;

	stateName[3]			= "Delay";
	stateTransitionOnTimeout[3]     = "FireLoadCheckA";
	stateTimeoutValue[3]            = 0.05;
	stateEmitter[3]					= gunSmokeEmitter;
	stateEmitterTime[3]				= 0.03;
	stateEmitterNode[3]				= "muzzleNode";
	
	stateName[4]				= "LoadCheckA";
	stateScript[4]				= "onLoadCheck";
	stateTimeoutValue[4]			= 0.01;
	stateTransitionOnTimeout[4]		= "LoadCheckB";
	
	stateName[5]				= "LoadCheckB";
	stateTransitionOnAmmo[5]		= "Ready";
	stateTransitionOnNoAmmo[5]		= "Reload";

	stateName[6]				= "Reload";
	stateTimeoutValue[6]			= 1.6;
	stateScript[6]				= "onReloadStart";
	stateTransitionOnTimeout[6]		= "Wait";
	stateWaitForTimeout[6]			= true;
	
	stateName[7]				= "Wait";
	stateTimeoutValue[7]			= 0.8;
	stateScript[7]				= "onReloadWait";
	stateTransitionOnTimeout[7]		= "Reloaded";
	
	stateName[8]				= "FireLoadCheckA";
	stateScript[8]				= "onLoadCheck";
	stateTimeoutValue[8]			= 0.01;
	stateTransitionOnTimeout[8]		= "FireLoadCheckB";
	
	stateName[9]				= "FireLoadCheckB";
	stateTransitionOnAmmo[9]		= "Smoke";
	stateTransitionOnNoAmmo[9]		= "ReloadSmoke";
	
	stateName[10] 				= "Smoke";
	stateEmitter[10]			= gunSmokeEmitter;
	stateEmitterTime[10]			= 0.3;
	stateEmitterNode[10]			= "muzzleNode";
	stateTimeoutValue[10]			= 0.2;
	stateTransitionOnTimeout[10]		= "Ready";
	stateTransitionOnTriggerDown[10]	= "Fire";
	
	stateName[11] 				= "ReloadSmoke";
	stateEmitter[11]			= gunSmokeEmitter;
	stateEmitterTime[11]			= 0.3;
	stateEmitterNode[11]			= "muzzleNode";
	stateTimeoutValue[11]			= 0.2;
	stateTransitionOnTimeout[11]		= "Reload";
	
	stateName[12]				= "Reloaded";
	stateTimeoutValue[12]			= 0.01;
	stateScript[12]				= "onReloaded";
	stateTransitionOnTimeout[12]		= "Ready";
};

function TommyGunImage::onFire(%this, %obj, %slot)
{
	TAssaultRifleImage::onFire(%this, %obj, %slot);
}

function TommyGunImage::onReloadStart(%this, %obj, %slot)
{
	TAssaultRifleImage::onReloadStart(%this, %obj, %slot);
}

function TommyGunImage::onReloadWait(%this, %obj, %slot)
{
	TAssaultRifleImage::onReloadWait(%this, %obj, %slot);
}

function TommyGunImage::onReloaded(%this, %obj, %slot)
{
	TAssaultRifleImage::onReloaded(%this, %obj, %slot);
}

function TommyGunImage::onMount(%this, %obj, %slot)
{
	Parent::onMount(%this,%obj,%slot);
	
	TAssaultRifleImage::onMount(%this, %obj, %slot);
}

function TommyGunImage::onUnMount(%this, %obj, %slot)
{
	TAssaultRifleImage::onUnMount(%this, %obj, %slot);
}