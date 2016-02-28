function MMGameMode_MTTChaosMode(%this)
{
	echo("fuck");

	%ct = MMRoles.getCount() - 1;

	for(%i = 0; %i < $MMGmembers; %i++)
	{
		%l = MMRoles.getObject(getRandom(%ct)).getLetter();
		echo(%l);
		$MMGct[%l]++;
		$MMGinnoRoles = $MMGinnoRoles SPC %l;
	}
}