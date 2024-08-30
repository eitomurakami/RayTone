//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS("signal in");
RAYTONE_DEFINE_INLETS("cutoff", "Q");
RAYTONE_DEFINE_OUTLET(false);
RAYTONE_LOADFILE(false);
//-----------------------------------------------------------------------------

// DSP chain
RAYTONE_INPUT(0) => BPF bpf => RAYTONE_OUTPUT => dac;
	
// infinite loop
while (true)
{
	// Read cutoff inlet; set to 1000 if not connected
	if (RAYTONE_INLET_STATUS(0) == 1)
	{
		Math.max(Math.min(RAYTONE_INLET(0), 20000), 0) => bpf.freq;
	}
	else
	{
		1000 => bpf.freq;
	}

	// Read Q inlet; set to 1 if not connected
	if (RAYTONE_INLET_STATUS(1) == 1)
	{
		RAYTONE_INLET(1) => bpf.Q;
	}
	else
	{
		1 => bpf.Q;
	}

	// Advance time
	1::ms => now;
}