import { useSyncExternalStore } from "react";
import { session, type Snapshot } from "../session";
import { Game } from "./Game";
import { Splash } from "./Splash";
import { StartScreen } from "./StartScreen";
import { useStageScale } from "./useStage";

export function useSnapshot() {
  return useSyncExternalStore(session.subscribe, () => session.snapshot);
}

/** The 1600×900 stage, scaled to fit the window. */
export function App() {
  const snap: Snapshot = useSnapshot();
  const scale = useStageScale();
  return (
    <div className="viewport">
      <div className="stage" style={{ transform: `translate(-50%, -50%) scale(${scale})` }}>
        {snap.phase === "start" && <StartScreen snap={snap} scale={scale} />}
        {snap.phase === "splash" && <Splash snap={snap} />}
        {(snap.phase === "game" || snap.phase === "over") && <Game snap={snap} scale={scale} />}
      </div>
    </div>
  );
}
