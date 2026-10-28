# Weave for the web

A port of Weave to the browser that looks and plays like the Godot game: the same rules, numbers, screens, music, and quirks. It's local multiplayer on one keyboard. There's no gamepad or phone support.

## Play

Press PLAY to open the lobby. Each player joins by pressing both keys of a free pair: `←/→`, `1/Q`, `B/N`, `Z/X`, `L/O`, or `5/6`. Hold both keys to leave. `F` toggles fullscreen.

Scores are kept in the browser (`localStorage`, key `weave-scores`), one entry per lobby with its best score, like the game's score log.

## Develop

Needs Node 20 and pnpm.

```sh
pnpm install
pnpm dev     # http://localhost:5173
pnpm build   # a static site in dist/; it works from any folder or host
```

`scripts/assets.mjs` copies the music, sounds, fonts, and sprites from `../weave/Assets` into `public/assets`. `static/` holds the only web-specific files: smaller copies of the background pattern and the tutorial picture.

## How it maps to the Godot project

| Web | Godot |
| --- | --- |
| `src/sim.ts` | `Main.cs`, `Player.cs`, `CurveSpawner.cs`, `Grid.cs`, `Goal.cs`, `Obstacle.cs`, `ScoreDisplay.cs` |
| `src/view.ts` | The scenes' drawing, animations, camera, and glow |
| `src/session.ts` | The flow in `StartScreen.cs`, `SplashScreen.cs`, and `GameOverOverlay.cs` |
| `src/ui/` | `StartScreen.tscn`, `SplashScreen.tscn`, `GameOverOverlay.tscn`, `Credits.tscn` |
| `src/fireflies.ts` | `Firefly.cs` and the start screen's curves |
| `src/util.ts` | `UniqueColorGenerator`, `UniqueNameGenerator`, .NET's `Random` for the player colours |

The simulation runs at a fixed 60 ticks a second, as `_PhysicsProcess` does. Quirks are kept on purpose, for example the speed decay that runs once per player each frame.
