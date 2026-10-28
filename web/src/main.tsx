import { createRoot } from "react-dom/client";
import { loadFonts, loadImages, preloadSfx, unlockAudio } from "./assets";
import { startLoop } from "./loop";
import { App } from "./ui/App";
import "./styles.css";

void Promise.all([
  loadFonts(),
  loadImages([
    "arrow.svg", "cube.svg", "lock.svg", "bg_pattern.png", "weave.png", "weave_front.png",
  ]),
]).then(() => {
  createRoot(document.getElementById("root")!).render(<App />);
  startLoop();
});
// iOS counts only some events as taps that may start audio (touchend, click), so listen to all.
const unlock = () => {
  unlockAudio();
  preloadSfx();
};
for (const event of ["pointerdown", "pointerup", "touchend", "click", "keydown"])
  addEventListener(event, unlock, { capture: true });
