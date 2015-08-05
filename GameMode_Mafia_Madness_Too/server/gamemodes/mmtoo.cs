MM_RegisterModeFile("./mmtoo.mmgm");
MM_RegisterModeFile("./mmtoo_att.mmgm");

function MMGameMode_MMTTransformRoles(%this)
{
	if($MMGctC > 0 && $MMGbringTheLaw && getRandom() < $MMGlawChance)
	{
		$MMGctC--;
		$MMGctLAW++;
	}

	if($MMGjohnCena > 0)
		for(%i = 0; %i < $MMGjohnCena; %i++)
			if(getRandom() < $MMGjohnCenaChance)
				$MMGctJOHNCENA++;

	if($MMGhonkHonk && $MMGctJ > 0 && getRandom() < $MMGhonkHonkChance)
	{
		$MMGctJ--;
		$MMGctCLOWN++;
	}
}