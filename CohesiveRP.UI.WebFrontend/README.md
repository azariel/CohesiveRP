# React + TypeScript + Vite

This template provides a minimal setup to get React working in Vite with HMR and some ESLint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Babel](https://babeljs.io/) (or [oxc](https://oxc.rs) when used in [rolldown-vite](https://vite.dev/guide/rolldown)) for Fast Refresh
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/) for Fast Refresh

## React Compiler

The React Compiler is not enabled on this template because of its impact on dev & build performances. To add it, see [this documentation](https://react.dev/learn/react-compiler/installation).

## Expanding the ESLint configuration

If you are developing a production application, we recommend updating the configuration to enable type-aware lint rules:

```js
export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // Other configs...

      // Remove tseslint.configs.recommended and replace with this
      tseslint.configs.recommendedTypeChecked,
      // Alternatively, use this for stricter rules
      tseslint.configs.strictTypeChecked,
      // Optionally, add this for stylistic rules
      tseslint.configs.stylisticTypeChecked,

      // Other configs...
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
])
```

You can also install [eslint-plugin-react-x](https://github.com/Rel1cx/eslint-react/tree/main/packages/plugins/eslint-plugin-react-x) and [eslint-plugin-react-dom](https://github.com/Rel1cx/eslint-react/tree/main/packages/plugins/eslint-plugin-react-dom) for React-specific lint rules:

```js
// eslint.config.js
import reactX from 'eslint-plugin-react-x'
import reactDom from 'eslint-plugin-react-dom'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // Other configs...
      // Enable lint rules for React
      reactX.configs['recommended-typescript'],
      // Enable lint rules for React DOM
      reactDom.configs.recommended,
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
])
```

# Project folders architecture

```
├── scripts/
│   └── update-readme-architecture.js
├── src/
│   ├── main/
│   │   ├── components/
│   │   │   ├── header/
│   │   │   │   ├── MainHeaderComponent.module.css
│   │   │   │   └── MainHeaderComponent.tsx
│   │   │   ├── main/
│   │   │   │   ├── MainCenterComponent.module.css
│   │   │   │   ├── MainCenterComponent.tsx
│   │   │   │   ├── MainLeftComponent.module.css
│   │   │   │   ├── MainLeftComponent.tsx
│   │   │   │   ├── MainRightComponent.module.css
│   │   │   │   └── MainRightComponent.tsx
│   │   │   └── modules/
│   │   │       ├── characters/
│   │   │       │   ├── characterDetails/
│   │   │       │   │   ├── illustration/
│   │   │       │   │   │   ├── ExpressionAvatarsComponent.module.css
│   │   │       │   │   │   ├── ExpressionAvatarsComponent.tsx
│   │   │       │   │   │   ├── IllustrationComponent.module.css
│   │   │       │   │   │   └── IllustrationComponent.tsx
│   │   │       │   │   ├── CharacterDetailsComponent.module.css
│   │   │       │   │   └── CharacterDetailsComponent.tsx
│   │   │       │   └── characterSheets/
│   │   │       │       ├── CharacterSheetComponent.module.css
│   │   │       │       └── CharacterSheetComponent.tsx
│   │   │       ├── charactersSelection/
│   │   │       │   ├── CharactersSelectionComponent.module.css
│   │   │       │   └── CharactersSelectionComponent.tsx
│   │   │       ├── chat/
│   │   │       │   ├── chatDetails/
│   │   │       │   │   ├── chatDetailsComponent.module.css
│   │   │       │   │   └── chatDetailsComponent.tsx
│   │   │       │   ├── chatRolls/
│   │   │       │   │   ├── ChatRollsComponent.module.css
│   │   │       │   │   └── ChatRollsComponent.tsx
│   │   │       │   ├── interactiveUserInput/
│   │   │       │   │   ├── InteractiveUserInputComponent.module.css
│   │   │       │   │   └── InteractiveUserInputComponent.tsx
│   │   │       │   ├── message/
│   │   │       │   │   ├── ChatMessageComponent.module.css
│   │   │       │   │   └── ChatMessageComponent.tsx
│   │   │       │   ├── mobileAvatarBanner/
│   │   │       │   │   ├── MobileAvatarBannerComponent.module.css
│   │   │       │   │   └── MobileAvatarBannerComponent.tsx
│   │   │       │   ├── sceneTracker/
│   │   │       │   │   ├── SceneTrackerComponent.module.css
│   │   │       │   │   └── SceneTrackerComponent.tsx
│   │   │       │   ├── userInput/
│   │   │       │   │   ├── UserInputComponent.module.css
│   │   │       │   │   └── UserInputComponent.tsx
│   │   │       │   ├── ChatComponent.module.css
│   │   │       │   └── ChatComponent.tsx
│   │   │       ├── chatCompletionPresets/
│   │   │       │   ├── ChatCompletionPresetsComponent.module.css
│   │   │       │   └── ChatCompletionPresetsComponent.tsx
│   │   │       ├── chatsSelection/
│   │   │       │   ├── ChatSelectionComponent.module.css
│   │   │       │   └── ChatSelectionComponent.tsx
│   │   │       ├── lorebookDetails/
│   │   │       │   ├── lorebookEntry/
│   │   │       │   │   ├── LorebookEntryComponent.module.css
│   │   │       │   │   └── LorebookEntryComponent.tsx
│   │   │       │   ├── LorebookDetailsComponent.module.css
│   │   │       │   └── LorebookDetailsComponent.tsx
│   │   │       ├── lorebooksSelection/
│   │   │       │   ├── LorebookSelectionComponent.module.css
│   │   │       │   └── LorebookSelectionComponent.tsx
│   │   │       ├── personaDetails/
│   │   │       │   ├── PersonaDetailsComponent.module.css
│   │   │       │   └── PersonaDetailsComponent.tsx
│   │   │       ├── personasSelection/
│   │   │       │   ├── PersonasSelectionComponent.module.css
│   │   │       │   └── PersonasSelectionComponent.tsx
│   │   │       └── settings/
│   │   │           ├── SettingsComponent.module.css
│   │   │           └── SettingsComponent.tsx
│   │   └── Constants.ts
│   ├── RequestDto/
│   │   ├── characters/
│   │   │   ├── characterSheets/
│   │   │   │   └── CharacterSheetRequestDto.ts
│   │   │   └── CharacterMainAvatarIllustrationQueryRequestDto.ts
│   │   ├── chat/
│   │   │   └── AddChatRequestDto.ts
│   │   └── lorebooks/
│   │       └── LorebookUpdateRequestDto.ts
│   ├── ResponsesDto/
│   │   ├── characters/
│   │   │   ├── characterSheets/
│   │   │   │   ├── CharacterSheet.ts
│   │   │   │   └── CharacterSheetResponseDto.ts
│   │   │   ├── AvatarPath.ts
│   │   │   ├── CharacterResponse.ts
│   │   │   ├── CharacterResponseDto.ts
│   │   │   ├── CharactersResponseDto.ts
│   │   │   ├── GeneratePromptInjectionForMainCharacterAvatarResponseDto.ts
│   │   │   ├── illustrationMapOutfits.ts
│   │   │   ├── IllustratorGenerationContent.ts
│   │   │   └── ImageGenerationConfiguration.ts
│   │   ├── chat/
│   │   │   ├── BusinessObjects/
│   │   │   │   ├── BackgroundQuery.ts
│   │   │   │   ├── CharacterAvatar.ts
│   │   │   │   ├── ChatMessage.ts
│   │   │   │   └── SceneTracker.ts
│   │   │   ├── interactiveUserInput/
│   │   │   │   └── InteractiveUserInputQueriesResponseDto.ts
│   │   │   ├── BackgroundQueriesResponseDto.ts
│   │   │   ├── BackgroundQueryResponseDto.ts
│   │   │   ├── ChatMessageResponseDto.ts
│   │   │   ├── ChatMessagesResponseDto.ts
│   │   │   ├── ChatResponseDto.ts
│   │   │   ├── PromptResponseDto.ts
│   │   │   └── SceneTrackerResponseDto.ts
│   │   ├── chatCompletionPresets/
│   │   │   ├── ChatCompletionPreset.ts
│   │   │   ├── ChatCompletionPresetsResponseDto.ts
│   │   │   └── ChatCompletionPresetsSettingsResponseDto.ts
│   │   ├── chatSelection/
│   │   │   ├── SelectableChatResponseDto.ts
│   │   │   └── SelectableChatsResponseDto.ts
│   │   ├── Exceptions/
│   │   │   ├── ExceptionResponseDto.ts
│   │   │   └── ServerApiExceptionResponseDto.ts
│   │   ├── lorebooks/
│   │   │   ├── BusinessObjects/
│   │   │   │   ├── Lorebook.ts
│   │   │   │   └── LorebookEntry.ts
│   │   │   ├── LorebookResponseDto.ts
│   │   │   └── LorebooksResponseDto.ts
│   │   ├── personas/
│   │   │   ├── BusinessObjects/
│   │   │   │   └── Persona.ts
│   │   │   ├── PersonaResponseDto.ts
│   │   │   └── PersonasResponseDto.ts
│   │   ├── settings/
│   │   │   ├── BusinessObjects/
│   │   │   │   ├── ChatCompletionPresetsMap.ts
│   │   │   │   ├── LLMProviderSettings.ts
│   │   │   │   ├── SettingsEnums.ts
│   │   │   │   ├── SummarySettings.ts
│   │   │   │   └── TimeoutStrategy.ts
│   │   │   └── SettingsResponseDto.ts
│   │   ├── ChatCharacterRollsResponseDto.ts
│   │   └── ServerApiResponseDto.ts
│   ├── store/
│   │   ├── AppSharedStoreContext.tsx
│   │   ├── MessagesStoreContext.tsx
│   │   ├── SharedContextCharacterType.ts
│   │   ├── SharedContextChatType.ts
│   │   ├── SharedContextLorebookType.ts
│   │   ├── SharedContextPersonaType.ts
│   │   ├── SharedContextSettings.ts
│   │   └── SharedContextType.ts
│   ├── utils/
│   │   ├── http/
│   │   │   ├── response/
│   │   │   │   └── HttpResponseUtils.ts
│   │   │   └── HttpRequestHelper.ts
│   │   ├── avatarUtils.tsx
│   │   ├── DateUtils.ts
│   │   ├── fontSizeUtils.tsx
│   │   ├── HighlightText.module.css
│   │   ├── HighlightText.tsx
│   │   └── uuid.ts
│   ├── App.css
│   ├── App.tsx
│   ├── index.css
│   └── main.tsx
├── CHANGELOG.md
├── CohesiveRP.UI.WebFrontend.esproj
├── eslint.config.js
├── index.html
├── package-lock.json
├── package.json
├── README.md
├── tsconfig.app.json
├── tsconfig.json
├── tsconfig.node.json
└── vite.config.ts
```

# Help-Prompt
We are working together on a project made using Vite, React, Typescript. This WebApp uses HttpRequest to communicate with a backend made in C# (Webserver). The application is named 'CohesiveRp', it's an application to allow roleplay between a User and an AI (using LLMs).

# Settings
Note that 'erasableSyntaxOnly' is enabled.

# UI
Dark Arcane Editorial theme (Dark cyan)

## Backend
The backend is made using c# 10. It exposes many routes. When we receive a new message from the User (typically from the UI), the backend queue two background tasks.

One to update the sceneTracker, which tracks the main themes of the scene (such as Romance, Combat, etc), nestedThemes, currentDateTime (in the roleplay), location, characters (name, mood, facialExpression, outfit, underwear, stateOfDress(partially dressed, fully dressed), exposedBodyParts, clothingStateOfDress, hairStyle, posture, semenOnBodyLocation, bodyPosition, personalGainInScene, relevantKinksInScene, relevantSecretKinksInScene, relevantPersonalityTraits, innerThoughtsOrMonologue, nextActionsAccordingToPersonality).

The second background task is to update the SkillCheckInitiator, which will analyse the scene and infer what Attributes or Skills may be in play. The backend will then roll dices, using the characterSheets attributes and skills to infer their success or failures in the scene.

After those two tasks, the main task is queued. The main task will query the main LLM with the context, characters in scene, rolls, etc.

After this main task, the backend will queue summarization background tasks to summarize the messages between the User and AI and to summarize the summaries to shrink the space required in the context.

## Frontend
The first tab is a view to select a specific chat. When the User clicks on one chat, the chat view opens. That view shows the messages, the input to send new message, the avatars, sceneTracker, buttons (edit message, delete message, etc).

The second tab is a view of the characters. There is a button to add a new character, filter available characters, etc. When the User clicks on a character, the CharacterDetails view opens. In this view, the character name, description, tags, first message, creator, creator notes is shown. Those are editable. There is also an option to open any related Chat or to create a new Chat from this character. The sub-tab 'Info' is selected by default. When selecting the sub-tab 'Character Sheet', the User is shown a component with the character sheet of this character. A button to generate the fields from an LLM is available. All fields are editable. When selecting the sub-tab 'Illustration', the User is shown a component with the character illustration fields and settings (UI), such as illustrator tag, outfit and illustrator prompt injection as well as a carousel with the generated images acting as that character avatar. 

The third tab is Personas. It's fairly similar to Characters, but those are reserved for User impersonation. When the user clicks on a Persona, the PersonaDetails view opens. In this view, contrary to the CharacterDetails, there is only the Description field available and thus editable. The sub-tab 'Info' is selected by default. When selecting the sub-tab 'Character Sheet', the User is shown a component with the character sheet of this character. A button to generate the fields from an LLM is available. All fields are editable. The component CharacterSheet is the same as the one for the characters.

The fourth tab is Lorebooks. The view show a list of available Lorebooks as well as a button to add new ones. When the User click on a Lorebook, a view opens to allow the User to edit the lorebook title and every fields within that lorebook.

The remaining tabs are the options/settings tabs. The fifth tab is the CompletionPresets tab. It allows the User to customize the prompt and context that is used by the background on the various backgroundTasks, such as when summarizing the story, generating a scene tracker, defining the skill checks initiator, dynamic creation of characters, dynamic creation of character sheets, dynamic generation of character avatars for illustration(with comfyui) or for the main chat.

The sixth and last tab shows the settings related to the LLM Apis, completion presets to use (the fifth tab is the one to customize them), summaries handling.