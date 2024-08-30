//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS("signal in");
RAYTONE_DEFINE_INLETS("pitch shift");
RAYTONE_DEFINE_OUTLET(false);
RAYTONE_LOADFILE(false);
//-----------------------------------------------------------------------------

// DSP chain
RAYTONE_INPUT(0) => PitShift pitch => RAYTONE_OUTPUT => dac;

// infinite loop
while (true)
{
	// Set pitch shift amount
	RAYTONE_INLET(0) + 1 => pitch.shift; 

	// Advance time
	1::ms => now;
}
