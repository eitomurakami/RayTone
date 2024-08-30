//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS("signal in");
RAYTONE_DEFINE_INLETS("gain multiplier");
RAYTONE_DEFINE_OUTLET(false);
RAYTONE_LOADFILE(false);
//-----------------------------------------------------------------------------

// DSP chain
RAYTONE_INPUT(0) => Gain g => RAYTONE_OUTPUT => dac;

// infinite loop
while (true)
{
	RAYTONE_INLET(0) => g.gain;
	1::ms => now;
}
