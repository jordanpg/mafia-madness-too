function getDaySuffix(%day)
{
	if(%day > 3 && %day < 21)
		%suffix = "th";
	else
	{
		switch(%day % 10)
		{
			case 1: %suffix = "st";
			case 2: %suffix = "nd";
			case 3: %suffix = "rd";
			default: %suffix = "th";
		}
	}

	return %suffix;
}

function getRandomFloat( %min, %max )
{
	return %min + getRandom() * ( %max - %min );
}

function pointBetween(%point, %vectA, %vectB, %xy)
{
	%xA = getWord(%vectA, 0);
	%yA = getWord(%vectA, 1);
	%zA = getWord(%vectA, 2);
	%xB = getWord(%vectB, 0);
	%yB = getWord(%vectB, 1);
	%zB = getWord(%vectB, 2);

	if(%xA > %xB)
	{
		%t = %xA;
		%xA = %xB;
		%xB = %t;
	}
	if(%yA > %yB)
	{
		%t = %yA;
		%yA = %yB;
		%yB = %t;
	}
	if(%zA > %zB)
	{
		%t = %zA;
		%zA = %zB;
		%zB = %t;
	}

	%x = getWord(%point, 0);
	%y = getWord(%point, 1);
	%z = getWord(%point, 2);

	if(%x < %xA || %x > %xB)
		return false;
	if(%y < %yA || %y > %yB)
		return false;
	if((%z < %zA || %z > %zB) && !%xy)
		return false;
	return true;
}

function VectorRandom(%vectA, %vectB)
{
	%xA = getWord(%vectA, 0);
	%yA = getWord(%vectA, 1);
	%zA = getWord(%vectA, 2);
	%xB = getWord(%vectB, 0);
	%yB = getWord(%vectB, 1);
	%zB = getWord(%vectB, 2);

	if(%xA != %xB)
		%x = getRandomFloat(%xA, %xB);
	else
		%x = %xA;
		
	if(%yA != %yB)
		%y = getRandomFloat(%yA, %yB);
	else
		%y = %xA;
		
	if(%zA != %zB)
		%z = getRandomFloat(%zA, %zB);
	else
		%z = %zA;

	return %x SPC %y SPC %z;
}

function isInList(%list, %search)
{
	%ct = getWordCount(%list);
	for(%i = 0; %i < %ct; %i++)
		if(getWord(%list, %i) $= %search) return true;

	return false;
}

function searchWord(%list, %search)
{
	%ct = getWordCount(%list);
	for(%i = 0; %i < %ct; %i++)
		if(getWord(%list, %i) $= %search) return %i;

	return -1;
}

function stripToBasicString(%str)
{
	%table = "abcdefghijklmnopqrstuvwxyz0123456789_";

	%i = 0;
	while(%i < strLen(%str))
	{
		%c = getSubStr(%str, %i, 1);

		if(striPos(%table, %c) != -1)
		{
			%i++;
			continue;
		}

		%str = strReplace(%str, %c, "");
	}

	return %str;
}

function i_Lerp(%y1, %y2, %mu)
{
	return (%y1 * (1 - %mu) + %y2 * %mu);
}

function fixFileName(%str)
{
	%str = strReplace(%str, "-", "DASH");
	%str = strReplace(%str, " ", "_");

	return %str;
}