using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.Injectors
{
    internal class DynamicCharacterAvatarPromptIllustrationCompletionPresetInjector : ICompletionPresetInjector
    {
        internal static ChatCompletionPresetsDbModel InjectPreset()
        {
            return new ChatCompletionPresetsDbModel
                {
                    Name = "Default-Illustrator-Character-Avatar-Prompt-Generator-Preset",
                    ChatCompletionPresetId = StorageConstants.DEFAULT_ILLUSTRATION_PROMPT_INJECTION_FOR_CHARACTER_AVATAR_COMPLETION_PRESET,
                    CreatedAtUtc = DateTime.UtcNow,
                    Format = new GlobalPromptContextFormat()
                    {
                        MaxTokensToGenerate = 1024,
                        OrderedElementsWithinTheGlobalPromptContext = new List<PromptContextFormatElement>
                        {
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.Directive,
                                Name = "Directive - Task",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "<task>\r\n  You are an helpful assistant, trained in analyzing and categorizing characters in books, novels, fanfictions and stories. You are also an expert of ComfyUI and image generation using LLM models (FLUX, SDXL, etc). You are tasked with providing a structured prompt for ComfyUI to generate a specific character. You must start by understanding the character from the information provided.\r\n  Your reply must be structured into a strict JSON representing the following serialized c# model:\r\n  ```\r\n  public class IllustratorGenerationContents\r\n  {\r\n    [JsonPropertyName(\"contents\")]\r\n    public List<IllustratorGenerationContent> Contents { get; set; }\r\n  }\r\n  ```\r\n  \r\n  Here's a reference to the inner types:\r\n  ```\r\n  public class IllustratorGenerationContent\r\n  {\r\n      [JsonPropertyName(\"content\")]\r\n      public string Content { get; set; }\r\n  \r\n      [JsonPropertyName(\"outfit\")]\r\n      public ClothingStateOfDress Outfit { get; set; }\r\n  }\r\n  \r\n  public enum ClothingStateOfDress\r\n  {\r\n      Unknown = 0,\r\n      Naked = 1,\r\n      Underwear = 2,\r\n      Clothed = 3,\r\n  }\r\n  ```\r\n  \r\n  You must generate a response embedding an array of Contents describing the character's Clothed, Underwear and Naked states.\r\n  The model used is 'cyberrealisticPony_v170'.\r\n</task>\n",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.Directive,
                                Name = "Directive - ContentFormat",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "<contentFormat>\r\n  To generate a prompt content that will generate good images, you must follow the following format:\r\n  ```\r\n  race(Ignore this field when 'human' or when unknown), height(choose between 'tiny', 'petite', 'short', 'average', 'tall'), bodyType (One to three words comma separated such as 'lean', 'athletic', 'overweight', 'chubby', 'muscular', 'bulky', 'slim', etc), penisSize (when applicable), breastsSize(When male, ignore this field. Select ONE SIZE LOWER than the character real breasts size. For example, if the character's breasts are 'small', then set it to 'very small', 'average' become 'small', 'large' become 'average', 'extra large' become 'large' and 'flat' remain 'flat'), areolasSize(When male or when generating non-naked content, ignore this field.), areolasDetails(When male or when generating non-naked content, ignore this field.),eyeColor(One color such as 'purple eyes', 'red eyes', 'brown eyes', 'grey eyes', 'green eyes', 'blue eyes', 'yellow eyes', etc), skinColor (Two to three words such as 'white skin', 'brown skin', 'dark skin', 'dark purple skin', etc), hairColor(Two or three words such as 'purple hair', 'red hair', 'brown hair', 'white hair', 'green hair', 'blue hair', etc. use simple color like light brown instead of copper, light gray instead of silver, blonde instead of gold, etc), hairLength('bald', 'short hair', 'average hair length', 'long hair', 'very long hair'),hairStyle(Two to three words for the length, two to three words for the style, two to three words for how it's wore. e.g.: 'straight hair, wavy hair, hair worn loose'), earShape (one word such as 'pointy ears', 'fox ears', 'cat ears', etc. IMPORTANT note, when the character is human, disregard this field), teethDetails (Ignore this field when their teeth are normal. Otherwise, use one or two words to describe them e.g. \"long fangs\", \"yellow teeth\", \"white teeth\", etc.), nailsColor (Ignore this field when their nails color is normal. Otherwise, use two words to describe the color e.g. \"pink nails\".), nailsDetails (Ignore this field when their nails are normal. Otherwise, use one to three words to describe them e.g. \"long nails\".), Lips (Ignore this field when their lips are normal. Otherwise, use two words to describe them e.g. \"big lips\".), eyebrows (Ignore this field when their eyebrows are normal. Otherwise, use two words to describe them e.g. \"fine eyebrows\".), clothesPreferences (simple keywords, list what is visible e.g.: 'brown hoodie, blue jeans, plated green skirt, pink sneakers', 'white lace bra, white frilly panties', etc).\r\n  ```\r\n</contentFormat>\n",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.Directive,
                                Name = "Directive - example_of_good_replies",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "<example_of_good_replies>\r\nWhen clothed:\r\n```human,short,athletic,lean,slim,small breasts,green eyes,brown skin,dark brown hair,long hair,straight hair,red glasses,nose piercing,ear piercings,eyebrow piercing,natural makeup,white shirt,brown cardigan,black jeans,white socks,pink running shoes```\r\n```elf,tall,lean,slim,slender,long legs,large breasts,blue eyes,pale skin,blonde hair,long hair,wavy hair,pointy ears,no makeup,blue elegant robe,golden accent on robe,black elegant shoes```\r\n```catgirl,tiny,chubby,overweight,short legs,large breasts,brown eyes,white skin,brown hair,short hair,hime style hair,cat ears,purple hoodie,dark blue jeans,brown running shoes```\r\n```human,very tall,muscular,bulky,athletic,long legs,average breasts,amber eyes,dark black skin,dark hair,long hair,dreadlocks,simple white t-shirt,blue cargo shorts,black training shoes```\r\nWhen underwear:\r\n```human,short,athletic,lean,slim,small breasts,green eyes,brown skin,dark brown hair,long hair,straight hair,red glasses,nose piercing,ear piercings,eyebrow piercing,natural makeup,white bra,white panties,white socks```\r\n```elf,tall,lean,slim,slender,long legs,large breasts,blue eyes,pale skin,blonde hair,long hair,wavy hair,pointy ears,no makeup,blue elegant robe,golden accent on robe,black elegant shoes```\r\n```catgirl,tiny,chubby,overweight,short legs,large breasts,brown eyes,white skin,brown hair,short hair,hime style hair,cat ears,purple hoodie,dark blue jeans,brown running shoes```\r\n```human,very tall,muscular,bulky,athletic,long legs,average breasts,amber eyes,dark black skin,dark hair,long hair,dreadlocks,simple white t-shirt,blue cargo shorts,black training shoes```\r\nWhen naked:\r\n```human,short,athletic,lean,slim,small breasts,large areolas,oval areolas,green eyes,brown skin,dark brown hair,long hair,straight hair,red glasses,nose piercing,ear piercings,eyebrow piercing,natural makeup,naked```\r\n```elf,tall,lean,slim,slender,long legs,large breasts,enormous areolas,normal areolas,blue eyes,pale skin,blonde hair,long hair,wavy hair,pointy ears,no makeup,naked```\r\n```catgirl,tiny,chubby,overweight,short legs,large breasts,average areolas,puffy areolas,brown eyes,white skin,brown hair,short hair,hime style hair,cat ears,naked```\r\n```human,very tall,muscular,bulky,athletic,long legs,average breasts,normal areolas,asymmetrical areolas,amber eyes,dark black skin,dark hair,long hair,dreadlocks,naked```\r\n</example_of_good_replies>\n",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.IllustrationPromptInjectionForCharacterAvatar,
                                Name = "PromptInjectionForCharacterAvatar",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "<character>\r\n  <race>\r\n    {{character_race}}\r\n  </race>\r\n  <bodyType>\r\n    {{character_bodyType}}\r\n  </bodyType>\r\n  <height>\r\n    {{character_height}}\r\n  </height>\r\n  <eyeColor>\r\n    {{character_eyeColor}}\r\n  </eyeColor>\r\n  <skinColor>\r\n    {{character_skinColor}}\r\n  </skinColor>\r\n  <teethDetails>\r\n    {{character_teethDetails}}\r\n  </teethDetails>\r\n  <nailsColor>\r\n    {{character_nailsColor}}\r\n  </nailsColor>\r\n  <nailsDetails>\r\n    {{character_nailsDetails}}\r\n  </nailsDetails>\r\n  <lipsDetails>\r\n    {{character_lipsDetails}}\r\n  </lipsDetails>\r\n  <eyebrows>\r\n    {{character_eyebrows}}\r\n  </eyebrows>\r\n  <hairColor>\r\n    {{character_hairColor}}\r\n  </hairColor>\r\n  <hairStyle>\r\n    {{character_hairStyle}}\r\n  </hairStyle>\r\n  <earShape>\r\n    {{character_earShape}}\r\n  </earShape>\r\n  <breastsSize>\r\n    {{character_breastsSize}}\r\n  </breastsSize>\r\n  <areolasSize>\r\n    {{character_areolasSize}}\r\n  </areolasSize>\r\n  <areolasDetails>\r\n    {{character_areolasDetails}}\r\n  </areolasDetails>\r\n  <penisSize>\r\n    {{character_penisSize}}\r\n  </penisSize>\r\n  <clothesPreferences>\r\n    {{character_clothesPreferences}}\r\n  </clothesPreferences>\r\n</character>\r\n\r\n<behavioral_instruction>\r\n  <important_reminders>\r\n    Your objective is to ensure a consistent, coherent and logical description of the character '{{character_name}}', providing complete details even when specifics are not explicitly stated.\r\n  Your response must ONLY contain the strict JSON representing the c# model.\r\n  Prefer keywords to long description. Use simple keyword and AVOID adjectives on clothes as this will serve for as prompt injection in comfy UI image generation.\r\n  </important_reminders>\r\n<behavioral_instruction>\n",
                                }
                            }
                        }
                    }
                };
        }
    }
}
