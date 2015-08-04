//trenchKnife.cs

$MM::LoadedTrenchKnife = true;

datablock ItemData(TrenchKnifeItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	 // Basic Item Properties
	shapeFile = "./trenchKnife.dts";
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Trench Knife";
	iconName = "Add-Ons/Weapon_Melee_Extended/combatknife";
	doColorShift = true;
	colorShiftColor = "0.75 0.75 0.75 1";

	 // Dynamic properties defined by the scripts
	l4ditemtype = "secondary";
	image = TrenchKnifeImage;
	canDrop = true;
};

datablock ShapeBaseImageData(TrenchKnifeImage)
{
	shapeFile = "./trenchKnife.dts";
   emap = true;

   // Specify mount point & offset for 3rd person, and eye offset
   // for first person rendering.
   mountPoint = 0;
   offset = "0.0 0 0.0";
   rotation = eulerToMatrix("0 0 0");
   eyeOffset = "0";

   // When firing from a point offset from the eye, muzzle correction
   // will adjust the muzzle vector to point to the eye LOS point.
   // Since this weapon doesn't actually fire from the muzzle point,
   // we need to turn this off.  
   correctMuzzleVector = false;

   className = "TF2MeleeWeaponImage";

   // Projectile && Ammo.
   item = TrenchKnifeItem;
   ammo = " ";
   projectile = hammerProjectile;
   projectileType = Projectile;

   //melee particles shoot from eye node for consistancy
   melee = true;
   doRetraction = false;
   //raise your arm up or not
   armReady = true;

   //casing = " ";
   doColorShift = true;
   colorShiftColor = TrenchKnifeItem.colorShiftColor;
   
   raycastWeaponRange = 3;
   raycastWeaponTargets = $TypeMasks::FxBrickObjectType |	//Targets the weapon can hit: Raycasting Bricks
   				$TypeMasks::PlayerObjectType |	//AI/Players
   				$TypeMasks::StaticObjectType |	//Static Shapes
   				$TypeMasks::TerrainObjectType |	//Terrain
   				$TypeMasks::VehicleObjectType;	//Vehicles
   raycastExplosionProjectile = hammerProjectile;
   raycastExplosionBrickSound = combatknifeHitSoundA;
   raycastExplosionPlayerSound = combatknifeHitSoundB;
   raycastDirectDamage = 105;
   raycastDirectDamageType = $DamageType::CombatKnife;
   
   // Images have a state system which controls how the animations
   // are run, which sounds are played, script callbacks, etc. This
   // state system is downloaded to the client so that clients can
   // predict state changes and animate accordingly.  The following
   // system supports basic ready->fire->reload transitions as
   // well as a no-ammo->dryfire idle state.

   // Initial start up state
	stateName[0]			= "Activate";
	stateTimeoutValue[0]		= 0.5;
	stateTransitionOnTimeout[0]	= "StabCooldown";
	stateSequence[0]		= "Activate";
	stateScript[0]                  = "onActivate";
	//stateSound[0]					= weaponSwitchSound;

	stateName[1]			= "Ready";
	stateTransitionOnTriggerDown[1]	= "Charge";
	stateScript[1]                  = "onReady";
	stateAllowImageChange[1]	= true;
	
	stateName[2]                    = "Charge";
	stateTransitionOnTimeout[2]	= "Armed";
	stateTimeoutValue[2]            = 0.4;
	stateWaitForTimeout[2]		= false;
	stateTransitionOnTriggerUp[2]	= "AbortCharge";
	stateSequence[2]		= "Prime";
	stateScript[2]                  = "onCharge";
	stateAllowImageChange[2]        = false;
	
	stateName[3]			= "AbortCharge";
	stateTransitionOnTimeout[3]	= "StabCooldown";
	stateTimeoutValue[3]		= 0.1;
	stateWaitForTimeout[3]		= true;
	stateEmitterNode[3]			= "tipNode";
	stateSequence[3]		= "Swing";
	stateEmitter[3]			= combatKnifeEmitter;
	stateEmitterTime[3]			= 0.17;
	stateScript[3]			= "onStabfire";
	stateSound[3]				= knifeFireSound;
	stateAllowImageChange[3]	= false;

	stateName[4]			= "Armed";
	stateTransitionOnTriggerUp[4]	= "Fire";
	stateAllowImageChange[4]	= false;
	stateScript[4]                  = "onCharge";

	stateName[5]			= "Fire";
	stateTransitionOnTimeout[5]	= "Stabcooldown";
	stateTimeoutValue[5]		= 0.4;
	stateEmitter[5]			= combatKnifeEmitter;
	stateEmitterTime[5]			= 0.289;
	stateEmitterNode[5]			= "tipNode";
	stateFire[5]			= true;
	stateSequence[5]		= "swipe";
	stateScript[5]			= "onFire";
	stateWaitForTimeout[5]		= true;
	stateAllowImageChange[5]	= false;
	//stateSound[5]				= knifeFireSound;

	stateName[6]			= "StabCooldown";
	stateTransitionOnTimeout[6]	= "Ready";
	stateTimeoutValue[6]		= 0.1;
	stateWaitForTimeout[6]		= true;
	stateSequence[6]		= "Prime";
	stateAllowImageChange[6]	= false;
};

function TrenchKnifeImage::onActivate(%this, %obj, %slot)
{
	%obj.playThread(2,plant);
}

function TrenchKnifeImage::onFire(%this, %obj, %slot)
{
	%obj.playThread(2,shiftto);
	%obj.playThread(3,spearthrow);
	%obj.playThread(4,shiftdown);
	%obj.playThread(5,shiftdown);
	%obj.playThread(6,shiftdown);
	%this.raycastExplosionBrickSound = 0;
	%this.raycastExplosionPlayerSound = 0;
	%this.raycastDirectDamage = 105;


	// if(getRandom(0,1))
	// {
		// //%this.raycastExplosionBrickSound = CombatKnifeHitSoundA;
		// //%this.raycastExplosionPlayerSound = CombatKnifeHitSoundB;
		// %this.raycastDirectDamage = 105;
	// }
	// else
	// {
		// //%this.raycastExplosionBrickSound = CombatKnifeHitSoundB;
		// //%this.raycastExplosionPlayerSound = CombatKnifeHitSoundB;
		// %this.raycastDirectDamage = 105;
	// }

	WeaponImage::onFire(%this, %obj, %slot);
}

function TrenchKnifeImage::onStabFire(%this, %obj, %slot)
{
	%obj.playThread(2,shiftto);
	%obj.playThread(3,shiftdown);

	if(getRandom(0,1))
	{
		%this.raycastExplosionBrickSound = CombatKnifeHitSoundA;
		%this.raycastExplosionPlayerSound = meleeSlashSound;
		//serverPlay3D(meleeSlashSound,%obj.getPosition());
		%this.raycastDirectDamage = 105;
	}
	else
	{
		%this.raycastExplosionBrickSound = CombatKnifeHitSoundB;
		%this.raycastExplosionPlayerSound = meleeSlashSound;
        // serverPlay3D(meleeSlashSound,%obj.getPosition());
		%this.raycastDirectDamage = 105;
	}

	WeaponImage::onFire(%this, %obj, %slot);
}