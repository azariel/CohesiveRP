import type { ServerApiResponseDto } from "../ServerApiResponseDto";
import type { Persona } from "./BusinessObjects/Persona";

export interface PersonaResponseDto extends ServerApiResponseDto {
  persona: Persona;
}