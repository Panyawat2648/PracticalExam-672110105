Stage 2 verification — 11 October 2026
Unity 6000.5.8f1 Play Mode, isolated version-3 worktree.
42 checks passed; game test completed with STAGE2_PLAY_OK and no runtime errors captured.
The full route uses virtual keyboard events through PlayerInput (WASD, Shift, E), without teleporting during that route. It collected 18,000 points with 19.1 simulated seconds left.
The test uses a fixed 1/60 simulation timestep to avoid editor import/render stalls changing input timing. Normal game runtime uses the actual frame delta time.
Boundary cases use explicit time advancement/teleports: timeout, exactly 10,000, falling, penalty reaching zero, and retries.
See verification.txt and Stage2-start/route/win.png.
Older Standard Assets scripts still produce Unity 6 obsolete-API warnings. The first fresh import also logged an API updater exception; later compilation and Play Mode completed successfully.
The local editor startup search index was disabled to work around a UnityEditor.Search startup exception in the fresh worktree; this does not affect game code.
Performance follow-up: cached scene items instead of searching every frame; HUD timer redraw limited to 10 Hz with score/prompt updated only on changes; reduced shadow/texture/AA load, 60 FPS target, camera LateUpdate. Stage2 checks rerun after changes. These are functional checks, not a measured interactive FPS benchmark.
