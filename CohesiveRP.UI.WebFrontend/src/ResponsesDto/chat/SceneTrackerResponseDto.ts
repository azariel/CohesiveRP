import type { ServerApiResponseDto } from "../ServerApiResponseDto";
import type { SceneTracker } from "./BusinessObjects/SceneTracker";

interface SceneTrackerResponseDto extends ServerApiResponseDto  {
    sceneTracker: SceneTracker | null;
}

export type {
    SceneTrackerResponseDto
};