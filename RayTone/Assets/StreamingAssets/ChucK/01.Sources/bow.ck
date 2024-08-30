//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS();
RAYTONE_DEFINE_INLETS("freq", "bow pressure", "bow position", "vibrato freq", "vibrato depth");
RAYTONE_DEFINE_OUTLET(false);
RAYTONE_LOADFILE(false);
//-----------------------------------------------------------------------------

// DSP chain
Bowed bow => RAYTONE_OUTPUT => dac;

// infinite loop
while (true)
{
	// Silence bow if frequency is 0
	if (RAYTONE_INLET(0) == 0)
	{
		0 => bow.gain;
	}
	else
	{
		RAYTONE_LOCAL_GAIN => bow.gain;

		Math.max(RAYTONE_INLET(0), 50) => bow.freq;
		Math.max(Math.min(RAYTONE_INLET(1), 1), 0) => bow.bowPressure;
		Math.max(Math.min(RAYTONE_INLET(2), 1), 0) => bow.bowPosition;
		Math.max(Math.min(RAYTONE_INLET(3), 1), 0) => bow.vibratoFreq;
		Math.max(Math.min(RAYTONE_INLET(4), 1), 0) => bow.vibratoGain;
	}

	// Start
    1 => bow.noteOn;

	// Advance time
	1::ms => now;
}

