Audio verification — 11 October 2026
Unity 6000.5.8f1, Play Mode with -force-audio.
Audio checks cover: all 10 imported clips, live AudioSource playback, one looped music source and one listener per scene, independent BGM/SFX sliders including zero, SFX preview, saved values and reopening Options, music continuing through additive Options, music in all main scenes, win/lose cues, collision pickups for all four Stage1 items, successful-interaction-only audio for all four Stage2 items, wall impact and cooldown.
The checks restore the user's previous volume preferences afterward.
Options.png captures the light UI with independent volume controls.
Stage2 gameplay checks were also rerun after audio integration: all 42 passed.
These verify Unity playback state and gameplay wiring, not the user's Windows output device or loudspeaker setting.