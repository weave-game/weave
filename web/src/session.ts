// The game's flow on one screen: start screen and lobby, splash, game, game over. Ports the
// lobby parts of StartScreen.cs, SplashScreen.cs, the scene changes in Main.cs, and
// GameOverOverlay.cs.

import { playMusic, setMusicPitch, unlockAudio } from "./assets";
import { KEY_PAIRS, keyLabel } from "./consts";
import { isPressed, onKeydown } from "./input";
import { Sim } from "./sim";
import { playerColor, teamName, uid } from "./util";
import { GameView } from "./view";

export type Phase = "start" | "splash" | "game" | "over";

/** A joined player: the index of their key pair in KEY_PAIRS. */
export type LobbyPlayer = { slot: number };

export type Snapshot = {
  open: boolean;
  players: LobbyPlayer[];
  phase: Phase;
  team: string;
  skipLabel: boolean;
  fading: boolean;
  game: number;
  /** When the score was last saved, for the "Saved" flash. */
  saved: number;
  /** When START GAME was pressed with nobody in the lobby. */
  noPlayers: number;
};

type Score = { name: string; points: number; players: number; rounds: number };

class Session {
  private listeners = new Set<() => void>();
  private snap: Snapshot = {
    open: false,
    players: [],
    phase: "start",
    team: teamName(),
    skipLabel: false,
    fading: false,
    game: 0,
    saved: 0,
    noPlayers: 0,
  };
  private latches = new Map<number, { since: number | null; done: boolean }>();
  private sim: Sim | null = null;
  private accumulator = 0;
  private splashAt = 0;
  private fadeAlpha = 0;
  /** A lobby is one group's session: retries keep it, Menu starts a new one. */
  private lobbyId = uid();
  private highScore = 0;
  private sessionScore: Score | null = null;

  view: GameView | null = null;
  /** Interpolation between the last two ticks. */
  alpha = 1;

  constructor() {
    playMusic("beginning.mp3", -5.943);
    onKeydown(() => this.keydown());
  }

  get snapshot() {
    return this.snap;
  }

  subscribe = (listener: () => void) => {
    this.listeners.add(listener);
    return () => {
      this.listeners.delete(listener);
    };
  };

  private set(patch: Partial<Snapshot>) {
    this.snap = { ...this.snap, ...patch };
    for (const listener of this.listeners) listener();
  }

  // --- Lobby ---

  toggleLobby() {
    this.set({ open: !this.snap.open });
  }

  private has(slot: number) {
    return this.snap.players.some((p) => p.slot === slot);
  }

  /** The first free key pair, for "JOIN: PRESS". */
  nextPair() {
    const i = KEY_PAIRS.findIndex((_, slot) => !this.has(slot));
    return i < 0 ? null : KEY_PAIRS[i].map(keyLabel);
  }

  /** The lobby's players, with colours and names by join order. */
  lobbyPlayers() {
    return this.snap.players.map((p, i) => ({ ...p, color: playerColor(i), name: String(i + 1) }));
  }

  pressed(slot: number) {
    const [left, right] = KEY_PAIRS[slot];
    return { l: isPressed(left), r: isPressed(right) };
  }

  /** Join (press both keys) and leave (hold both for 0.5 s), as StartScreen.KeyboardPressed. */
  private pollPairs(now: number) {
    if (this.snap.phase !== "start" || !this.snap.open) {
      this.latches.clear();
      return;
    }
    KEY_PAIRS.forEach(([left, right], slot) => {
      const both = isPressed(left) && isPressed(right);
      const latch = this.latches.get(slot) ?? { since: null, done: false };
      if (!both) {
        this.latches.set(slot, { since: null, done: false });
        return;
      }
      if (latch.done) return;
      if (!this.has(slot)) {
        this.set({ players: [...this.snap.players, { slot }] });
        this.latches.set(slot, { since: null, done: true });
      } else if (latch.since === null) {
        this.latches.set(slot, { since: now, done: false });
      } else if (now - latch.since >= 500) {
        this.set({ players: this.snap.players.filter((p) => p.slot !== slot) });
        this.latches.set(slot, { since: null, done: true });
      }
    });
  }

  // --- Phases ---

  startGame() {
    if (this.snap.phase !== "start") return;
    if (this.snap.players.length === 0) {
      this.set({ noPlayers: performance.now() });
      return;
    }
    this.splashAt = performance.now();
    this.fadeAlpha = 0;
    playMusic("tutor.mp3", 0);
    this.set({ phase: "splash", skipLabel: false, fading: false });
  }

  private keydown() {
    unlockAudio();
    if (this.snap.phase !== "splash" || performance.now() - this.splashAt < 1000) return;
    if (this.snap.skipLabel) this.set({ fading: true });
    else this.set({ skipLabel: true });
  }

  private beginGame() {
    const slots = this.snap.players.map((p) => p.slot);
    this.sim = new Sim(slots.length, (i) => {
      const { l, r } = this.pressed(slots[i]);
      return { left: l, right: r };
    });
    this.view = new GameView({
      players: slots.map((_, i) => ({ name: String(i + 1), color: playerColor(i) })),
    });
    this.accumulator = 0;
    this.sessionScore = null;
    playMusic("weave.mp3", -8.419);
    this.set({ phase: "game", game: this.snap.game + 1, saved: 0, skipLabel: false, fading: false });
  }

  retry() {
    this.beginGame();
  }

  /** Back to the start screen with a new lobby. */
  menu() {
    this.sim = null;
    this.view = null;
    this.lobbyId = uid();
    this.highScore = 0;
    playMusic("beginning.mp3", -5.943);
    this.set({ phase: "start", open: false, players: [], team: teamName() });
  }

  setTeam(team: string) {
    this.set({ team });
  }

  /** GameOverOverlay.SaveScore: one entry per lobby, holding its best score. */
  saveScore(score?: Score) {
    this.sessionScore ??= score ?? null;
    const s = this.sessionScore;
    if (!s || s.points <= 0) return;
    if (s.points < this.highScore) s.points = this.highScore;
    else this.highScore = s.points;
    if (this.snap.team.trim()) s.name = this.snap.team;
    try {
      const log = JSON.parse(localStorage.getItem("weave-scores") ?? "{}");
      log[this.lobbyId] = { ...s, savedAt: new Date().toISOString() };
      localStorage.setItem("weave-scores", JSON.stringify(log));
    } catch {
      // Private browsing or a full store: the game goes on without saving.
    }
    this.set({ saved: performance.now() });
  }

  // --- Loop ---

  update(dt: number) {
    this.pollPairs(performance.now());
    if (this.snap.phase === "splash") this.updateSplash(dt);
    this.step(dt);
    this.view?.update(dt);
  }

  /** SplashScreen.cs: after 7 s or a second key press, fade to #1e1e1e and slow the music. */
  private updateSplash(dt: number) {
    if (!this.snap.fading && performance.now() - this.splashAt >= 7000) this.set({ fading: true });
    if (!this.snap.fading) return;
    this.fadeAlpha += (1 - this.fadeAlpha) * Math.min(1, dt * 1.5);
    setMusicPitch(1 - this.fadeAlpha * 0.7);
    if (this.fadeAlpha >= 0.99) this.beginGame();
  }

  get splashFade() {
    return this.fadeAlpha;
  }

  /** The game runs at a fixed 60 ticks a second, whatever the display's refresh rate. */
  private step(dt: number) {
    if (!this.sim || !this.view) return;
    // After game over, run on briefly for a pending round bonus, then stop.
    if (this.sim.isOver && this.sim.overFor > 1.5) return;
    this.accumulator = Math.min(this.accumulator + dt, 5 / 60);
    while (this.accumulator >= 1 / 60) {
      this.accumulator -= 1 / 60;
      const frame = this.sim.step();
      this.view.apply(frame);
      if (frame.ev?.some((e) => e.e === "over")) {
        this.set({ phase: "over" });
        this.saveScore({
          name: this.snap.team,
          points: this.sim.finalScore,
          players: this.snap.players.length,
          rounds: this.sim.rounds,
        });
      }
    }
    this.alpha = this.accumulator * 60;
  }
}

export const session = new Session();
