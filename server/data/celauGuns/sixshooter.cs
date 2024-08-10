

if(!$MM::LoadedGun)
	exec("./MMgun.cs");

$MM::LoadedGun_SixShooter = true;

$MM::GunItem[$MM::GunItems] = "SixShooterItem";
$MM::GunItems++;

datablock ItemData(SixShooterItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	 // Basic Item Properties
	shapeFile = "./SixShooter.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Six Shooter";
	iconName = "Add-Ons/Weapon_Gun/icon_gun";
	doColorShift = true;
	colorShiftColor = "0.75 0.75 0.75 1";

	 // Dynamic properties defined by the scripts
	image = SixShooterImage;
	canDrop = true;
	
	//Ammo Guns Parameters
	MMmaxAmmo = 6;
	MMcanReload = 1;
};

datablock ShapeBaseImageData(SixShooterImage : PolicePistolImage)
{
   shapeFile = "./SixShooter.dts";
   item = SixShooterItem;
};

function SixShooterImage::onFire(%this, %obj, %slot)
{
	MMGunImage::onFire(%this, %obj, %slot);
}

function SixShooterImage::onReady(%this,%obj,%slot)
{
	MMGunImage::onReady(%this, %obj, %slot);
}

function SixShooterImage::onReloaded(%this, %obj, %slot)
{
	MMGunImage::onReloaded(%this, %obj, %slot);
}

function SixShooterImage::onReloadStart(%this, %obj, %slot)
{
	MMGunImage::onReloadStart(%this, %obj, %slot);
}

function SixShooterImage::onReloadMid(%this, %obj, %slot)
{
	MMGunImage::onReloadMid(%this, %obj, %slot);
}