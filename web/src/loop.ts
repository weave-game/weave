import { session } from "./session";

const renderers = new Set<(dt: number) => void>();

/** Runs every animation frame, after the session has stepped. */
export function onFrame(render: (dt: number) => void) {
  renderers.add(render);
  return () => {
    renderers.delete(render);
  };
}

let last = performance.now();
function frame(now: number) {
  const dt = Math.min(0.1, (now - last) / 1000);
  last = now;
  session.update(dt);
  for (const render of renderers) render(dt);
  requestAnimationFrame(frame);
}

export function startLoop() {
  requestAnimationFrame(frame);
}
