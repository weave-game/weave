import { db } from "./consts";

const base = import.meta.env.BASE_URL + "assets/";

export const FONT_DISPLAY = "'Open Sans ExtraBold'";
export const FONT_BOLD = "'Open Sans Bold'";
export const FONT_DEFAULT = "'Open Sans SemiBold'";

export async function loadFonts() {
  const fonts: [string, string][] = [
    ["Open Sans ExtraBold", "fonts/OpenSans-ExtraBold.ttf"],
    ["Open Sans Bold", "fonts/OpenSans-Bold.ttf"],
    ["Open Sans SemiBold", "fonts/OpenSans-SemiBold.ttf"],
  ];
  await Promise.all(
    fonts.map(async ([family, file]) => {
      const face = new FontFace(family, `url(${base}${file})`);
      document.fonts.add(await face.load());
    }),
  );
}

const images = new Map<string, HTMLImageElement>();

export function image(name: string) {
  let img = images.get(name);
  if (!img) {
    img = new Image();
    img.src = base + "img/" + name;
    images.set(name, img);
  }
  return img;
}

export async function loadImages(names: string[]) {
  await Promise.all(names.map((name) => image(name).decode().catch(() => undefined)));
}

const tinted = new Map<string, HTMLCanvasElement>();

/** A sprite multiplied by a colour (Godot's modulate), rasterised at `size` px. */
export function tint(name: string, color: string, width: number, height = width) {
  const key = `${name}|${color}|${width}|${height}`;
  let canvas = tinted.get(key);
  if (canvas) return canvas;
  canvas = document.createElement("canvas");
  canvas.width = Math.max(1, Math.ceil(width));
  canvas.height = Math.max(1, Math.ceil(height));
  const ctx = canvas.getContext("2d")!;
  ctx.drawImage(image(name), 0, 0, canvas.width, canvas.height);
  ctx.globalCompositeOperation = "multiply";
  ctx.fillStyle = color;
  ctx.fillRect(0, 0, canvas.width, canvas.height);
  ctx.globalCompositeOperation = "destination-in";
  ctx.drawImage(image(name), 0, 0, canvas.width, canvas.height);
  tinted.set(key, canvas);
  return canvas;
}

// --- Audio ---
//
// Mobile Safari only lets audio start from a tap. Two things survive that:
// - One music element, started by the first tap, then reused for every track (new src, play).
// - An AudioContext resumed by the first tap, which then plays sound effects at any time.
// The music also goes through the AudioContext, since iOS ignores `volume` on media elements.

let ctx: AudioContext | null = null;
let musicEl: HTMLAudioElement | null = null;
let musicGain: GainNode | null = null;
let music: { file: string; volumeDb: number } | null = null;
let musicPitch = 1;
let unlocked = false;
const buffers = new Map<string, Promise<AudioBuffer | null>>();

function applyVolume() {
  if (!musicEl) return;
  const volume = music ? db(music.volumeDb) : 1;
  if (musicGain) musicGain.gain.value = volume;
  else musicEl.volume = Math.min(1, volume);
}

/** Call from every tap and key press; it does its work once audio runs. */
export function unlockAudio() {
  if (!ctx) {
    // Safari: play like a media app, so the ringer switch does not silence Web Audio.
    const session = (navigator as Navigator & { audioSession?: { type: string } }).audioSession;
    if (session) session.type = "playback";
    try {
      ctx = new AudioContext();
    } catch {
      ctx = null;
    }
  }
  if (ctx && ctx.state !== "running") void ctx.resume().catch(() => undefined);
  if (!musicEl) {
    musicEl = new Audio();
    musicEl.preload = "auto";
    musicEl.loop = true;
    setPitchPreserving(musicEl);
    if (ctx) {
      try {
        musicGain = ctx.createGain();
        ctx.createMediaElementSource(musicEl).connect(musicGain).connect(ctx.destination);
      } catch {
        musicGain = null;
      }
    }
  }
  if (!unlocked) {
    unlocked = true;
    if (music) startMusic();
  } else if (music && musicEl.paused) void musicEl.play().catch(() => undefined);
}

function setPitchPreserving(el: HTMLAudioElement) {
  el.preservesPitch = false;
  (el as HTMLAudioElement & { webkitPreservesPitch?: boolean }).webkitPreservesPitch = false;
}

function startMusic() {
  if (!musicEl || !music) return;
  const src = base + "audio/" + music.file;
  if (!musicEl.src.endsWith(src.replace(/^\./, ""))) musicEl.src = src;
  musicEl.currentTime = 0;
  musicEl.playbackRate = musicPitch;
  applyVolume();
  void musicEl.play().catch(() => undefined);
}

export function playMusic(file: string, volumeDb: number) {
  music = { file, volumeDb };
  musicPitch = 1;
  if (unlocked) startMusic();
}

export function setMusicPitch(pitch: number) {
  musicPitch = Math.max(0.0625, pitch);
  if (musicEl) musicEl.playbackRate = musicPitch;
}

export function stopMusic() {
  music = null;
  musicEl?.pause();
}

function buffer(file: string) {
  let pending = buffers.get(file);
  if (!pending && ctx) {
    const context = ctx;
    pending = fetch(base + "audio/" + file)
      .then((response) => response.arrayBuffer())
      .then((data) => context.decodeAudioData(data))
      .catch(() => null);
    buffers.set(file, pending);
  }
  return pending ?? Promise.resolve(null);
}

export function playSfx(file: string, volumeDb: number) {
  if (!ctx) return;
  const context = ctx;
  void buffer(file).then((data) => {
    if (!data || context.state !== "running") return;
    const source = context.createBufferSource();
    const gain = context.createGain();
    source.buffer = data;
    gain.gain.value = db(volumeDb);
    source.connect(gain).connect(context.destination);
    source.start();
  });
}

/** Decodes the sound effects ahead of time, so the first ding is on time. */
export function preloadSfx() {
  for (const file of ["complete.mp3", "ding.mp3", "ding2.mp3", "explosion.mp3"]) void buffer(file);
}

export const SFX = {
  complete: () => playSfx("complete.mp3", -16),
  ding: () => playSfx("ding.mp3", -4.571),
  unlock: () => playSfx("ding2.mp3", -4),
  explosion: () => playSfx("explosion.mp3", -7.429),
};
