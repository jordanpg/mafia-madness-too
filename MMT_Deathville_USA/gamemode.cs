//gamemode.cs
//Code for special functionality of the Deathville USA gamemode.

$MM::GPRaiseAtNight = true;
$MM::GPRisingInfectionRate = 0.125;
$MM::GPRisingInfectedBonus = 2;

function MMGamemode_MMTActivateDeathville(%this)
{

}

package MM_DeathvilleUSA
{
	function MinigameSO::MM_RaiseDead(%this)
	{
		if(!$MM::GMDeathvilleUSA)
			return parent::MM_RaiseDead(%this);

		if(%this.isDay && $MM::GPRaiseAtNight)
			return;

		parent::MM_RaiseDead(%this);
	}

	function MinigameSO::MM_Rise(%this, %cl, %tCl)
	{
		if(!$MM::GMDeathvilleUSA)
			return parent::MM_Rise(%this, %cl, %tCl);

		if(isObject(%tCl))
			%corpse = %tCl.corpse;
		else
			%corpse = %cl.corpse;

		if(!%cl.mmignore && isObject(%cl) && isObject(%corpse))
		{
			%rate = $MM::GPRisingInfectionRate * ((%cl.turned || %cl.MM_isZombie()) ? $MM::GPRisingInfectedBonus : 1);

			if(getRandom() > %rate)
				return;

			%corpse.MM_ZombieInfectCorpse("", %tCl);
		}
	}

	function MinigameSO::MM_onTimeTransition(%this)
	{
		if(!$MM::GMDeathvilleUSA)
			return parent::MM_onTimeTransition(%this);

		if(%this.day >= $MM::GPDeadRising && !%this.isDay)
			%this.MM_RaiseDead();
	}
};
activatePackage(MM_DeathvilleUSA);