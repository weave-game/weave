// Keyboard state by KeyboardEvent.code.

const pressed = new Set<string>();
const keydownListeners = new Set<(e: KeyboardEvent) => void>();

const typing = (e: KeyboardEvent) =>
  e.target instanceof HTMLElement && (e.target.tagName === "INPUT" || e.target.isContentEditable);

addEventListener("keydown", (e) => {
  if (typing(e)) return;
  if (e.code.startsWith("Arrow") || (e.code === "Space" && !(e.target instanceof HTMLButtonElement))) e.preventDefault();
  pressed.add(e.code);
  if (e.code === "KeyF" && !e.repeat) {
    if (document.fullscreenElement) void document.exitFullscreen();
    else void document.documentElement.requestFullscreen?.().catch(() => undefined);
  }
  for (const listener of keydownListeners) listener(e);
});
addEventListener("keyup", (e) => pressed.delete(e.code));
addEventListener("blur", () => pressed.clear());

export const isPressed = (code: string) => pressed.has(code);

export function onKeydown(listener: (e: KeyboardEvent) => void) {
  keydownListeners.add(listener);
  return () => {
    keydownListeners.delete(listener);
  };
}
