//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS("signal in");
RAYTONE_DEFINE_INLETS("delay time(ms)", "feedback 0-1");
RAYTONE_DEFINE_OUTLET(false);
RAYTONE_LOADFILE(false);
//-----------------------------------------------------------------------------

// DSP chain
RAYTONE_INPUT(0) => Gain g => RAYTONE_OUTPUT => dac;
g => Gain feedback => DelayL delay => g;
10000::ms => delay.max;

// infinite loop
while (true)
{
	// Read delay length inlet; set to 500 ms if not connected
	if (RAYTONE_INLET_STATUS(0) == 1)
	{
		Math.max(Math.min(RAYTONE_INLET(0), 10000), 0)::ms => delay.delay;
	}
	else
	{
		500::ms => delay.delay;
	}

	// Set feedback
	Math.max(Math.min(RAYTONE_INLET(1), 1), 0) => feedback.gain;

	// Advance time
	1::ms => now;
}
