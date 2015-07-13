//bricks.cs
//Brick datablocks and methods.

$MM::LoadedBricks = true;

datablock fxDTSBrickData(BrickAfterLifeSpawnPointData : BrickSpawnPointData)
{
	uiName = "Afterlife Spawn Point";
	category = "Special";
	subCategory = "Mafia Madness";
};

datablock fxDTSBrickData(BrickDumpsterPointData : Brick4x4fData)
{
	uiName = "Dumpster (Abduction) Point";
	category = "Special";
	subCategory = "Mafia Madness";
};

function BrickAfterLifeSpawnPointData::onPlant(%this, %obj)
{
	if(!isObject(MMAfterLifeSpawns))
		new SimSet(MMAfterLifeSpawns);

	MMAfterLifeSpawns.add(%obj);
}

function BrickAfterLifeSpawnPointData::onLoadPlant(%this, %obj)
{
	if(!isObject(MMAfterLifeSpawns))
		new SimSet(MMAfterLifeSpawns);

	MMAfterLifeSpawns.add(%obj);
}

function BrickDumpsterPointData::onPlant(%this, %obj)
{
	if(!isObject(MMAbductionSpawns))
		new SimSet(MMAbductionSpawns);

	%obj.setColliding(false);
	%obj.setRendering(false);
	%obj.setRayCasting(false);

	MMAbductionSpawns.add(%obj);
}

function BrickDumpsterPointData::onLoadPlant(%this, %obj)
{
	if(!isObject(MMAbductionSpawns))
		new SimSet(MMAbductionSpawns);

	MMAbductionSpawns.add(%obj);
}