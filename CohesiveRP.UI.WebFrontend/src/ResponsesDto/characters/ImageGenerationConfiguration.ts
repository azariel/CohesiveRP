import type { illustrationMapOutfits } from "./illustrationMapOutfits";

interface ImageGenerationConfiguration {
    illustratorTag?: string | null;
    illustrationMapOutfits?: illustrationMapOutfits[] | null;
}

export type {
    ImageGenerationConfiguration
};