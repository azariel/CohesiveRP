export interface Persona {
  personaId: string;
  name: string;
  description: string;
  isDefault: boolean;
  lastActivityAtUtc: Date;
}