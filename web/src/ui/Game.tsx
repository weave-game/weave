import { useEffect, useRef, useState } from "react";
import { session, type Snapshot } from "../session";
import { Glow } from "../view";
import { Button } from "./Button";
import { StageCanvas } from "./StageCanvas";

const glow = new Glow();

function draw(ctx: CanvasRenderingContext2D, unit: number) {
  const view = session.view;
  if (!view) {
    ctx.setTransform(1, 0, 0, 1, 0, 0);
    ctx.fillStyle = "#111312";
    ctx.fillRect(0, 0, ctx.canvas.width, ctx.canvas.height);
    return;
  }
  view.render(ctx, unit, session.alpha);
  glow.apply(ctx);
}

export function Game({ snap, scale }: { snap: Snapshot; scale: number }) {
  return (
    <>
      <StageCanvas scale={scale} draw={draw} />
      {snap.phase === "over" && <GameOver snap={snap} />}
    </>
  );
}

/** UI/GameOverOverlay.tscn. */
function GameOver({ snap }: { snap: Snapshot }) {
  const retry = useRef<HTMLButtonElement>(null);
  const [team, setTeam] = useState(snap.team);
  useEffect(() => setTeam(snap.team), [snap.team]);
  useEffect(() => retry.current?.focus(), []);

  return (
    <div className="game-over">
      <div className="game-over-box">
        <div className="game-over-label">GAME OVER</div>
        <form
          className="name-row"
          onSubmit={(e) => {
            e.preventDefault();
            session.setTeam(team);
            session.saveScore();
          }}
        >
          <input
            className="line-edit"
            placeholder="Team Name"
            value={team}
            onChange={(e) => setTeam(e.target.value)}
            maxLength={40}
          />
          <Button label="Save" type="submit" className="save-button" />
        </form>
        <div className="game-over-gap" />
        <div className="game-over-buttons">
          <Button ref={retry} icon="play-circle.svg" label="Retry" onClick={() => session.retry()} />
          <Button icon="arrow-right-on-rectangle.svg" label="Menu" onClick={() => session.menu()} />
        </div>
      </div>
      {snap.saved > 0 && (
        <div key={snap.saved} className="saved-label">
          Saved
        </div>
      )}
    </div>
  );
}
