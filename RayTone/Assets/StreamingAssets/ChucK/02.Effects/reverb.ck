//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS("signal in");
RAYTONE_DEFINE_INLETS("dry/wet 0-1");
RAYTONE_DEFINE_OUTLET(false);
RAYTONE_LOADFILE(false);
//-----------------------------------------------------------------------------

// DSP chain
RAYTONE_INPUT(0) => JCRev rev => RAYTONE_OUTPUT => dac;

// infinite loop
while (true)
{
	// Read dry/wet inlet; set to 0.03 if not connected
	if (RAYTONE_INLET_STATUS(0) == 1)
	{
		Math.max(RAYTONE_INLET(0), 0) => rev.mix;
	}
	else
	{
		0.03 => rev.mix;
	}

	// Advance time
	1::ms => now;
}
