$MM::LoadedData_Misc = true;

datablock StaticShapeData(SmoothSphereShapeData)
{
	shapeFile = "./smoothSphere.dts";
};

datablock AudioProfile(ClownHornSound)
{
	description = "AudioClose3D";
	fileName = "./horn.wav";
	preload = true;
};

datablock ShapeBaseImageData(BlankImage)
{
	shapeFile = "base/data/shapes/empty.dts";
	mountPoint = 2;
};