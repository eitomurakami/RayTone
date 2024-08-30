//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS("signal in");
RAYTONE_DEFINE_INLETS("cutoff", "Q");
RAYTONE_DEFINE_OUTLET(false);
RAYTONE_LOADFILE(false);
//-----------------------------------------------------------------------------

// DSP chain
RAYTONE_INPUT(0) => HPF hpf => RAYTONE_OUTPUT => dac;
	
// infinite loop
while (true)
{
	// Read cutoff inlet; set to 1000 if not connected
	if (RAYTONE_INLET_STATUS(0) == 1)
	{
		Math.max(Math.min(RAYTONE_INLET(0), 20000), 0) => hpf.freq;
	}
	else
	{
		1000 => hpf.freq;
	}

	// Read Q inlet; set to 1 if not connected
	if (RAYTONE_INLET_STATUS(1) == 1)
	{
		Math.max(Math.min(RAYTONE_INLET(1), 10), 0) => hpf.Q;
	}
	else
	{
		1 => hpf.Q;
	}

	// Advance time
	1::ms => now;
}