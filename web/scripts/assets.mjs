// Puts the game's files in public/assets, where Vite serves them. Most come straight from the
// Godot project (../weave/Assets); static/ holds the few made for the web (smaller images).
import { cpSync, mkdirSync, rmSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const root = join(dirname(fileURLToPath(import.meta.url)), "..");
const godot = join(root, "../weave/Assets");
const out = join(root, "public/assets");

const files = {
  "audio/beginning.mp3": "Music/beginning.mp3",
  "audio/tutor.mp3": "Music/tutor.mp3",
  "audio/weave.mp3": "Music/weave.mp3",
  "audio/complete.mp3": "SFX/complete.mp3",
  "audio/ding.mp3": "SFX/ding.mp3",
  "audio/ding2.mp3": "SFX/ding2.mp3",
  "audio/explosion.mp3": "SFX/explosion.mp3",
  "fonts/OpenSans-SemiBold.ttf": "Fonts/Open_Sans/static/OpenSans-SemiBold.ttf",
  "fonts/OpenSans-Bold.ttf": "Fonts/Open_Sans/static/OpenSans-Bold.ttf",
  "fonts/OpenSans-ExtraBold.ttf": "Fonts/Open_Sans/static/OpenSans-ExtraBold.ttf",
  "img/arrow.svg": "Sprites/arrow.svg",
  "img/cube.svg": "Sprites/cube.svg",
  "img/lock.svg": "Sprites/lock.svg",
  "img/arrow-right-on-rectangle.svg": "Icons/arrow-right-on-rectangle.svg",
  "img/empty_key.png": "Icons/empty_key.png",
  "img/info.svg": "Icons/info.svg",
  "img/keyboard.svg": "Icons/keyboard.svg",
  "img/play-circle.svg": "Icons/play-circle.svg",
  "img/logo-small.png": "logo-small.png",
  "img/weave.png": "weave.png",
  "img/weave_front.png": "weave_front.png",
};

rmSync(out, { recursive: true, force: true });
for (const [to, from] of Object.entries(files)) {
  mkdirSync(dirname(join(out, to)), { recursive: true });
  cpSync(join(godot, from), join(out, to));
}
cpSync(join(root, "static"), out, { recursive: true });
console.log(`assets: ${Object.keys(files).length} from the Godot project, plus static/`);
