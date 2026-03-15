import type { ServerApiResponseDto } from "../ServerApiResponseDto";
import type { Persona } from "./BusinessObjects/Persona";

export interface PersonasResponseDto extends ServerApiResponseDto {
  personas: Persona[];
}