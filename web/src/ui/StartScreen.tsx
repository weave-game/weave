import { useEffect, useRef, useState } from "react";
import { image } from "../assets";
import { CLEAR, KEY_PAIRS, keyLabel } from "../consts";
import { fireflies } from "../fireflies";
import { onFrame } from "../loop";
import { session, type Snapshot } from "../session";
import { Glow } from "../view";
import { Button, Icon, KeyIcon } from "./Button";
import { StageCanvas } from "./StageCanvas";

const glow = new Glow();

/** Background (layer -2), fireflies, then the front logo (layer -1), all through the glow. */
function drawBackground(ctx: CanvasRenderingContext2D, unit: number, dt: number) {
  for (const firefly of fireflies) firefly.update(dt);
  ctx.setTransform(1, 0, 0, 1, 0, 0);
  ctx.fillStyle = CLEAR;
  ctx.fillRect(0, 0, ctx.canvas.width, ctx.canvas.height);
  ctx.setTransform(unit, 0, 0, unit, 0, 0);
  ctx.drawImage(image("bg_pattern.png"), 0, 0, 1600, 900);
  ctx.drawImage(image("weave.png"), 748, 365, 649, 163);
  for (const firefly of fireflies) firefly.draw(ctx);
  ctx.drawImage(image("weave_front.png"), 748, 365, 649, 163);
  glow.apply(ctx);
}

export function StartScreen({ snap, scale }: { snap: Snapshot; scale: number }) {
  const [credits, setCredits] = useState(false);
  const [hover, setHover] = useState(false);
  const open = snap.open;
  const expanded = !open || hover;
  const players = session.lobbyPlayers();
  const playRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    if (!open) playRef.current?.focus();
    else playRef.current?.blur();
  }, [open]);

  useEffect(() => {
    if (!credits) return;
    const close = () => setCredits(false);
    const timer = setTimeout(() => {
      addEventListener("keydown", close, { once: true });
      addEventListener("pointerdown", close, { once: true });
    });
    return () => {
      clearTimeout(timer);
      removeEventListener("keydown", close);
      removeEventListener("pointerdown", close);
    };
  }, [credits]);

  return (
    <>
      <StageCanvas scale={scale} draw={drawBackground} />
      {open && <div className="blur" />}
      <div className="ui">
        <div className="menu-row">
          <div className="menu-buttons" onMouseEnter={() => setHover(true)} onMouseLeave={() => setHover(false)}>
            <Button
              ref={playRef}
              accent
              icon="play-circle.svg"
              label={expanded ? "PLAY" : ""}
              onClick={() => session.toggleLobby()}
            />
            <Button icon="info.svg" label={expanded ? "CREDITS" : ""} onClick={() => setCredits(true)} />
          </div>
          {open && <div className="vseparator" />}
          {open && players.length === 0 && <div className="empty-lobby">Waiting for players...</div>}
          {open && players.length > 0 && (
            <div className="player-list">
              {players.map((p) => (
                <LobbyPlayer key={p.slot} player={p} />
              ))}
            </div>
          )}
        </div>

        {open && (
          <>
            <div className="memories">
              Ready to <b>weave</b> some memories?
            </div>
            <Button className="start-button" label="START GAME" onClick={() => session.startGame()} />
            <Instructions />
          </>
        )}
        {snap.noPlayers > 0 && <NoPlayers key={snap.noPlayers} />}
      </div>
      {credits && <Credits />}
    </>
  );
}

function LobbyPlayer({ player }: { player: ReturnType<typeof session.lobbyPlayers>[number] }) {
  const arrow = useRef<HTMLDivElement>(null);
  // StartScreen._PhysicsProcess: the arrow turns at 200°/s with the player's keys.
  useEffect(() => {
    let rotation = 0;
    return onFrame((dt) => {
      const input = session.pressed(player.slot);
      if (input.r) rotation += 200 * dt;
      if (input.l) rotation -= 200 * dt;
      if (arrow.current) arrow.current.style.transform = `rotate(${rotation}deg)`;
    });
  }, [player.slot]);
  const [left, right] = KEY_PAIRS[player.slot].map(keyLabel);
  return (
    <div className="lobby-player" style={{ color: player.color }}>
      <div ref={arrow} className="lobby-arrow">
        <Icon name="arrow.svg" size={38} />
      </div>
      <div className="bindings">
        <div className="binding">
          <KeyIcon label={left} tinted />
        </div>
        <div className="binding right">
          <KeyIcon label={right} tinted />
        </div>
      </div>
    </div>
  );
}

/** The instructions panel. The original also had gamepad and phone sections. */
function Instructions() {
  const next = session.nextPair();
  return (
    <div className="instructions">
      <div className="section keyboard">
        <Icon name="keyboard.svg" size={64} className="section-icon" />
        <div className="label join">
          <b>JOIN: </b> PRESS
        </div>
        <div className="join-keys">
          {next ? (
            <>
              <KeyIcon label={next[0]} />
              <span> + </span>
              <KeyIcon label={next[1]} />
            </>
          ) : (
            <span>-</span>
          )}
        </div>
        <div className="label leave">
          <b>LEAVE: </b>HOLD YOUR KEYS
        </div>
      </div>
    </div>
  );
}

function NoPlayers() {
  const [visible, setVisible] = useState(true);
  useEffect(() => {
    const timer = setTimeout(() => setVisible(false), 2000);
    return () => clearTimeout(timer);
  }, []);
  return visible ? <div className="no-players">Can't start with no players</div> : null;
}

function Credits() {
  return (
    <div className="credits">
      <div>
        <b>Game by:</b>
        <br />
        Elias Carlson
        <br />
        Christos Vazouras
        <br />
        Levente Varga
        <br />
        Erik Wessman
        <br />
        Theo Wiik
        <br />
        Krystyna Ziobro
        <br />
        <br />
        <b>Networking</b>
        <br />
        Erik Wessman
        <br />
        <br />
        <b>Theme &amp; UI</b>
        <br />
        Elias Carlson
        <br />
        Krystyna Ziobro
        <br />
        <br />
        <b>Music</b>
        <br />
        Theo Wiik
        <br />
        <br />
        <b>Icons</b>
        <br />
        Icons8
      </div>
    </div>
  );
}
