interface CharacterStatusEffect {
  content?: string;
  expiresAt?: string;// "PERMANENT", "SEMI-PERMANENT", "UNKNOWN" or an exact datetime like "4 October 1995 18:00:00"
}

export type {
    CharacterStatusEffect
};