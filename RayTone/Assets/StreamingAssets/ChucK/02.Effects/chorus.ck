//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS("signal in");
RAYTONE_DEFINE_INLETS("chorus freq", "chorus depth");
RAYTONE_DEFINE_OUTLET(false);
RAYTONE_LOADFILE(false);
//-----------------------------------------------------------------------------

// DSP chain
RAYTONE_INPUT(0) => Chorus chorus => RAYTONE_OUTPUT => dac;

// infinite loop
while (true)
{
	// Read inlets
	RAYTONE_INLET(0) => chorus.modFreq;

	// Set modulation depth to 0 if modulation frequency is 0
    if (chorus.modFreq() == 0)
	{
		0 => chorus.modDepth;
	}
	else
	{
		Math.min(RAYTONE_INLET(1), 5) => chorus.modDepth;
	}

	// Advance time
	1::ms => now;
}
