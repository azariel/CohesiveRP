import { PATHFINDER_ATTR_KEYS, PATHFINDER_SKILL_KEYS } from "./UseCharacterSheetFields";
import type { UseCharacterSheetFields } from "./UseCharacterSheetFields";
import {
  Section, Field, SheetInput, SheetTextarea, SheetSelect, SheetNumberInput,
  ArrayField, StatusEffectListField,
  GENDER_OPTIONS, AGE_GROUP_OPTIONS, GENITALS_OPTIONS, BREASTS_SIZE_OPTIONS,
} from "./CharacterSheetFormPrimitives";
import styles from "./CharacterSheetForm.module.css";

type Props = Pick<UseCharacterSheetFields, "sheet" | "updateField" | "getAttr" | "setAttr" | "getSkill" | "setSkill">;

export default function CharacterSheetFormBody({ sheet, updateField, getAttr, setAttr, getSkill, setSkill }: Props) {
  return (
    <>
      {/* ── Identity ── */}
      <Section title="Identity">
        <div className={styles.twoCol}>
          <Field label="First Name"><SheetInput value={sheet.firstName ?? ""} onChange={(v) => updateField("firstName", v)} placeholder="Daphne" /></Field>
          <Field label="Last Name"><SheetInput value={sheet.lastName ?? ""} onChange={(v) => updateField("lastName", v)} placeholder="Greengrass" /></Field>
        </div>
        <div className={styles.threeCol}>
          <Field label="Birthday"><SheetInput value={sheet.birthday ?? ""} onChange={(v) => updateField("birthday", v)} placeholder="01 March 1990" /></Field>
          <Field label="Age Group"><SheetSelect value={sheet.ageGroup ?? ""} onChange={(v) => updateField("ageGroup", v)} options={AGE_GROUP_OPTIONS} /></Field>
          <Field label="Age Group Appearance"><SheetSelect value={sheet.ageGroupAppearance ?? ""} onChange={(v) => updateField("ageGroupAppearance", v)} options={AGE_GROUP_OPTIONS} /></Field>
        </div>
        <div className={styles.threeCol}>
          <Field label="Race / Species"><SheetInput value={sheet.race ?? ""} onChange={(v) => updateField("race", v)} placeholder="Human (Pure-blood witch)" /></Field>
          <Field label="Gender"><SheetSelect value={sheet.gender ?? ""} onChange={(v) => updateField("gender", v)} options={GENDER_OPTIONS} /></Field>
          <Field label="Sexuality"><SheetInput value={sheet.sexuality ?? ""} onChange={(v) => updateField("sexuality", v)} placeholder="Heterosexual" /></Field>
        </div>
        <Field label="Profession"><SheetInput value={sheet.profession ?? ""} onChange={(v) => updateField("profession", v)} placeholder="Hogwarts student (Slytherin)" /></Field>
      </Section>

      {/* ── Appearance ── */}
      <Section title="Appearance">
        <div className={styles.oneCol}>
          <Field label="Body Type"><SheetInput value={sheet.bodyType ?? ""} onChange={(v) => updateField("bodyType", v)} placeholder="Lean and slender" /></Field>
        </div>
        <div className={styles.threeCol}>
          <Field label="Height"><SheetInput value={sheet.height ?? ""} onChange={(v) => updateField("height", v)} placeholder="5'4 (162 cm)" /></Field>
          <Field label="Eye Color"><SheetInput value={sheet.eyeColor ?? ""} onChange={(v) => updateField("eyeColor", v)} placeholder="Emerald green" /></Field>
          <Field label="Skin Color"><SheetInput value={sheet.skinColor ?? ""} onChange={(v) => updateField("skinColor", v)} placeholder="Very pale" /></Field>
        </div>
        <div className={styles.oneCol}>
          <Field label="Hair Color"><SheetInput value={sheet.hairColor ?? ""} onChange={(v) => updateField("hairColor", v)} placeholder="Platinum blonde" /></Field>
        </div>
        <div className={styles.oneCol}>
          <Field label="Hair Style"><SheetInput value={sheet.hairStyle ?? ""} onChange={(v) => updateField("hairStyle", v)} placeholder="Long, straight, slicked back" /></Field>
        </div>
        <div className={styles.threeCol}>
          <Field label="Teeth Details"><SheetInput value={sheet.teethDetails ?? ""} onChange={(v) => updateField("teethDetails", v)} placeholder="White" /></Field>
          <Field label="Lips Details"><SheetInput value={sheet.lipsDetails ?? ""} onChange={(v) => updateField("lipsDetails", v)} placeholder="Full, naturally pink" /></Field>
          <Field label="Eyebrows"><SheetInput value={sheet.eyebrows ?? ""} onChange={(v) => updateField("eyebrows", v)} placeholder="Thin, arched" /></Field>
        </div>
        <div className={styles.threeCol}>
          <Field label="Nails Color"><SheetInput value={sheet.nailsColor ?? ""} onChange={(v) => updateField("nailsColor", v)} placeholder="Pale pink" /></Field>
          <Field label="Nails Details"><SheetInput value={sheet.nailsDetails ?? ""} onChange={(v) => updateField("nailsDetails", v)} placeholder="Short, neatly filed" /></Field>
          <Field label="Ear Shape"><SheetInput value={sheet.earShape ?? ""} onChange={(v) => updateField("earShape", v)} placeholder="Normal" /></Field>
        </div>
        <div className={styles.threeCol}>
          <Field label="Breasts Size"><SheetSelect value={sheet.breastsSize ?? ""} onChange={(v) => updateField("breastsSize", v)} options={BREASTS_SIZE_OPTIONS} /></Field>
          <Field label="Genitals"><SheetSelect value={sheet.genitals ?? ""} onChange={(v) => updateField("genitals", v)} options={GENITALS_OPTIONS} /></Field>
          <Field label="Penis Size"><SheetInput value={sheet.penisSize ?? ""} onChange={(v) => updateField("penisSize", v)} placeholder="Average (5 inches)" /></Field>
        </div>
        <div className={styles.threeCol}>
          <Field label="Areolas Color"><SheetInput value={sheet.areolasColor ?? ""} onChange={(v) => updateField("areolasColor", v)} placeholder="pink" /></Field>
          <Field label="Areolas Size"><SheetInput value={sheet.areolasSize ?? ""} onChange={(v) => updateField("areolasSize", v)} /></Field>
          <Field label="Areolas Details"><SheetInput value={sheet.areolasDetails ?? ""} onChange={(v) => updateField("areolasDetails", v)} /></Field>
        </div>
        <div className={styles.oneCol}>
          <Field label="Attractiveness"><SheetInput value={sheet.attractiveness ?? ""} onChange={(v) => updateField("attractiveness", v)} placeholder="Very High" /></Field>
        </div>
        <Field label="Clothes Preference"><SheetTextarea value={sheet.clothesPreference ?? ""} onChange={(v) => updateField("clothesPreference", v)} minHeight="6em" /></Field>
      </Section>

      {/* ── Voice & Manner ── */}
      <Section title="Voice & Manner">
        <Field label="Speech Pattern"><SheetTextarea value={sheet.speechPattern ?? ""} onChange={(v) => updateField("speechPattern", v)} minHeight="6em" /></Field>
        <Field label="Speech Impairment"><SheetInput value={sheet.speechImpairment ?? ""} onChange={(v) => updateField("speechImpairment", v)} placeholder="None" /></Field>
        <Field label="Mannerisms"><SheetTextarea value={sheet.mannerisms ?? ""} onChange={(v) => updateField("mannerisms", v)} minHeight="5em" /></Field>
        <Field label="Social Anxiety"><SheetInput value={sheet.socialAnxiety ?? ""} onChange={(v) => updateField("socialAnxiety", v)} placeholder="None" /></Field>
      </Section>

      {/* ── Personality ── */}
      <Section title="Personality">
        <Field label="Behavior"><SheetTextarea value={sheet.behavior ?? ""} onChange={(v) => updateField("behavior", v)} minHeight="10em" /></Field>
        <ArrayField label="Personality Traits" value={sheet.personalityTraits ?? []} onChange={(v) => updateField("personalityTraits", v)} minHeight="10em" />
        <ArrayField label="Likes" value={sheet.likes ?? []} onChange={(v) => updateField("likes", v)} minHeight="10em" />
        <ArrayField label="Dislikes" value={sheet.dislikes ?? []} onChange={(v) => updateField("dislikes", v)} minHeight="10em" />
        <ArrayField label="Fears" value={sheet.fears ?? []} onChange={(v) => updateField("fears", v)} minHeight="10em" />
        <ArrayField label="Secrets" value={sheet.secrets ?? []} onChange={(v) => updateField("secrets", v)} minHeight="10em" />
        <ArrayField label="Skills" value={sheet.skills ?? []} onChange={(v) => updateField("skills", v)} minHeight="10em" />
        <ArrayField label="Weaknesses" value={sheet.weaknesses ?? []} onChange={(v) => updateField("weaknesses", v)} minHeight="10em" />
      </Section>

      {/* ── Background ── */}
      <Section title="Background & Social">
        <Field label="Reputation"><SheetTextarea value={sheet.reputation ?? ""} onChange={(v) => updateField("reputation", v)} minHeight="6em" /></Field>
        <ArrayField label="Relationships" value={sheet.relationships ?? []} onChange={(v) => updateField("relationships", v)} minHeight="10em" />
      </Section>

      {/* ── Combat ── */}
      <Section title="Combat">
        <Field label="Preferred Combat Style"><SheetTextarea value={sheet.preferredCombatStyle ?? ""} onChange={(v) => updateField("preferredCombatStyle", v)} minHeight="10em" /></Field>
        <Field label="Weapons Proficiency"><SheetInput value={sheet.weaponsProficiency ?? ""} onChange={(v) => updateField("weaponsProficiency", v)} placeholder="Wand magic" /></Field>
        <Field label="Combat Affinity — Attack"><SheetTextarea value={sheet.combatAffinityAttack ?? ""} onChange={(v) => updateField("combatAffinityAttack", v)} minHeight="8em" /></Field>
        <Field label="Combat Affinity — Defense"><SheetTextarea value={sheet.combatAffinityDefense ?? ""} onChange={(v) => updateField("combatAffinityDefense", v)} minHeight="8em" /></Field>
      </Section>

      {/* ── Goals ── */}
      <Section title="Goals">
        <ArrayField label="Goals for Next Year" value={sheet.goalsForNextYear ?? []} onChange={(v) => updateField("goalsForNextYear", v)} minHeight="12em" />
        <ArrayField label="Long-Term Goals" value={sheet.longTermGoals ?? []} onChange={(v) => updateField("longTermGoals", v)} minHeight="10em" />
      </Section>

      {/* ── Adult / Private ── */}
      <Section title="Private">
        <ArrayField label="Kinks" value={sheet.kinks ?? []} onChange={(v) => updateField("kinks", v)} minHeight="20em" />
        <ArrayField label="Secret Kinks" value={sheet.secretKinks ?? []} onChange={(v) => updateField("secretKinks", v)} minHeight="20em" />
      </Section>

      {/* ── Status Effects ── */}
      <Section title="Status Effects">
        <StatusEffectListField label="Magical Effects" value={sheet.magicalEffects ?? []} onChange={(v) => updateField("magicalEffects", v)} />
        <StatusEffectListField label="Body Status" value={sheet.bodyStatus ?? []} onChange={(v) => updateField("bodyStatus", v)} />
        <StatusEffectListField label="Wounds" value={sheet.wounds ?? []} onChange={(v) => updateField("wounds", v)} />
      </Section>

      {/* ── Interaction State (new fields on CharacterSheet) ── */}
      <Section title="Interaction State">
        <Field label="Last Interaction With Player"><SheetTextarea value={sheet.lastInteractionWithPlayer ?? ""} onChange={(v) => updateField("lastInteractionWithPlayer", v)} minHeight="6em" /></Field>
        <Field label="Latent Mood for Next Interaction"><SheetTextarea value={sheet.latentMoodForNextInteractionWithPlayer ?? ""} onChange={(v) => updateField("latentMoodForNextInteractionWithPlayer", v)} minHeight="6em" /></Field>
        <ArrayField label="Recent Important Events" value={sheet.recentImportantEvents ?? []} onChange={(v) => updateField("recentImportantEvents", v)} minHeight="8em" />
        <Field label="Arousal (0–100)"><SheetNumberInput value={sheet.arousal ?? 0} onChange={(v) => updateField("arousal", v)} min={0} max={100} /></Field>
      </Section>

      {/* ── Pathfinder Attributes ── */}
      <Section title="Pathfinder Attributes">
        <div className={styles.statGrid}>
          {PATHFINDER_ATTR_KEYS.map((key) => (
            <div key={key} className={styles.statCell}>
              <label className={styles.statLabel}>{key}</label>
              <input
                type="number"
                className={styles.statInput}
                value={getAttr(key)}
                min={0}
                max={99}
                onChange={(e) => setAttr(key, Number(e.target.value))}
              />
            </div>
          ))}
        </div>
      </Section>

      {/* ── Pathfinder Skills ── */}
      <Section title="Pathfinder Skills">
        <div className={styles.statGrid}>
          {PATHFINDER_SKILL_KEYS.map((key) => (
            <div key={key} className={styles.statCell}>
              <label className={styles.statLabel}>{key}</label>
              <input
                type="number"
                className={styles.statInput}
                value={getSkill(key)}
                min={0}
                max={99}
                onChange={(e) => setSkill(key, Number(e.target.value))}
              />
            </div>
          ))}
        </div>
      </Section>
    </>
  );
}