# Project folders architecture

```
├── scripts/
│   └── update-readme-architecture.js
├── src/
│   ├── main/
│   │   ├── components/
│   │   │   ├── header/
│   │   │   │   ├── subHeader/
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
├── CLAUDE.md
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
