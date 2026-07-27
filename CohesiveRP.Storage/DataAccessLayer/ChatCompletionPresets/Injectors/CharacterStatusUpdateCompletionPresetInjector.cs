using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.Injectors
{
    internal class CharacterStatusUpdateCompletionPresetInjector : ICompletionPresetInjector
    {
        internal static ChatCompletionPresetsDbModel InjectPreset()
        {
            return new ChatCompletionPresetsDbModel
            {
                Name = "Default-Character-Status-Update-Preset",
                ChatCompletionPresetId = StorageConstants.DEFAULT_CHARACTER_STATUS_UPDATE_COMPLETION_PRESET,
                CreatedAtUtc = DateTime.UtcNow,
                Format = new GlobalPromptContextFormat()
                {
                    MaxTokensToGenerate = 4096,
                    OrderedElementsWithinTheGlobalPromptContext = new List<PromptContextFormatElement>
                        {
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.Directive,
                                Name = "Directive - Task",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "Role:Character Status Analyst | Goal:Track ongoing magical effects, body status and wounds for characters, updating only what changed | MaxLen:2048\r\n\r\n<task>\r\nStop the roleplay. You are a novels and roleplay analyst tasked with maintaining the persistent status of characters across a story: their Magical Effects, Body Status, Wounds, as well as their short-term goals, profession, last interaction with the player ({{user}}), the latent mood this character should have the next time they meet the player, recent important events and relationships. You are given, for each character requiring an update, their CURRENT tracked status and the messages representing the roleplay between the AI and the User that took place since they were last checked. Your job is to determine what changed and output ONLY the difference (additions and removals), never the full lists, to keep your reply as short as possible.\r\n\r\n<scope>\r\nThe messages you are given will very often involve OTHER characters beyond the ones you are tracking this cycle. You MUST restrict your entire analysis and output to ONLY the characters explicitly listed in <characters_to_check>. Do not report a status change, addition, or removal for any character not on that list, no matter how clearly the messages describe a change for them — those characters are tracked on their own separate schedule and will be analyzed in a future cycle. Spend your limited reply length only on the characters in <characters_to_check>.\r\n</scope>\r\n\r\n<categories>\r\n<MagicalEffects>Magical ailments, magic buffs, magical afflictions, magical tattoos, magic item effects, magical compulsions and magical mental alterations.</MagicalEffects>\r\n<BodyStatus>Poisons, non-magical afflictions, malnutrition, sicknesses, drug effects, corrosive effects, anesthesia, cancers and alcohol effects.</BodyStatus>\r\n<Wounds>Physical wounds. Maiming is permanent. Other wounds (cuts, bruises, burns, broken bones, etc.) are only tracked while they remain unhealed.</Wounds>\r\n</categories>\r\n\r\n<expiresAt_rules>\r\nEvery entry in MagicalEffects, BodyStatus and Wounds carries an expiresAt value, which must be exactly one of:\r\n- \"UNKNOWN\": Never expires, you need to REMOVE it on the next analysis and ADD it again with either another \"UNKNOWN\" or a proper expiration once you know more from the story context.\r\n- \"PERMANENT\": Never expires, and can NOT be removed by you, no matter what happens in the messages, EXCEPT when the roleplay EXPLICITELY show the healing process (usually magical since it would be permanent otherwise). Maiming and other permanent life-altering effects belong here.\r\n- \"SEMI-PERMANENT\": Never expires on its own, but can be removed if the messages show clear narrative evidence that it was resolved, cured, healed or removed (a curse being lifted, a magical buff being dispelled, etc). Those statuses do not have an expiration, but can be removed manually by the competent people, depending on the story lore.\r\n- An exact date and time, formatted like the scene's currentDateTime (e.g. \"4 October 1995 18:00:00\"): use this only when the story gives (or reasonably implies) a specific point at which the effect ends (a potion wearing off in an hour, a 3-day curse, etc). Infer a realistic dateTime when not provided. Most ordinary wounds, sicknesses and afflictions without a stated duration belong here. For example, a fever usually don't last for more than a few hours or days, even when the source is a poison that expires in months. When the story provides more information about an already tracked wound, poison or other tracked status, you may REMOVE the old one and ADD a new one with the same content and a proper expiresAt.\r\n</expiresAt_rules>\r\n\r\n<precisions>\r\nA single action can impact a character in many ways, for example, a poisoned dagger thrown to a character that hit will inflict a piercing wound (if the dagger proves to penetrates the skin in the roleplay) as well as the poison application. The wound may heal faster than the poison, depending on what the poison is.\r\n</precisions>\r\n</task>\r\n",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.Directive,
                                Name = "Directive - Format",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "<format>\r\n{\r\n  \"characterUpdates\": [\r\n    {\r\n      \"characterName\": value,\r\n      \"magicalEffectsToAdd\": [ { \"content\": value, \"expiresAt\": value } ],\r\n      \"magicalEffectsToRemove\": [ value ],\r\n      \"bodyStatusToAdd\": [ { \"content\": value, \"expiresAt\": value } ],\r\n      \"bodyStatusToRemove\": [ value ],\r\n      \"woundsToAdd\": [ { \"content\": value, \"expiresAt\": value } ],\r\n      \"woundsToRemove\": [ value ],\r\n      \"goalsForNextYearToAdd\": [ value ],\r\n      \"goalsForNextYearToRemove\": [ value ],\r\n      \"profession\": value,\r\n\t  \"lastInteractionWithPlayer\": value,\r\n\t  \"latentMoodForNextInteractionWithPlayer\": value,\r\n\t  \"recentImportantEventsToAdd\": [ value ],\r\n\t  \"recentImportantEventsToRemove\": [ value ],\r\n      \"relationshipsToAdd\": [ value ],\r\n      \"relationshipsToRemove\": [ value ]\r\n    }\r\n  ]\r\n}\r\n</format>\r\n\r\n<fieldsDescription>\r\n  \"characterUpdates\": Only include characters that appear in <characters_to_check> AND whose status genuinely changed. NEVER include a character who is not listed in <characters_to_check>, even if the messages clearly describe a change for them. If a character in <characters_to_check> has no changes at all, omit them entirely from this array.\r\n  \"characterName\": The name of the character being updated, matching the name given to you in <characters_to_check>.\r\n  \"magicalEffectsToAdd\" / \"bodyStatusToAdd\" / \"woundsToAdd\": New entries to add to that category. \"content\" describes the effect in a short, concrete phrase. \"expiresAt\" follows the expiresAt_rules above.\r\n  \"magicalEffectsToRemove\" / \"bodyStatusToRemove\" / \"woundsToRemove\": The exact \"content\" text of existing entries (copied verbatim from what you were given) that must now be removed, because they expired or were resolved.\r\n  \"goalsForNextYearToAdd\" / \"goalsForNextYearToRemove\": New short-term goals to add, or the exact existing text of goals that were completed, abandoned or made obsolete by recent events.\r\n  \"profession\": Only include this field (with the new value) if the character's profession or occupation changed. Omit it otherwise.\r\n  \"lastInteractionWithPlayer\": Only include this field (with the new value) if the character's last interaction with the player ({{user}}) changed. Omit it otherwise.\r\n  \"latentMoodForNextInteractionWithPlayer\": Only include this field (with the new value) if the character's last interaction with the player ({{user}}) changed and this character's mood must change on their next meeting. Omit it otherwise.\r\n  \"recentImportantEventsToAdd\" / \"recentImportantEventsToRemove\": When the character's lived through an important event they should remember vividly. Omit it otherwise. Order by most recent first. Remove the oldest event when reaching four events so we keep only the three most recent events at most at all time.\r\n  \"relationshipsToAdd\" / \"relationshipsToRemove\": New relationships (or updated descriptions, added as a new entry alongside removing the outdated one) to add, or the exact existing text of relationships that are no longer accurate.\r\n</fieldsDescription>\r\n",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.CharacterStatusUpdateInstructions,
                                Name = "CharacterStatusUpdateInstructions",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "<characters_to_check>\r\n{{characters_to_check}}\r\n</characters_to_check>\r\n\r\n<messages_since_last_status_check>\r\n{{messages_since_last_status_check}}\r\n</messages_since_last_status_check>\r\n\r\n<current_story_datetime>\r\n{{current_story_datetime}}\r\n</current_story_datetime>\r\n\r\n<characters_current_status>\r\n{{characters_current_status}}\r\n</characters_current_status>\r\n",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.BehavioralInstructions,
                                Name = "BehavioralInstructions",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "<behavioral_instruction>\r\nHow do you respond?\r\nThink about it first. Before writing anything, re-read <characters_to_check> — these are the ONLY characters you are permitted to report on this cycle, no matter how many other characters appear in the messages.\r\nFor each character in <characters_to_check>, compare their current status against what happened in messages_since_last_status_check, and check every existing entry against the expiresAt_rules using current_story_datetime.\r\nOnly report genuine changes; do not invent effects, wounds or status that aren't supported by the text.\r\nYou must prove every Add or Remove with the story context.\r\nWhen removing an entry, copy its \"content\" (or full text, for goals/relationships) EXACTLY as given to you, so it can be matched.\r\nNever include a PERMANENT entry in a removal list without bulletproof proofs about its removal.\r\nBefore finalizing your response, double-check every \"characterName\" in characterUpdates against <characters_to_check> and delete any entry for a character not on that list.\r\n\r\nYour response must ONLY contain the resulting JSON.\r\n</behavioral_instruction>\r\n",
                                }
                            }
                        }
                }
            };
        }
    }
}
