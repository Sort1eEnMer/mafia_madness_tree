if(!$MM::LoadedGun)
	exec("./MMgun.cs");

$MM::LoadedGun_DesertEagle = true;

$MM::GunItem[$MM::GunItems] = "DesertEagleItem";
$MM::GunItems++;

datablock DebrisData(DesertEagleMagDebris)
{
	shapeFile = "./deagle_clip.dts";
	lifetime = 4.0;
	minSpinSpeed = -20.0;
	maxSpinSpeed = 20.0;
	elasticity = 0.5;
	friction = 0.2;
	numBounces = 5;
	staticOnMaxBounce = true;
	snapOnMaxBounce = false;
	fade = true;

	gravModifier = 3;
};

datablock ExplosionData(DesertEagleMagExplosion)
{

   lifeTimeMS = 200;
    
    debris = DesertEagleMagDebris;
   debrisThetaMin = 0;
   debrisThetaMax = 1; //90 
   debrisPhiMin = 0;
   debrisPhiMax = 1;
   debrisNum = 1;
   debrisNumVariance = 0;
   debrisVelocity = 0;
   debrisVelocityVariance = 0;

   faceViewer     = true;
   explosionScale = "1 1 1";

   shakeCamera = false;
   camShakeFreq = "10.0 11.0 10.0";
   camShakeAmp = "1.0 1.0 1.0";
   camShakeDuration = 0.3;
   camShakeRadius = 3.0;

   // Dynamic light
   lightStartRadius = 0;
   lightEndRadius = 0;
   lightStartColor = "0.0 0.0 0.0";
   lightEndColor = "0 0 0";

   impulseRadius = 0;
   impulseForce = 0;
};

datablock ItemData(DesertEagleItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	 // Basic Item Properties
	shapeFile = "./Deagle.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Desert Eagle";
	iconName = "Add-Ons/Weapon_Gun/icon_gun";
	doColorShift = true;
	colorShiftColor = "0.75 0.75 0.75 1";

	 // Dynamic properties defined by the scripts
	image = DesertEagleImage;
	canDrop = true;
	
	//Ammo Guns Parameters
	MMmaxAmmo = 6;
	MMcanReload = 1;
};

datablock ShapeBaseImageData(DesertEagleImage)
{
   // Basic Item properties
   shapeFile = "./deagle.dts";
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
   item = DesertEagleItem;
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
	stateTimeoutValue[2]            = 0.2;
	stateFire[2]                    = true;
	stateAllowImageChange[2]        = false;
	stateSequence[2]                = "Fire";
	stateScript[2]                  = "onFire";
	stateWaitForTimeout[2]			= true;
	stateEmitter[2]					= gunFlashEmitter;
    stateEjectShell[2] = true;
	stateEmitterTime[2]				= 0.05;
	stateEmitterNode[2]				= "muzzlePoint";
	stateSound[2]					= gunShot1Sound; //gunShot4Sound;
	// stateEjectShell[2]       = true;

	stateName[3] = "Smoke";
	stateSequence[3] = "Reload";
	stateEmitter[3]					= gunSmokeEmitter;
	stateEmitterTime[3]				= 1;
	stateEmitterNode[3]				= "muzzlePoint";
	stateTimeoutValue[3]            = 0.8;
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
	stateTimeoutValue[6]			= 0.8;
	stateScript[6]				= "onReloadStart";
	stateTransitionOnTimeout[6]		= "ReloadMid";
	stateWaitForTimeout[6]			= true;
	stateSound[6]			= Block_ChangeBrick_Sound;
	
	stateName[7]				= "ReloadMid";
	stateSequence[7] = "Reload";
	stateTimeoutValue[7]			= 1.8;
	stateScript[7]				= "onReloadMid";
	stateTransitionOnTimeout[7]		= "Reloaded";
	stateWaitForTimeout[7]			= true;
	
	stateName[8]				= "Reloaded";
	stateTimeoutValue[8]			= 0.2;
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
	

};

function DesertEagleImage::onFire(%this, %obj, %slot)
{
	MMGunImage::onFire(%this, %obj, %slot);
}

function DesertEagleImage::onReady(%this,%obj,%slot)
{
	MMGunImage::onReady(%this, %obj, %slot);
}

function DesertEagleImage::onReloaded(%this, %obj, %slot)
{
	MMGunImage::onReloaded(%this, %obj, %slot);
}

function DesertEagleImage::onReloadStart(%this, %obj, %slot)
{
	MMGunImage::onReloadStart(%this, %obj, %slot);
}

function DesertEagleImage::onReloadMid(%this, %obj, %slot)
{
	MMGunImage::onReloadMid(%this, %obj, %slot);
}